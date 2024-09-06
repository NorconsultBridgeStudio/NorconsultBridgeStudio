using Autodesk.Revit.DB;
using NorconsultBridgeStudio.Revit.Core.Utils;
using System;
using System.Linq;

namespace NorconsultBridgeStudio.Revit.Core.Extensions
{
    public static class DocumentExtensions
    {
        /// <summary>
        /// Returns a unique name of a given type
        /// </summary>
        /// <param name="document"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string UniqueName(this Document document, Type type, string name, int c = 0)
        {
            var suffix = c == 0 ? "" : $"_{c}";
            var filteredElements = new FilteredElementCollector(document)
                        .OfClass(type)
                        .Where(x => x.Name == name + $"{suffix}")
                        .ToList();

            if (filteredElements.Count == 0)
            {
                return name + suffix;
            }
            else
            {
                return document.UniqueName(type, name, c + 1);
            }
        }
        /// <summary>
        /// Create or get sketch plan in the document, given a name.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="name">The name of the sketch plane</param>
        /// <returns></returns>
        public static SketchPlane GetSketchPlane(this Document doc, string name)
        {
            SketchPlane sketchPlane = new FilteredElementCollector(doc).OfClass(typeof(SketchPlane)).Cast<SketchPlane>().FirstOrDefault<SketchPlane>(x => x.Name.ToLower().Contains(name.ToLower()));

            return sketchPlane;
        }
        /// <summary>
        /// Returns an XYZ of the project base point. If the document is a family document, null is returned.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static BasePoint GetProjectBasePoint(this Document doc)
        {
            if (doc.IsFamilyDocument)
                return null;
            BasePoint basePoint = new FilteredElementCollector(doc)
                                        .OfClass(typeof(BasePoint))
                                        .ToElements()
                                        .Cast<BasePoint>()
                                        .First(b => !b.IsShared);

            return basePoint;
        }
        /// <summary>
        /// Returns the angle to true north of the project document. If the document is a family document, null is returned.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static double? GetProjectRotation(this Document doc)
        {
            if (doc.IsFamilyDocument)
                return null;
            BasePoint basePoint = doc.GetProjectBasePoint();
            double basePointTheta = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_ANGLETON_PARAM).AsDouble();

            return basePointTheta;
        }
    }
}