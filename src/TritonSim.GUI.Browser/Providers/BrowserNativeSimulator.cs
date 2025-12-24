using Avalonia.Threading;
using System;
using System.Runtime.InteropServices;
using TritonSim.GUI.Infrastructure;
using TritonSim.GUI.Providers;

public partial class BrowserNativeSimulator : INativeSimulator
{
    private readonly ILogger m_logger;

    private const string LIB_NAME = "libTritonSimRenderer";

    public BrowserNativeSimulator(ILogger logger)
    {
        m_logger = logger;
    }

    public ResponseCode Init(ref SimConfig config, out SimContext ctx)
    {
        m_logger.Debug("[BrowserNative] Initializing Native Simulator...");
        try
        {
            return NativeInit(ref config, out ctx);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserNative] FATAL Error in Init: {ex.Message}\n{ex.StackTrace}");
            ctx = default;
            return ResponseCode.FailedUnknown;
        }
    }

    public ResponseCode UpdateConfig(ref SimContext ctx, ref SimConfig config)
    {
        m_logger.Debug("[BrowserNative] Updating Native Simulator Config...");
        try
        {
            return NativeUpdateConfig(ref ctx, ref config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserNative] Error in UpdateConfig: {ex.Message}");
            return ResponseCode.FailedUnknown;
        }
    }

    public ResponseCode RenderFrame(ref SimContext ctx)
    {
        try
        {
            return NativeRenderFrame(ref ctx);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserNative] Error in RenderFrame: {ex.Message}");
            return ResponseCode.FailedUnknown;
        }
    }

    public ResponseCode Start(ref SimContext ctx)
    {
        m_logger.Debug("[BrowserNative] Starting Native Simulator...");
        try
        {
            return NativeStart(ref ctx);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserNative] Error in Start: {ex.Message}");
            return ResponseCode.FailedUnknown;
        }
    }

    public ResponseCode Stop(ref SimContext ctx)
    {
        m_logger.Debug("[BrowserNative] Stopping Native Simulator...");
        try
        {
            return NativeStop(ref ctx);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserNative] Error in Stop: {ex.Message}");
            return ResponseCode.FailedUnknown;
        }
    }

    public ResponseCode Shutdown(ref SimContext ctx)
    {
        m_logger.Debug("[BrowserNative] Shutting down Native Simulator...");
        try
        {
            return NativeShutdown(ref ctx);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserNative] Error in Shutdown: {ex.Message}");
            return ResponseCode.FailedUnknown;
        }
    }


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