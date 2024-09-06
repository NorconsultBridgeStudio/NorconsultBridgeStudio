using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using NorconsultBridgeStudio.Revit.BridgeModelling.ViewModels;
using NorconsultBridgeStudio.Revit.Core;
using NorconsultBridgeStudio.Revit.Core.Exceptions;
using NorconsultBridgeStudio.Revit.Core.Utils;
using Microsoft.Win32;
using NorconsultBridgeStudio.Revit.BridgeModelling.Utils;
using System.Globalization;
using System.Data.Common;
using NorconsultBridgeStudio.Revit.Core.Extensions;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Models
{
    class AlignmentByPointsModel
    {
        public void Execute()
        {
            var beams = GetBeamFamilies(out int? defaultIndex);
            var vm = new SingleComboboxViewModel("NorconsultBridgeStudio - Alignment by Points", "Beams", beams, defaultIndex);
            
            if (vm.View.ShowDialog() != true)
                return;
            
            string path = GetPointFilePath();
            if (string.IsNullOrEmpty(path))
                return;

            Level level = GetLevel();
            List<XYZ> points = ParsePoints(path);
            List<Curve> curves = CombineToCurves(points);
            CreateBeams(curves, vm.Element.Id, level);
        }
        private List<XYZ> ParsePoints(string path)
        {
            string[] content = File.ReadAllLines(path);

            List<XYZ> points = new List<XYZ>();

            foreach(string line in content)
            {
                string[] cells = line.Split(' ');
                //double station = double.Parse(cells[0]);
                if (!double.TryParse(cells[0], NumberStyles.Number, CultureInfo.CurrentCulture, out double station))
                    if (!double.TryParse(cells[0], NumberStyles.Number, CultureInfo.InvariantCulture, out station))
                        throw new NorconsultBridgeStudioError("Error parsing numbers. Please ensure proper structure of the point file");

                if (!double.TryParse(cells[2], NumberStyles.Number, CultureInfo.CurrentCulture, out double northing))
                    if (!double.TryParse(cells[2], NumberStyles.Number, CultureInfo.InvariantCulture, out northing))
                        throw new NorconsultBridgeStudioError("Error parsing numbers. Please ensure proper structure of the point file");

                if (!double.TryParse(cells[1], NumberStyles.Number, CultureInfo.CurrentCulture, out double easting))
                    if (!double.TryParse(cells[1], NumberStyles.Number, CultureInfo.InvariantCulture, out easting))
                        throw new NorconsultBridgeStudioError("Error parsing numbers. Please ensure proper structure of the point file");

                if (!double.TryParse(cells[3], NumberStyles.Number, CultureInfo.CurrentCulture, out double elevation))
                    if (!double.TryParse(cells[3], NumberStyles.Number, CultureInfo.InvariantCulture, out elevation))
                        throw new NorconsultBridgeStudioError("Error parsing numbers. Please ensure proper structure of the point file");

                XYZ globalCoordinates = new XYZ(easting*Constants.MeterToFeet, northing * Constants.MeterToFeet, elevation * Constants.MeterToFeet);
                XYZ relativeCoordinates = GetRelativePointInRevit(globalCoordinates);
                points.Add(relativeCoordinates);
            }

            return points;
        }
        private List<Curve> CombineToCurves(List<XYZ> points)
        {
            List<Curve> curves = new List<Curve>();

            if (points.Count < 2)
                throw new NorconsultBridgeStudioError("The PENZ file needs to contain at least two points");

            int i = 0;
            while (i < points.Count - 1)
            {
                bool success = true;
                try
                {
                    Arc arc1 = Arc.Create(points[i], points[i + 2], points[i + 1]);
                    if (arc1.Radius > 9000) // Revit can't handle radius with too large arcs, and fails somewhere betweeen 8983ft and 10383ft. 
                        success = false;
                }
                catch
                {
                    success = false;
                }

                if (success)
                {
                    Arc arc = Arc.Create(points[i], points[i + 2], points[i + 1]);
                    curves.Add(arc);
                    i++;
                }
                else
                {
                    Line line = Line.CreateBound(points[i], points[i + 1]);
                    curves.Add(line);
                }
                i++;
            }

            return curves;
        }
        private Level GetLevel()
        {
            var levels = new FilteredElementCollector(App.CurrentDocument)
                            .OfClass(typeof(Level))
                            .WhereElementIsNotElementType()
                            .ToElements()
                            .OfType<Level>()
                            .ToList();

            Level level = levels.First(l => Math.Abs(l.Elevation) < 0.001);

            return level;
        }

        private void CreateBeams(List<Curve> curves, ElementId symbolId, Level level)
        {
            using (Transaction transaction = new Transaction(App.CurrentDocument))
            {
                transaction.Start("Alignment by Points");
                transaction.SuppressWarnings();

                FamilySymbol symbol = App.CurrentDocument.GetElement(symbolId) as FamilySymbol;
                if (!symbol.IsActive)
                    symbol.Activate();

                foreach (Curve curve in curves)
                {
                    FamilyInstance beam = App.CurrentDocument.Create.NewFamilyInstance(curve, symbol, level, StructuralType.Beam);
                    beam.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(2);
                    StructuralFramingUtils.DisallowJoinAtEnd(beam, 0);
                    StructuralFramingUtils.DisallowJoinAtEnd(beam, 1);
                }
                transaction.Commit();
            }
        }
        private List<RevitElementModel> GetBeamFamilies(out int? defaultIndex)
        {
            var symbols = new FilteredElementCollector(App.CurrentDocument)
                            .OfClass(typeof(FamilySymbol))
                            .OfCategory(BuiltInCategory.OST_StructuralFraming)
                            .Cast<FamilySymbol>()
                            .Where(b => b.Family.FamilyPlacementType == FamilyPlacementType.CurveDrivenStructural)
                            .ToList();

            var defaultBeam = symbols.FirstOrDefault(b => b.Name.ToLower().Contains("alignment"));

            if (defaultBeam == null)
            {
                using (Transaction transaction = new Transaction(App.CurrentDocument))
                {
                    transaction.Start("Load Beam");

                    var familyPath = Path.Combine(Path.GetDirectoryName(Constants.ThisAssemblyPath), "NBS_Alignment.rfa");
                    App.CurrentDocument.LoadFamily(familyPath, new AlignmentLoadOptions(), out Family family );
                    
                    transaction.Commit();
                }

                symbols = new FilteredElementCollector(App.CurrentDocument)
                                .OfClass(typeof(FamilySymbol))
                                .OfCategory(BuiltInCategory.OST_StructuralFraming)
                                .Cast<FamilySymbol>()
                                .Where(b => b.Family.FamilyPlacementType == FamilyPlacementType.CurveDrivenStructural)
                                .ToList();

                defaultBeam = symbols.FirstOrDefault(b => b.Name.ToLower().Contains("alignment"));
            }

            List<RevitElementModel> elements = new List<RevitElementModel>();
            RevitElementModel defaultElement = null;
            foreach (FamilySymbol symbol in symbols)
            {
                var name = $"{symbol.FamilyName}: {symbol.Name}";
                var element = new RevitElementModel(symbol.Id, name);
                if (symbol == defaultBeam)
                    defaultElement = element;
                elements.Add(element);
            }

            elements = elements.OrderBy(e => e.Name).ToList();
            defaultIndex = elements.IndexOf(defaultElement);

            return elements;
        }

        private string GetPointFilePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (!string.IsNullOrEmpty(Paths.LastDirectory))
                openFileDialog.InitialDirectory = Paths.LastDirectory;

            openFileDialog.Filter = "PENZ File (_*.txt)|*.txt|All files (*.*)|*.*";

            if (!openFileDialog.ShowDialog() == true)
                return "";

            string path = openFileDialog.FileName;
            Paths.LastDirectory = Path.GetDirectoryName(path);

            return path;
        }


        private XYZ GetRelativePointInRevit(XYZ point)
        {
            XYZ projectBasePoint = GetProjectBasePoint();
            return point - projectBasePoint;
        }

        private XYZ GetProjectBasePoint()
        {
            BasePoint basePoint = GetBasePoint();

            double basePointX = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble();
            double basePointY = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble();
            double basePointZ = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble();

            XYZ origin = new XYZ(basePointX, basePointY, basePointZ);
            return origin;
        }

        private BasePoint GetBasePoint()
        {
            BasePoint basePoint = new FilteredElementCollector(App.CurrentDocument)
                                        .OfClass(typeof(BasePoint))
                                        .ToElements()
                                        .Cast<BasePoint>()
                                        .First(b => !b.IsShared);

            return basePoint;
        }
    }
}