using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.CommandAvailability
{
    public class AvailabilityUtils
    {
        public static bool IsThree3DView(UIApplication applicationData)
        {
            if (applicationData.ActiveUIDocument == null)
                return false;

            return applicationData.ActiveUIDocument.ActiveView.ViewType == ViewType.ThreeD;
        }
        public static bool IsSectionView(UIApplication applicationData)
        {
            if (applicationData.ActiveUIDocument == null)
                return false;

            return applicationData.ActiveUIDocument.ActiveView.ViewType == ViewType.Section;
        }
        public static bool IsModelView(UIApplication applicationData)
        {
            if (applicationData.ActiveUIDocument == null)
                return false;

            return applicationData.ActiveUIDocument.ActiveView.ViewType == ViewType.FloorPlan ||
                   applicationData.ActiveUIDocument.ActiveView.ViewType == ViewType.CeilingPlan ||
                   applicationData.ActiveUIDocument.ActiveView.ViewType == ViewType.Elevation ||
                   applicationData.ActiveUIDocument.ActiveView.ViewType == ViewType.ThreeD ||
                   applicationData.ActiveUIDocument.ActiveView.ViewType == ViewType.DraftingView ||
                   applicationData.ActiveUIDocument.ActiveView.ViewType == ViewType.EngineeringPlan ||
                   applicationData.ActiveUIDocument.ActiveView.ViewType == ViewType.AreaPlan ||
                   applicationData.ActiveUIDocument.ActiveView.ViewType == ViewType.Section ||
                   applicationData.ActiveUIDocument.ActiveView.ViewType == ViewType.Detail;
        }
        public static bool IsProject(UIApplication applicationData)
        {
            if (applicationData.ActiveUIDocument == null)
                return false;

            return !applicationData.ActiveUIDocument.Document.IsFamilyDocument;
        }
        public static bool IsFamily(UIApplication applicationData)
        {
            if (applicationData.ActiveUIDocument == null)
                return false;

            return applicationData.ActiveUIDocument.Document.IsFamilyDocument;
        }
        public static bool IsAdaptiveFamily(UIApplication applicationData)
        {
            if (applicationData.ActiveUIDocument == null)
                return false;

            Document doc = applicationData.ActiveUIDocument.Document;
            return IsFamily(applicationData)
                && doc.OwnerFamily?.FamilyCategoryId == new ElementId(BuiltInCategory.OST_GenericModel)
                && AdaptiveComponentFamilyUtils.IsAdaptiveComponentFamily(doc.OwnerFamily);
        }
    }
}
