using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using TritonSim.GUI.Browser.Providers;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Browser
{
    public class BrowserApp : App
    {
        public const string AppClientID = "tritonsim-app";

        public BrowserApp(ILogger logger): base(logger)
        {
            m_logger.Info("BrowserApp created.");
        }

        protected override void RegisterServices(IServiceCollection services)
        {
            m_logger.Info("Registering Browser specific services...");

            services.AddSingleton<INativeSimulator, BrowserNativeSimulator>();
            services.AddSingleton<INativeCanvasProvider, BrowserCanvasProvider>((sp) => new BrowserCanvasProvider(AppClientID, sp.GetRequiredService<ILogger>()));

            base.RegisterServices(services);

            m_logger.Info("Browser specific services registered.");
        }
    }
}