using TritonSimGUI.ViewModels;

namespace TritonSimGUI
{
    public partial class MainPage : ContentPage
    {
        private VmMain m_vm;

        public MainPage(VmMain vm)
        {
            m_vm = vm;
            m_vm.OnStartSimulation += OnStartSimulation;
            m_vm.OnStopSimulation += OnStopSimulation;
            BindingContext = vm;


            InitializeComponent();
        }

        private void OnStopSimulation()
        {
            rendererView.Stop();
        }

        private void OnStartSimulation()
        {
            rendererView.Start();
        }
    }

}
