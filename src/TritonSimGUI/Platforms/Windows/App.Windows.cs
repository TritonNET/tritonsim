using TritonSimGUI.Platforms.Windows;

namespace TritonSimGUI
{
    public partial class App : Application
    {
        partial void CustomizeWindow(Window window)
        {
            window.Created += (s, e) => ConfigureNativeWindow(window);
        }

        private void ConfigureNativeWindow(Window mauiWindow)
        {
            var nativeWindow = mauiWindow.Handler?.PlatformView;
            if (nativeWindow is Microsoft.UI.Xaml.Window winuiWindow)
            {
                // Access your Colors.xaml resources
                var bgMaui = GetMauiColor("OffBlack");              // #1F1F1F
                var borderMaui = GetMauiColor("ColorGrayDisabled"); // #A5A5A5
                var textMaui = GetMauiColor("ColorWhite");          // #FFFFFF

                // Convert for WinUI
                var bgWin = ToWinUIColor(bgMaui);
                var borderWin = ToWinUIColor(borderMaui);
                var textWin = ToWinUIColor(textMaui);

                // Convert for DWM
                uint bgRef = ToDwmColorRef(bgMaui);
                uint borderRef = ToDwmColorRef(borderMaui);
                uint textRef = ToDwmColorRef(textMaui);

                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(winuiWindow);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                if (appWindow != null)
                {
                    var titleBar = appWindow.TitleBar;

                    // Title bar button area
                    titleBar.BackgroundColor = bgWin;
                    titleBar.ForegroundColor = textWin;

                    titleBar.ButtonBackgroundColor = bgWin;
                    titleBar.ButtonForegroundColor = textWin;

                    // Hover
                    titleBar.ButtonHoverBackgroundColor = bgWin;
                    titleBar.ButtonHoverForegroundColor = textWin;

                    // Pressed
                    titleBar.ButtonPressedBackgroundColor = bgWin;
                    titleBar.ButtonPressedForegroundColor = textWin;

                    // Inactive
                    titleBar.ButtonInactiveBackgroundColor = bgWin;
                    titleBar.ButtonInactiveForegroundColor = textWin;
                }

                // Title bar background
                DWMNative.DwmSetWindowAttribute(hWnd, (int)DWMWINDOWATTRIBUTE.DWMWA_CAPTION_COLOR, ref bgRef, sizeof(uint));

                // Title text
                DWMNative.DwmSetWindowAttribute(hWnd, (int)DWMWINDOWATTRIBUTE.DWMWA_TEXT_COLOR, ref textRef, sizeof(uint));

                // Border
                DWMNative.DwmSetWindowAttribute(hWnd, (int)DWMWINDOWATTRIBUTE.DWMWA_BORDER_COLOR, ref borderRef, sizeof(uint));

                // Square corners
                int cornerPref = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DONOTROUND;
                DWMNative.DwmSetWindowAttribute(hWnd, (int)DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPref, sizeof(int));
            }
        }


        private Color GetMauiColor(string key)
        {
            if (Application.Current.Resources.TryGetValue(key, out var value) && value is Color c)
                return c;

            throw new Exception($"Color resource '{key}' not found in Colors.xaml");
        }

        private Windows.UI.Color ToWinUIColor(Color c)
        {
            return global::Windows.UI.Color.FromArgb(
                (byte)(c.Alpha * 255),
                (byte)(c.Red * 255),
                (byte)(c.Green * 255),
                (byte)(c.Blue * 255)
            );
        }

        private uint ToDwmColorRef(Color c)
        {
            byte r = (byte)(c.Red * 255);
            byte g = (byte)(c.Green * 255);
            byte b = (byte)(c.Blue * 255);

            return (uint)(b | (g << 8) | (r << 16));
        }
    }
}
