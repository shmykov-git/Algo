using System;
using System.Windows.Input;

namespace ViewMotion.Commands
{
    class UICommand<TArg> : ICommand
    {
        private readonly Action<TArg> execute;
        private readonly Func<TArg, bool>? canExecute;
        private readonly Action? refresh;

        public UICommand(Action<TArg> execute, Func<TArg, bool>? canExecute = null, Action<Action>? buttonRefresher = null, Action? refresh = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            this.refresh = refresh;

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
            refresh?.Invoke();
        }

        public event EventHandler? CanExecuteChanged;
    }
}
