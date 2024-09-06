using Autodesk.Revit.DB;
using NorconsultBridgeStudio.Revit.Core;
using NorconsultBridgeStudio.Revit.BridgeModelling.Extensions;
using System.Linq;
using System;
using NorconsultBridgeStudio.Revit.Core.Extensions;
using NorconsultBridgeStudio.Revit.Core.Utils;
using System.Collections.Generic;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Utils
{
    public static class ViewUtils
    {
        public static void CreateSection(Element element)
        {
            Curve curve = element.GetCurve();

            XYZ curveStartPoint = curve.GetEndPoint(0);
            XYZ curveStartTangent = curve.ComputeDerivatives(0, true).BasisX.ToPlan().Normalize();

            Plane viewPlane = Plane.CreateByNormalAndOrigin(curveStartTangent, curveStartPoint);

            // Create section box
            BoundingBoxXYZ sectionBox = CreateSectionBox(element, viewPlane);

            // Determine view family type to use
            ViewFamilyType viewFamilyType = new FilteredElementCollector(App.CurrentDocument)
                                            .OfClass(typeof(ViewFamilyType))
                                            .Cast<ViewFamilyType>()
                                            .FirstOrDefault<ViewFamilyType>(x => ViewFamily.Section == x.ViewFamily);

            // Create transaction
            ViewSection viewSection;
            using (Transaction trans = new Transaction(App.CurrentDocument))
            {
                trans.Start("Create Section at Element Start");

                View lastView = App.CurrentDocument.ActiveView;
                viewSection = ViewSection.CreateSection(App.CurrentDocument, viewFamilyType.Id, sectionBox);
                viewSection.Name = App.CurrentDocument.UniqueName(typeof(ViewSection), "NorconsultBridgeStudio Section");
                viewSection.CopyTemplateAndWorksetSettings(lastView);
                ExtensibleStorageUtils.StoreOrigin(viewSection, curveStartPoint);

                trans.Commit();
            }

            // Change active view
            App.UIApplication.ActiveUIDocument.ActiveView = viewSection;
        }
        private static BoundingBoxXYZ CreateSectionBox(Element elem, Plane plane)
        {
            List<Solid> solids = elem.GetSolids();
            double width = double.MaxValue;
            double height = double.MaxValue;
            double minWidth = 10000 * Constants.MillimeterToFeet;
            double minHeight = 10000 * Constants.MillimeterToFeet;
            XYZ planOrigin = plane.Origin.ToPlan();

            foreach(Solid solid in solids)
            {
                var analyze = ExtrusionAnalyzer.Create(solid, plane, plane.Normal.Negate());

                var minPoint = analyze.GetExtrusionBase().Evaluate(new UV(0, 0));
                if (planOrigin.DistanceTo(minPoint.ToPlan()) < width)
                    width = planOrigin.DistanceTo(minPoint);

                if (Math.Abs(minPoint.Z - planOrigin.Z) < height)
                    height = Math.Abs(minPoint.Z - planOrigin.Z);

                var maxPoint = analyze.GetExtrusionBase().Evaluate(new UV(1, 1));
                if (planOrigin.DistanceTo(maxPoint.ToPlan()) < width)
                    width = planOrigin.DistanceTo(maxPoint);

                if (Math.Abs(maxPoint.Z - planOrigin.Z) < height)
                    height = Math.Abs(maxPoint.Z - planOrigin.Z);
            }

            if (width < minWidth || width == double.MaxValue)
                width = minWidth;
            
            if (height < minHeight || height == double.MaxValue)
                height = minHeight;

            XYZ viewdir = plane.Normal;
            XYZ up, tangentNorm;
            if (plane.YVec.Z > 0)
            {
                up = plane.YVec;
                tangentNorm = (plane.XVec).Normalize();

            }
            else
            {
                up = -plane.YVec;
                tangentNorm = -(plane.XVec).Normalize();
            }

            double depth = 100 * Constants.MillimeterToFeet;
            double offset = depth * 0.1;

            // Min and max of BoundaryBox
            XYZ minbb = new XYZ(-width, -height, 0);
            XYZ maxbb = new XYZ(width, height, offset);

            // Create transform for section box
            Transform transform = Transform.Identity;
            transform.BasisZ = viewdir;
            transform.BasisY = up;
            transform.BasisX = tangentNorm;
            transform.Origin = plane.Origin;

            // Section box
            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ
            {
                Min = minbb,
                Max = maxbb,
                Transform = transform
            };
            return sectionBox;
        }

        private static string UniqueName(string name, int c = 0)
        {
            var suffix = c == 0 ? "" : $"_{c}";
            var filteredViews = new FilteredElementCollector(App.CurrentDocument)
                        .OfClass(typeof(ViewSection))
                        .Where(x => x.Name == name + $"{suffix}")
                        .ToList();

            if (filteredViews.Count == 0)
            {
                return name + suffix;
            }
            else
            {
                return UniqueName(name, c + 1);
            }
        }
    }
}