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
    std::string rawId = "";
    if (cfg.handle != nullptr) {
        rawId = static_cast<const char*>(cfg.handle);
    }

    // 2. Formatting: Ensure it starts with '#' for CSS selector compatibility
    if (!rawId.empty() && rawId[0] != '#') {
        m_canvasid = "#" + rawId;
    }
    else {
        m_canvasid = rawId;
    }

    LOG_DEBUG("[C++] Parsed Canvas Selector: '%s' (Length: %zu)\n", m_canvasid.c_str(), m_canvasid.length());
}

RendererBase::~RendererBase()
{
    Stop();
    Shutdown();

    delete m_sp;
}

int canvas_exists_main(const char* id)
{
    return EM_ASM_INT({
        return document.getElementById(UTF8ToString($0)) !== null;
        }, id);
}

int canvas_exists(const char* id)
{
    return emscripten_sync_run_in_main_runtime_thread(
        EM_FUNC_SIG_II, canvas_exists_main, id
    );
}

//int create_webgl_context_main_thread()
//{
//    LOG_DEBUG("Running on pthread: %s", emscripten_is_main_browser_thread() ? "MAIN" : "WORKER");
//
//    EmscriptenWebGLContextAttributes attrs;
//    emscripten_webgl_init_context_attributes(&attrs);
//    attrs.majorVersion = 2;
//    attrs.minorVersion = 0;
//
//    EMSCRIPTEN_WEBGL_CONTEXT_HANDLE ctx = emscripten_webgl_create_context("#tbgfxcs", &attrs);
//    if (ctx <= 0) {
//        printf("Failed to create WebGL context on main thread\n");
//        return 0;
//    }
//
//    emscripten_webgl_make_context_current(ctx);
//    bgfx::PlatformData pd;
//    pd.nwh = nullptr;
//    pd.context = (void*)ctx;
//    bgfx::setPlatformData(pd);
//
//    return 1;
//}

ResponseCode RendererBase::Init()
{
#ifdef __EMSCRIPTEN__
    // -----------------------------------------------------------------------
    // THE CLEAN FIX: Let BGFX create the context.
    // -----------------------------------------------------------------------
    // Since m_canvasid is now correctly set to "#tbgfxcs", BGFX will:
    // 1. Find the canvas.
    // 2. Create the WebGL 2.0 context internally.
    // 3. Own the handle, preventing the 'GLctx' crash.
    // -----------------------------------------------------------------------

    bgfx::Init init;
    init.type = bgfx::RendererType::OpenGL; // Forces WebGL

    // Pass the ID String so BGFX knows WHICH canvas to use
    init.platformData.nwh = (void*)m_canvasid.c_str();

    // Pass NULL context so BGFX knows it must create one
    init.platformData.context = nullptr;

    init.platformData.backBuffer = nullptr;
    init.platformData.backBufferDS = nullptr;

#else
    // Windows/Desktop
    bgfx::Init init;
    init.type = bgfx::RendererType::Count;
    init.platformData.nwh = m_nwh;
    init.platformData.context = nullptr;
#endif

    // Common Settings
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