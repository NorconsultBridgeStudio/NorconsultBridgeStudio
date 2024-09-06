using System;
using System.Windows.Input;

namespace NorconsultBridgeStudio.Revit.Core.Utils
{
    public class ActionCommand : ICommand
    {

        private readonly Action<object> _excecuteAction;
        private readonly Func<object, bool> _canExecuteAction;

        public ActionCommand(Action<object> excecuteAction, Func<object, bool> canExecuteAction)
        {
            _excecuteAction = excecuteAction;
            _canExecuteAction = canExecuteAction;
        }

        public void Execute(object parameter) => _excecuteAction(parameter);

        public bool CanExecute(object parameter) => _canExecuteAction?.Invoke(parameter) ?? true;

        public event EventHandler CanExecuteChanged;

        public void InvokeCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    }
}
