using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace TritonSim.GUI.Browser.Infrastructure
{
    [SupportedOSPlatform("browser")]
    public static partial class BrowserInterop
    {
        private const string MODULE_NAME = "canvasProviderModule";

        [JSImport("createCanvas", MODULE_NAME)]
        [return: JSMarshalAs<JSType.Object>]
        private static partial JSObject? CreateCanvasInternal(string parentId, string canvasId, double width, double height);

        public static bool CreateCanvas(string parentId, string canvasId, double width, double height, out JSObject? canvas)
        {
            canvas = CreateCanvasInternal(parentId, canvasId, width, height);
            return canvas != null;
        }

        [JSImport("updatePosition", MODULE_NAME)]
        [return: JSMarshalAs<JSType.Boolean>]
        public static partial bool UpdatePosition(
            string canvasId,
            double x,
            double y,
            double width,
            double height);

        [JSImport("removeCanvas", MODULE_NAME)]
        [return: JSMarshalAs<JSType.Boolean>]
        public static partial bool RemoveCanvas(string canvasId);
    }
}
