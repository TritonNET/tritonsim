using TritonSimGUI.Controls;
using TritonSimGUI.Infrastructure;
using Microsoft.UI.Xaml;

#if WINDOWS
using System.Runtime.InteropServices;
#endif

namespace TritonSimGUI.Views;

public partial class TritonSimView : ContentView
{
    private bool m_initialized = false;
    private IDispatcherTimer m_dispatcherTimer;

    public TritonSimView()
	{
        m_dispatcherTimer = Dispatcher.CreateTimer();
        m_dispatcherTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
        m_dispatcherTimer.Tick += OnRenderTick;

        InitializeComponent();
        Loaded += TritonSimView_Loaded;
    }

    private void TritonSimView_Loaded(object? sender, EventArgs e)
    {
#if WINDOWS
        var hwnd = ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView).WindowHandle;

        var size = this.GetSize();

        int ok = TritonSimNative.tritonsim_init(
            hwnd,
            size.width,
            size.height);

        if (ok == 0)
            throw new Exception("bgfx init failed");
#endif
        m_initialized = true;

        m_dispatcherTimer.Start();
    }

    private void OnRenderTick(object sender, EventArgs e)
    {
        if (!m_initialized)
            return;

#if WINDOWS
        TritonSimNative.tritonsim_render_frame(0);
#endif
    }
}