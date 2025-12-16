using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;
using TritonSim.GUI.Browser.Infrastructure;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Browser.Providers
{
    public class BrowserCanvasProvider : INativeCanvasProvider
    {
        private readonly string m_appParentID;

        private readonly string m_canvasID;

        private readonly ILogger m_logger;

        public BrowserCanvasProvider(string appParentID, ILogger logger)
        {
            m_appParentID = appParentID;
            m_canvasID = $"tritonsim-{Guid.NewGuid():N}";
            m_logger = logger;
        }

        public bool Create(IntPtr parent, double width, double height, out IPlatformHandle handle)
        {
            var ptr = IntPtr.Zero;

            var success = BrowserInterop.CreateCanvas(m_appParentID, m_canvasID);

            if (!success)
                m_logger.Error($"Creating Browser canvas failed. (Canvas ID: {m_canvasID})");

            if (success)
                ptr = Marshal.StringToHGlobalAnsi($"#{m_canvasID}");

            handle = new PlatformHandle(ptr, "BrowserCanvasHandle");

            return success;
        }

        public bool Destroy(IPlatformHandle handle)
        {
            var success = BrowserInterop.RemoveCanvas(m_canvasID);
            if (!success)
                m_logger.Error($"Removing render canvas from dom failed. (Canvas ID: {m_canvasID})");

            return success;
        }

        public bool UpdatePosition(double x, double y, double width, double height)
        {
            var success = BrowserInterop.UpdatePosition(m_canvasID, x, y, width, height);
            if (!success)
                m_logger.Error($"Updating canvas position failed (Canvas ID: {m_canvasID})");

            return success;
        }
    }
}
