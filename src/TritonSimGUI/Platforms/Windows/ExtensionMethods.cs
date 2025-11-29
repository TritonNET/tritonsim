using WinRT.Interop;

namespace TritonSimGUI.Infrastructure
{
    public static partial class ExtensionMethods
    {
        public static partial IntPtr GetNativeHandle(this View view)
        {
#if WINDOWS
            // Ensure the view is in the visual tree and has a Window
            var mauiWindow = view.Window;
            if (mauiWindow == null)
                return IntPtr.Zero;

            // Get the WinUI Window from the MAUI Window handler
            var nativeWindow = mauiWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (nativeWindow == null)
                return IntPtr.Zero;

            // Get HWND from the real WinUI Window
            IntPtr hwnd = WindowNative.GetWindowHandle(nativeWindow);
            return hwnd;
#else
            return IntPtr.Zero;
#endif
        }

        public static partial (int width, int height) GetSize(this View view)
        {
            return ((int)view.Width, (int)view.Height);
        }
    }
}
