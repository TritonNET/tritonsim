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
    // Setup the Init struct
    bgfx::Init init;
    init.type = bgfx::RendererType::OpenGL;
    init.vendorId = BGFX_PCI_ID_NONE;
    init.resolution.width = m_width;
    init.resolution.height = m_height;
    init.resolution.reset = m_resetFlags;

#ifdef __EMSCRIPTEN__
    init.type = bgfx::RendererType::OpenGL; // WebGL uses OpenGL backend
#else
    // Windows/Desktop Specifics
    init.type = bgfx::RendererType::Direct3D11;
#endif

    init.platformData.nwh = m_nwh;
    init.platformData.ndt = NULL;
    init.platformData.context = NULL;
    init.platformData.backBuffer = NULL;
    init.platformData.backBufferDS = NULL;
    
    if (!bgfx::init(init))
        return RC_FAILED;

    bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH, m_backgroundColor, 1.0f, 0);
    bgfx::setViewRect(0, 0, 0, m_width, m_height);
    bgfx::touch(0);
    bgfx::frame();

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

#ifdef __EMSCRIPTEN__
    OnUpdate();
#endif

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

#ifndef __EMSCRIPTEN__
    // Windows: Spawn the background thread as usual
    m_thread = std::thread(&RendererBase::WorkerLoop, this);
#else
    // Wasm: Do NOT spawn a thread. The browser is the thread.
    // We just return Success. C# will start calling RenderFrame().
    LOG_DEBUG("WASM Start: Ready for frame callbacks.");
#endif

    return RC_SUCCESS;
}

ResponseCode RendererBase::Stop()
{
    if (!m_running.load())
        return RC_SUCCESS;

    m_running.store(false);

#ifndef __EMSCRIPTEN__
    // Windows: Join the thread
    if (m_thread.joinable())
        m_thread.join();
#endif

    return RC_SUCCESS;
}

void RendererBase::WorkerLoop()
{
#ifndef __EMSCRIPTEN__
    while (m_running.load())
    {
        OnUpdate();

        std::this_thread::sleep_for(std::chrono::milliseconds(1));
    }
#endif
}

ResponseCode RendererBase::Shutdown()
{
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