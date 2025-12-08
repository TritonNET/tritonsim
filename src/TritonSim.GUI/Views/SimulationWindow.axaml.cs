using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TritonSim.GUI.ViewModels;

namespace TritonSim.GUI.Views;

public partial class SimulationWindow : Window
{
    private readonly VmSimulation m_vm;

    public SimulationWindow(VmSimulation vm)
    {
        InitializeComponent();

        DataContext = vm;
        m_vm = vm;

        vm.StartSimulation += StartSimulation;
        vm.StopSimulation += StopSimulation;
    }

    private void StopSimulation()
    {
        simControl.Stop();
    }

    private void StartSimulation()
    {
        simControl.Start();
    }
}