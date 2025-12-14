#include "pch.h"
#include "renderer_api.h"
#include "RendererFactory.h"
#include "RendererBase.h"

TRITON_EXPORT ResponseCode tritonsim_init(const SimConfig& config, SimContext& ctx)
{
    LOG_DEBUG_CONFIG(config);

    ResponseCode rc = RendererFactory::CreateRenderer(config, ctx);

    LOG_DEBUG("CreateRenderer response: %d", rc);

    if (rc & RC_FAILED)
        return rc;

    rc = ctx.Renderer->Init();

    LOG_DEBUG("Init response: %d", rc);

    return rc;
}

TRITON_EXPORT ResponseCode tritonsim_update_config(const SimContext& ctx, const SimConfig& config)
{   
    LOG_DEBUG_CONTEXT(ctx);
    LOG_DEBUG_CONFIG(config);

    if (ctx.Renderer == nullptr)
        return RC_RENDERER_NOT_INITIALIZED;

    auto rc = ctx.Renderer->UpdateConfig(config);

    LOG_DEBUG("UpdateConfig response: %d", rc);

    return rc;
}

TRITON_EXPORT ResponseCode tritonsim_render_frame(const SimContext& ctx)
{
    if (ctx.Renderer == nullptr)
        return RC_RENDERER_NOT_INITIALIZED;

    return ctx.Renderer->RenderFrame();
}

TRITON_EXPORT ResponseCode tritonsim_start(const SimContext& ctx)
{
    LOG_DEBUG_CONTEXT(ctx);

    if (ctx.Renderer == nullptr)
        return RC_RENDERER_NOT_INITIALIZED;

    auto rc = ctx.Renderer->Start();

    LOG_DEBUG("Start response: %d", rc);

    return rc;
}

TRITON_EXPORT ResponseCode tritonsim_stop(const SimContext& ctx)
{
    LOG_DEBUG_CONTEXT(ctx);

    if (ctx.Renderer == nullptr)
        return RC_RENDERER_NOT_INITIALIZED;

    auto rc = ctx.Renderer->Stop();

    LOG_DEBUG("Stop response: %d", rc);

    return rc;
}

TRITON_EXPORT ResponseCode tritonsim_shutdown(const SimContext& ctx)
{
    LOG_DEBUG_CONTEXT(ctx);

    if (ctx.Renderer == nullptr)
        return RC_RENDERER_NOT_INITIALIZED;

    delete ctx.Renderer;

    return RC_SUCCESS;

}
