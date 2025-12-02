namespace TritonSimGUI.EntityModels
{
    public class EmStatusMessage : EmBase
    {
        private StatusMessageType m_type;
        public StatusMessageType Type
        {
            get => m_type;
            set => SetProperty(ref m_type, value);
        }

        private string m_message;
        public string Message
        {
            get => m_message;
            set => SetProperty(ref m_message, value);
        }
    }
}
