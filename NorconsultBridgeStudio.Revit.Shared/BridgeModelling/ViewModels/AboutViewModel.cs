using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;
using NorconsultBridgeStudio.Revit.Core.Utils;
using NorconsultBridgeStudio.Revit.BridgeModelling.Models;
using NorconsultBridgeStudio.Revit.BridgeModelling.Views;
using System;
using NorconsultBridgeStudio.Revit.Core;
using System.Diagnostics;
using NorconsultBridgeStudio.Revit.BridgeModelling.Commands;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.ViewModels
{
    public class AboutViewModel : NotifierBase
    {
        private string licenseText;
        private string version;
        public string LicenseText
        {
            get => licenseText;
            set
            {
                SetNotify(ref licenseText, value);
            }
        }
        public string Version
        {
            get => version;
            set
            {
                SetNotify(ref version, value);
            }
        }
        public AboutView View { get; set; }
        public AboutViewModel()
        {
            LicenseText = "Copyright (c) 2024 Norconsult\r\n\r\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\r\n\r\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\r\n\r\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.";

            FileVersionInfo info = FileVersionInfo.GetVersionInfo(typeof(AboutCommand).Assembly.Location);
            Version = info.FileVersion;

            View = new AboutView() { DataContext = this };
            WindowInteropHelper helper = new WindowInteropHelper(View);
            helper.Owner = Autodesk.Windows.ComponentManager.ApplicationWindow;
        }
    }
}