using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TritonSim.GUI.Browser.Providers;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Browser
{
    public class App : GUI.App
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<INativeSimulator, BrowserNativeSimulator>();
            services.AddSingleton<INativeWindowProvider, BrowserWindowProvider>();

            base.RegisterServices(services);
        }
    }
}