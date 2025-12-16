using System.ComponentModel;

namespace TritonSim.GUI.Infrastructure;

// AUTO-GENERATED FROM enums.json at 2025-12-16 19:27:11
public enum ResponseCode : int
{
    [Description("Operation successful.")]
    Success = 0x00000000,

    [Description("Operation successful, but with some issues.")]
    PartialSuccess = 0x00000001,

    [Description("Success, but the requested functionality is not fully implemented.")]
    SuccessNotImplemented = 0x00000002,

    [Description("General or unspecified failure.")]
    Failed = unchecked((int)0x80000000),

    [Description("An unknown error occurred.")]
    FailedUnknown = unchecked((int)0x80000001),

    [Description("The native function call failed.")]
    FailedNativeCall = unchecked((int)0x80000002),

    [Description("The specified renderer type is not recognized.")]
    UnknownRendererType = unchecked((int)0x80010000),

    [Description("The renderer has not been initialized.")]
    RendererNotInitialized = unchecked((int)0x80010001),

    [Description("The render surface height is invalid.")]
    InvalidRenderSurfaceHeight = unchecked((int)0x80010002),

    [Description("The render surface width is invalid.")]
    InvalidRenderSurfaceWidth = unchecked((int)0x80010003),

    [Description("The handle provided for the render surface is invalid.")]
    InvalidRenderSurfaceHandle = unchecked((int)0x80010004),

    [Description("Failed to open the specified file.")]
    FailedOpenFile = unchecked((int)0x80020000),

    [Description("Failed to load the requested resource file.")]
    FailedToLoadResourceFile = unchecked((int)0x80020001),

    [Description("Could not determine the handle for the DLL module.")]
    FailedToDetermineDllModuleHandle = unchecked((int)0x80030000),
}
