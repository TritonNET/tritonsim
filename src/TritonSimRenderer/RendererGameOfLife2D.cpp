#include "pch.h"
#include "RendererGameOfLife2D.h"
#include <algorithm> // For std::min

struct QuadVertex {
    float x, y, z;
    float u, v;
};

RendererGameOfLife2D::RendererGameOfLife2D(ShaderPacker* sp, const SimConfig& cfg)
    : RendererBase(sp, ShaderType::GameOfLife2D, cfg)
{
}

RendererGameOfLife2D::~RendererGameOfLife2D()
{
    if (bgfx::isValid(m_vbh)) bgfx::destroy(m_vbh);
    if (bgfx::isValid(m_ibh)) bgfx::destroy(m_ibh);
    if (bgfx::isValid(m_program)) bgfx::destroy(m_program);
    if (bgfx::isValid(m_textureGrid)) bgfx::destroy(m_textureGrid);

    if (bgfx::isValid(u_gridSize)) bgfx::destroy(u_gridSize);
    if (bgfx::isValid(u_colorAlive)) bgfx::destroy(u_colorAlive);
    if (bgfx::isValid(u_colorDead)) bgfx::destroy(u_colorDead);
    if (bgfx::isValid(s_texState)) bgfx::destroy(s_texState);
}

ResponseCode RendererGameOfLife2D::Init()
{
    auto rc = RendererBase::Init();
    if ((rc & RC_FAILED)) return rc;

    rc = LoadProgram(&m_program);
    if ((rc & RC_FAILED)) return rc;

    m_layout.begin()
        .add(bgfx::Attrib::Position, 3, bgfx::AttribType::Float)
        .add(bgfx::Attrib::TexCoord0, 2, bgfx::AttribType::Float)
        .end();

    // 2., 3. & 4. Calculate Centered Square Quad
    createQuad();

    u_gridSize = bgfx::createUniform("u_gridSize", bgfx::UniformType::Vec4);
    u_colorAlive = bgfx::createUniform("u_colorAlive", bgfx::UniformType::Vec4);
    u_colorDead = bgfx::createUniform("u_colorDead", bgfx::UniformType::Vec4);
    s_texState = bgfx::createUniform("s_texState", bgfx::UniformType::Sampler);

    size_t totalCells = m_gridWidth * m_gridHeight;
    m_dataCurrent.resize(totalCells);
    m_dataNext.resize(totalCells);

    m_textureGrid = bgfx::createTexture2D(
        (uint16_t)m_gridWidth, (uint16_t)m_gridHeight,
        false, 1, bgfx::TextureFormat::R8,
        BGFX_SAMPLER_POINT | BGFX_SAMPLER_U_CLAMP | BGFX_SAMPLER_V_CLAMP
    );

    // 5. Start from Single Box
    setupStartCondition();

    m_ready = true;
    return RC_SUCCESS;
}

void RendererGameOfLife2D::createQuad()
{
    // --- 3. Find smaller dimension ---
    float screenW = (float)m_width;
    float screenH = (float)m_height;

    // We want the box to fit in the screen. 
    // We assume pixels are square in Ortho projection (0..W, 0..H).
    float smallestDim = std::min(screenW, screenH);

    // Optional: Add a margin (e.g., 20 pixels padding)
    float displaySize = smallestDim - 40.0f;
    if (displaySize < 1.0f) displaySize = 10.0f; // Safety

    // --- 4. Calculate Centering Offsets ---
    float offsetX = (screenW - displaySize) * 0.5f;
    float offsetY = (screenH - displaySize) * 0.5f;

    // Define vertices based on calculated offsets
    float x0 = offsetX;
    float y0 = offsetY;
    float x1 = offsetX + displaySize;
    float y1 = offsetY + displaySize;

    QuadVertex vertices[] = {
        { x0, y0, 0.0f,  0.0f, 0.0f }, // Top-Left
        { x1, y0, 0.0f,  1.0f, 0.0f }, // Top-Right
        { x0, y1, 0.0f,  0.0f, 1.0f }, // Bottom-Left
        { x1, y1, 0.0f,  1.0f, 1.0f }  // Bottom-Right
    };

    uint16_t indices[] = { 0, 1, 2, 1, 3, 2 };

    if (bgfx::isValid(m_vbh)) bgfx::destroy(m_vbh);
    if (bgfx::isValid(m_ibh)) bgfx::destroy(m_ibh);

    m_vbh = bgfx::createVertexBuffer(bgfx::copy(vertices, sizeof(vertices)), m_layout);
    m_ibh = bgfx::createIndexBuffer(bgfx::copy(indices, sizeof(indices)));
}

