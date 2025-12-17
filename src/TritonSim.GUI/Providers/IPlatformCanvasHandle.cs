using Avalonia.Platform;
using System;

namespace TritonSim.GUI.Providers
{
    public interface IPlatformCanvasHandle : IPlatformHandle
    {
        IntPtr GetCanvasHandle();
    }
}
