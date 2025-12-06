using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TritonSim.GUI.Infrastructure;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.ViewModels
{
    public partial class VmSimulation : ObservableObject
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartCommand), nameof(StopCommand))]
        private SimulationState m_currentState = SimulationState.Initialized;

        public bool CanStart => true;//CurrentState == SimulationState.Initialized || CurrentState == SimulationState.Paused;

        public bool CanStop => CurrentState == SimulationState.Running;

        public ObservableCollection<VmRendererType> RendererTypes { get; }

        private VmRendererType m_selectedRenderer;
        public VmRendererType SelectedRenderer
        {
            get => m_selectedRenderer;
            set => SetProperty(ref m_selectedRenderer, value);
        }

        public ITritonSimNativeProvider SimProvider { get; }
        public INativeWindowProvider WindowProvider { get; }

        public VmSimulation(ITritonSimNativeProvider simProvider, INativeWindowProvider windowProvider)
        {
            SimProvider = simProvider;
            WindowProvider = windowProvider;

            RendererTypes = new ObservableCollection<VmRendererType>();

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

            SelectedRenderer = RendererTypes.FirstOrDefault();
        }

        [RelayCommand(CanExecute = nameof(CanStart))]
        private void Start()
        {
            CurrentState = SimulationState.Running;
        }

        [RelayCommand(CanExecute = nameof(CanStop))]
        private void Stop()
        {
            CurrentState = SimulationState.Initialized;
        }
    }
}
