namespace TritonSimGUI
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public App(IServiceProvider provider)
        {
            InitializeComponent();
            Services = provider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var mainPage = Services.GetRequiredService<MainPage>();

            var window = new Window(mainPage);
            window.Title = "TritonSimGUI";

            CustomizeWindow(window);

            return window;
        }
        partial void CustomizeWindow(Window window);
    }
}