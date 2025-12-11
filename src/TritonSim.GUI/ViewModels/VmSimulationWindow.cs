using CommunityToolkit.Mvvm.ComponentModel;

namespace TritonSim.GUI.ViewModels
{
    public partial class VmSimulationWindow : ObservableObject
    {
        [ObservableProperty]
        private string m_title = "TritonNET - Simulation";

        [ObservableProperty]
        private VmSimulation m_simulation;

        public VmSimulationWindow(VmSimulation vm)
        {
            m_simulation = vm;
        }
    }
}
