#pragma once
#include "defs.h"

#pragma once

// --- Add this block ---
#if defined(_WIN32)
#define TRITON_EXPORT __declspec(dllexport)
#elif defined(__EMSCRIPTEN__)
#include <emscripten.h>
// EMSCRIPTEN_KEEPALIVE ensures the function is exported and not removed by the optimizer/linker.
#define TRITON_EXPORT EMSCRIPTEN_KEEPALIVE extern "C"
#else
#define TRITON_EXPORT __attribute__((visibility("default")))
#endif
// ----------------------

extern "C"
{
    TRITON_EXPORT int tritonsim_test();
    TRITON_EXPORT ResponseCode tritonsim_init(const SimConfig& config, SimContext& ctx);
    TRITON_EXPORT ResponseCode tritonsim_update_config(const SimContext& ctx, const SimConfig& config);
    TRITON_EXPORT ResponseCode tritonsim_render_frame(const SimContext& ctx);
    TRITON_EXPORT ResponseCode tritonsim_start(const SimContext& ctx);
    TRITON_EXPORT ResponseCode tritonsim_stop(const SimContext& ctx);
    TRITON_EXPORT ResponseCode tritonsim_shutdown(const SimContext& ctx);
}