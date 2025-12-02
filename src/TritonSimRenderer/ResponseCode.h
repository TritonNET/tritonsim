#pragma once

enum ResponseCode
{
    // FAILURE FLAGS (negative mask)
    RC_FAILED = 0x80000000,   // Top bit = indicates failure
    RC_UNKNOWN_RENDERER_TYPE = RC_FAILED | 0x01,
    RC_RENDERER_NOT_INITIALIZED = RC_FAILED | 0x02,

    // SUCCESS FLAGS (positive mask)
    RC_SUCCESS = 0x00000001,
    //RC_PARTIAL_SUCCESS = 0x00000002
};