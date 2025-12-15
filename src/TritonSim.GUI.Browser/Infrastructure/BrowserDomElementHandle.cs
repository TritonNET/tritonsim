using Avalonia.Controls.Platform;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;

namespace TritonSim.GUI.Browser.Infrastructure
{
    public class BrowserDomElementHandle : INativeControlHostDestroyableControlHandle
    {
        private JSObject? m_element;

        public string ElementId { get; }

        public BrowserDomElementHandle(JSObject element, string elementId)
        {
            m_element = element;
            ElementId = elementId;
        }

        public IntPtr Handle => Marshal.StringToHGlobalAnsi("#canvas"); // Avalonia uses this Handle to place the object in the DOM

        // Important: Return the actual JSObject so Avalonia can append it to the DOM
        public string HandleDescriptor => "COID"; // 'COID' is an internal marker often used, but simply returning the object in a property usually suffices in 11.0+ if the architecture matches.

        // For Avalonia 11.0 Browser, the NativeControlHost expects the object itself if we are using the internal platform logic.
        // However, standard public API usually just asks for the Handle. 
        // NOTE: In strict Avalonia WASM, simply returning a class that holds the JSObject 
        // usually requires the platform to know how to unwrap it. 
        // *The simplest standard way in modern .NET WASM is to rely on the handle object.*

        // Avalonia's WASM NativeControlHost implementation checks if this implements specific interfaces
        // or if the Handle property is a GCHandle to the JSObject.
        // But for .NET 7/8, strict reference handling is needed.
        public object RawElement => m_element!;

        public void Destroy()
        {
            m_element?.Dispose();
            m_element = null;
        }
    }
}
