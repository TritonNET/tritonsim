using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using TritonSim.GUI.ViewModels;

namespace TritonSim.GUI.Views;

public partial class SimulationWindow : Window
{
    private readonly VmSimulationWindow m_vm;

    public SimulationWindow(VmSimulationWindow vm)
    {
        InitializeComponent();

        DataContext = vm;
        m_vm = vm;
    }

    private void TitleBar_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}