using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.VisualTree;
using System;
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

        public static readonly StyledProperty<ILogger?> LoggerProperty =
            AvaloniaProperty.Register<TritonSimRenderControl, ILogger?>(nameof(Logger));

        private IPlatformCanvasHandle? m_canvasHandle;

        public INativeCanvasProvider? WindowProvider
        {
            get => GetValue(WindowProviderProperty);
            set => SetValue(WindowProviderProperty, value);
        }

        public ILogger? Logger
        {
            get => GetValue(LoggerProperty);
            set => SetValue(LoggerProperty, value);
        }

        public void SetSize(int width, int height)
        {
            Logger?.Debug($"SimulationNativeControlHost SetSize called. Width: {width}, Height: {height}");
            Width = width;
            Height = height;
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            Logger?.Debug($"SimulationNativeControlHost OnSizeChanged: New Size = {e.NewSize}");

            base.OnSizeChanged(e);

            var visualRoot = this.GetVisualRoot() as Visual;
            if (visualRoot != null)
            {
                var position = this.TranslatePoint(new Point(0, 0), visualRoot);

                if (position.HasValue && WindowProvider != null)
                    WindowProvider.UpdatePosition(position.Value.X, position.Value.Y, e.NewSize.Width, e.NewSize.Height);
            }
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            Logger?.Debug($"SimulationNativeControlHost CreateNativeControlCore called. Name: {Name}");

            if (WindowProvider == null)
                return base.CreateNativeControlCore(parent);

            var w = double.IsNaN(Width) ? 100 : Width;
            var h = double.IsNaN(Height) ? 100 : Height;

            var success = WindowProvider.Create(parent.Handle, w, h, out m_canvasHandle);
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
