using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TritonSim.GUI.Browser.Providers;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Browser
{
    public class App : GUI.App
    {
        public const string AppClientID = "app";

        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<INativeSimulator, BrowserNativeSimulator>();
            services.AddSingleton<INativeCanvasProvider, BrowserCanvasProvider>((sp) => new BrowserCanvasProvider(AppClientID, sp.GetRequiredService<ILogger>()));

            base.RegisterServices(services);
        }
    }
}