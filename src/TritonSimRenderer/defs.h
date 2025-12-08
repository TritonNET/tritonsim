#pragma once
#include "ResponseCode.h"
#include "RendererType.h"

class RendererBase;  // forward declare

struct SimConfig
{
    void* handle;
    int width;
    int height;
    RendererType Type;
    int BackgroundColor;
};

struct SimContext
{
    RendererBase* Renderer{};
};

enum COLOR : uint32_t
{
    // Format: 0xAABBGGRR
    COLOR_BLACK = 0xFF000000,
    COLOR_WHITE = 0xFFFFFFFF,

    // R and B are swapped compared to your list
    COLOR_RED = 0xFF0000FF, // R=FF (LSB)
    COLOR_GREEN = 0xFF00FF00,
    COLOR_BLUE = 0xFFFF0000, // B=FF (High byte of low word)

    // Yellow (R+G) and Cyan (G+B)
    COLOR_YELLOW = 0xFF00FFFF, // R=FF, G=FF, B=00
    COLOR_CYAN = 0xFFFF00FF, // R=00, G=FF, B=FF
    COLOR_MAGENTA = 0xFFFF00FF, // R=FF, B=FF (Symmetric, same as before)

    COLOR_GRAY = 0xFF808080,

    // Orange: Red=FF, Green=A5, Blue=00
    // ABGR: 0xFF(Alpha) 00(Blue) A5(Green) FF(Red)
    COLOR_ORANGE = 0xFF00A5FF,
};