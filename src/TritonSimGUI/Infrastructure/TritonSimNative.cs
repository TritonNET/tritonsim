using System;
using System.Runtime.InteropServices;

namespace TritonSimGUI.Infrastructure
{
    internal static partial class TritonSimNative
    {
        const string LIB_NAME =
#if WINDOWS
            "TritonSimRenderer.dll";
#elif ANDROID
            "TritonSimRenderer.so";
#elif IOS
            "__Internal";
#else
            "TritonSimRenderer";
#endif

        [LibraryImport(LIB_NAME)]
        public static partial int init(IntPtr windowHandle, int width, int height);

        [LibraryImport(LIB_NAME)]
        public static partial void set_params(float v1, float v2);

        [LibraryImport(LIB_NAME)]
        public static partial void render_frame(int clearColor);

        [LibraryImport(LIB_NAME)]
        public static partial void shutdown();
    }
}
