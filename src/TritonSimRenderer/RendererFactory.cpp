#include "pch.h"
#include "RendererFactory.h"
#include "RendererTestColorChanging.h"
#include "RendererTestEdges.h"
#include "RendererGameOfLife2D.h"
#include "RendererBouncingCircle.h"
#include "RendererNeonPulse.h"
#include "resource.h"

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

	ShaderPacker* sp{nullptr};
	const auto rc = CreateShaderPacker(&sp);
	if (rc & RC_FAILED)
		return rc;

	switch (config.Type)
	{
	// Test Renderes
	case RT_TEST_BOUNCING_CIRCLE:
		ctx.Renderer = new RendererBouncingCircle(sp, config);
		break;
	case RT_TEST_COLOR_CHANGING:
		ctx.Renderer = new RendererTestColorChanging(sp, config);
		break;
	case RT_TEST_EDGES:
		ctx.Renderer = new RendererTestEdges(sp, config);
		break;

	/* 2D Renderes*/
	case RT_GAMEOFLIFE2D:
		ctx.Renderer = new RendererGameOfLife2D(sp, config);
		break;

	/* 3D Renderes*/
	case RT_NEONPULSE3D:
		ctx.Renderer = new RendererNeonPulse(sp, config);
		break;

	case RT_UNKNOWN:
	default:
		ctx.Renderer = nullptr;
		delete sp;
		return RC_UNKNOWN_RENDERER_TYPE;
	}

	return RC_SUCCESS;
}

ResponseCode RendererFactory::CreateShaderPacker(ShaderPacker** sp)
{
#ifdef WINDOWS
	HMODULE hDll = NULL;
	GetModuleHandleEx(
		GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
		(LPCTSTR)&CreateShaderPacker,
		&hDll
	);

	if (!hDll)
		return RC_FAILED_TO_DETERMINE_DLL_MODULE_HANDLE;

	HRSRC hRes = FindResource(hDll, MAKEINTRESOURCE(IDR_SHADER_PACK), RT_RCDATA);
	if (!hRes)
		return RC_FAILED_TO_LOAD_RESOURCE_FILE;

	HGLOBAL hData = LoadResource(hDll, hRes);
	const void* pData = LockResource(hData);
	DWORD dataSize = SizeofResource(hDll, hRes);

	*sp = new ShaderPacker(pData, dataSize);

#elif defined(__EMSCRIPTEN__)
	// --- WEB IMPLEMENTATION ---
	// In WASM, the shader pack is statically linked as a global array.
	// 'kShaderPack' is defined in "tritonsim_asm.h".
	*sp = new ShaderPacker(kShaderPack, sizeof(kShaderPack));
#else
#error Not implemented for other platforms
#endif 

	return RC_SUCCESS;
}