using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using NorconsultBridgeStudio.Revit.BridgeModelling.Commands;
using NorconsultBridgeStudio.Revit.Core.Extensions;
using NorconsultBridgeStudio.Revit.Core.Interfaces;
using NorconsultBridgeStudio.Revit.Core.Utils;

namespace NorconsultBridgeStudio.Revit.Core.Startup
{
    static class Ribbon
    {
        private static UIControlledApplication Application = null;

        internal static void AddRibbonPanel(UIControlledApplication application)
        {
            Initialize(application);
            AddPanel();
        }

        private static void Initialize(UIControlledApplication application)
        {
            Application = application;
            Application.CreateRibbonTab(Constants.TabName);
        }
        private static void AddPanel()
        {
            RibbonPanel panel = Application.CreateRibbonPanel(Constants.TabName, Constants.BridgeModellingPanelName);

            panel.AddLargePushButton(new AlignmentByPointsCommand());
            panel.AddLargePushButton(new SectionAtStartCommand());
            panel.AddLargePushButton(new AdaptiveCrossSectionCommand());
            panel.AddLargePushButton(new CreateSuperStructureCommand());
            panel.AddLargePushButton(new PlaceSuperStructureCommand());

            PushButtonData buttonDataAbout = CreatePushButtonData(new AboutCommand());
            PushButtonData buttonDataHelp = CreatePushButtonData(new HelpCommand());
            panel.AddPulldownButton(new List<PushButtonData> { buttonDataAbout, buttonDataHelp }, "SplitAbout", "About", "about");

            panel.AddLargePushButton(new OrientToCurveCommand());

        }
        public static PushButtonData CreatePushButtonData(INorconsultBridgeStudioCommand command)
        {
            PushButtonData buttonData = new PushButtonData($"cmd{command.GetType().FullName}", command.PublicName, Constants.ThisAssemblyPath, command.GetType().ToString())
            {
                ToolTip = command.ToolTip,
                AvailabilityClassName = command.CommandAvailability
            };

            BitmapImage image = ResourceUtils.GetImageOrDefault(command.IconName);
            buttonData.LargeImage = image;

            return buttonData;
        }
    }
}
