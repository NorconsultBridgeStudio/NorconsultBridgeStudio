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
    class AdaptiveCrossSectionCommand : INorconsultBridgeStudioCommand
    {
        public string ToolTip => "Creates a cross section family from detail lines, in the view plane of the section view.";

        public string IconName => "adaptiveCrossSection";

        public string PublicName => "Adaptive\nCross Section";

        public string CommandAvailability => typeof(ProjectAndSectionViewAvailability).FullName;

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
            var filter = new DetailCurveFilter();
            var selection = App.UIApplication.ActiveUIDocument.Selection.GetPreselection(filter);

            if (selection.Count == 0)
                selection = App.UIApplication.ActiveUIDocument.Selection.PickObjects(ObjectType.Element, filter, "Select detail curves for adaptive family cross section").ToList();

            var model = new AdaptiveCrossSectionModel(selection);
            model.Execute();
        }
    }
}