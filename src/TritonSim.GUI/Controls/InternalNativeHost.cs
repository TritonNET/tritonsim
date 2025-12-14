using Avalonia.Controls;
using Avalonia.Platform;
using System;

namespace TritonSim.GUI.Controls
{
    // This handles the raw P/Invoke window creation
    internal class InternalNativeHost : NativeControlHost
    {
        private readonly TritonSimRenderControl m_parent;

        public InternalNativeHost(TritonSimRenderControl parent)
        {
            m_parent = parent;
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            if (OperatingSystem.IsBrowser())
            {
                // IN THE BROWSER:
                // You cannot return a C++ pointer here. 
                // You usually return a handle to a DIV or CANVAS created in JavaScript.

                // If you are just rendering to the main Avalonia canvas via Skia,
                // you might NOT need a NativeControlHost at all.

                // If you are trying to use a separate WebGL canvas, you need to create it in JS
                // and return its ID string wrapped in a handle.
                m_parent.OnNativeHandleCreated(parent.Handle);
                return base.CreateNativeControlCore(parent);
            }

            if (m_parent.WindowProvider == null)
                throw new InvalidOperationException("WindowProvider is null.");

            // Create window via the parent's provider
            IntPtr rawHandle = m_parent.WindowProvider.CreateChildWindow(parent.Handle, this.Bounds.Width, this.Bounds.Height);

            m_parent.OnNativeHandleCreated(rawHandle);

            return new PlatformHandle(rawHandle, "HWND");
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            m_parent.OnNativeHandleDestroyed();

            if (m_parent.WindowProvider != null)
            {
                m_parent.WindowProvider.DestroyWindow();
            }

            base.DestroyNativeControlCore(control);
        }
    }
}