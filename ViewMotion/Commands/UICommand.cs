using System;
using System.Diagnostics;
using System.Windows.Input;

namespace ViewMotion.Commands
{
    class UICommand<TArg> : ICommand
    {
        private readonly Action<TArg> execute;
        private readonly Func<TArg, bool>? canExecute;

        public UICommand(Action<TArg> execute, Func<TArg, bool>? canExecute = null, Action<Action>? buttonRefresher = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            
            buttonRefresher?.Invoke(() =>
                {
                    if (CanExecuteChanged != null)
                        CanExecuteChanged(this, EventArgs.Empty);
                });
        }

        public bool CanExecute(object? parameter)
        {
            return canExecute?.Invoke(parameter == null ? default! : (TArg)parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            execute(parameter == null ? default! : (TArg)parameter);
        }

        public event EventHandler? CanExecuteChanged;
    }
}
