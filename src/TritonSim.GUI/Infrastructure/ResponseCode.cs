using System;

namespace TritonSim.GUI.Infrastructure
{
    [Flags]
    public enum ResponseCode : int
    {
        // Standard: 0 is Success (S_OK)
        Success = 0,

        // Positive values for "Success with issues" (Warnings)
        PartialSuccess = 1,

        // Negative values for Failures (Bit 31 set)
        // unchecked is required because 0x80000000 overflows signed int
        Failed = unchecked((int)0x80000000),

        // Specific Failures (Failed Bit + Unique ID)
        // Note: We intentionally skip 0x01 to avoid confusion with PartialSuccess, 
        // though strictly strictly speaking, the 'Failed' bit makes them distinct values.
        UnknownRendererType = Failed | 0x04,
        RendererNotInitialized = Failed | 0x05,
        FailedOpenFile = Failed | 0x06,

        InvalidRenderSurfaceHeight = Failed | 0x07,
        InvalidRenderSurfaceWidth = Failed | 0x08,
        InvalidRenderSurfaceHandle = Failed | 0x09,
    }
}
