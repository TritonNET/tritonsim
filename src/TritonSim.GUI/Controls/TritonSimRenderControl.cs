using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using TritonSim.GUI.Infrastructure;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Controls
{
    public class TritonSimRenderControl : NativeControlHost
    {
        public static readonly StyledProperty<SimulationState> StateProperty = AvaloniaProperty.Register<TritonSimRenderControl, SimulationState>(nameof(State), SimulationState.Unknown);

        public static readonly StyledProperty<ITritonSimNativeProvider?> SimProviderProperty = AvaloniaProperty.Register<TritonSimRenderControl, ITritonSimNativeProvider?>(nameof(SimProvider));

        public static readonly StyledProperty<INativeWindowProvider?> WindowProviderProperty = AvaloniaProperty.Register<TritonSimRenderControl, INativeWindowProvider?>(nameof(WindowProvider));

        public SimulationState State
        {
            get => GetValue(StateProperty);
            set => SetValue(StateProperty, value);
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

        private DispatcherTimer? m_renderTimer;
        protected SimContext m_context;
        protected SimConfig m_config;

        private IntPtr m_mountedHandle = IntPtr.Zero;

        public TritonSimRenderControl()
        {
            m_renderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            m_renderTimer.Tick += RenderTimer_Tick;

            this.AttachedToVisualTree += OnAttachedToVisualTree;
            this.DetachedFromVisualTree += OnDetachedFromVisualTree;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == StateProperty)
                HandleStateChange(change.GetNewValue<SimulationState>());
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            // Validate Providers
            if (WindowProvider == null)
                throw new InvalidOperationException("WindowProvider is null.");

            IntPtr rawHandle = WindowProvider.CreateChildWindow(parent.Handle, this.Bounds.Width, this.Bounds.Height);

            m_config.Handle = rawHandle;

            InitializeBgfx();

            return new PlatformHandle(rawHandle, "HWND");
        }

        private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            StopRenderLoop();
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {

        }

        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            if (!m_context.IsInitialized())
                return;

            if (SimProvider == null)
                throw new InvalidOperationException("SimProvider is null. Ensure it is bound in XAML and the DataContext is set.");

            SimProvider.RenderFrame(ref m_context);
        }

        private void HandleStateChange(SimulationState newState)
        {
            if (SimProvider == null)
                throw new InvalidOperationException("SimProvider is null. Ensure it is bound in XAML and the DataContext is set.");

            if (newState == SimulationState.Running)
            {
                if (!m_context.IsInitialized())
                    InitializeBgfx();

                SimProvider.Start(ref m_context);
                StartRenderLoop();
            }
            else if (newState == SimulationState.Initialized || newState == SimulationState.Paused)
            {
                StopRenderLoop();
                if (m_context.IsInitialized())
                {
                    SimProvider.Stop(ref m_context);
                }
            }
        }

        public void StartRenderLoop()
        {
            if (!m_renderTimer.IsEnabled)
                m_renderTimer.Start();
        }

        public void StopRenderLoop()
        {
            m_renderTimer?.Stop();
        }

        protected void InitializeBgfx()
        {
            if (m_context.IsInitialized()) return;

            if (SimProvider == null)
                throw new InvalidOperationException("SimProvider is null. Ensure it is bound in XAML and the DataContext is set.");

            var topLevel = TopLevel.GetTopLevel(this);
            double scale = topLevel?.RenderScaling ?? 1.0;

            m_config.Width = (ushort)(this.Bounds.Width * scale);
            m_config.Height = (ushort)(this.Bounds.Height * scale);
            m_config.Type = RendererType.RT_TEST_BOUNCING_CIRCLE; // Or bind this property

            ResponseCode result = SimProvider.Init(ref m_config, out m_context);

            if ((result & ResponseCode.Success) != 0)
            {
                Debug.WriteLine("[Bgfx] Init Success");
                if (State == SimulationState.Running)
                {
                    SimProvider.Start(ref m_context);
                    StartRenderLoop();
                }
                else
                {
                    SetCurrentValue(StateProperty, SimulationState.Initialized);
                }
            }
            else
            {
                Debug.WriteLine("[Bgfx] Init Failed");
                SetCurrentValue(StateProperty, SimulationState.Error);
            }
        }

        private void ShutdownBgfx()
        {
            if (SimProvider == null)
                throw new InvalidOperationException("SimProvider is null. Ensure it is bound in XAML and the DataContext is set.");

            if (m_context.IsInitialized())
            {
                SimProvider.Stop(ref m_context);
                SimProvider.Shutdown(ref m_context);

                m_context = default;
            }
        }
    }
}
