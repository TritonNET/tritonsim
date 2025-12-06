using System;
using System.Runtime.InteropServices;
using TritonSim.GUI.Desktop.Infrastructure;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Desktop.Providers
{
    public class WindowsWindowProvider : INativeWindowProvider
    {
        public IntPtr CreateChildWindow(IntPtr parentWindowHandle, double width, double height)
        {
            // WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN | WS_CLIPSIBLINGS
            uint wsStyle = 0x40000000 | 0x10000000 | 0x02000000 | 0x04000000;

            // Note: GetHINSTANCE might need a specific module, or IntPtr.Zero for current process usually works
            //IntPtr hInstance = Marshal.GetHINSTANCE(typeof(WindowsWindowProvider).Module);
            IntPtr hInstance = IntPtr.Zero;

            return User32Native.CreateWindowEx(
                0, "STATIC", "", wsStyle,
                0, 0, (int)width, (int)height,
                parentWindowHandle, IntPtr.Zero, hInstance, IntPtr.Zero);
        }

        public void DestroyWindow(IntPtr windowHandle)
        {
            if (windowHandle != IntPtr.Zero)
            {
                User32Native.DestroyWindow(windowHandle);
            }
        }
    }
}
