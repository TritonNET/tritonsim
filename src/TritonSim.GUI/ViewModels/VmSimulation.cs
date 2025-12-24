using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TritonSim.GUI.Infrastructure;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.ViewModels
{
    public delegate void SimulationModeEventHandler();

    public partial class VmSimulation : ObservableObject
    {
        public event SimulationModeEventHandler? StartSimulation;

        public event SimulationModeEventHandler? StopSimulation;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(StopCommand))]
        private SimulationMode m_currentMode = SimulationMode.NotReady;

        public bool CanStart => CurrentMode == SimulationMode.Ready;

        public bool CanStop => CurrentMode == SimulationMode.Running;

        [ObservableProperty]
        private VmRendererType? m_selectedRenderer;

        public ObservableCollection<VmRendererType> RendererTypes { get; }

        public ITritonSimNativeProvider SimProvider { get; }
        public INativeCanvasProvider WindowProvider { get; }

        public ILogger Logger { get; }

        public VmSimulation(
            ILogger logger,
            ITritonSimNativeProvider simProvider, 
            INativeCanvasProvider windowProvider)
        {
            SimProvider = simProvider;
            WindowProvider = windowProvider;
            Logger = logger;

            logger.Debug("VmSimulation creating.");

            RendererTypes = new ObservableCollection<VmRendererType>();

            logger.Debug("VmSimulation created.");
        }

        public void Init()
        {
            Logger.Debug("VmSimulation initializing.");

            foreach (RendererType type in Enum.GetValues(typeof(RendererType)))
            {
                if ((type & RendererType.RT_MASK_ID) == 0 || type == RendererType.RT_UNKNOWN || type == RendererType.RT_MASK_ID)
                    continue;

                RendererTypes.Add(new VmRendererType
                {
                    Type = type,
                    Name = type.GetDescription()
                });
            }

            SelectedRenderer = RendererTypes.Where(e => e.Type == RendererType.RT_TEST_EDGES).FirstOrDefault();

            Logger.Debug("VmSimulation initialized.");
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(CurrentMode))
            {
                OnPropertyChanged(nameof(CanStart));
                OnPropertyChanged(nameof(CanStop));
            }
        }

        [RelayCommand(CanExecute = nameof(CanStart))]
        private void Start() => StartSimulation?.Invoke();

        [RelayCommand(CanExecute = nameof(CanStop))]
        private void Stop() => StopSimulation?.Invoke();
    }
}
