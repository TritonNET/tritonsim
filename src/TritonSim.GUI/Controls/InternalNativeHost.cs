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