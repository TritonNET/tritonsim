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

        public uint BackgroundColor;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SimConfigX
    {
        public int x;
    }
}
