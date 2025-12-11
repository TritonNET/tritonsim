using System;
using System.Runtime.InteropServices;

namespace TritonSim.GUI.Infrastructure
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SimContext
    {
        public IntPtr Renderer;

        public bool IsInitialized() => Renderer != IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SimContextX
    {
        
    }
}
