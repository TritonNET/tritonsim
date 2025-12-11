using System.Runtime.InteropServices;
using TritonSim.GUI.Infrastructure;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Desktop.Providers
{
    public partial class DesktopNativeSimulator : INativeSimulator
    {
        private const string LIB_NAME = "TritonSimRenderer.dll";

        public ResponseCode Init(ref SimConfig config, out SimContext ctx) 
            => NativeInit(ref config, out ctx);

        public ResponseCode UpdateConfig(ref SimContext ctx, ref SimConfig config)
            => NativeUpdateConfig(ref ctx, ref config);

        public ResponseCode RenderFrame(ref SimContext ctx)
            => NativeRenderFrame(ref ctx);

        public ResponseCode Start(ref SimContext ctx)
            => NativeStart(ref ctx);

        public ResponseCode Stop(ref SimContext ctx)
            => NativeStop(ref ctx);

        public ResponseCode Shutdown(ref SimContext ctx)
            => NativeShutdown(ref ctx);

        [LibraryImport(LIB_NAME, EntryPoint = "tritonsim_init")]
        private static partial ResponseCode NativeInit(ref SimConfig config, out SimContext ctx);

        [LibraryImport(LIB_NAME, EntryPoint = "tritonsim_update_config")]
        private static partial ResponseCode NativeUpdateConfig(ref SimContext ctx, ref SimConfig config);

        [LibraryImport(LIB_NAME, EntryPoint = "tritonsim_render_frame")]
        private static partial ResponseCode NativeRenderFrame(ref SimContext ctx);

        [LibraryImport(LIB_NAME, EntryPoint = "tritonsim_start")]
        private static partial ResponseCode NativeStart(ref SimContext ctx);

        [LibraryImport(LIB_NAME, EntryPoint = "tritonsim_stop")]
        private static partial ResponseCode NativeStop(ref SimContext ctx);

        [LibraryImport(LIB_NAME, EntryPoint = "tritonsim_shutdown")]
        private static partial ResponseCode NativeShutdown(ref SimContext ctx);
    }
}