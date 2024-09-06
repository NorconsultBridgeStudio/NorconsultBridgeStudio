using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using NorconsultBridgeStudio.Revit.BridgeModelling.CommandAvailability;
using NorconsultBridgeStudio.Revit.BridgeModelling.Models;
using NorconsultBridgeStudio.Revit.Core;
using NorconsultBridgeStudio.Revit.Core.Exceptions;
using NorconsultBridgeStudio.Revit.Core.Extensions;
using NorconsultBridgeStudio.Revit.Core.Interfaces;
using NorconsultBridgeStudio.Revit.Core.Utils;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class HelpCommand : INorconsultBridgeStudioCommand
    {
        public string ToolTip => "Opens website for help and documentation";

        public string IconName => "help";

        public string PublicName => "Help";

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
            string url = @"http://www.bimforbridges.com/help";
            System.Diagnostics.Process.Start(url);
        }
    }
}