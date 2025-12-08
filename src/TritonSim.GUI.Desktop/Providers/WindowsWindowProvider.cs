using System;
using System.Runtime.InteropServices;
using TritonSim.GUI.Desktop.Infrastructure;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Desktop.Providers
{
    [Flags]
    public enum WindowStyles : uint
    {
        WS_CHILD = 0x40000000,
        WS_VISIBLE = 0x10000000,
        WS_CLIPCHILDREN = 0x02000000,
        WS_CLIPSIBLINGS = 0x04000000
    }

    public class WindowsWindowProvider : INativeWindowProvider
    {
        private IntPtr m_windowHandle = IntPtr.Zero;

        public IntPtr CreateChildWindow(IntPtr parentWindowHandle, double width, double height)
        {
            WindowStyles style = WindowStyles.WS_CHILD |
                                 WindowStyles.WS_VISIBLE |
                                 WindowStyles.WS_CLIPCHILDREN |
                                 WindowStyles.WS_CLIPSIBLINGS;

            IntPtr hInstance = Marshal.GetHINSTANCE(typeof(WindowsWindowProvider).Module);

            m_windowHandle = User32Native.CreateWindowEx(
                0,
                "STATIC",
                "",
                (uint)style, // 3. Cast the enum back to uint for the P/Invoke call
                0, 0,
                (int)width, (int)height,
                parentWindowHandle,
                IntPtr.Zero,
                hInstance,
                IntPtr.Zero);

            return m_windowHandle;
        }

        public void DestroyWindow()
        {
            if (m_windowHandle != IntPtr.Zero)
            {
                User32Native.DestroyWindow(m_windowHandle);
            }
        }
    }
}