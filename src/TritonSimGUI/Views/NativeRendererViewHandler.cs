using Microsoft.Maui.Handlers;

namespace TritonSimGUI.Views
{
    public partial class NativeRendererViewHandler
    {
        public static IPropertyMapper<NativeRendererView, NativeRendererViewHandler> PropertyMapper =
            new PropertyMapper<NativeRendererView, NativeRendererViewHandler>(ViewHandler.ViewMapper);

        public static CommandMapper<NativeRendererView, NativeRendererViewHandler> CommandMapper =
            new CommandMapper<NativeRendererView, NativeRendererViewHandler>(ViewHandler.ViewCommandMapper)
            {
                [nameof(NativeRendererView.Start)] = (handler, view, args) => handler.StartVirtualView(),
                [nameof(NativeRendererView.Stop)] = (handler, view, args) => handler.StopVirtualView(),
            };

        public NativeRendererViewHandler() : base(PropertyMapper, CommandMapper) { }

        public void StartVirtualView() => StartPlatform();
        public void StopVirtualView() => StopPlatform();

        public partial void StartPlatform();
        public partial void StopPlatform();
    }
}