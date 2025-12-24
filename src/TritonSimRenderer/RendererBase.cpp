#include "pch.h"

RendererBase::RendererBase(ShaderPacker* sp, ShaderType st, const SimConfig& cfg)
	: m_type(cfg.Type)
    , m_width(cfg.width)
    , m_height(cfg.height)
    , m_nwh(cfg.handle)
    , m_backgroundColor(cfg.BackgroundColor)
    , m_st(st)
    , m_sp(sp)
{
}

RendererBase::~RendererBase()
{
    Stop();
    Shutdown();

    delete m_sp;
}

ResponseCode RendererBase::Init()
{
    bgfx::Init init;
    init.type = bgfx::RendererType::Count; // Forces WebGL
    
    init.platformData.nwh = m_nwh;
    init.platformData.context = nullptr;
    init.platformData.backBuffer = nullptr;
    init.platformData.backBufferDS = nullptr;

    init.resolution.width = m_width;
    init.resolution.height = m_height;
    init.resolution.reset = BGFX_RESET_VSYNC;

    if (!bgfx::init(init)) {
        LOG_DEBUG("BGFX init failed");
        return RC_FAILED;
    }

    LOG_DEBUG("BGFX initialized successfully");

    bgfx::setViewRect(0, 0, 0, m_width, m_height);
    bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH, m_backgroundColor, 1.0f, 0);
    bgfx::touch(0);

    return RC_SUCCESS;
}

ResponseCode RendererBase::UpdateConfig(const SimConfig& cfg)
{
    m_width = cfg.width;
    m_height = cfg.height;
    m_backgroundColor = cfg.BackgroundColor;

    bgfx::reset(m_width, m_height, m_resetFlags);

    bgfx::setViewRect(0, 0, 0, m_width, m_height);

    bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH, m_backgroundColor, 1.0f, 0);

    bgfx::touch(0);
    bgfx::frame();

    return RC_SUCCESS;
}

ResponseCode RendererBase::RenderFrame()
{
    if (!m_running.load()) return RC_SUCCESS;

    bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH, m_backgroundColor, 1.0f, 0);
    bgfx::touch(0);
    bgfx::frame();

    return RC_SUCCESS;
}

ResponseCode RendererBase::Start()
{
    if (m_running.load())
        return RC_SUCCESS;

    m_running.store(true);
    
    m_thread = std::thread(&RendererBase::WorkerLoop, this);

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

void RendererBase::WorkerLoop()
{
    while (m_running.load())
    {
        OnUpdate();

        std::this_thread::sleep_for(std::chrono::milliseconds(1));
    }
}

ResponseCode RendererBase::Shutdown()
{
    if (m_running.load())
    {
        auto rc = Stop();
        if (rc & RC_FAILED)
			return rc;
    }

    bgfx::shutdown();

    return RC_SUCCESS;
}

ResponseCode RendererBase::LoadProgram(bgfx::ProgramHandle* handle)
{
    bgfx::ShaderHandle vsh;
    if (!m_sp->Unpack(m_st, ShaderStage::Vertex, vsh))
        return RC_FAILED_OPEN_FILE;

    bgfx::ShaderHandle fsh;
    if (!m_sp->Unpack(m_st, ShaderStage::Fragment, fsh))
        return RC_FAILED_OPEN_FILE;

    *handle = bgfx::createProgram(vsh, fsh, true);

    if (!bgfx::isValid(*handle))
        return RC_FAILED;

    return RC_SUCCESS;
}