#include "pch.h"
#include "RendererFactory.h"
#include "RendererTest.h"
#include "RendererGameOfLife.h"

ResponseCode RendererFactory::CreateRenderer(const SimConfig& config, SimContext& ctx)
{
	switch (config.Type)
	{
	case RT_TEST:
		ctx.Renderer = new RendererTest(config);
		break;
	case RT_GAMEOFLIFE:
		ctx.Renderer = new RendererGameOfLife(config);
		break;
	case RT_UNKNOWN:
	default:
		ctx.Renderer = nullptr;
		return RC_UNKNOWN_RENDERER_TYPE;
	}

	return RC_SUCCESS;
}