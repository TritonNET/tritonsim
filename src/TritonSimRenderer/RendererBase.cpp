#include "pch.h"
#include "RendererBase.h"
#include <bgfx/bgfx.h>
#include <bgfx/platform.h>

RendererBase::RendererBase(const SimConfig& cfg)
	: m_type(cfg.Type)
    , m_width(cfg.width)
    , m_height(cfg.height)
    , m_nwh(cfg.handle)
{

}

ResponseCode RendererBase::Init()
{
    // Setup the Init struct
    bgfx::Init init;
    init.type = bgfx::RendererType::Direct3D11; // Force D3D11 for WinUI 3
    init.vendorId = BGFX_PCI_ID_NONE;
    init.resolution.width = m_width;
    init.resolution.height = m_height;
    init.resolution.reset = BGFX_RESET_VSYNC;

    init.platformData.nwh = m_nwh;
    init.platformData.ndt = NULL;
    init.platformData.context = NULL;
    init.platformData.backBuffer = NULL;
    init.platformData.backBufferDS = NULL;
    
    if (!bgfx::init(init))
        return RC_FAILED;

    bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH, 0xFF0000FF, 1.0f, 0);
    bgfx::setViewRect(0, 0, 0, m_width, m_height);

	return RC_SUCCESS;
}

ResponseCode RendererBase::RenderFrame()
{
    bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH, 0xFF0000FF, 1.0f, 0);
    bgfx::touch(0);
    bgfx::frame();

    return RC_SUCCESS;
}

ResponseCode RendererBase::Shutdown()
{
    bgfx::shutdown();

    return RC_SUCCESS;
}