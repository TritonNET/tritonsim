#pragma once
#include "defs.h"

#pragma once

// --- Add this block ---
#if defined(_WIN32)
#define TRITON_EXPORT __declspec(dllexport)
#elif defined(__EMSCRIPTEN__)
#include <emscripten.h>
// EMSCRIPTEN_KEEPALIVE ensures the function is exported and not removed by the optimizer/linker.
#define TRITON_EXPORT EMSCRIPTEN_KEEPALIVE
#else
#define TRITON_EXPORT __attribute__((visibility("default")))
#endif
// ----------------------

extern "C"
{
    TRITON_EXPORT ResponseCode init(const SimConfig& config, SimContext& ctx);
    TRITON_EXPORT ResponseCode update_config(const SimContext& ctx, const SimConfig& config);
    TRITON_EXPORT ResponseCode render_frame(const SimContext& ctx);
    TRITON_EXPORT ResponseCode start(const SimContext& ctx);
    TRITON_EXPORT ResponseCode stop(const SimContext& ctx);
    TRITON_EXPORT ResponseCode shutdown(const SimContext& ctx);
}