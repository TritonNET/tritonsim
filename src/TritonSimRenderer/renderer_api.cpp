#include "pch.h"
#include "renderer_api.h"
#include "RendererFactory.h"
#include "RendererBase.h"

ResponseCode tritonsim_init_internal(const SimConfig* config, SimContext* ctx)
{
    LOG_DEBUG("Running on pthread: %s", emscripten_is_main_browser_thread() ? "MAIN" : "WORKER");

    ResponseCode rc = RendererFactory::CreateRenderer(*config, *ctx);
    if (rc & RC_FAILED) 
        return rc;

    LOG_DEBUG("CreateRenderer response: %d", rc);

    rc = ctx->Renderer->Init();

    return rc;
}

int tritonsim_init_trampoline(int configPtr, int ctxPtr)
{
	LOG_DEBUG("Config ptr: %d, Context ptr: %d", configPtr, ctxPtr);

    const SimConfig* config = reinterpret_cast<const SimConfig*>(configPtr);
    SimContext* ctx = reinterpret_cast<SimContext*>(ctxPtr);

    ResponseCode rc = tritonsim_init_internal(config, ctx);

    int rci = static_cast<int>(rc);

    LOG_DEBUG("Response %d", rci);

    return rci;
}

TRITON_EXPORT ResponseCode tritonsim_init(const SimConfig& config, SimContext& ctx)
{
    LOG_DEBUG_CONFIG(config);

    const int rc = emscripten_sync_run_in_main_runtime_thread(
        EM_FUNC_SIG_III,
        tritonsim_init_trampoline,
        reinterpret_cast<int>(&config),
        reinterpret_cast<int>(&ctx)
    );

    LOG_DEBUG("Init response: %d", rc);

	return static_cast<ResponseCode>(rc);
}

//TRITON_EXPORT ResponseCode tritonsim_init(const SimConfig& config, SimContext& ctx)
//{
//    LOG_DEBUG_CONFIG(config);
//
//    ResponseCode rc = RendererFactory::CreateRenderer(config, ctx);
//
//    LOG_DEBUG("CreateRenderer response: %d", rc);
//
//    if (rc & RC_FAILED)
//        return rc;
//
//    rc = ctx.Renderer->Init();
//
//    LOG_DEBUG("Init response: %d", rc);
//
//    return rc;
//}

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
