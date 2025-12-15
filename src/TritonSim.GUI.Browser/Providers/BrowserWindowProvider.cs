using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;
using TritonSim.GUI.Browser.Infrastructure;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Browser.Providers
{
    public class BrowserWindowProvider : INativeWindowProvider
    {
        public IntPtr CreateChildWindow(IntPtr parent, double width, double height)
        {
            return Marshal.StringToHGlobalAnsi("#canvas"); // no one owns the string allocation hereafter. But its singleton.
        }

        public IPlatformHandle CreateNativeControl(nint parentHandle, double width, double height)
        {
            string divId = "triton-sim-" + Guid.NewGuid().ToString("N");

            var divElement = DomHelper.CreateContainer(divId);

            return new BrowserDomElementHandle(divElement, divId);
        }

        public void DestroyNativeControl(IPlatformHandle handle)
        {
            if (handle is BrowserDomElementHandle bhandle)
                bhandle.Destroy();
        }
    }
}
