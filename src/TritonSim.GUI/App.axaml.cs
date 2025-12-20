using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using TritonSim.GUI.Providers;
using TritonSim.GUI.ViewModels;
using TritonSim.GUI.Views;

namespace TritonSim.GUI
{
    public partial class App : Application
    {
        protected readonly ILogger m_logger;

        public App(ILogger logger)
        {
            m_logger = logger;
            m_logger.Info("App created.");
        }

        public IServiceProvider Services { get; private set; }

        public override void Initialize()
        {
            m_logger.Info("App initializing...");

            AvaloniaXamlLoader.Load(this);

            m_logger.Info("App initialized.");
        }

        public override void OnFrameworkInitializationCompleted()
        {
            m_logger.Info("App framework initialization completed. Registering services...");

            var collection = new ServiceCollection();

            RegisterServices(collection);

            Services = collection.BuildServiceProvider();

            m_logger.Info("Services registered.");

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                m_logger.Info("Setting up MainWindow for Desktop application lifetime.");

                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = Services.GetRequiredService<SimulationWindow>();

                m_logger.Info("MainWindow set for Desktop application lifetime.");
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                m_logger.Info("Setting up MainView for Single View application lifetime.");

                singleViewPlatform.MainView = Services.GetRequiredService<SimulationView>();

                m_logger.Info("MainView set for Single View application lifetime.");
            }

            base.OnFrameworkInitializationCompleted();

            m_logger.Info("App framework initialization process completed.");
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            m_logger.Info("Disabling Avalonia DataAnnotationsValidationPlugin to prevent duplicate validations.");

            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }

            m_logger.Info("Avalonia DataAnnotationsValidationPlugin disabled.");
        }

        protected virtual void RegisterServices(IServiceCollection services)
        {
            m_logger.Info("Registering base application services...");

            services.AddSingleton<ILogger>(m_logger);

            services.AddSingleton<ITritonSimNativeProvider, NativeProvider>();

            services.AddTransient<VmSimulation>();
            services.AddTransient<VmSimulationWindow>();

            services.AddTransient<SimulationView>();
            services.AddTransient<SimulationWindow>();

            m_logger.Info("Base application services registered.");
        }
    }
}