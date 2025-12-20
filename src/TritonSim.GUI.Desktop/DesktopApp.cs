using Microsoft.Extensions.DependencyInjection;
using TritonSim.GUI.Desktop.Providers;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Desktop
{
    public class DesktopApp : App
    {
        public DesktopApp(ILogger logger): base(logger)
        {
            
        }

        protected override void RegisterServices(IServiceCollection services)
        {
            m_logger.Info("Registering Desktop specific services...");

            services.AddSingleton<INativeSimulator, DesktopNativeSimulator>();
            services.AddSingleton<INativeCanvasProvider, WindowsCanvasProvider>();

            base.RegisterServices(services);

            m_logger.Info("Desktop specific services registered.");
        }
    }
}
