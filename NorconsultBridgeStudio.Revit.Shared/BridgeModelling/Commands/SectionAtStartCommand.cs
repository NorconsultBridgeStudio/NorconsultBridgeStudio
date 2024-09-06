using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using NorconsultBridgeStudio.Revit.BridgeModelling.CommandAvailability;
using NorconsultBridgeStudio.Revit.BridgeModelling.Utils;
using NorconsultBridgeStudio.Revit.Core;
using NorconsultBridgeStudio.Revit.Core.Exceptions;
using NorconsultBridgeStudio.Revit.Core.Extensions;
using NorconsultBridgeStudio.Revit.Core.Interfaces;
using NorconsultBridgeStudio.Revit.Core.Utils;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class SectionAtStartCommand : INorconsultBridgeStudioCommand
    {
        public string ToolTip => "Creates a new section view at this elements start, looking in the direction of the element. The workset settings of the current view is copied to the new section view.";

        public string IconName => "sectionAtStart";

        public string PublicName => "Section at\nStart";

        public string CommandAvailability => typeof(ProjectAndModelViewAvailability).FullName;

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
            var filter = new CurveFamilyFilter();
            var references = App.UIApplication.ActiveUIDocument.Selection.GetPreselection(filter);

            Reference reference = references.Count == 1 ? references.First() : App.UIApplication.ActiveUIDocument.Selection.PickObject(ObjectType.Element, filter, "Select any curve based elements for creating section at start, e.g. lines, beams etc..");

            Element element = App.CurrentDocument.GetElement(reference);

            ViewUtils.CreateSection(element);
        }
    }
}