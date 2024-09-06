using Autodesk.Revit.DB;
using System.Collections.Generic;


namespace NorconsultBridgeStudio.Revit.BridgeModelling.Extensions
{
    public static class GeometryElementExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geomElem"></param>
        /// <param name="curves"></param>
        /// <param name="solids"></param>
        public static void AddCurvesAndSolids(this GeometryElement geomElem, ref List<Curve> curves, ref List<Solid> solids)
        {
            foreach (GeometryObject geomObj in geomElem)
            {
                switch (geomObj)
                {
                    case Curve c:
                        curves.Add(c);
                        break;
                    case Solid s:
                        solids.Add(s);
                        break;
                    case GeometryInstance g:
                        var transformedGeometryElement = g.GetInstanceGeometry();
                        transformedGeometryElement.AddCurvesAndSolids(ref curves, ref solids);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}