void RendererGameOfLife2D::setupStartCondition()
{
    // Clear grid
    std::fill(m_dataCurrent.begin(), m_dataCurrent.end(), 0);

    // --- 5. Start from a single box in the middle ---
    uint32_t centerX = m_gridWidth / 2;
    uint32_t centerY = m_gridHeight / 2;

    // Safety check
    if (centerX < m_gridWidth && centerY < m_gridHeight)
    {
        m_dataCurrent[centerY * m_gridWidth + centerX] = 255;

        // Optional: A single dot in Game of Life dies instantly.
        // If you want it to survive/grow, you need a pattern (like a Glider or Block).
        // To strictly follow requirement 5 ("Single box"):
        // It will disappear in the next frame unless rules allow 0 neighbors.
        // Uncomment below to add neighbors if you want it to actually do something:
        /*
        m_dataCurrent[centerY * m_gridWidth + (centerX + 1)] = 255;
        m_dataCurrent[(centerY + 1) * m_gridWidth + centerX] = 255;
        m_dataCurrent[(centerY + 1) * m_gridWidth + (centerX + 1)] = 255;
        */
    }
}

void RendererGameOfLife2D::updateSimulation()
{
    auto getIdx = [&](int x, int y) {
        if (x < 0) x = m_gridWidth - 1; else if (x >= (int)m_gridWidth) x = 0;
        if (y < 0) y = m_gridHeight - 1; else if (y >= (int)m_gridHeight) y = 0;
        return y * m_gridWidth + x;
        };

    for (int y = 0; y < (int)m_gridHeight; ++y)
    {
        for (int x = 0; x < (int)m_gridWidth; ++x)
        {
            int idx = y * m_gridWidth + x;
            int neighbors = 0;

            for (int ny = -1; ny <= 1; ++ny) {
                for (int nx = -1; nx <= 1; ++nx) {
                    if (nx == 0 && ny == 0) continue;
                    if (m_dataCurrent[getIdx(x + nx, y + ny)] > 0) neighbors++;
                }
            }

            bool isAlive = m_dataCurrent[idx] > 0;

            if (isAlive) {
                m_dataNext[idx] = (neighbors == 2 || neighbors == 3) ? 255 : 0;
            }
            else {
                m_dataNext[idx] = (neighbors == 3) ? 255 : 0;
            }
        }
    }

    std::swap(m_dataCurrent, m_dataNext);
    const bgfx::Memory* mem = bgfx::copy(m_dataCurrent.data(), (uint32_t)m_dataCurrent.size());
    bgfx::updateTexture2D(m_textureGrid, 0, 0, 0, 0, (uint16_t)m_gridWidth, (uint16_t)m_gridHeight, mem);
}

void RendererGameOfLife2D::OnUpdate()
{
    m_updateTimer += 0.016f;
    if (m_updateTimer >= m_updateInterval)
    {
        updateSimulation();
        m_updateTimer = 0.0f;
    }
}

ResponseCode RendererGameOfLife2D::RenderFrame()
{
    if (!m_ready) return RC_SUCCESS;

    float view[16], proj[16];
    bx::mtxIdentity(view);
    // Standard ortho 0..Width, 0..Height
    bx::mtxOrtho(proj, 0.0f, (float)m_width, (float)m_height, 0.0f, 0.0f, 100.0f, 0.0f, bgfx::getCaps()->homogeneousDepth);
    bgfx::setViewTransform(0, view, proj);
    bgfx::setViewRect(0, 0, 0, m_width, m_height);

    // Uniforms
    float sizeData[4] = { (float)m_gridWidth, (float)m_gridHeight, 0.0f, 0.0f };
    bgfx::setUniform(u_gridSize, sizeData);

    float aliveData[4] = { 0.0f, 1.0f, 0.0f, 1.0f }; // Green
    bgfx::setUniform(u_colorAlive, aliveData);

    float deadData[4] = { 0.1f, 0.1f, 0.1f, 1.0f }; // Dark Gray
    bgfx::setUniform(u_colorDead, deadData);

    bgfx::setTexture(0, s_texState, m_textureGrid);

    bgfx::setVertexBuffer(0, m_vbh);
    bgfx::setIndexBuffer(m_ibh);
    bgfx::setState(BGFX_STATE_WRITE_RGB);
    bgfx::submit(0, m_program);
    bgfx::frame();

    return RC_SUCCESS;
}