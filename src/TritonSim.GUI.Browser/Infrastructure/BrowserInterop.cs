using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace TritonSim.GUI.Browser.Infrastructure
{
    [SupportedOSPlatform("browser")]
    public static partial class BrowserInterop
    {
        [JSImport("createCanvas", "canvasProviderModule")]
        [return: JSMarshalAs<JSType.Boolean>]
        public static partial bool CreateCanvas(string parentId, string divId);

        [JSImport("updatePosition", "canvasProviderModule")]
        [return: JSMarshalAs<JSType.Boolean>]
        public static partial bool UpdatePosition(
            string divId,
            double x,
            double y,
            double width,
            double height);

        [JSImport("removeCanvas", "canvasProviderModule")]
        [return: JSMarshalAs<JSType.Boolean>]
        public static partial bool RemoveCanvas(string divId);
    }
}
