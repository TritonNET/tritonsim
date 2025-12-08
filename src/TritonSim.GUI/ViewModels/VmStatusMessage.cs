using CommunityToolkit.Mvvm.ComponentModel;

namespace TritonSim.GUI.ViewModels
{
    public enum StatusMessageType
    {
        None = 0,

        Info = 1,

        Success = 2,

        Error = 3
    }

    public class VmStatusMessage : ObservableObject
    {
        private StatusMessageType m_type;
        public StatusMessageType Type
        {
            get => m_type;
            set => SetProperty(ref m_type, value);
        }

        private string? m_message;
        public string? Message
        {
            get => m_message;
            set => SetProperty(ref m_message, value);
        }
    }
}
