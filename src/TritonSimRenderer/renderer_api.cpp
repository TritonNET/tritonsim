#include "pch.h"
#include "renderer_api.h"

#include <bgfx/bgfx.h>
#include <bgfx/platform.h>

static float gValue1 = 0.0f;
static float gValue2 = 0.0f;

BOOL init(void* nativeWindowHandle, int width, int height)
{
    // Setup the Init struct
    bgfx::Init init;
    init.type = bgfx::RendererType::Direct3D11; // Force D3D11 for WinUI 3
    init.vendorId = BGFX_PCI_ID_NONE;
    init.resolution.width = width;
    init.resolution.height = height;
    init.resolution.reset = BGFX_RESET_VSYNC;

    // CRITICAL FIX: Set platform data INSIDE the Init struct
    // The global bgfx::setPlatformData() is ignored if this is passed.
    init.platformData.nwh = nativeWindowHandle;
    init.platformData.ndt = NULL;
    init.platformData.context = NULL;
    init.platformData.backBuffer = NULL;
    init.platformData.backBufferDS = NULL;

    if (!bgfx::init(init))
    {
        return 0;
    }

    // Set clear color to something obvious (Red) to prove it's working
    bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH, 0xFF0000FF, 1.0f, 0);
    bgfx::setViewRect(0, 0, 0, width, height);

    return 1;
}

void set_params(float v1, float v2)
{
    gValue1 = v1;
    gValue2 = v2;
}

void render_frame(int clearColor)
{
    bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH, (uint32_t)clearColor, 1.0f, 0);
    bgfx::touch(0);
    bgfx::frame();
}

void shutdown()
{
    bgfx::shutdown();
}
