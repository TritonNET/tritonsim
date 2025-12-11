using System;
using System.Runtime.InteropServices;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Browser.Providers
{
    public class BrowserWindowProvider : INativeWindowProvider
    {
        public IntPtr CreateChildWindow(IntPtr parent, double width, double height)
        {
            // BGFX expects a string pointer to the canvas selector ID
            // We allocate this string in unmanaged memory so C++ can read it.
            // Note: This leaks a tiny amount of memory once, which is acceptable for a singleton.
            IntPtr ptr = Marshal.StringToHGlobalAnsi("#canvas");
            return ptr;
        }

        public void DestroyWindow() { }
    }
}
