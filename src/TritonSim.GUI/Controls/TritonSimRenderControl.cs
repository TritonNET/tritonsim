using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.Drawing;
using TritonSim.GUI.Infrastructure;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Controls
{
    public class TritonSimRenderControl : NativeControlHost
    {
        public static readonly StyledProperty<SimulationMode> ModeProperty =
            AvaloniaProperty.Register<TritonSimRenderControl, SimulationMode>(nameof(Mode), SimulationMode.NotReady);

        public static readonly StyledProperty<ITritonSimNativeProvider?> SimProviderProperty =
            AvaloniaProperty.Register<TritonSimRenderControl, ITritonSimNativeProvider?>(nameof(SimProvider));

        public static readonly StyledProperty<INativeWindowProvider?> WindowProviderProperty =
            AvaloniaProperty.Register<TritonSimRenderControl, INativeWindowProvider?>(nameof(WindowProvider));

        public static readonly StyledProperty<RendererType> RendererProperty =
            AvaloniaProperty.Register<TritonSimRenderControl, RendererType>(nameof(Renderer), RendererType.RT_UNKNOWN);

        public SimulationMode Mode
        {
            get => GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        public ITritonSimNativeProvider? SimProvider
        {
            get => GetValue(SimProviderProperty);
            set => SetValue(SimProviderProperty, value);
        }

        public INativeWindowProvider? WindowProvider
        {
            get => GetValue(WindowProviderProperty);
            set => SetValue(WindowProviderProperty, value);
        }

        public RendererType Renderer
        {
            get => GetValue(RendererProperty);
            set => SetValue(RendererProperty, value);
        }

        private DispatcherTimer? m_renderTimer;
        private Popup? m_overlayPopup;
        private bool m_attached;

        public TritonSimRenderControl()
        {
            m_renderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            m_renderTimer.Tick += RenderTimer_Tick;

            m_attached = false;

            AttachedToVisualTree += OnAttachedToVisualTree;
            DetachedFromVisualTree += OnDetachedFromVisualTree;
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            m_attached = true;

            InitializeRenderer();
        }

        private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            StopRenderLoop();
            m_attached = false;
        }

        public void Start() 
        {
            if (!m_attached) return;

            PerformProviderAction(() => {

                var success = SimProvider.Start();

                if (success)
                    StartRenderLoop();

                return success;
            });

        }

        public void Stop() 
        {
            if (!m_attached) return;

            PerformProviderAction(() => {

                StopRenderLoop();
                return SimProvider.Stop();
            });
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (!m_attached) return;

            if (change.Property == RendererProperty)
                HandleRendererChange(change.GetNewValue<RendererType>());
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            if (WindowProvider == null)
                throw new InvalidOperationException("WindowProvider is null.");

            IntPtr rawHandle = WindowProvider.CreateChildWindow(parent.Handle, this.Bounds.Width, this.Bounds.Height);

            PerformProviderAction(() => SimProvider.SetWindowHandle(rawHandle));

            return new PlatformHandle(rawHandle, "HWND");
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            ShutdownRenderer();

            if (WindowProvider != null)
                WindowProvider.DestroyWindow();

            base.DestroyNativeControlCore(control);
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            var size = e.NewSize;

            if (size.Width <= 0 || size.Height <= 0)
                return;

            var scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;

            PerformProviderAction(() => SimProvider.SetSize(new Avalonia.Size(size.Width * scale, size.Height * scale)));
        }

        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            PerformProviderAction(() => SimProvider.RenderFrame());
        }

        private void HandleRendererChange(RendererType newType)
        {
            if (SimProvider == null)
                return;

            Debug.WriteLine($"[Bgfx] Switching Renderer to: {newType}");

            StopRenderLoop();

            ShutdownRenderer();

            InitializeRenderer();
        }

        private void StartRenderLoop()
        {
            if (m_renderTimer != null && !m_renderTimer.IsEnabled)
                m_renderTimer.Start();
        }

        private void StopRenderLoop()
        {
            if (m_renderTimer.IsEnabled)
                m_renderTimer?.Stop();
        }

        protected void InitializeRenderer()
        {
            if (SimProvider == null) return;

            PerformProviderAction(() => SimProvider.Init());
        }

        private void PerformProviderAction(Func<bool> action)
        {
            var success = action();

            SetCurrentValue(ModeProperty, SimProvider.GetMode());

            if (!success)
                ShowOverlayText(SimProvider.GetLastError());
        }

        private void ShutdownRenderer()
        {
            var success = SimProvider.Shutdown();

            SetCurrentValue(ModeProperty, SimProvider.GetMode());

            if (!success)
                ShowOverlayText(SimProvider.GetLastError());
        }

        private void ShowOverlayText(string text)
        {
            // 1. Clear existing popup if present
            HideOverlayText();

            // 2. Create the content (TextBox inside Border)
            var textBox = new TextBox
            {
                Text = text,
                IsReadOnly = true,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 200,
                Background = Brushes.White,
                Foreground = Brushes.Black
            };

            var overlayBorder = new Border
            {
                Child = textBox,
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(2),
                Background = Brushes.White,
                Padding = new Thickness(5),
                CornerRadius = new CornerRadius(4)
            };

            // 3. Create a Popup
            // A Popup creates a new native window handle, allowing it to sit 
            // visually on top of the NativeControlHost's window handle.
            m_overlayPopup = new Popup
            {
                PlacementTarget = this,            // Position relative to this control
                Placement = PlacementMode.Center,  // Center it
                Child = overlayBorder,
                IsOpen = true,
                Topmost = true                     // Ensure it stays on top
            };
        }

        private void HideOverlayText()
        {
            if (m_overlayPopup != null)
            {
                m_overlayPopup.IsOpen = false;
                m_overlayPopup.Child = null; // Help GC
                m_overlayPopup = null;
            }
        }
    }
}