using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Controls
{
    public delegate void NativeHandleCreatedEventHandler(IntPtr handle);

    public class SimulationNativeControlHost : NativeControlHost
    {
        public event NativeHandleCreatedEventHandler NativeHandleCreated;

        public static readonly StyledProperty<INativeCanvasProvider?> WindowProviderProperty =
            AvaloniaProperty.Register<TritonSimRenderControl, INativeCanvasProvider?>(nameof(WindowProvider));

        private IPlatformHandle m_platformHandle;

        public INativeCanvasProvider? WindowProvider
        {
            get => GetValue(WindowProviderProperty);
            set => SetValue(WindowProviderProperty, value);
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            if(WindowProvider == null)
                return base.CreateNativeControlCore(parent);

            var success = WindowProvider.Create(parent.Handle, Bounds.Width, Bounds.Height, out m_platformHandle);

            NativeHandleCreated?.Invoke(m_platformHandle.Handle);

            if (OperatingSystem.IsBrowser())
                return base.CreateNativeControlCore(parent);

            return m_platformHandle;
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            if (WindowProvider == null)
            {
                base.DestroyNativeControlCore(control);
                return;
            }

            var success = WindowProvider.Destroy(control);
        }
    }
}
