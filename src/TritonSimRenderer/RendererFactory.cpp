#include "pch.h"
#include "RendererFactory.h"
#include "RendererTestColorChanging.h"
#include "RendererTestEdges.h"
#include "RendererGameOfLife.h"
#include "RendererBouncingCircle.h"

ResponseCode RendererFactory::CreateRenderer(const SimConfig& config, SimContext& ctx)
{
	if (config.height <= 0)
		return RC_INVALID_RENDER_SURFACE_HEIGHT;

	if (config.width <= 0)
		return RC_INVALID_RENDER_SURFACE_WIDTH;

	if (config.Type == RT_UNKNOWN)
		return RC_UNKNOWN_RENDERER_TYPE;

	if (config.handle == nullptr)
		return RC_INVALID_RENDER_SURFACE_HANDLE;

	switch (config.Type)
	{
	case RT_TEST_COLOR_CHANGING:
		ctx.Renderer = new RendererTestColorChanging(config);
		break;
	case RT_TEST_EDGES:
		ctx.Renderer = new RendererTestEdges(config);
		break;
	case RT_GAMEOFLIFE2D:
		ctx.Renderer = new RendererGameOfLife(config);
		break;
	case RT_TEST_BOUNCING_CIRCLE:
		ctx.Renderer = new RendererBouncingCircle(config);
		break;
	case RT_UNKNOWN:
	default:
		ctx.Renderer = nullptr;
		return RC_UNKNOWN_RENDERER_TYPE;
	}

	return RC_SUCCESS;
}