#pragma once
#include <cstdint>

// Enforce int32_t to match C# 'int' (System.Int32)
enum ResponseCode : int32_t
{
    // Standard: 0 is Success
    RC_SUCCESS = 0,

    // Positive values for "Success with issues" (Warnings)
    RC_PARTIAL_SUCCESS = 1,

    // FAILURE FLAGS (negative mask)
    // We static_cast to force the unsigned hex literal into a signed int representation
    RC_FAILED = static_cast<int32_t>(0x80000000),

    // Specific Failures
    // These must match the IDs used in your C# Enum exactly
    RC_UNKNOWN_RENDERER_TYPE = RC_FAILED | 0x04,
    RC_RENDERER_NOT_INITIALIZED = RC_FAILED | 0x05,
    RC_FAILED_OPEN_FILE = RC_FAILED | 0x06,

    RC_INVALID_RENDER_SURFACE_HEIGHT = RC_FAILED | 0x07,
    RC_INVALID_RENDER_SURFACE_WIDTH = RC_FAILED | 0x08,
    RC_INVALID_RENDER_SURFACE_HANDLE = RC_FAILED | 0x09,
    RC_FAILED_TO_LOAD_RESOURCE_FILE = RC_FAILED | 0x10,

    RC_FAILED_TO_DETERMINE_DLL_MODULE_HANDLE = RC_FAILED | 0x11,
};

// Optional: Helper macros or inline functions for C++ logic
inline bool RC_IS_SUCCESS(ResponseCode rc) { return rc >= 0; }
inline bool RC_IS_FAILURE(ResponseCode rc) { return rc < 0; }