using System.Windows.Input;

namespace TritonSimGUI.Infrastructure
{
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> m_executeAsync;
        private readonly Func<T, bool>? m_canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<T, Task> executeAsync, Func<T, bool>? canExecute = null)
        {
            m_executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            m_canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (_isExecuting)
                return false;

            return m_canExecute?.Invoke((T)parameter!) ?? true;
        }

        public async void Execute(object? parameter)
        {
            if (CanExecute(parameter))
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();

                try
                {
                    await m_executeAsync((T)parameter!);
                }
                finally
                {
                    _isExecuting = false;
                    RaiseCanExecuteChanged();
                }
            }
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class AsyncRelayCommand : AsyncRelayCommand<object?>
    {
        public AsyncRelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
            : base(
                _ => executeAsync(),
                canExecute is null ? null : new Func<object?, bool>(_ => canExecute())
            )
        {
        }
    }
}