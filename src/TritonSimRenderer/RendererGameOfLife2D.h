#pragma once
#include "RendererBase.h"
#include <vector>

class RendererGameOfLife2D : public RendererBase
{
public:
    RendererGameOfLife2D(ShaderPacker* sp, const SimConfig& cfg);
    ~RendererGameOfLife2D();

    ResponseCode Init() override;
    ResponseCode RenderFrame() override;

protected:
    void OnUpdate() override;

private:
    void setupStartCondition();
    void updateSimulation();
    void createQuad(); // Now calculates centering logic

    // --- 1. Increased Grid Size ---
    const uint32_t m_gridWidth = 200;
    const uint32_t m_gridHeight = 200;

    bgfx::ProgramHandle m_program = BGFX_INVALID_HANDLE;
    bgfx::VertexBufferHandle m_vbh = BGFX_INVALID_HANDLE;
    bgfx::IndexBufferHandle  m_ibh = BGFX_INVALID_HANDLE;
    bgfx::VertexLayout m_layout;
    bgfx::TextureHandle m_textureGrid = BGFX_INVALID_HANDLE;

    bgfx::UniformHandle u_gridSize = BGFX_INVALID_HANDLE;
    bgfx::UniformHandle u_colorAlive = BGFX_INVALID_HANDLE;
    bgfx::UniformHandle u_colorDead = BGFX_INVALID_HANDLE;
    bgfx::UniformHandle s_texState = BGFX_INVALID_HANDLE;

    std::vector<uint8_t> m_dataCurrent;
    std::vector<uint8_t> m_dataNext;

    float m_updateTimer = 0.0f;
    float m_updateInterval = 0.05f; // Faster updates for 200x200
    bool m_ready = false;
};