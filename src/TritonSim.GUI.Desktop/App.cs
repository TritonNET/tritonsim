using Microsoft.Extensions.DependencyInjection;
using TritonSim.GUI.Desktop.Providers;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Desktop
{
    public class App : GUI.App
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<INativeSimulator, DesktopNativeSimulator>();
            services.AddSingleton<INativeCanvasProvider, WindowsCanvasProvider>();

            base.RegisterServices(services);
        }
    }
}
