using Microsoft.Extensions.DependencyInjection;
using TritonSim.GUI.Desktop.Providers;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Desktop
{
    public class App : GUI.App
    {
        protected override void RegisterServices(IServiceCollection serviceCollection)
        {
            base.RegisterServices(serviceCollection);

            serviceCollection.AddSingleton<ITritonSimNativeProvider, DesktopNativeProvider>();
            serviceCollection.AddSingleton<INativeWindowProvider, WindowsWindowProvider>();
        }
    }
}
