using Autodesk.Revit.UI;
using NorconsultBridgeStudio.Revit.Core.Interfaces;
using NorconsultBridgeStudio.Revit.Core.Startup;
using NorconsultBridgeStudio.Revit.Core.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace NorconsultBridgeStudio.Revit.Core.Extensions
{
    public static class RibbonPanelExtensions
    {
        /// <summary>
        /// Adds a Norconsult Bridge Studio Command button to the ribbon panel as a large push button
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="command">The command to add</param>
        /// <returns></returns>
        public static PushButton AddLargePushButton(this RibbonPanel panel, INorconsultBridgeStudioCommand command)
        {
            PushButtonData buttonData = Ribbon.CreatePushButtonData(command);
            PushButton newButton = panel.AddItem(buttonData) as PushButton;
            
            return newButton;
        }
        /// <summary>
        /// Adds several Norconsult Bridge Studio Command button to the ribbon panel as part of a large pulldown button
        /// </summary>
        /// <param name="ribbonPanel"></param>
        /// <param name="buttonDatas">The button datas of the commands to add</param>
        /// <param name="pulldownButtonName">The name of the pulldown button</param>
        /// <param name="pulldownButtonText">The text of the pulldown button</param>
        /// <param name="pulldownButtonImageName">The image name of the pulldown button</param>
        /// <returns></returns>
        public static PulldownButton AddPulldownButton(this RibbonPanel ribbonPanel, List<PushButtonData> buttonDatas, string pulldownButtonName, string pulldownButtonText, string pulldownButtonImageName)
        {
            PulldownButtonData buttonData = new PulldownButtonData(pulldownButtonName, pulldownButtonText);
            var pulldownButton = ribbonPanel.AddItem(buttonData) as PulldownButton;
            pulldownButton.LargeImage = ResourceUtils.GetImageOrDefault(pulldownButtonImageName);

            foreach (var data in buttonDatas)
                pulldownButton.AddPushButton(data);

            return pulldownButton;
        }
    }
}
