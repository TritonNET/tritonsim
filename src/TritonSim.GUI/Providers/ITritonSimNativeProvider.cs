using Avalonia;
using System;
using TritonSim.GUI.Infrastructure;

namespace TritonSim.GUI.Providers
{
    public interface ITritonSimNativeProvider
    {
        bool Init();

        bool RenderFrame();

        bool Start();

        bool Stop();

        bool Shutdown();

        bool SetWindowHandle(IntPtr handle);

        bool SetType(RendererType type);

        bool SetSize(Size size);

        SimulationMode GetMode();

        string GetLastError();
    }
}
