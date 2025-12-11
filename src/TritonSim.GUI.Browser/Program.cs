using Avalonia;
using Avalonia.Browser;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using TritonSim.GUI;
using TritonSim.GUI.Browser.Providers;
using TritonSim.GUI.Providers;

internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            await BuildAvaloniaApp()
                .WithInterFont()
                .StartBrowserAppAsync("app");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL STARTUP ERROR: {ex}");
            Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        // This now refers to TritonSim.GUI.Browser.App, which has the RegisterServices override
        => AppBuilder.Configure<TritonSim.GUI.Browser.App>();
}