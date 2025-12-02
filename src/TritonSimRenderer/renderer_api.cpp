#include "pch.h"
#include "renderer_api.h"
#include "RendererFactory.h"
#include "RendererBase.h"

static float gValue1 = 0.0f;
static float gValue2 = 0.0f;

ResponseCode init(const SimConfig& config, SimContext& ctx)
{
    ResponseCode result = RendererFactory::CreateRenderer(config, ctx);
    if (!result & RC_SUCCESS)
        return result;

    return ctx.Renderer->Init();
}

ResponseCode render_frame(const SimContext& ctx)
{
    if (ctx.Renderer == nullptr)
        return RC_RENDERER_NOT_INITIALIZED;

    return ctx.Renderer->RenderFrame();
}

ResponseCode shutdown(const SimContext& ctx)
{
    if (ctx.Renderer == nullptr)
        return RC_RENDERER_NOT_INITIALIZED;

    delete ctx.Renderer;

    return RC_SUCCESS;
}
