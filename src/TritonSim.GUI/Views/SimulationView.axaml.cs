using Avalonia.Controls;
using System;
using TritonSim.GUI.ViewModels;

namespace TritonSim.GUI.Views;

public partial class SimulationView : UserControl
{
    private VmSimulation? m_vm;

    public SimulationView()
    {
        InitializeComponent();
    }

    public SimulationView(VmSimulation vm)
    {
        InitializeComponent();

        m_vm = vm;
        DataContext = vm;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (m_vm != null)
        {
            m_vm.StartSimulation -= StartSimulation;
            m_vm.StopSimulation -= StopSimulation;
        }

        m_vm = DataContext as VmSimulation;

        if (m_vm != null)
        {
            m_vm.StartSimulation += StartSimulation;
            m_vm.StopSimulation += StopSimulation;
        }
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