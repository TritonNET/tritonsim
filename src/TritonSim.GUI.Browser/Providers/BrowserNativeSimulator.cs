using System;
using System.Runtime.InteropServices;
using TritonSim.GUI.Providers; // Assuming ResponseCode is here
using TritonSim.GUI.Infrastructure;

public partial class BrowserNativeSimulator : INativeSimulator
{
    private const string LIB_NAME = "__Internal";

    public ResponseCode Init(ref SimConfig config, out SimContext ctx)
    {
        int test = 10;
        try
        {
            test = NativeTest();

            Console.WriteLine($"[BrowserNative] TEST RESULT: {test}");

            ResponseCode rc = (ResponseCode)NativeInit(ref config, out ctx);

            return rc;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserNative] FATAL Error in Init: test int: {test} | {ex.Message}\n{ex.StackTrace}");
            ctx = default;
            return ResponseCode.FailedUnknown;
        }
    }

    public ResponseCode UpdateConfig(ref SimContext ctx, ref SimConfig config)
    {
        try
        {
            int test = NativeTest();

            Console.WriteLine($"[BrowserNative] TEST RESULT: {test}");

            ResponseCode rc= (ResponseCode)NativeUpdateConfig();

            return rc;
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
            return (ResponseCode)NativeRenderFrame(ref ctx);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserNative] Error in RenderFrame: {ex.Message}");
            return ResponseCode.FailedUnknown;
        }
    }

    public ResponseCode Start(ref SimContext ctx)
    {
        try
        {
            return (ResponseCode)NativeStart(ref ctx);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserNative] Error in Start: {ex.Message}");
            return ResponseCode.FailedUnknown;
        }
    }

    public ResponseCode Stop(ref SimContext ctx)
    {
        try
        {
            return (ResponseCode)NativeStop(ref ctx);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserNative] Error in Stop: {ex.Message}");
            return ResponseCode.FailedUnknown;
        }
    }

    public ResponseCode Shutdown(ref SimContext ctx)
    {
        try
        {
            return (ResponseCode)NativeShutdown(ref ctx);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserNative] Error in Shutdown: {ex.Message}");
            return ResponseCode.FailedUnknown;
        }
    }


    [LibraryImport(LIB_NAME, EntryPoint = "tritonsim_test")]
    private static partial int NativeTest();

    [LibraryImport(LIB_NAME, EntryPoint = "tritonsim_init")]
    private static partial int NativeInit(ref SimConfig config, out SimContext ctx);

    [LibraryImport(LIB_NAME, EntryPoint = "tritonsim_update_config")]
    private static partial int NativeUpdateConfig();

    [LibraryImport(LIB_NAME, EntryPoint = "tritonsim_render_frame")]
    private static partial int NativeRenderFrame(ref SimContext ctx);

    [LibraryImport(LIB_NAME, EntryPoint = "tritonsim_start")]
    private static partial int NativeStart(ref SimContext ctx);

    [LibraryImport(LIB_NAME, EntryPoint = "tritonsim_stop")]
    private static partial int NativeStop(ref SimContext ctx);

    [LibraryImport(LIB_NAME, EntryPoint = "tritonsim_shutdown")]
    private static partial int NativeShutdown(ref SimContext ctx);
}