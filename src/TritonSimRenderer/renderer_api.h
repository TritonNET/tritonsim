#pragma once

extern "C"
{
    __declspec(dllexport) BOOL tritonsim_init_with_swapchainpanel(void* swapChainPanelNative, int width, int height);
    __declspec(dllexport) BOOL tritonsim_init(void* handle, int width, int height);
    __declspec(dllexport) void tritonsim_set_params(float value1, float value2);
    __declspec(dllexport) void tritonsim_render_frame(int clearColor);
    __declspec(dllexport) void tritonsim_shutdown();
}