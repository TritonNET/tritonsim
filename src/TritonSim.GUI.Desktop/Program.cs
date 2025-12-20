using Avalonia;
using System;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Desktop
{
    internal sealed class Program
    {
        private readonly static ILogger s_logger = new ConsoleLogger();

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure(() => new DesktopApp(s_logger))
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
