namespace TritonSimGUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new MainPage());
            window.Title = "TritonSimGUI";

            CustomizeWindow(window);

            return window;
        }
        partial void CustomizeWindow(Window window);
    }
}