using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using TritonSim.GUI.Infrastructure;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Controls
{
    public partial class TritonSimRenderControl : UserControl
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

        private readonly InternalNativeHost m_nativeHost;
        private readonly DispatcherTimer? m_renderTimer;
        
        private RendererInitState m_initState = RendererInitState.None;

        public TritonSimRenderControl()
        {
            InitializeComponent();

            m_nativeHost = new InternalNativeHost(this);
            NativeContainer.Child = m_nativeHost;

            m_renderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            m_renderTimer.Tick += RenderTimer_Tick;

            AttachedToVisualTree += OnAttachedToVisualTree;
            DetachedFromVisualTree += OnDetachedFromVisualTree;
        }

        private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            StopRenderLoop();
            SetInitState(RendererInitState.AttachedToVisualTree, set: false);
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            SetInitState(RendererInitState.AttachedToVisualTree);
        }

        public void OnNativeHandleCreated(IntPtr handle)
        {
            PerformProviderAction(() => SimProvider.SetWindowHandle(handle));

            SetInitState(RendererInitState.HandleSet);
        }

        public void OnNativeHandleDestroyed()
        {
            ShutdownRenderer();

            SetInitState(RendererInitState.HandleSet, set: false);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == RendererProperty)
            {
                var newType = change.GetNewValue<RendererType>();
                
                if (m_initState.IsInitCompleted())
                {
                    HandleRendererChange(newType);
                }
                else
                {
                    PerformProviderAction(() => SimProvider.SetType(newType));

                    SetInitState(RendererInitState.TypeSet);
                }
            }
            else if (change.Property == BoundsProperty)
            {
                UpdateSize();

                SetInitState(RendererInitState.SizeSet);
            }
            else if (change.Property == BackgroundProperty)
            {
                UpdateBackgroundColor();
            }
        }

        private void SetInitState(RendererInitState state, bool set = true)
        {
            if (!set)
            {
                m_initState ^= state;
                return;
            }

            m_initState |= state;

            if (m_initState.IsReadyToInit() && !m_initState.IsInitCompleted())
                InitializeRenderer();
        }

        private void UpdateSize()
        {
            if (SimProvider == null) return;

            var size = Bounds.Size;
            
            if (size.Width <= 0 || size.Height <= 0)
                return;
            
            var topLevel = TopLevel.GetTopLevel(this);
            var scale = topLevel?.RenderScaling ?? 1.0;

            PerformProviderAction(() => SimProvider.SetSize(new Size(size.Width * scale, size.Height * scale)));
        }

        private void UpdateBackgroundColor()
        {
            var rgb = GetBackgroundColor();

            PerformProviderAction(() => SimProvider.SetBackgroundColor(rgb));
        }

        private void ShowOverlayText(string text)
        {
            OverlayTextBox.Text = text;

            OverlayBorder.IsVisible = true;
            NativeContainer.IsVisible = false;
        }

        private void HideOverlayText()
        {
            OverlayBorder.IsVisible = false;
            NativeContainer.IsVisible = true;
        }

        public void Start()
        {
            if (!m_initState.IsInitCompleted()) return;

            PerformProviderAction(() =>
            {
                var success = SimProvider.Start();
                if (success)
                {
                    StartRenderLoop();
                    HideOverlayText();
                }
                return success;
            });
        }

        public void Stop()
        {
            if (!m_initState.IsInitCompleted()) return;

            PerformProviderAction(() =>
            {
                StopRenderLoop();
                var res = SimProvider.Stop();

                return res;
            });
        }

        private void InitializeRenderer()
        {
            PerformProviderAction(() =>
            {
                HideOverlayText();

                UpdateSize();
                UpdateBackgroundColor();

                var success = SimProvider.Init();

                SetInitState(RendererInitState.NativeInitialized);

                return success;
            });
        }

        private void ShutdownRenderer()
        {
            if (SimProvider == null) return;

            SimProvider.Shutdown();

            SetCurrentValue(ModeProperty, SimProvider.GetMode());

            SetInitState(RendererInitState.NativeInitialized, set: false);
        }

        private void HandleRendererChange(RendererType newType)
        {
            StopRenderLoop();
            ShutdownRenderer();

            PerformProviderAction(() => SimProvider.SetType(newType));

            SetInitState(RendererInitState.TypeSet);
        }

        private void PerformProviderAction(Func<bool> action)
        {
            if (SimProvider == null) return;

            bool success = action();
            SetCurrentValue(ModeProperty, SimProvider.GetMode());

            if (!success)
                ShowOverlayText(SimProvider.GetLastError());
        }

        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            if (NativeContainer.IsVisible)
                SimProvider?.RenderFrame();
        }

        private void StartRenderLoop()
        {
            if (m_renderTimer != null && !m_renderTimer.IsEnabled) m_renderTimer.Start();
        }

        private void StopRenderLoop()
        {
            m_renderTimer?.Stop();
        }

        public uint GetBackgroundColor()
        {
            if (Background is ISolidColorBrush solidBrush)
            {
                var c = solidBrush.Color;

                return ((uint)c.R << 24) | ((uint)c.G << 16) | ((uint)c.B << 8) | (uint)c.A;
            }

            return 0;
        }
    }
}