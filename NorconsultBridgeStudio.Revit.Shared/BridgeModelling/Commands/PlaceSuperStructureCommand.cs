using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NorconsultBridgeStudio.Revit.BridgeModelling.CommandAvailability;
using NorconsultBridgeStudio.Revit.BridgeModelling.Models;
using NorconsultBridgeStudio.Revit.Core.Exceptions;
using NorconsultBridgeStudio.Revit.Core.Interfaces;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class PlaceSuperStructureCommand : INorconsultBridgeStudioCommand
    {
        public string ToolTip => "Places a superstructure family";

        public string IconName => "placeBridge";

        public string PublicName => "Place\nSuperstructure";

        public string CommandAvailability => typeof(ProjectAvailability).FullName;

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
            PlaceSuperStructureModel.Execute();
        }
    }
}