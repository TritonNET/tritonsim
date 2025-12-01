using Microsoft.Maui.Handlers;

namespace TritonSimGUI.Views
{
    // Fix: Explicitly use 'Android.Views.View' for the second generic argument.
    // This tells the compiler strictly to use the Android Native View, not the MAUI View.
    public partial class NativeRendererViewHandler : ViewHandler<NativeRendererView, Android.Views.View>
    {
        protected override Android.Views.View CreatePlatformView()
        {
            // Return the native Android view. 
            // 'Context' is available because we are inside a Maui ViewHandler on Android.
            return new Android.Views.View(Context);
        }
    }
}