using Avalonia.Platform;
using System;

namespace TritonSim.GUI.Providers
{
    public interface INativeWindowProvider
    {
        /// <summary>
        /// Creates a child native control hosted inside the parent handle.
        /// </summary>
        /// <param name="parentHandle">The handle provided by Avalonia (HWND, XID, etc).</param>
        /// <param name="width">Initial width.</param>
        /// <param name="height">Initial height.</param>
        /// <returns>The handle of the newly created child window.</returns>
        IPlatformHandle CreateNativeControl(IntPtr parentHandle, double width, double height);

        /// <summary>
        /// Destroys the window handle created by CreateChildWindow.
        /// </summary>
        void DestroyNativeControl(IPlatformHandle handle);
    }
}
