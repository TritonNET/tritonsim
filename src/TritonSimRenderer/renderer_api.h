#pragma once

extern "C"
{
    __declspec(dllexport) BOOL init(void* handle, int width, int height);
    __declspec(dllexport) void set_params(float value1, float value2);
    __declspec(dllexport) void render_frame(int clearColor);
    __declspec(dllexport) void shutdown();
}