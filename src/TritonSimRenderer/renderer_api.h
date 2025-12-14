#pragma once
#include "defs.h"

extern "C"
{
    TRITON_EXPORT ResponseCode tritonsim_init(const SimConfig& config, SimContext& ctx);
    TRITON_EXPORT ResponseCode tritonsim_update_config(const SimContext& ctx, const SimConfig& config);
    TRITON_EXPORT ResponseCode tritonsim_render_frame(const SimContext& ctx);
    TRITON_EXPORT ResponseCode tritonsim_start(const SimContext& ctx);
    TRITON_EXPORT ResponseCode tritonsim_stop(const SimContext& ctx);
    TRITON_EXPORT ResponseCode tritonsim_shutdown(const SimContext& ctx);
}