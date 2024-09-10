using Autodesk.Revit.DB;
using NorconsultBridgeStudio.Revit.BridgeModelling.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using NorconsultBridgeStudio.Revit.Core;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using Autodesk.Revit.UI;

namespace NorconsultBridgeStudio.Revit.Shared.BridgeModelling.Models
{
    public class OrientToCurveModel
    {
        public void Execute()
        {
            Reference curveReference = App.UIApplication.ActiveUIDocument.Selection.PickObject(ObjectType.Element, "Pick curve");
            ModelCurve modelCurve = App.CurrentDocument.GetElement(curveReference) as ModelCurve;
            Curve curve = modelCurve.GeometryCurve;

            List<Reference> elementsReference = App.UIApplication.ActiveUIDocument.Selection.PickObjects(ObjectType.Element, "Pick elements to orient").ToList();
            List<Element> elements = new List<Element>();
            foreach (Reference element in elementsReference)
            {
                elements.Add(App.CurrentDocument.GetElement(element));
            }

            using (Transaction transaction = new Transaction(App.CurrentDocument))
            {
                transaction.Start("Orient to curve");
                foreach (Element element in elements)
                {
                    OrientElement(element, curve);
                }
                TaskDialog.Show("Bridge Studio", $"Rotated {elements.Count} family instances");
                transaction.Commit();
            }
        }

        public bool OrientElement(Element element, Curve curve)
        {

            LocationPoint locationPoint = element.Location as LocationPoint;

            XYZ insertionPoint = locationPoint.Point;
            
            XYZ rotationPoint = new XYZ(Math.Cos(locationPoint.Rotation), Math.Sin(locationPoint.Rotation), 0);

            // Get Tangent
            Double parameter = curve.Project(insertionPoint).Parameter;
            XYZ tangent = curve.ComputeDerivatives(parameter, false).BasisX;

            // Angle of rotation
            double angle = rotationPoint.AngleOnPlaneTo(tangent, new XYZ(0, 0, 1)) - Math.PI / 2;

            Line rotationAxis = Line.CreateBound(insertionPoint, insertionPoint + new XYZ(0, 0, 10000));
            
            return locationPoint.Rotate(rotationAxis, angle);
        }
    }
}
