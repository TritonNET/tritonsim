#include "pch.h"
#include "renderer_api.h"
#include <bgfx/bgfx.h>
#include <bgfx/platform.h>
#include <Windows.h>
#include <fstream>


static float gValue1 = 0.0f;
static float gValue2 = 0.0f;

void tritonsim_init(void* windowHandle, int width, int height)
{
    std::ofstream log("C:\\temp\\tritonsim_log.txt", std::ios::app);
    if (log.is_open())
    {
        log << "tritonsim_init called. HWND=" << windowHandle
            << ", width=" << width
            << ", height=" << height
            << std::endl;
    }

    bgfx::PlatformData pd{};
    pd.nwh = windowHandle;

    bgfx::setPlatformData(pd);

    bgfx::Init init;
    init.type = bgfx::RendererType::Direct3D11;
    init.resolution.width = width;
    init.resolution.height = height;
    init.resolution.reset = BGFX_RESET_VSYNC;

    bgfx::init(init);
}

void tritonsim_set_params(float v1, float v2)
{
    gValue1 = v1;
    gValue2 = v2;
}

void tritonsim_render_frame()
{
    bgfx::touch(0);

    // Example clear color uses hardcoded params
    uint32_t r = (uint8_t)(gValue1);
    uint32_t g = (uint8_t)(gValue2);
    uint32_t b = 128;

    bgfx::setViewClear(0, BGFX_CLEAR_COLOR, (r << 16) | (g << 8) | b);
    bgfx::frame();
}

void tritonsim_shutdown()
{
    bgfx::shutdown();
}
