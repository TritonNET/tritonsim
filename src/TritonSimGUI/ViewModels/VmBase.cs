using TritonSimGUI.EntityModels;

namespace TritonSimGUI.ViewModels
{
    public abstract class VmBase : EmBase
    {
        public EmStatusMessage StatusMessage { get; }

        protected VmBase()
        {
            StatusMessage = new EmStatusMessage
            {
                Type = StatusMessageType.None,
                Message = string.Empty
            };
        }

        private bool m_isBusy = false;
        public bool IsBusy
        {
            get => m_isBusy;
            set => SetProperty(ref m_isBusy, value);
        }

        public void Initialize()
        {
            _ = InitializeAsync().ContinueWith(t =>
            {
                ShowStatusMessage(StatusMessageType.Error, "Initialization failed.");
            },
            TaskContinuationOptions.OnlyOnFaulted);
        }

        protected void ShowStatusMessage(StatusMessageType type, string message)
        {
            StatusMessage.Type = type;
            StatusMessage.Message = message;

            OnPropertyChanged(nameof(StatusMessage));
        }

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
