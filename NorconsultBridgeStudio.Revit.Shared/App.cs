using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NorconsultBridgeStudio.Revit.Core.Startup;

namespace NorconsultBridgeStudio.Revit.Core
{
    public class App : IExternalApplication
    {
        #region General Revit Properties
        public static Document CurrentDocument { get; internal set; }
        public static UIApplication UIApplication { get; internal set; }
        #endregion

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                Ribbon.AddRibbonPanel(application);
                Events.AddEvents(application.ControlledApplication);
            }
            catch
            {
                return Result.Failed;
            }

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            Events.RemoveEvents(application.ControlledApplication);
            return Result.Succeeded;
        }

    }
}