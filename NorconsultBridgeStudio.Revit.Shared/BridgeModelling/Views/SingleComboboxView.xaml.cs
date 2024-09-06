using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NorconsultBridgeStudio.Revit.BridgeModelling.ViewModels;

namespace NorconsultBridgeStudio.Revit.BridgeModelling.Views
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SingleComboboxView : Window
    {
        SingleComboboxViewModel ViewModel
        {
            get
            {
                return DataContext as SingleComboboxViewModel;
            }
        }
        public SingleComboboxView()
        {
            InitializeComponent();
            Elements_Combobox.Focus();
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
            else if (e.Key == Key.Enter)
            {
                ViewModel.RunCommand.Execute(null);
                
            }
        }
    }
}
