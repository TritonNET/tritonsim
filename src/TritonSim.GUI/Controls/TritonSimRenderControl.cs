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
        public static readonly StyledProperty<SimulationState> StateProperty =
            AvaloniaProperty.Register<TritonSimRenderControl, SimulationState>(nameof(State), SimulationState.Unknown);

        public static readonly StyledProperty<ITritonSimNativeProvider?> SimProviderProperty =
            AvaloniaProperty.Register<TritonSimRenderControl, ITritonSimNativeProvider?>(nameof(SimProvider));

        public static readonly StyledProperty<INativeWindowProvider?> WindowProviderProperty =
            AvaloniaProperty.Register<TritonSimRenderControl, INativeWindowProvider?>(nameof(WindowProvider));

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
            if (WindowProvider == null)
                throw new InvalidOperationException("WindowProvider is null.");

            IntPtr rawHandle = WindowProvider.CreateChildWindow(parent.Handle, this.Bounds.Width, this.Bounds.Height);

            m_mountedHandle = rawHandle;
            m_config.Handle = rawHandle;

            return new PlatformHandle(rawHandle, "HWND");
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            ShutdownBgfx();

            if (WindowProvider != null && m_mountedHandle != IntPtr.Zero)
            {
                WindowProvider.DestroyWindow(m_mountedHandle);
            }

            m_mountedHandle = IntPtr.Zero;
            base.DestroyNativeControlCore(control);
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            if (m_mountedHandle == IntPtr.Zero || SimProvider == null)
                return;

            var size = e.NewSize;

            if (size.Width <= 0 || size.Height <= 0)
                return;

            if (!m_context.IsInitialized())
            {
                InitializeBgfx();
            }
            else
            {
                ResizeBgfx(size);
            }
        }

        private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            StopRenderLoop();
        }

        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            if (!m_context.IsInitialized()) return;
            SimProvider?.RenderFrame(ref m_context);
        }

        private void HandleStateChange(SimulationState newState)
        {
            if (SimProvider == null) return;

            if (newState == SimulationState.Running)
            {
                if (!m_context.IsInitialized())
                    InitializeBgfx();

                if (m_context.IsInitialized())
                {
                    SimProvider.Start(ref m_context);
                    StartRenderLoop();
                }
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
            if (m_renderTimer != null && !m_renderTimer.IsEnabled)
                m_renderTimer.Start();
        }

        public void StopRenderLoop()
        {
            m_renderTimer?.Stop();
        }

        protected void InitializeBgfx()
        {
            if (m_context.IsInitialized()) return;
            if (SimProvider == null) return;
            if (m_mountedHandle == IntPtr.Zero) return;

            var topLevel = TopLevel.GetTopLevel(this);
            double scale = topLevel?.RenderScaling ?? 1.0;
            var bounds = this.Bounds;

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                Debug.WriteLine("[Bgfx] Deferred Init: Bounds are still empty.");
                return;
            }

            m_config.Handle = m_mountedHandle;
            m_config.Width = (ushort)(bounds.Width * scale);
            m_config.Height = (ushort)(bounds.Height * scale);
            m_config.Type = RendererType.RT_TEST_BOUNCING_CIRCLE;

            ResponseCode result = SimProvider.Init(ref m_config, out m_context);

            if ((result & ResponseCode.Success) != 0)
            {
                Debug.WriteLine($"[Bgfx] Init Success ({m_config.Width}x{m_config.Height})");

                // If the state was already set to Running waiting for Init, start now.
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

        protected void ResizeBgfx(Size newSize)
        {
            if (!m_context.IsInitialized() || SimProvider == null) return;

            var topLevel = TopLevel.GetTopLevel(this);
            double scale = topLevel?.RenderScaling ?? 1.0;

            m_config.Width = (ushort)(newSize.Width * scale);
            m_config.Height = (ushort)(newSize.Height * scale);

            SimProvider.UpdateConfig(ref m_context, ref m_config);
        }

        private void ShutdownBgfx()
        {
            if (SimProvider != null && m_context.IsInitialized())
            {
                SimProvider.Stop(ref m_context);
                SimProvider.Shutdown(ref m_context);
                m_context = default;
            }
        }
    }
}