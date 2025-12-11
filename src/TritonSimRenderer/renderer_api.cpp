#include "pch.h"
#include "renderer_api.h"
#include "RendererFactory.h"
#include "RendererBase.h"

static float gValue1 = 0.0f;
static float gValue2 = 0.0f;

TRITON_EXPORT int tritonsim_test()
{
    return 55;
}

TRITON_EXPORT ResponseCode tritonsim_init(const SimConfig& config, SimContext& ctx)
{
    ResponseCode result = RendererFactory::CreateRenderer(config, ctx);
    if (result & RC_FAILED)
        return result;

    return ctx.Renderer->Init();
}

TRITON_EXPORT ResponseCode tritonsim_update_config(const SimContext& ctx, const SimConfig& config)
{
    if (ctx.Renderer == nullptr)
        return RC_RENDERER_NOT_INITIALIZED;

    return ctx.Renderer->UpdateConfig(config);
}

TRITON_EXPORT ResponseCode tritonsim_render_frame(const SimContext& ctx)
{
    if (ctx.Renderer == nullptr)
        return RC_RENDERER_NOT_INITIALIZED;

    return ctx.Renderer->RenderFrame();
}

TRITON_EXPORT ResponseCode tritonsim_start(const SimContext& ctx)
{
    if (ctx.Renderer == nullptr)
        return RC_RENDERER_NOT_INITIALIZED;

    return ctx.Renderer->Start();
}

TRITON_EXPORT ResponseCode tritonsim_stop(const SimContext& ctx)
{
    if (ctx.Renderer == nullptr)
        return RC_RENDERER_NOT_INITIALIZED;

    return ctx.Renderer->Stop();
}

TRITON_EXPORT ResponseCode tritonsim_shutdown(const SimContext& ctx)
{
    if (ctx.Renderer == nullptr)
        return RC_RENDERER_NOT_INITIALIZED;

    delete ctx.Renderer;

    return RC_SUCCESS;
}
