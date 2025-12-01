#include "pch.h"
#include "renderer_api.h"

#include <bgfx/bgfx.h>
#include <bgfx/platform.h>
#include <Windows.h>

#include <Unknwn.h>            // <= REQUIRED for IUnknown
#include <wrl.h>               // <= REQUIRED for ComPtr
#include <wrl/client.h>        // <= REQUIRED for ComPtr

#include <windows.ui.xaml.media.dxinterop.h>
#include <dxgi1_3.h>
#include <d3d11.h>
#include <fstream>

#include <string>
#include <windows.h> // Needed for OutputDebugString

static float gValue1 = 0.0f;
static float gValue2 = 0.0f;

BOOL tritonsim_init_with_swapchainpanel(void* panelNative, int width, int height)
{
    // Guard against invalid size
    if (width == 0 || height == 0)
        return FALSE;

    // Get ISwapChainPanelNative from incoming IUnknown
    Microsoft::WRL::ComPtr<ISwapChainPanelNative> panel;
    HRESULT hr = ((IUnknown*)panelNative)->QueryInterface(
        __uuidof(ISwapChainPanelNative),
        &panel
    );
    if (FAILED(hr) || !panel)
        return FALSE;

    //
    // === Create D3D11 Device ===
    //
    Microsoft::WRL::ComPtr<ID3D11Device> d3dDevice;
    Microsoft::WRL::ComPtr<ID3D11DeviceContext> d3dContext;

    D3D_FEATURE_LEVEL featureLevels[] = {
        D3D_FEATURE_LEVEL_11_0,
    };

    hr = D3D11CreateDevice(
        nullptr,                        // default adapter
        D3D_DRIVER_TYPE_HARDWARE,
        0,
        D3D11_CREATE_DEVICE_BGRA_SUPPORT,
        featureLevels,
        ARRAYSIZE(featureLevels),
        D3D11_SDK_VERSION,
        &d3dDevice,
        nullptr,
        &d3dContext
    );

    if (FAILED(hr) || !d3dDevice || !d3dContext)
        return FALSE;

    //
    // === Create DXGI SwapChain for SwapChainPanel ===
    //
    Microsoft::WRL::ComPtr<IDXGIDevice> dxgiDevice;
    hr = d3dDevice.As(&dxgiDevice);
    if (FAILED(hr))
        return FALSE;

    Microsoft::WRL::ComPtr<IDXGIAdapter> adapter;
    hr = dxgiDevice->GetAdapter(&adapter);
    if (FAILED(hr))
        return FALSE;

    Microsoft::WRL::ComPtr<IDXGIFactory2> factory;
    hr = adapter->GetParent(IID_PPV_ARGS(&factory));
    if (FAILED(hr))
        return FALSE;

    DXGI_SWAP_CHAIN_DESC1 desc{};
    desc.Width = width;
    desc.Height = height;
    desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    desc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    desc.BufferCount = 2;
    desc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL;
    desc.SampleDesc.Count = 1;
    desc.AlphaMode = DXGI_ALPHA_MODE_IGNORE;

    Microsoft::WRL::ComPtr<IDXGISwapChain1> swapChain;
    hr = factory->CreateSwapChainForComposition(
        d3dDevice.Get(),
        &desc,
        nullptr,
        &swapChain
    );

    if (FAILED(hr) || !swapChain)
        return FALSE;

    //
    // === Bind swapchain to the SwapChainPanel ===
    //
    hr = panel->SetSwapChain(swapChain.Get());
    if (FAILED(hr))
        return FALSE;

    //
    // === Feed platform data to bgfx ===
    //
    bgfx::PlatformData pd{};
    pd.ndt = swapChain.Get();          // bgfx uses this as the native display type
    pd.nwh = nullptr;                  // SwapChainPanel doesn't use HWND
    pd.context = d3dContext.Get();     // D3D11 device context
    pd.backBuffer = nullptr;           // let bgfx manage backbuffer
    pd.backBufferDS = nullptr;
    //pd.session = nullptr;

    bgfx::setPlatformData(pd);

    //
    // === Initialize bgfx ===
    //
    bgfx::Init init;
    init.type = bgfx::RendererType::Direct3D11;
    init.resolution.width = width;
    init.resolution.height = height;
    init.resolution.reset = BGFX_RESET_VSYNC;

    if (!bgfx::init(init))
        return FALSE;

    return TRUE;
}

BOOL tritonsim_init(void* nativeWindowHandle, int width, int height)
{
    if (nativeWindowHandle == nullptr) {
        OutputDebugStringA("[Native] Error: Handle is NULL!\n");
        return 0;
    }

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
        OutputDebugStringA("[Native] bgfx::init failed\n");
        return 0;
    }

    // Set clear color to something obvious (Red) to prove it's working
    bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH, 0xFF0000FF, 1.0f, 0);
    bgfx::setViewRect(0, 0, 0, width, height);

    return 1;
}

void tritonsim_set_params(float v1, float v2)
{
    gValue1 = v1;
    gValue2 = v2;
}

void tritonsim_render_frame(int clearColor)
{
    bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH, (uint32_t)clearColor, 1.0f, 0);
    bgfx::touch(0);
    bgfx::frame();
}

void tritonsim_shutdown()
{
    bgfx::shutdown();
}
