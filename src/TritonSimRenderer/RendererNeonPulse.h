#pragma once
#include "RendererBase.h"

// Define a simpler vertex for procedural quads (Pos + UV only)
struct QuadVertex
{
    float x, y, z;
    float u, v;
};

class RendererNeonPulse : public RendererBase
{
public:
    RendererNeonPulse(ShaderPacker* sp, const SimConfig& cfg);
    ~RendererNeonPulse();

    ResponseCode Init() override;
    ResponseCode RenderFrame() override;

protected:
    void OnUpdate() override;

private:
    void createFullscreenQuad();

    bgfx::ProgramHandle m_program = BGFX_INVALID_HANDLE;
    bgfx::VertexBufferHandle m_vbh = BGFX_INVALID_HANDLE;
    bgfx::IndexBufferHandle  m_ibh = BGFX_INVALID_HANDLE;
    bgfx::VertexLayout m_layout;

    // Uniform handles
    bgfx::UniformHandle u_time = BGFX_INVALID_HANDLE;
    bgfx::UniformHandle u_screenParams = BGFX_INVALID_HANDLE;

    float m_timeAccumulator = 0.0f;
    bool m_ready = false;
};