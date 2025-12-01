using Microsoft.Maui.Handlers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TritonSimGUI.Infrastructure;

namespace TritonSimGUI.Views
{
    public partial class NativeRendererViewHandler : ViewHandler<NativeRendererView,
#if WINDOWS
        Microsoft.UI.Xaml.Controls.Grid
#else
        object
#endif
    >
    {
        private static bool m_bgfxInitialized = false;
        private IDispatcherTimer? m_renderTimer;
        private IntPtr m_childWindowHandle = IntPtr.Zero;

#if WINDOWS
        private Microsoft.UI.Xaml.Controls.Grid? _platformGrid;
#endif

        public static IPropertyMapper<NativeRendererView, NativeRendererViewHandler> PropertyMapper =
            new PropertyMapper<NativeRendererView, NativeRendererViewHandler>(ViewMapper);

        public NativeRendererViewHandler() : base(PropertyMapper) { }

        protected override
#if WINDOWS
            Microsoft.UI.Xaml.Controls.Grid
#else
            object
#endif
        CreatePlatformView()
        {
#if WINDOWS
            return new Microsoft.UI.Xaml.Controls.Grid();
#else
            return new object();
#endif
        }

        protected override void ConnectHandler(
#if WINDOWS
            Microsoft.UI.Xaml.Controls.Grid
#else
            object
#endif
        platformView)
        {
            base.ConnectHandler(platformView);

#if WINDOWS
            _platformGrid = platformView;

            void EnsureNativeView()
            {
                // If dimensions are invalid, stop.
                if (_platformGrid.ActualWidth <= 0 || _platformGrid.ActualHeight <= 0)
                    return;

                IntPtr parentWindowHandle = IntPtr.Zero;
                var mauiWindow = this.VirtualView?.Window;
                var nativeWindow = mauiWindow?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;

                if (nativeWindow != null)
                {
                    parentWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                }

                if (parentWindowHandle == IntPtr.Zero) return;

                // 2. Create Child Window (if not already created)
                if (m_childWindowHandle == IntPtr.Zero)
                {
                    m_childWindowHandle = CreateChildWindow(parentWindowHandle);
                    if (m_childWindowHandle == IntPtr.Zero)
                    {
                        Debug.WriteLine($"[Bgfx Error] CreateWindowEx FAILED. Error: {Marshal.GetLastWin32Error()}");
                        return;
                    }
                }

                // 3. Initialize BGFX
                _platformGrid.DispatcherQueue.TryEnqueue(() =>
                {
                    // Sync position and force Z-order
                    UpdateRect();

                    if (!m_bgfxInitialized)
                    {
                        double scale = _platformGrid.XamlRoot?.RasterizationScale ?? 1.0;
                        ushort physWidth = (ushort)(_platformGrid.ActualWidth * scale);
                        ushort physHeight = (ushort)(_platformGrid.ActualHeight * scale);

                        int initResult = TritonSimNative.tritonsim_init(m_childWindowHandle, physWidth, physHeight);
                        m_bgfxInitialized = initResult == 1;
                        Debug.WriteLine($"[Bgfx Status] Initialization {(m_bgfxInitialized ? "SUCCESS" : "FAILED")}");

                        if (m_bgfxInitialized)
                        {
                            StartRenderLoop();
                        }
                    }
                });
            }

            _platformGrid.Loaded += async (s, e) =>
            {
                // Delay slightly to allow WinUI visual tree to stabilize
                await Task.Delay(100);
                EnsureNativeView();

                // Double-check update to force window to top if layout shifted
                await Task.Delay(100);
                UpdateRect();
            };

            _platformGrid.SizeChanged += (s, e) =>
            {
                if (!m_bgfxInitialized) EnsureNativeView();
                UpdateRect();
            };

            _platformGrid.LayoutUpdated += (s, e) => UpdateRect();

            _platformGrid.Unloaded += (s, e) =>
            {
                StopRenderLoop();

                if (m_bgfxInitialized)
                {
                    TritonSimNative.tritonsim_shutdown();
                    m_bgfxInitialized = false;
                }

                if (m_childWindowHandle != IntPtr.Zero)
                {
                    DestroyWindow(m_childWindowHandle);
                    m_childWindowHandle = IntPtr.Zero;
                }
            };
#endif
        }

#if WINDOWS
        private void UpdateRect()
        {
            if (m_childWindowHandle == IntPtr.Zero || _platformGrid == null || _platformGrid.XamlRoot == null) return;

            double scale = _platformGrid.XamlRoot.RasterizationScale;
            int x = 0, y = 0;
            int w = (int)(_platformGrid.ActualWidth * scale);
            int h = (int)(_platformGrid.ActualHeight * scale);

            try
            {
                // Try to get exact position relative to window
                var transform = _platformGrid.TransformToVisual(null);
                var topLeft = transform.TransformPoint(new Windows.Foundation.Point(0, 0));
                x = (int)(topLeft.X * scale);
                y = (int)(topLeft.Y * scale);
            }
            catch (Exception)
            {
                // If TransformToVisual fails (common during init), ignore and assume 0,0 relative offset
                // The important part is that we still call SetWindowPos to ensure visibility/size
            }

            if (w > 0 && h > 0)
            {
                // SWP_NOACTIVATE | SWP_NOZORDER (removed to force top)
                // We use HWND_TOP (0) to move it to top of Z-order
                // Flags: 0x0040 (SWP_SHOWWINDOW)
                SetWindowPos(m_childWindowHandle, new IntPtr(0), x, y, w, h, 0x0040);
            }
        }

        private IntPtr CreateChildWindow(IntPtr parentHwnd)
        {
            // WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN
            uint wsStyle = 0x40000000 | 0x10000000 | 0x04000000 | 0x02000000;
            IntPtr hInstance = Marshal.GetHINSTANCE(typeof(NativeRendererViewHandler).Module);

            return CreateWindowEx(
                0, "STATIC", "", wsStyle,
                0, 0, 0, 0,
                parentHwnd, IntPtr.Zero, hInstance, IntPtr.Zero);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);
#endif

        private void StartRenderLoop()
        {
            m_renderTimer = this.VirtualView.Dispatcher.CreateTimer();
            m_renderTimer.Interval = TimeSpan.FromMilliseconds(16);
            m_renderTimer.Tick += (s, e) =>
            {
                if (m_bgfxInitialized)
                {
                    long ticks = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    int r = (int)((Math.Sin(ticks * 0.005) + 1.0) * 127.5);
                    uint color = (uint)((r << 24) | 0x000000FF);

                    TritonSimNative.tritonsim_render_frame((int)color);
                }
            };
            m_renderTimer.Start();
        }

        private void StopRenderLoop()
        {
            m_renderTimer?.Stop();
        }
    }
}