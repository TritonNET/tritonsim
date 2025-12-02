#pragma once
#include "defs.h"

extern "C"
{
    __declspec(dllexport) ResponseCode init(const SimConfig& config, SimContext& ctx);
    __declspec(dllexport) ResponseCode render_frame(const SimContext& ctx);
    __declspec(dllexport) ResponseCode shutdown(const SimContext& ctx);
}