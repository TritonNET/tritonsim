#include "pch.h"

RendererBase::RendererBase(const SimConfig& cfg)
	: m_type(cfg.Type)
    , m_width(cfg.width)
    , m_height(cfg.height)
    , m_nwh(cfg.handle)
    , m_backgroundColor(cfg.BackgroundColor)
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
    init.resolution.reset = m_resetFlags;

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

        std::this_thread::sleep_for(std::chrono::milliseconds(1)); // avoid 100% CPU
    }
}

ResponseCode RendererBase::Shutdown()
{
    bgfx::shutdown();

    return RC_SUCCESS;
}

ResponseCode RendererBase::LoadFile(const char* path, const bgfx::Memory** mem)
{
    std::ifstream f(path, std::ios::binary | std::ios::ate);
    if (!f.is_open()) 
        return RC_FAILED_OPEN_FILE;

    size_t size = (size_t)f.tellg();
    f.seekg(0);

    const bgfx::Memory* pMem = bgfx::alloc(uint32_t(size));

    f.read((char*)pMem->data, size);
    f.close();

    *mem = pMem;

    return RC_SUCCESS;
}

ResponseCode RendererBase::LoadProgram(const char* vs, const char* fs, bgfx::ProgramHandle* handle)
{
    const bgfx::Memory* vshMem = nullptr;
    auto rc = LoadFile(vs, &vshMem);
    if (rc & RC_FAILED)
        return rc;

    const bgfx::Memory* fshMem = nullptr;
    rc = LoadFile(fs, &fshMem);
    if (rc & RC_FAILED)
        return rc;

    auto vsh = bgfx::createShader(vshMem);
    auto fsh = bgfx::createShader(fshMem);
    *handle = bgfx::createProgram(vsh, fsh, true);

    if (!bgfx::isValid(*handle))
        return RC_FAILED;

    return RC_SUCCESS;
}