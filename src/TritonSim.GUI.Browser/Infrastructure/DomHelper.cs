using System.Runtime.InteropServices.JavaScript;

namespace TritonSim.GUI.Browser.Infrastructure
{
    public static partial class DomHelper
    {
        [JSImport("globalThis.document.createElement")]
        private static partial JSObject CreateElement(string tagName);

        public static JSObject CreateContainer(string id)
        {
            var div = CreateElement("div");

            div.SetProperty("id", id);

            // Ensure it fills the container
            var style = div.GetPropertyAsJSObject("style");
            style?.SetProperty("width", "100%");
            style?.SetProperty("height", "100%");
            style?.SetProperty("backgroundColor", "transparent"); // Let Avalonia bg show through if needed

            return div;
        }
    }
}
