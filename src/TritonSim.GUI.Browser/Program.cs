using Avalonia;
using Avalonia.Browser;
using System;
using System.Threading.Tasks;
using TritonSim.GUI.Browser;
using TritonSim.GUI.Providers;

internal sealed partial class Program
{
    private readonly static ILogger s_logger = new ConsoleLogger();

    private static async Task Main(string[] args)
    {
        s_logger.Info("Starting TritonSim Browser App...");

        try
        {
            await BuildAvaloniaApp()
                .WithInterFont()
                .StartBrowserAppAsync(BrowserApp.AppClientID);

            s_logger.Info("TritonSim Browser App started successfully.");
        }
        catch (Exception ex)
        {
            s_logger.Fatal($"FATAL STARTUP ERROR: {ex}");
            s_logger.Fatal($"STACK TRACE: {ex.StackTrace}");
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure(() => new BrowserApp(s_logger));
}