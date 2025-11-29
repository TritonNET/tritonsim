using TritonSimGUI.Infrastructure;

namespace TritonSimGUI.Views
{
    public class TritonSimView : ContentView
    {
        private IntPtr m_nativeHandle;
        private bool m_initialized;

        public TritonSimView()
        {
            m_initialized = false;

            Loaded += TritonSimView_Loaded;
            Unloaded += TritonSimView_Unloaded;
        }

        private void TritonSimView_Loaded(object? sender, EventArgs e)
        {
            m_nativeHandle = this.GetNativeHandle();

            var size = this.GetSize();

            TritonSimNative.tritonsim_init(m_nativeHandle, size.width, size.height);
            m_initialized = true;
        }

        private void TritonSimView_Unloaded(object? sender, EventArgs e)
        {
            if (m_initialized)
            {
                TritonSimNative.tritonsim_shutdown();
                m_initialized = false;
            }
        }
    }
}
