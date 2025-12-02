using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TritonSimGUI.Infrastructure;
using TritonSimGUI.Platforms.Windows;
using MGrid = Microsoft.UI.Xaml.Controls.Grid;

namespace TritonSimGUI.Views
{
    // This file is implicitly compiled ONLY for Windows, so no #if checks are needed.
    public partial class NativeRendererViewHandler : ViewHandler<NativeRendererView, MGrid>
    {
        private IDispatcherTimer? m_renderTimer;

        private SimContext m_context;
        private SimConfig m_config;

        private MGrid? m_platformGrid;

        protected override MGrid CreatePlatformView()
        {
            return new MGrid();
        }

        protected override void ConnectHandler(MGrid platformView)
        {
            base.ConnectHandler(platformView);

            m_platformGrid = platformView;
            m_platformGrid.Loaded += PlatformGrid_Loaded;
            m_platformGrid.SizeChanged += PlatformGrid_SizeChanged;
            m_platformGrid.LayoutUpdated += (s, e) => UpdateRect();
            m_platformGrid.Unloaded += PlatformGrid_Unloaded;
        }

        private void PlatformGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            StopRenderLoop();

            if (m_context.IsInitialized())
            {
                var response = TritonSimNative.shutdown(ref m_context);
                if ((response & ResponseCode.Success) != 0)
                    throw new InvalidOperationException("Failed to shutdown the renderer.");

                Debug.Assert(!m_context.IsInitialized(), "Renderer is not properly uninitialized");
            }

            if (m_config.Handle != IntPtr.Zero)
            {
                User32Native.DestroyWindow(m_config.Handle);
                m_config.Handle = IntPtr.Zero;
            }
        }

        private void PlatformGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!m_context.IsInitialized())
                EnsureNativeView();

            UpdateRect();
        }

        private async void PlatformGrid_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(100);

            EnsureNativeView();

            await Task.Delay(100);

            UpdateRect();
        }

        private void EnsureNativeView()
        {
            if (m_platformGrid.ActualWidth <= 0 || m_platformGrid.ActualHeight <= 0)
                return;

            IntPtr parentWindowHandle = IntPtr.Zero;
            var mauiWindow = this.VirtualView?.Window;
            var nativeWindow = mauiWindow?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;

            if (nativeWindow != null)
                parentWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);

            if (parentWindowHandle == IntPtr.Zero) return;

            if (m_config.Handle == IntPtr.Zero)
            {
                m_config.Handle = CreateChildWindow(parentWindowHandle);
                if (m_config.Handle == IntPtr.Zero)
                {
                    Debug.WriteLine($"[Bgfx Error] CreateWindowEx FAILED. Error: {Marshal.GetLastWin32Error()}");
                    return;
                }
            }

            m_platformGrid.DispatcherQueue.TryEnqueue(() =>
            {
                UpdateRect();

                if (!m_context.IsInitialized())
                {
                    double scale = m_platformGrid.XamlRoot?.RasterizationScale ?? 1.0;

                    m_config.Width = (ushort)(m_platformGrid.ActualWidth * scale);
                    m_config.Height = (ushort)(m_platformGrid.ActualHeight * scale);
                    m_config.Type = RendererType.RT_TEST;

                    ResponseCode result = TritonSimNative.init(ref m_config, out m_context);

                    var success = (result & ResponseCode.Success) == 0;

                    Debug.WriteLine($"[Bgfx Status] Initialization {(success ? "SUCCESS" : "FAILED")}");
                    Debug.Assert(success ? m_context.IsInitialized() : !m_context.IsInitialized(), "Got success response, but renderer is not initialized");

                    if (m_context.IsInitialized())
                    {
                        StartRenderLoop();
                    }
                }
            });
        }

        private void UpdateRect()
        {
            if (m_config.Handle == IntPtr.Zero || m_platformGrid == null || m_platformGrid.XamlRoot == null) 
                return;

            double scale = m_platformGrid.XamlRoot.RasterizationScale;
            int x = 0, y = 0;
            int w = (int)(m_platformGrid.ActualWidth * scale);
            int h = (int)(m_platformGrid.ActualHeight * scale);

            try
            {
                var transform = m_platformGrid.TransformToVisual(null);
                var topLeft = transform.TransformPoint(new Windows.Foundation.Point(0, 0));
                x = (int)(topLeft.X * scale);
                y = (int)(topLeft.Y * scale);
            }
            catch (Exception)
            {
                // Fallback if not in visual tree
            }

            if (w > 0 && h > 0)
            {
                User32Native.SetWindowPos(m_config.Handle, new IntPtr(0), x, y, w, h, 0x0040);
            }
        }

        private IntPtr CreateChildWindow(IntPtr parentHwnd)
        {
            uint wsStyle = 0x40000000 | 0x10000000 | 0x04000000 | 0x02000000;
            IntPtr hInstance = Marshal.GetHINSTANCE(typeof(NativeRendererViewHandler).Module);

            return User32Native.CreateWindowEx(
                0, "STATIC", "", wsStyle,
                0, 0, 0, 0,
                parentHwnd, IntPtr.Zero, hInstance, IntPtr.Zero);
        }

        private void StartRenderLoop()
        {
            m_renderTimer = this.VirtualView.Dispatcher.CreateTimer();
            m_renderTimer.Interval = TimeSpan.FromMilliseconds(16);
            m_renderTimer.Tick += RenderTimer_Tick;
            m_renderTimer.Start();
        }

        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            if (!m_context.IsInitialized())
                return;

            TritonSimNative.render_frame(ref m_context);
        }

        private void StopRenderLoop()
        {
            m_renderTimer?.Stop();
        }
    }
}