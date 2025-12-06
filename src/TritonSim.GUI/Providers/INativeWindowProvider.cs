using System;

namespace TritonSim.GUI.Providers
{
    public interface INativeWindowProvider
    {
        /// <summary>
        /// Creates a child window hosted inside the parent handle.
        /// </summary>
        /// <param name="parentWindowHandle">The handle provided by Avalonia (HWND, XID, etc).</param>
        /// <param name="width">Initial width.</param>
        /// <param name="height">Initial height.</param>
        /// <returns>The handle of the newly created child window.</returns>
        IntPtr CreateChildWindow(IntPtr parentWindowHandle, double width, double height);

        /// <summary>
        /// Destroys the window handle created by CreateChildWindow.
        /// </summary>
        void DestroyWindow(IntPtr windowHandle);
    }
}
