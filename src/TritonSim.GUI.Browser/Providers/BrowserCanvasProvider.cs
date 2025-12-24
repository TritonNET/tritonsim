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
            m_canvasID = $"tbgfxcs";
            m_logger = logger;
        }

        public bool Create(IntPtr parent, double width, double height, out IPlatformCanvasHandle? handle)
        {
            m_logger.Debug($"Creating Browser canvas. (Canvas ID: {m_canvasID}, Parent ID: {m_appParentID}, Width: {width}, Height: {height})");

            if (!BrowserInterop.CreateCanvas(m_appParentID, m_canvasID, width, height, out var canvasObj))
            {
                m_logger.Error($"Creating Browser canvas failed. (Canvas ID: {m_canvasID})");
                handle = null;
            
                return false;
            }

            handle = new BrowserCanvasHandle(canvasObj, m_canvasID);

            m_logger.Debug($"Browser canvas created successfully. (Canvas ID: {m_canvasID})");

            return true;
        }

        public bool Destroy(IPlatformHandle handle)
        {
            m_logger.Debug($"Removing Browser canvas from DOM. (Canvas ID: {m_canvasID})");

            var success = BrowserInterop.RemoveCanvas(m_canvasID);
            if (!success)
            {
                m_logger.Error($"Removing render canvas from dom failed. (Canvas ID: {m_canvasID})");
                return false;
            }

            if (handle is IDisposable disposable)
                disposable.Dispose();

            m_logger.Debug($"Browser canvas removed from DOM successfully. (Canvas ID: {m_canvasID})");

            return success;
        }

        public bool UpdatePosition(double x, double y, double width, double height)
        {
            m_logger.Debug($"Updating canvas position. (Canvas ID: {m_canvasID}, X: {x}, Y: {y}, Width: {width}, Height: {height})");

            var success = BrowserInterop.UpdatePosition(m_canvasID, x, y, width, height);
            if (!success)
            {
                m_logger.Error($"Updating canvas position failed (Canvas ID: {m_canvasID})");
                return false;
            }

            m_logger.Debug($"Canvas position updated successfully. (Canvas ID: {m_canvasID})");

            return success;
        }
    }
}
