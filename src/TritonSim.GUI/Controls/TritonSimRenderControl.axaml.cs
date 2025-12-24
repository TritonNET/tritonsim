using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Drawing;
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

        public static readonly StyledProperty<INativeCanvasProvider?> WindowProviderProperty =
            AvaloniaProperty.Register<TritonSimRenderControl, INativeCanvasProvider?>(nameof(WindowProvider));

        public static readonly StyledProperty<ILogger?> LogHandlerProperty =
            AvaloniaProperty.Register<TritonSimRenderControl, ILogger?>(nameof(LogHandler));

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

        public INativeCanvasProvider? WindowProvider
        {
            get => GetValue(WindowProviderProperty);
            set => SetValue(WindowProviderProperty, value);
        }

        public ILogger? LogHandler
        {
            get => GetValue(LogHandlerProperty);
            set => SetValue(LogHandlerProperty, value);
        }

        public RendererType Renderer
        {
            get => GetValue(RendererProperty);
            set => SetValue(RendererProperty, value);
        }

        private readonly DispatcherTimer? m_renderTimer;
        private RendererInitState m_initState = RendererInitState.None;
        private ILogger m_logger => LogHandler;

        public TritonSimRenderControl()
        {
            m_logger?.Debug("TritonSimRenderControl creating.");

            InitializeComponent();

            m_renderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            m_renderTimer.Tick += RenderTimer_Tick;

            AttachedToVisualTree += OnAttachedToVisualTree;
            DetachedFromVisualTree += OnDetachedFromVisualTree;

            m_logger?.Debug("TritonSimRenderControl created.");
        }

        private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            m_logger?.Debug("TritonSimRenderControl detached from visual tree.");
            SetInitState(RendererInitState.AttachedToVisualTree, set: false);
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            m_logger?.Debug("TritonSimRenderControl attached to visual tree.");
            SetInitState(RendererInitState.AttachedToVisualTree);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            m_logger?.Debug($"TritonSimRenderControl property changed: {change.Property.Name}");

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
                if (UpdateSize())
                    SetInitState(RendererInitState.SizeSet);
            }
            else if (change.Property == BackgroundProperty)
            {
                UpdateBackgroundColor();
            }
        }

        private void SetInitState(RendererInitState state, bool set = true)
        {
            m_logger.Debug($"Setting init state: {state}, set: {set}");

            if (!set)
            {
                m_initState ^= state;
                return;
            }

            bool wasAttachedToVisualTree = m_initState.HasFlag(RendererInitState.AttachedToVisualTree);

            m_initState |= state;

            m_logger.Debug($"Current init state: {m_initState}");

            if (m_initState.IsReadyToInit() && !m_initState.IsInitCompleted())
                InitializeRenderer();
            else if (!m_initState.HasFlag(RendererInitState.AttachedToVisualTree) && wasAttachedToVisualTree)
                StopRenderLoop();
        }

        private bool UpdateSize()
        {
            if (SimProvider == null) return false;

            var size = Bounds.Size;

            if (size.Width <= 0 || size.Height <= 0)
                return false;

            var topLevel = TopLevel.GetTopLevel(this);
            var scale = topLevel?.RenderScaling ?? 1.0;

            PerformProviderAction(() => SimProvider.SetSize(new Avalonia.Size(size.Width * scale, size.Height * scale)));

            return true;
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
            m_logger?.Debug("Initializing renderer.");

            PerformProviderAction(() =>
            {
                HideOverlayText();

                UpdateBackgroundColor();

                var success = SimProvider!.Init();

                SetInitState(success ? RendererInitState.NativeInitSuccess : RendererInitState.NativeInitFailed);

                return success;
            });

            m_logger?.Debug("Renderer initialized.");
        }

        private void ShutdownRenderer()
        {
            if (SimProvider == null) return;

            SimProvider.Shutdown();

            SetCurrentValue(ModeProperty, SimProvider.GetMode());

            SetInitState(RendererInitState.NativeInitCompleted, set: false);
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

        private void NativeContainer_NativeHandleCreated(nint handle)
        {
            m_logger?.Debug($"Native handle created: {handle}");

            PerformProviderAction(() => SimProvider.SetWindowHandle(handle));

            SetInitState(RendererInitState.HandleSet);
        }

        private void NativeContainer_NativeHandleFailed(string errorMsg)
        {
            m_logger?.Error($"Native handle creation failed: {errorMsg}");

            ShowOverlayText(errorMsg);
        }
    }
}