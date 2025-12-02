#include "pch.h"

RendererBase::RendererBase(const SimConfig& cfg)
	: m_type(cfg.Type)
    , m_width(cfg.width)
    , m_height(cfg.height)
    , m_nwh(cfg.handle)
{

}

RendererBase::~RendererBase()
{
    Stop();
    Shutdown();
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

ResponseCode RendererBase::Start()
{
    if (m_running.load())
        return RC_SUCCESS;

    m_running.store(true);
    m_thread = std::thread(&RendererBase::RunAsync, this);

    return RC_SUCCESS;
}

ResponseCode RendererBase::Stop()
{
    if (!m_running.load())
        return RC_SUCCESS;

    m_running.store(false);

    if (m_thread.joinable())
        m_thread.join();

    return RC_SUCCESS;
}

void RendererBase::RunAsync()
{
    while (m_running.load())
    {
        std::this_thread::sleep_for(std::chrono::milliseconds(1)); // avoid 100% CPU
    }
}

ResponseCode RendererBase::Shutdown()
{
    bgfx::shutdown();

    return RC_SUCCESS;
}