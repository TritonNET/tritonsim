using Avalonia.Platform;
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

    public class WindowsCanvasProvider : INativeCanvasProvider
    {
        public bool Create(IntPtr parent, double width, double height, out IPlatformHandle handle)
        {
            WindowStyles style = WindowStyles.WS_CHILD |
                                 WindowStyles.WS_VISIBLE |
                                 WindowStyles.WS_CLIPCHILDREN |
                                 WindowStyles.WS_CLIPSIBLINGS;

            IntPtr hInstance = Marshal.GetHINSTANCE(typeof(WindowsCanvasProvider).Module);

            var ptr = User32Native.CreateWindowEx(
                0,
                "STATIC",
                "",
                (uint)style,
                0, 0,
                (int)width, (int)height,
                parent,
                IntPtr.Zero,
                hInstance,
                IntPtr.Zero);

            handle = new PlatformHandle(ptr, "WindowsCanvasHandle");

            return true;
        }

        public bool Destroy(IPlatformHandle handle)
        {
            return User32Native.DestroyWindow(handle.Handle);
        }

        public bool UpdatePosition(double x, double y, double width, double height)
        {
            // TODO
            return true;
        }
    }
}