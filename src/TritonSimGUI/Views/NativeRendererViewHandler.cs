using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System.Runtime.InteropServices;
using TritonSimGUI.Infrastructure;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Controls.Platform;
using System.Diagnostics; // For Debug.WriteLine

#if WINDOWS
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
#endif

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
        private static bool _bgfxInitialized = false;
        private IDispatcherTimer? _renderTimer;
        private IntPtr _childWindowHandle = IntPtr.Zero;

        public static IPropertyMapper<NativeRendererView, NativeRendererViewHandler> PropertyMapper =
            new PropertyMapper<NativeRendererView, NativeRendererViewHandler>(ViewHandler.ViewMapper);

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
            var grid = platformView;

            grid.Loaded += (s, e) =>
            {
                // Ensure we have valid dimensions
                if (grid.ActualWidth <= 0 || grid.ActualHeight <= 0) return;

                // 1. Get Parent Window Handle
                IntPtr parentWindowHandle = IntPtr.Zero;
                var mauiWindow = this.VirtualView?.Window;
                var nativeWindow = mauiWindow?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;

                if (nativeWindow != null)
                {
                    parentWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                }

                if (parentWindowHandle == IntPtr.Zero)
                {
                    Debug.WriteLine("[Bgfx Error] Could not retrieve Parent Window Handle.");
                    return;
                }

                // 2. Create Child Window
                _childWindowHandle = CreateChildWindow(parentWindowHandle, (int)grid.ActualWidth, (int)grid.ActualHeight);

                if (_childWindowHandle == IntPtr.Zero)
                {
                    // CRITICAL: Check the Output window for this error code if it fails
                    int errorCode = Marshal.GetLastWin32Error();
                    Debug.WriteLine($"[Bgfx Error] CreateWindowEx FAILED. Error Code: {errorCode}");
                    return;
                }

                ushort width = (ushort)grid.ActualWidth;
                ushort height = (ushort)grid.ActualHeight;

                // 3. Initialize Bgfx
                if (!_bgfxInitialized)
                {
                    int initResult = TritonSimNative.tritonsim_init(_childWindowHandle, width, height);
                    _bgfxInitialized = initResult == 1;
                    Debug.WriteLine($"[Bgfx Status] Initialization {(_bgfxInitialized ? "SUCCESS" : "FAILED")}");
                }

                if (_bgfxInitialized)
                {
                    StartRenderLoop();
                }
            };

            grid.SizeChanged += (s, e) =>
            {
                if (_childWindowHandle != IntPtr.Zero && _bgfxInitialized)
                {
                    MoveWindow(_childWindowHandle, 0, 0, (int)e.NewSize.Width, (int)e.NewSize.Height, true);
                    // TODO: Call a C++ function here to resize bgfx backbuffers (e.g. bgfx::reset)
                }
            };

            grid.Unloaded += (s, e) =>
            {
                _renderTimer?.Stop();

                if (_bgfxInitialized)
                {
                    TritonSimNative.tritonsim_shutdown();
                    _bgfxInitialized = false;
                }

                if (_childWindowHandle != IntPtr.Zero)
                {
                    DestroyWindow(_childWindowHandle);
                    _childWindowHandle = IntPtr.Zero;
                }
            };
#endif
        }

        private void StartRenderLoop()
        {
            _renderTimer = this.VirtualView.Dispatcher.CreateTimer();
            _renderTimer.Interval = TimeSpan.FromMilliseconds(16);
            _renderTimer.Tick += (s, e) =>
            {
                if (_bgfxInitialized)
                {
                    int pulse = (int)(DateTime.Now.Millisecond / 4.0);
                    uint r = (uint)(Math.Sin(pulse * 0.05) * 127 + 128) << 24;
                    uint g = (uint)(Math.Sin(pulse * 0.05 + Math.PI / 2) * 127 + 128) << 16;
                    uint b = (uint)(Math.Sin(pulse * 0.05 + Math.PI) * 127 + 128) << 8;
                    uint color = r | g | b | 0xFF;

                    TritonSimNative.tritonsim_render_frame((int)color);
                }
            };
            _renderTimer.Start();
        }

#if WINDOWS
        private IntPtr CreateChildWindow(IntPtr parentHwnd, int width, int height)
        {
            // WS_CHILD (0x40000000) | WS_VISIBLE (0x10000000) | WS_CLIPCHILDREN (0x02000000)
            uint wsStyle = 0x40000000 | 0x10000000 | 0x02000000;

            // Get the module handle for the current assembly
            IntPtr hInstance = Marshal.GetHINSTANCE(typeof(NativeRendererViewHandler).Module);

            return CreateWindowEx(
                0,
                "STATIC", // System class name (uppercase is safer)
                "",
                wsStyle,
                0, 0,
                width, height,
                parentHwnd,
                IntPtr.Zero,
                hInstance, // Pass valid instance handle
                IntPtr.Zero);
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(
           uint dwExStyle,
           string lpClassName,
           string lpWindowName,
           uint dwStyle,
           int x,
           int y,
           int nWidth,
           int nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);
#endif
    }
}