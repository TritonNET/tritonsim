using Avalonia;
using Avalonia.Browser;
using System;
using System.Threading.Tasks;

using BrowserApp = TritonSim.GUI.Browser.App;

internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            await BuildAvaloniaApp()
                .WithInterFont()
                .StartBrowserAppAsync(BrowserApp.AppClientID);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL STARTUP ERROR: {ex}");
            Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<BrowserApp>();
}