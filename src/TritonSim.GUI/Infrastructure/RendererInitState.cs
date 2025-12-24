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

        NativeInitSuccess = 1 << 5,
        NativeInitFailed = 1 << 6,

        NativeInitCompleted = NativeInitSuccess | NativeInitFailed,
    }
}
