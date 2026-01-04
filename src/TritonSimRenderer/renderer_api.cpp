#include "pch.h"
#include "renderer_api.h"
#include "RendererFactory.h"
#include "RendererBase.h"
#include "internal_dispatch.h"

ResponseCode tritonsim_init_internal(const SimConfig* config, SimContext* ctx) 
{
    ASSERT_MAIN_THREAD();

    ResponseCode rc = RendererFactory::CreateRenderer(*config, *ctx);
    if (rc & RC_FAILED) return rc;

    LOG_DEBUG("CreateRenderer response: %d", rc);
    
    return ctx->Renderer->Init();
}

ResponseCode tritonsim_update_config_internal(const SimContext* ctx, const SimConfig* config) 
{
    ASSERT_MAIN_THREAD();

    if (ctx->Renderer == nullptr) return RC_RENDERER_NOT_INITIALIZED;

    return ctx->Renderer->UpdateConfig(*config);
}

ResponseCode tritonsim_render_frame_internal(const SimContext* ctx) 
{
    ASSERT_MAIN_THREAD();

    if (!ctx) return RC_INVALID_RENDER_CONTEXT;
    
    if (ctx->Renderer == nullptr) return RC_RENDERER_NOT_INITIALIZED;
    
    return ctx->Renderer->RenderFrame();
}

ResponseCode tritonsim_start_internal(const SimContext* ctx) 
{
    ASSERT_MAIN_THREAD();

    if (ctx->Renderer == nullptr) return RC_RENDERER_NOT_INITIALIZED;
    
    return ctx->Renderer->Start();
}

ResponseCode tritonsim_stop_internal(const SimContext* ctx) 
{
    ASSERT_MAIN_THREAD();
    
    if (ctx->Renderer == nullptr) return RC_RENDERER_NOT_INITIALIZED;
 
    return ctx->Renderer->Stop();
}

ResponseCode tritonsim_shutdown_internal(const SimContext* ctx) 
{
    ASSERT_MAIN_THREAD();

    if (ctx->Renderer == nullptr) return RC_RENDERER_NOT_INITIALIZED;
    
    delete ctx->Renderer;
    
    return RC_SUCCESS;
}

TRITON_EXPORT ResponseCode tritonsim_init(const SimConfig& config, SimContext& ctx)
{
    LOG_DEBUG_CONFIG(config);

    ResponseCode rc = Dispatch(EM_FUNC_SIG_III, tritonsim_init_internal, &config, &ctx);

    if (ctx.Renderer == nullptr)
        return RC_FAILED_INITIALIZE_RENDERER_INVALID_HANDLE;

    LOG_DEBUG("Init response: %d", rc);
    return rc;
}

TRITON_EXPORT ResponseCode tritonsim_update_config(const SimContext& ctx, const SimConfig& config)
{
    LOG_DEBUG_CONTEXT(ctx);
    LOG_DEBUG_CONFIG(config);

    ResponseCode rc = Dispatch(EM_FUNC_SIG_III, tritonsim_update_config_internal, &ctx, &config);

    LOG_DEBUG("UpdateConfig response: %d", rc);
    return rc;
}

TRITON_EXPORT ResponseCode tritonsim_render_frame(const SimContext& ctx)
{
    return Dispatch(EM_FUNC_SIG_II, tritonsim_render_frame_internal, &ctx);
}

TRITON_EXPORT ResponseCode tritonsim_start(const SimContext& ctx)
{
    LOG_DEBUG_CONTEXT(ctx);
    ResponseCode rc = Dispatch(EM_FUNC_SIG_II, tritonsim_start_internal, &ctx);
    LOG_DEBUG("Start response: %d", rc);
    return rc;
}

TRITON_EXPORT ResponseCode tritonsim_stop(const SimContext& ctx)
{
    LOG_DEBUG_CONTEXT(ctx);
    ResponseCode rc = Dispatch(EM_FUNC_SIG_II, tritonsim_stop_internal, &ctx);
    LOG_DEBUG("Stop response: %d", rc);
    return rc;
}

TRITON_EXPORT ResponseCode tritonsim_shutdown(const SimContext& ctx)
{
    LOG_DEBUG_CONTEXT(ctx);
    return Dispatch(EM_FUNC_SIG_II, tritonsim_shutdown_internal, &ctx);
}