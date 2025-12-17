using Avalonia.Browser;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using TritonSim.GUI.Browser.Infrastructure;
using TritonSim.GUI.Providers;

namespace TritonSim.GUI.Browser.Providers
{
    public class BrowserCanvasHandle : JSObjectControlHandle, IPlatformCanvasHandle, IDisposable
    {
        private readonly IntPtr m_canvasHandle;

        private const string m_descriptor = "JSObject";

        public BrowserCanvasHandle(JSObject obj, string canvasID) : base(obj)
        {
            m_canvasHandle = Marshal.StringToHGlobalAnsi($"#{canvasID}");
        }

        public nint GetCanvasHandle()
        {
            return m_canvasHandle;
        }

        public void Dispose()
        {
            if (m_canvasHandle != IntPtr.Zero)
                Marshal.FreeHGlobal(m_canvasHandle);
        }
    }

    public class BrowserCanvasProvider : INativeCanvasProvider
    {
        private readonly string m_appParentID;

        private readonly string m_canvasID;

        private readonly ILogger m_logger;

        public BrowserCanvasProvider(string appParentID, ILogger logger)
        {
            m_appParentID = appParentID;
            m_canvasID = $"tritonsim-bgfx-canvas";
            m_logger = logger;
        }

        public bool Create(IntPtr parent, double width, double height, out IPlatformCanvasHandle handle)
        {
            if (!BrowserInterop.CreateCanvas(m_appParentID, m_canvasID, out var canvasObj))
            {
                m_logger.Error($"Creating Browser canvas failed. (Canvas ID: {m_canvasID})");
                handle = null;

                return false;
            }

            handle = new BrowserCanvasHandle(canvasObj, m_canvasID);

            return true;
        }

        public bool Destroy(IPlatformHandle handle)
        {
            var success = BrowserInterop.RemoveCanvas(m_canvasID);
            if (!success)
                m_logger.Error($"Removing render canvas from dom failed. (Canvas ID: {m_canvasID})");

            if (handle is IDisposable disposable)
                disposable.Dispose();

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
