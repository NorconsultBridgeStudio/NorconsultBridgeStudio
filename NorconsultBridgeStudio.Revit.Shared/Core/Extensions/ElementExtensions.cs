using Autodesk.Revit.DB;
using NorconsultBridgeStudio.Revit.Core;
using System.Collections.Generic;
using System.Linq;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Extensions
{
    public static class ElementExtensions
    {

        /// <summary>
        /// Returns a list containing all solids in the element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static List<Solid> GetSolids(this Element element)
        {
            var geomElement = element.GetGeometryElement();

            var curves = new List<Curve>();
            var solids = new List<Solid>();

            geomElement.AddCurvesAndSolids(ref curves, ref solids);
            return solids.Where(s => s != null && !s.Faces.IsEmpty).ToList();
        }
        /// <summary>
        /// Returns a list containing all curves in the element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static List<Curve> GetCurves(this Element element)
        {
            var geomElement = element.GetGeometryElement();

            var curves = new List<Curve>();
            var solids = new List<Solid>();

            geomElement.AddCurvesAndSolids(ref curves, ref solids);
            return curves.Where(c => c != null && c.Length >= App.UIApplication.Application.ShortCurveTolerance).ToList();
        }
        /// <summary>
        /// Get the underlying curve of the element. For line based elements like beams and curves, the underlying curve is returned.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Curve GetCurve(this Element element)
        {
            Curve curve;
            if (element is DetailCurve dc)
            {
                curve = dc.GeometryCurve;
            }
            else if (element is ModelCurve mc)
            {
                curve = mc.GeometryCurve;
            }
            else if (element.Location is LocationCurve lc)
            {
                curve = lc.Curve;
            }
            else
            {
                curve = element.GetCurves().FirstOrDefault();
            }
            return curve;
        }
        /// <summary>
        /// Returns the GeometryElement of a Revit element, where DetailLevel is set to ViewDetailLevel.Fine
        /// </summary>
        /// <param name="element"></param>
        /// <returns>The GeometryElement of a Revit element</returns>
        private static GeometryElement GetGeometryElement(this Element element)
        {
            var options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;

            var geomElement = element.get_Geometry(options);
            return geomElement;
        }
    }
}