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
            // WS_CHILD (0x40000000) | WS_VISIBLE (0x10000000) 
            // | WS_CLIPCHILDREN (0x02000000) | WS_CLIPSIBLINGS (0x04000000)
            uint wsStyle = 0x40000000 | 0x10000000 | 0x02000000 | 0x04000000;

            // Use IntPtr.Zero for hInstance usually works fine for this context in .NET
            IntPtr hInstance = Marshal.GetHINSTANCE(typeof(WindowsWindowProvider).Module);

            // "STATIC" is a standard Windows class name. 
            // If you need a custom WndProc, you'll need to register a custom class instead.
            return User32Native.CreateWindowEx(
                0,
                "STATIC",
                "",
                wsStyle,
                0, 0,
                (int)width, (int)height,
                parentWindowHandle,
                IntPtr.Zero,
                hInstance,
                IntPtr.Zero);
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