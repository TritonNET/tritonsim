using System;

namespace TritonSim.GUI.Infrastructure
{
    [Flags]
    public enum ResponseCode : int
    {
        // FAILURE FLAG (top bit)
        Failed = unchecked((int)0x80000000),

        // Specific Failures
        UnknownRendererType = Failed | 0x01,
        RendererNotInitialized = Failed | 0x02,

        // SUCCESS FLAG
        Success = 0x00000001,
        PartialSuccess = 0x00000002,
    }
}
