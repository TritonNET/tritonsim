namespace TritonSimGUI.Infrastructure
{
    public static partial class ExtensionMethods
    {
        public static partial IntPtr GetNativeHandle(this View view);

        public static partial (int width, int height) GetSize(this View view);
    }
}
