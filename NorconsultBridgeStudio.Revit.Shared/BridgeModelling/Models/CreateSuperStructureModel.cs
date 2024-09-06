using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using NorconsultBridgeStudio.Revit.BridgeModelling.Extensions;
using NorconsultBridgeStudio.Revit.BridgeModelling.Utils;
using NorconsultBridgeStudio.Revit.Core;
using NorconsultBridgeStudio.Revit.Core.Exceptions;
using NorconsultBridgeStudio.Revit.Core.Extensions;
using NorconsultBridgeStudio.Revit.Core.Utils;
using Microsoft.Win32;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Models
{
    class CreateSuperStructureModel
    {
        private readonly IEnumerable<Reference> _references;
        private readonly Document _rvtDocument;
        private XYZ _midPoint;
        public CreateSuperStructureModel(IEnumerable<Reference> references) 
        { 
            _references = references;
            _rvtDocument = App.CurrentDocument;
        }
        public void Execute() 
        {
            try
            {
                var alignmentCurves = DefineAlignment();
                var path = GetCrossSectionPath();
                CreateFamilyDocument(path, out FamilySymbol symbol);
                CreateForms(alignmentCurves, symbol);
                ElementId symbolId = LoadFamily(path);
                PlaceSuperStructureModel.PlaceInDocument(_rvtDocument, symbolId);
                App.CurrentDocument = _rvtDocument;
            }
            catch (Exception ex)
            {
                App.CurrentDocument = _rvtDocument;
                throw ex;
            }

        }
        private List<Curve> DefineAlignment()
        {
            List<Curve> curves = new List<Curve>();

            foreach (var curveElementReference in _references)
            {
                Element element = App.CurrentDocument.GetElement(curveElementReference);
                curves.Add(element.GetCurve());
            }

            Curve startCurve = GetStartCurve(curves);
            List<Curve> sortedCurves = SortCurves(curves, startCurve);
            List<Curve> simplifiedCurves = SimplifyCurveList(sortedCurves); 
            _midPoint = (simplifiedCurves.First().GetEndPoint(0) + simplifiedCurves.Last().GetEndPoint(1)) / 2;
            Transform translation = Transform.CreateTranslation(_midPoint.Negate());
            simplifiedCurves = simplifiedCurves.Select(c => c.CreateTransformed(translation)).ToList();
            
            return simplifiedCurves;
        }
        private string GetCrossSectionPath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (!string.IsNullOrEmpty(Paths.LastDirectory))
                openFileDialog.InitialDirectory = Paths.LastDirectory;

            openFileDialog.Filter = "NorconsultBridgeStudio cross section family file (_*.rfa)|*.rfa|Revit family file (*.rfa)|*.rfa|All files (*.*)|*.*";

            if (!openFileDialog.ShowDialog() == true)
                throw new NorconsultBridgeStudioError("Operation cancelled by user, can not proceed without any cross section file");

            string path = openFileDialog.FileName;
            Paths.LastDirectory = Path.GetDirectoryName(path);

            return path;
        }
        private void CreateForms(List<Curve> curves, FamilySymbol symbol)
        {
            List<Form> forms = new List<Form>();

            using (TransactionGroup transactionGroup = new TransactionGroup(App.CurrentDocument))
            {
                transactionGroup.Start("Create Forms");

                foreach (Curve curve in curves)
                {
                    Form form = CreateForm(curve, symbol);
                    forms.Add(form);
                }

                //TryJoinForms(forms);

                transactionGroup.Assimilate();
            }
        }

        //private void TryJoinForms(List<Form> forms)
        //{
        //    var combinableElementArray = new CombinableElementArray();
        //    foreach(Form form in forms)
        //        combinableElementArray.Append(form);

        //    using (Transaction transaction = new Transaction(App.CurrentDocument))
        //    {
        //        try
        //        {
        //            transaction.Start("Try Join Forms");
        //            App.CurrentDocument.CombineElements(combinableElementArray);
        //            transaction.Commit();
        //        }
        //        catch { }
        //    }
        //}
        private ElementId LoadFamily(string path)
        {
            string uniqueFamilyName = _rvtDocument.UniqueName(typeof(Family), $"Superstructure{Path.GetFileNameWithoutExtension(path)}");
            string threeDFamilyPath = Path.Combine(Path.GetDirectoryName(path), $"{uniqueFamilyName}.rfa");
            var opt = new SaveAsOptions()
            {
                OverwriteExistingFile = true,
                MaximumBackups = 1,
            };
            App.CurrentDocument.SaveAs(threeDFamilyPath, opt);

            App.CurrentDocument.Close();

            Family family = null;
            using (Transaction transaction = new Transaction(_rvtDocument))
            {
                transaction.Start("Load Family");
                _rvtDocument.LoadFamily(threeDFamilyPath, new SuperstructureLoadOptions(), out family);
                transaction.Commit();
            }

            return family.GetFamilySymbolIds().FirstOrDefault();
        }

        private void CreateFamilyDocument(string path, out FamilySymbol symbol)
        {
            Document familyDocument = App.UIApplication.Application.NewFamilyDocument(Paths.GetAdaptivePath());

            using (Transaction transaction = new Transaction(familyDocument))
            {
                transaction.Start("Load family");

                var wasLoaded = familyDocument.LoadFamily(path, new CrossSectionLoadOptions(), out Family family);
                var symbolId = family.GetFamilySymbolIds().First();
                symbol = App.CurrentDocument.GetElement(symbolId) as FamilySymbol;
                symbol.Activate();

                transaction.Commit();
            }

            FamilyManager familyManager = familyDocument.FamilyManager;

            using (Transaction transaction = new Transaction(familyDocument))
            {
                transaction.Start("Setup family document");
                familyManager.NewType("Type 1");

                FamilyParameter xParameter = familyManager.AddParameter(Constants.FamilyBasePointX, GroupTypeId.Constraints, SpecTypeId.Number, false);
                FamilyParameter yParameter = familyManager.AddParameter(Constants.FamilyBasePointY, GroupTypeId.Constraints, SpecTypeId.Number, false);
                FamilyParameter zParameter = familyManager.AddParameter(Constants.FamilyBasePointZ, GroupTypeId.Constraints, SpecTypeId.Number, false);
                FamilyParameter watermarkParameter = familyManager.AddParameter(Constants.WatermarkParameter, GroupTypeId.IdentityData, SpecTypeId.String.Text, false);
                FamilyParameter materialParameter = familyManager.AddParameter(Constants.Material, GroupTypeId.Materials, SpecTypeId.Reference.Material, true);

                familyManager.SetFormula(watermarkParameter, Constants.WatermarkFormula);
                familyManager.SetFormula(xParameter, (_midPoint.X).ToString(System.Globalization.CultureInfo.InvariantCulture));
                familyManager.SetFormula(yParameter, (_midPoint.Y).ToString(System.Globalization.CultureInfo.InvariantCulture));
                familyManager.SetFormula(zParameter, (_midPoint.Z).ToString(System.Globalization.CultureInfo.InvariantCulture));

                Parameter hostRebarParameter = App.CurrentDocument.OwnerFamily.get_Parameter(BuiltInParameter.FAMILY_CAN_HOST_REBAR);
                hostRebarParameter.Set(1);

                Parameter sharedParameter = App.CurrentDocument.OwnerFamily.get_Parameter(BuiltInParameter.FAMILY_SHARED);
                sharedParameter.Set(1);

                transaction.Commit();
            }
        }

        private Form CreateForm(Curve curve, FamilySymbol familySymbol)
        {
            Dictionary<XYZ, double> origins = ParseCurve(curve);
            var ids = new List<ElementId>();

            using (Transaction transaction = new Transaction(App.CurrentDocument))
            {
                transaction.Start("Place Cross Sections");
                transaction.SuppressWarnings(); 
                foreach (var origin in origins)
                {
                    FamilyInstance familyInstance = App.CurrentDocument.FamilyCreate.NewFamilyInstance(origin.Key, familySymbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                    ElementTransformUtils.RotateElement(App.CurrentDocument, familyInstance.Id, Line.CreateBound(origin.Key, origin.Key + XYZ.BasisZ), -origin.Value);
                    familyInstance.get_Parameter(BuiltInParameter.IS_VISIBLE_PARAM).Set(0);
                    ids.Add(familyInstance.Id);
                }
                transaction.Commit();
            }

            Form form = LoftSolids(ids);

            return form;
        }

        private Form LoftSolids(IEnumerable<ElementId> ids)
        {
            var elements = new List<FamilyInstance>();

            var materialParameter = App.CurrentDocument.FamilyManager.get_Parameter(Constants.Material);
            var view = new FilteredElementCollector(App.CurrentDocument).OfClass(typeof(View3D)).Cast<View>().First(v => v.Name == "{3D}");

            Options options = new Options
            {
                ComputeReferences = true,
                View = view
            };

            foreach (var id in ids)
                elements.Add(App.CurrentDocument.GetElement(id) as FamilyInstance);

            var referenceArrayArray = new ReferenceArrayArray();

            foreach (var crossSection in elements)
            {
                List<GeometryObject> objects = new List<GeometryObject>();

                GeometryElement geometryElement = crossSection.get_Geometry(options);
                var geometry = CollectConcreteGeometry(geometryElement, true);

                var referenceArray = GetReferences(geometry);

                referenceArrayArray.Append(referenceArray);
            }

            Form form = null;
            using (Transaction transaction = new Transaction(App.CurrentDocument))
            {
                transaction.Start("Loft Forms");
                form = App.CurrentDocument.FamilyCreate.NewLoftForm(true, referenceArrayArray);

                var materialFormParameter = form.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
                App.CurrentDocument.FamilyManager.AssociateElementParameterToFamilyParameter(materialFormParameter, materialParameter);

                transaction.Commit();
            }

            return form;

        }
        private IEnumerable<GeometryObject> CollectConcreteGeometry(GeometryElement geometryElement, bool useSymbolGeometry = false)
        {
            var instanceGeometryObjects = new List<GeometryObject>();

            if (geometryElement == null) return instanceGeometryObjects;

            foreach (GeometryObject geob in geometryElement)
            {
                var geomInstance = geob as GeometryInstance;
                var geomElement = geob as GeometryElement;

                if (geomInstance != null)
                {
                    var instanceGeom = useSymbolGeometry ? geomInstance.GetSymbolGeometry() : geomInstance.GetInstanceGeometry();
                    instanceGeometryObjects.AddRange(CollectConcreteGeometry(instanceGeom));
                }
                else if (geomElement != null)
                {
                    instanceGeometryObjects.AddRange(CollectConcreteGeometry(geometryElement));
                }
                else
                {
                    instanceGeometryObjects.Add(geob);
                }
            }

            // Certain kinds of Elements will return Solids with zero faces - make sure to filter them out
            return instanceGeometryObjects.Where(x => !(x is Solid) || (x as Solid).Faces.Size > 0);
        }
        private ReferenceArray GetReferences(IEnumerable<GeometryObject> geometryObjects)
        {
            var referenceArray = new ReferenceArray();
            foreach (var geometryObject in geometryObjects)
            {
                Curve revitCurve = geometryObject as Curve;
                if (revitCurve == null && revitCurve.Reference == null)
                    continue;
                referenceArray.Append(revitCurve.Reference);
            }

            return referenceArray;
        }
        private Dictionary<XYZ, double> ParseCurve(Curve curve)
        {
            Dictionary<XYZ, double> origins = new Dictionary<XYZ, double>();
            List<double> parameters = new List<double>();

            if (curve is Line)
            {
                parameters.Add(0);
                parameters.Add(1);
            }
            else
            {
                parameters.Add(0);
                parameters.Add(0.25);
                parameters.Add(0.5);
                parameters.Add(0.75);
                parameters.Add(1);
            }

            foreach (double parameter in parameters)
            {
                XYZ point = curve.Evaluate(parameter, true);
                double angle = curve.ComputeDerivatives(parameter, true).BasisX.AngleOnPlaneTo(XYZ.BasisY, XYZ.BasisZ);
                origins.Add(point, angle);
            }

            return origins;
        }
        private List<Curve> SimplifyCurveList(List<Curve> curves)
        {
            List<Curve> simplifiedCurves = new List<Curve>();
            List<Curve> toSimplify = new List<Curve>();
            Curve simplifiedCurve;
            toSimplify.Add(curves.First());

            for (int i = 0; i < curves.Count - 1; i++)
            {
                if (curves[i] is Arc arc1 && curves[i + 1] is Arc arc2 && MathUtils.AreFloatsEqual(arc1.Radius, arc2.Radius) && arc1.Center.IsAlmostEqualTo(arc2.Center))
                {
                    toSimplify.Add(curves[i + 1]);
                }
                else if (curves[i] is Line line1 && curves[i + 1] is Line line2 && line1.Direction.IsAlmostEqualTo(line2.Direction))
                {
                    toSimplify.Add(curves[i + 1]);
                }
                else
                {
                    simplifiedCurve = SimplifyCurves(ref toSimplify);
                    simplifiedCurves.Add(simplifiedCurve);
                    toSimplify.Add(curves[i + 1]);
                }
            }

            simplifiedCurve = SimplifyCurves(ref toSimplify);
            simplifiedCurves.Add(simplifiedCurve);

            return simplifiedCurves;
        }
        private Curve SimplifyCurves(ref List<Curve> curves)
        {
            Curve curve = null;
            if (curves.Count == 1)
            {
                curve = curves.First();
            }
            else if (curves.Count > 1)
            {
                if (curves.First() is Line)
                    curve = Line.CreateBound(curves.First().GetEndPoint(0), curves.Last().GetEndPoint(1));
                else
                    curve = Arc.Create(curves.First().GetEndPoint(0), curves.Last().GetEndPoint(1), curves.First().GetEndPoint(1));
            }

            curves.Clear();

            return curve;
        }

        private List<Curve> SortCurves(List<Curve> curveElements, Curve startCurve)
        {
            List<Curve> sortedCurves = new List<Curve>() { startCurve };
            while (curveElements.Any())
            {
                Curve current = sortedCurves.Last();
                XYZ endXYZ = current.GetEndPoint(1);

                Curve next = curveElements.FirstOrDefault(ce => ce.GetEndPoint(0).IsAlmostEqualTo(endXYZ));
                if (next != null)
                {
                    curveElements.Remove(next);
                    sortedCurves.Add(next);
                }
                else
                {
                    Curve nextReversed = curveElements.FirstOrDefault(ce => ce.GetEndPoint(1).IsAlmostEqualTo(endXYZ)) ?? throw new NorconsultBridgeStudioError("Not all curves are connected!");
                    curveElements.Remove(nextReversed);
                    sortedCurves.Add(nextReversed.CreateReversed());
                }
            }
            return sortedCurves;
        }

        private Curve GetStartCurve(List<Curve> curveElements)
        {
            // find start curve
            foreach (Curve curve in curveElements)
            {
                XYZ start = curve.GetEndPoint(0);
                XYZ end = curve.GetEndPoint(1);

                bool foundStart = false;
                bool foundEnd = false;
                foreach (Curve nextCurve in curveElements)
                {
                    if (curve == nextCurve)
                        continue;

                    XYZ curveStart = nextCurve.GetEndPoint(0);
                    XYZ curveEnd = nextCurve.GetEndPoint(1);
                    if (start.IsAlmostEqualTo(curveStart) || start.IsAlmostEqualTo(curveEnd))
                        foundStart = true;
                    else if (end.IsAlmostEqualTo(curveStart) || end.IsAlmostEqualTo(curveEnd))
                        foundEnd = true;
                }

                if (foundStart == false) // start not found => start curve
                {
                    curveElements.Remove(curve);
                    return curve;
                }

                else if (foundEnd == false) // end not found => start curve
                {
                    curveElements.Remove(curve);
                    return curve.CreateReversed();
                }
            }
            throw new Exception("No start curve found!");
        }
    }
}