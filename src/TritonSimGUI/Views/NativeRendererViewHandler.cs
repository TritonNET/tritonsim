using Microsoft.Maui.Handlers;

namespace TritonSimGUI.Views
{
    // The base class and implementation are defined in the Platforms/Windows directory.
    // This partial class connects the shared code to the platform-specific implementation.
    public partial class NativeRendererViewHandler
    {
        public static IPropertyMapper<NativeRendererView, NativeRendererViewHandler> PropertyMapper =
            new PropertyMapper<NativeRendererView, NativeRendererViewHandler>(ViewHandler.ViewMapper);

        public NativeRendererViewHandler() : base(PropertyMapper) { }
    }
}