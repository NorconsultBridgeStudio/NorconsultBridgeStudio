using Autodesk.Revit.DB;

namespace NorconsultBridgeStudio.Revit.Core.Extensions
{
    public static class ViewExtensions
    {
        /// <summary>
        /// Copies template and workset settings from another view to this view.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="doc"></param>
        /// <param name="copyFromView"></param>
        public static void CopyTemplateAndWorksetSettings(this View view, View copyFromView)
        {

            // Assign the None view template
            ElementId id = copyFromView.ViewTemplateId;
            view.ViewTemplateId = id;

            if (id != ElementId.InvalidElementId)
                return;

            // Assign the same workset settings as the active view
            FilteredWorksetCollector worksets = new FilteredWorksetCollector(App.CurrentDocument).OfKind(WorksetKind.UserWorkset);

            foreach (Workset workset in worksets)
            {
                // Copies the settings from copyFromView to view
                WorksetId wId = workset.Id;
                WorksetVisibility vis = copyFromView.GetWorksetVisibility(wId);
                view.SetWorksetVisibility(wId, vis);
            }
        }
    }
}