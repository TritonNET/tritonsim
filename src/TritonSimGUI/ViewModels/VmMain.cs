using System.Windows.Input;
using TritonSimGUI.Infrastructure;

namespace TritonSimGUI.ViewModels
{
    public delegate void StartEventHandler();
    public delegate void StopEventHandler();

    public class VmMain: VmBase
    {
        public event StartEventHandler OnStartSimulation;
        public event StopEventHandler OnStopSimulation;

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        private bool m_isrunning;
        public bool IsRunning
        {
            get => m_isrunning;
            set => SetProperty(ref m_isrunning, value);
        }

        public VmMain()
        {
            m_isrunning = false;

            StartCommand = new AsyncRelayCommand(OnStartAsync);
            StopCommand = new AsyncRelayCommand(OnStopAsync);
        }

        private Task OnStartAsync()
        {
            IsRunning = !IsRunning;

            OnStartSimulation();

            return Task.CompletedTask;
        }

        private Task OnStopAsync()
        {
            IsRunning = !IsRunning;

            OnStopSimulation();

            return Task.CompletedTask;
        }
    }
}
