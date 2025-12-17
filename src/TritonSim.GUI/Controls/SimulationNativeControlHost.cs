using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Platform;
using Avalonia.VisualTree;
using System;
using System.Runtime.InteropServices;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Controls
{
    public delegate void NativeHandleCreatedEventHandler(IntPtr handle);
    public delegate void NativeHandleFailedEventHandler(string errorMsg);

    public class SimulationNativeControlHost : NativeControlHost
    {
        public event NativeHandleCreatedEventHandler? NativeHandleCreated;

        public event NativeHandleFailedEventHandler? NativeHandleFailed;

        public static readonly StyledProperty<INativeCanvasProvider?> WindowProviderProperty =
            AvaloniaProperty.Register<TritonSimRenderControl, INativeCanvasProvider?>(nameof(WindowProvider));

        private IPlatformCanvasHandle? m_canvasHandle;

        public INativeCanvasProvider? WindowProvider
        {
            get => GetValue(WindowProviderProperty);
            set => SetValue(WindowProviderProperty, value);
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            var visualRoot = this.GetVisualRoot() as Visual;
            if (visualRoot != null)
            {
                var position = this.TranslatePoint(new Point(0, 0), visualRoot);

                if (position.HasValue && WindowProvider != null)
                    WindowProvider.UpdatePosition(position.Value.X, position.Value.Y, Bounds.Width, Bounds.Height);
            }
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            if(WindowProvider == null)
                return base.CreateNativeControlCore(parent);

            var success = WindowProvider.Create(parent.Handle, Bounds.Width, Bounds.Height, out m_canvasHandle);
            if (success)
                NativeHandleCreated?.Invoke(m_canvasHandle.GetCanvasHandle());
            else
                NativeHandleFailed?.Invoke("Native window creation failed.");

            return m_canvasHandle;
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
