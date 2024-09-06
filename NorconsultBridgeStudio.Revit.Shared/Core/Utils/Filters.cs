using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace NorconsultBridgeStudio.Revit.Core.Utils
{
    public class CurveFamilyFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem.Location is LocationCurve;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
    public class DetailCurveFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is DetailCurve;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
    public class StructuralFramingFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is FamilyInstance fi && fi.Category?.BuiltInCategory == BuiltInCategory.OST_StructuralFraming;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}