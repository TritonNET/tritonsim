#include "pch.h"

#include <chrono>
#include <cmath>

#include "RendererTestColorChanging.h"

ResponseCode RendererTestColorChanging::RenderFrame()
{
    bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH, m_color, 1.0f, 0);
    bgfx::touch(0);
    bgfx::frame();

    return RC_SUCCESS;
}

void RendererTestColorChanging::OnUpdate()
{
    using namespace std::chrono;

    long long ticks = duration_cast<milliseconds>(
        steady_clock::now().time_since_epoch()
    ).count();

    double r = (std::sin(ticks * 0.005) + 1.0) * 127.5;  // 0–255

    int red = static_cast<int>(r);

    m_color = static_cast<uint32_t>((red << 24) | 0x000000FF);
}