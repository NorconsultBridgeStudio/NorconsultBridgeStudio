using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NorconsultBridgeStudio.Revit.BridgeModelling.CommandAvailability;
using NorconsultBridgeStudio.Revit.BridgeModelling.ViewModels;
using NorconsultBridgeStudio.Revit.Core.Exceptions;
using NorconsultBridgeStudio.Revit.Core.Interfaces;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class AboutCommand : INorconsultBridgeStudioCommand
    {
        public string ToolTip => "About Norconsult Bridge Studio";

        public string IconName => "about";

        public string PublicName => "About";

        public string CommandAvailability => typeof(AlwaysAvailability).FullName;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                RunCommand();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                return ExceptionHandler.HandleException(ex, PublicName);
            }
        }

        public void RunCommand()
        {
            var vm = new AboutViewModel();
            vm.View.ShowDialog();
        }
    }
}