#pragma once
#include <cstdint>

// AUTO-GENERATED FROM enums.json at 2025-12-15 15:48:11
// HRESULT: [Sev:1][Res:5][Fac:11][Code:15]
enum ResponseCode : int32_t
{
    RC_SUCCESS = 0x00000000, // Code: 0
    RC_PARTIAL_SUCCESS = 0x00000001, // Code: 1
    RC_SUCCESS_NOT_IMPLEMENTED = 0x00000002, // Code: 2
    RC_FAILED = static_cast<int32_t>(0x80000000), // Code: 0
    RC_FAILED_UNKNOWN = static_cast<int32_t>(0x80000001), // Code: 1
    RC_FAILED_NATIVE_CALL = static_cast<int32_t>(0x80000002), // Code: 2
    RC_UNKNOWN_RENDERER_TYPE = static_cast<int32_t>(0x80010000), // Code: 0
    RC_RENDERER_NOT_INITIALIZED = static_cast<int32_t>(0x80010001), // Code: 1
    RC_INVALID_RENDER_SURFACE_HEIGHT = static_cast<int32_t>(0x80010002), // Code: 2
    RC_INVALID_RENDER_SURFACE_WIDTH = static_cast<int32_t>(0x80010003), // Code: 3
    RC_INVALID_RENDER_SURFACE_HANDLE = static_cast<int32_t>(0x80010004), // Code: 4
    RC_FAILED_OPEN_FILE = static_cast<int32_t>(0x80020000), // Code: 0
    RC_FAILED_TO_LOAD_RESOURCE_FILE = static_cast<int32_t>(0x80020001), // Code: 1
    RC_FAILED_TO_DETERMINE_DLL_MODULE_HANDLE = static_cast<int32_t>(0x80030000), // Code: 0
};

#define RC_IS_SUCCESS(x) ((x) >= 0)
#define RC_IS_FAILURE(x) ((x) < 0)
