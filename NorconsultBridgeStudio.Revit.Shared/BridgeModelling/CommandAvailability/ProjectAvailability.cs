using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.CommandAvailability
{
    public class ProjectAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            return AvailabilityUtils.IsProject(applicationData);
        }
    }
}
