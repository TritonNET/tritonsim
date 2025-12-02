using System.Runtime.InteropServices;

namespace TritonSimGUI.Platforms.Windows
{
    public static class DWMNative
    {
        // P/Invoke for DWM (UInt version for Colors)
        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref uint attrValue, int attrSize);

        // P/Invoke for DWM (Int version for Enums)
        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
    }
}
