using System;

namespace TritonSim.GUI.Infrastructure
{
    [Flags]
    public enum RendererInitState
    {
        None = 0,

        TypeSet = 1 << 0,
        HandleSet = 1 << 1,
        SizeSet = 1 << 2,
        AttachedToVisualTree = 1 << 3,

        NativeInitialized = 1 << 5
    }
}
