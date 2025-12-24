using Avalonia.Platform;
using System;

namespace TritonSim.GUI.Providers
{
    public interface INativeCanvasProvider
    {
        bool Create(IntPtr parent, double width, double height, out IPlatformCanvasHandle? handle);

        bool UpdatePosition(double x, double y, double width, double height);

        bool Destroy(IPlatformHandle handle);
    }
}
