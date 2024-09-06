using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;
using NorconsultBridgeStudio.Revit.Core.Utils;
using NorconsultBridgeStudio.Revit.BridgeModelling.Models;
using NorconsultBridgeStudio.Revit.BridgeModelling.Views;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.ViewModels
{
    public class SingleComboboxViewModel : NotifierBase
    {
        private string title;
        private string label;
        private RevitElementModel element;
        public string Title
        {
            get => title;
            set
            {
                SetNotify(ref title, value);
            }
        }
        public string Label
        {
            get => label;
            set
            {
                SetNotify(ref label, value);
            }
        }
        public RevitElementModel Element
        {
            get => element;
            set
            {
                SetNotify(ref element, value);
            }
        }
        public List<RevitElementModel> Elements { get; set; }
        public SingleComboboxView View { get; set; }
        public SingleComboboxViewModel(string title, string label, IEnumerable<RevitElementModel> elements, int? defaultElementIndex = null)
        {
            Title = title;
            Label = label;
            Elements = elements.ToList();
            
            if (defaultElementIndex != null)
                Element = Elements[defaultElementIndex.Value];
            else
                Element = Elements.First();

            View = new SingleComboboxView() { DataContext = this };
            WindowInteropHelper helper = new WindowInteropHelper(View);
            helper.Owner = Autodesk.Windows.ComponentManager.ApplicationWindow;
            _runCommand = new ActionCommand(OnRunCommand, p => true);
        }

        private ActionCommand _runCommand;

        public ActionCommand RunCommand => _runCommand;

        private void OnRunCommand(object obj)
        {
            this.View.DialogResult = true;
        }
    }
}