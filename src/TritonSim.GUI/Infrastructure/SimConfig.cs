using System;
using System.Runtime.InteropServices;

namespace TritonSim.GUI.Infrastructure
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SimConfig
    {
        public IntPtr Handle;

        public int Width;
        
        public int Height;

        public RendererType Type;

        public UInt32 BackgroundColor;
    }
}
