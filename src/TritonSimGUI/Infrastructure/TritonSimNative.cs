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
        public static partial ResponseCode init(ref SimConfig config, out SimContext ctx);

        [LibraryImport(LIB_NAME)]
        public static partial ResponseCode update_config(ref SimContext ctx, ref SimConfig config);

        [LibraryImport(LIB_NAME)]
        public static partial ResponseCode render_frame(ref SimContext ctx);

        [LibraryImport(LIB_NAME)]
        public static partial ResponseCode start(ref SimContext ctx);

        [LibraryImport(LIB_NAME)]
        public static partial ResponseCode stop(ref SimContext ctx);

        [LibraryImport(LIB_NAME)]
        public static partial ResponseCode shutdown(ref SimContext ctx);
    }
}
