using Microsoft.Maui.Handlers;

namespace TritonSimGUI.Views
{
    public partial class NativeRendererViewHandler : ViewHandler<NativeRendererView, Android.Views.View>
    {
        protected override Android.Views.View CreatePlatformView()
        {
            // Return the native Android view. 
            // 'Context' is available because we are inside a Maui ViewHandler on Android.
            return new Android.Views.View(Context);
        }

        public partial void StartPlatform()
        {
        }

        public partial void StopPlatform()
        {
        }
    }
}