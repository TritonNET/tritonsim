using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System.Runtime.InteropServices;
using TritonSimGUI.Infrastructure;
using Microsoft.Maui.Dispatching;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace TritonSimGUI.Views
{
    // This file is implicitly compiled ONLY for Windows, so no #if checks are needed.
    public partial class NativeRendererViewHandler : ViewHandler<NativeRendererView, Microsoft.UI.Xaml.Controls.Grid>
    {
        private static bool _bgfxInitialized = false;
        private IDispatcherTimer? _renderTimer;
        private IntPtr _childWindowHandle = IntPtr.Zero;

        private Microsoft.UI.Xaml.Controls.Grid? _platformGrid;

        protected override Microsoft.UI.Xaml.Controls.Grid CreatePlatformView()
        {
            return new Microsoft.UI.Xaml.Controls.Grid();
        }

        protected override void ConnectHandler(Microsoft.UI.Xaml.Controls.Grid platformView)
        {
            base.ConnectHandler(platformView);

            _platformGrid = platformView;

            void EnsureNativeView()
            {
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

                if (_childWindowHandle == IntPtr.Zero)
                {
                    _childWindowHandle = CreateChildWindow(parentWindowHandle);
                    if (_childWindowHandle == IntPtr.Zero)
                    {
                        Debug.WriteLine($"[Bgfx Error] CreateWindowEx FAILED. Error: {Marshal.GetLastWin32Error()}");
                        return;
                    }
                }

                _platformGrid.DispatcherQueue.TryEnqueue(() =>
                {
                    UpdateRect();

                    if (!_bgfxInitialized)
                    {
                        double scale = _platformGrid.XamlRoot?.RasterizationScale ?? 1.0;
                        ushort physWidth = (ushort)(_platformGrid.ActualWidth * scale);
                        ushort physHeight = (ushort)(_platformGrid.ActualHeight * scale);

                        int initResult = TritonSimNative.init(_childWindowHandle, physWidth, physHeight);
                        _bgfxInitialized = initResult == 1;
                        Debug.WriteLine($"[Bgfx Status] Initialization {(_bgfxInitialized ? "SUCCESS" : "FAILED")}");

                        if (_bgfxInitialized)
                        {
                            StartRenderLoop();
                        }
                    }
                });
            }

            _platformGrid.Loaded += async (s, e) =>
            {
                await Task.Delay(100);
                EnsureNativeView();
                await Task.Delay(100);
                UpdateRect();
            };

            _platformGrid.SizeChanged += (s, e) =>
            {
                if (!_bgfxInitialized) EnsureNativeView();
                UpdateRect();
            };

            _platformGrid.LayoutUpdated += (s, e) => UpdateRect();

            _platformGrid.Unloaded += (s, e) =>
            {
                StopRenderLoop();

                if (_bgfxInitialized)
                {
                    TritonSimNative.shutdown();
                    _bgfxInitialized = false;
                }

                if (_childWindowHandle != IntPtr.Zero)
                {
                    DestroyWindow(_childWindowHandle);
                    _childWindowHandle = IntPtr.Zero;
                }
            };
        }

        private void UpdateRect()
        {
            if (_childWindowHandle == IntPtr.Zero || _platformGrid == null || _platformGrid.XamlRoot == null) return;

            double scale = _platformGrid.XamlRoot.RasterizationScale;
            int x = 0, y = 0;
            int w = (int)(_platformGrid.ActualWidth * scale);
            int h = (int)(_platformGrid.ActualHeight * scale);

            try
            {
                var transform = _platformGrid.TransformToVisual(null);
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
                SetWindowPos(_childWindowHandle, new IntPtr(0), x, y, w, h, 0x0040);
            }
        }

        private IntPtr CreateChildWindow(IntPtr parentHwnd)
        {
            uint wsStyle = 0x40000000 | 0x10000000 | 0x04000000 | 0x02000000;
            IntPtr hInstance = Marshal.GetHINSTANCE(typeof(NativeRendererViewHandler).Module);

            return CreateWindowEx(
                0, "STATIC", "", wsStyle,
                0, 0, 0, 0,
                parentHwnd, IntPtr.Zero, hInstance, IntPtr.Zero);
        }

        private void StartRenderLoop()
        {
            _renderTimer = this.VirtualView.Dispatcher.CreateTimer();
            _renderTimer.Interval = TimeSpan.FromMilliseconds(16);
            _renderTimer.Tick += (s, e) =>
            {
                if (_bgfxInitialized)
                {
                    long ticks = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    int r = (int)((Math.Sin(ticks * 0.005) + 1.0) * 127.5);
                    uint color = (uint)((r << 24) | 0x000000FF);

                    TritonSimNative.render_frame((int)color);
                }
            };
            _renderTimer.Start();
        }

        private void StopRenderLoop()
        {
            _renderTimer?.Stop();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);
    }
}