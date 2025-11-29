#pragma once

extern "C"
{
    __declspec(dllexport) void tritonsim_init(void* handle, int width, int height);
    __declspec(dllexport) void tritonsim_set_params(float value1, float value2);
    __declspec(dllexport) void tritonsim_render_frame();
    __declspec(dllexport) void tritonsim_shutdown();
}