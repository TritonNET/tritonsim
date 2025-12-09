#include "pch.h"
#include "RendererGameOfLife2D.h"
#include <random>

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

    randomizeGrid();
    m_ready = true;
    return RC_SUCCESS;
}

void RendererGameOfLife2D::randomizeGrid()
{
    std::srand((unsigned int)time(0));

    for (size_t i = 0; i < m_dataCurrent.size(); ++i) {
        m_dataCurrent[i] = (std::rand() % 5 == 0) ? 255 : 0;
    }
}

void RendererGameOfLife2D::updateSimulation()
{
    auto getIdx = [&](int x, int y) 
    {
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

            for (int ny = -1; ny <= 1; ++ny) 
            {
                for (int nx = -1; nx <= 1; ++nx) 
                {
                    if (nx == 0 && ny == 0) continue; // Skip self

                    if (m_dataCurrent[getIdx(x + nx, y + ny)] > 0) neighbors++;
                }
            }

            bool isAlive = m_dataCurrent[idx] > 0;

            if (isAlive) 
            {
                // Die if lonely (<2) or overpopulated (>3)
                m_dataNext[idx] = (neighbors == 2 || neighbors == 3) ? 255 : 0;
            }
            else 
            {
                // Reproduce if exactly 3 neighbors
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

void RendererGameOfLife2D::createQuad()
{
    float w = (float)m_width;
    float h = (float)m_height;

    QuadVertex vertices[] = {
        { 0.0f, 0.0f, 0.0f,  0.0f, 0.0f },
        { w,    0.0f, 0.0f,  1.0f, 0.0f },
        { 0.0f, h,    0.0f,  0.0f, 1.0f },
        { w,    h,    0.0f,  1.0f, 1.0f }
    };
    uint16_t indices[] = { 0, 1, 2, 1, 3, 2 };

    m_vbh = bgfx::createVertexBuffer(bgfx::copy(vertices, sizeof(vertices)), m_layout);
    m_ibh = bgfx::createIndexBuffer(bgfx::copy(indices, sizeof(indices)));
}

ResponseCode RendererGameOfLife2D::RenderFrame()
{
    if (!m_ready) return RC_SUCCESS;

    // Setup Standard Ortho View
    float view[16], proj[16];
    bx::mtxIdentity(view);
    bx::mtxOrtho(proj, 0.0f, (float)m_width, (float)m_height, 0.0f, 0.0f, 100.0f, 0.0f, bgfx::getCaps()->homogeneousDepth);
    bgfx::setViewTransform(0, view, proj);
    bgfx::setViewRect(0, 0, 0, m_width, m_height);

    // Set Uniforms
    float sizeData[4] = { (float)m_gridWidth, (float)m_gridHeight, 0.0f, 0.0f };
    bgfx::setUniform(u_gridSize, sizeData);

    float aliveData[4] = { 0.0f, 1.0f, 0.0f, 1.0f }; // Green
    bgfx::setUniform(u_colorAlive, aliveData);

    float deadData[4] = { 0.1f, 0.1f, 0.1f, 1.0f }; // Dark Gray
    bgfx::setUniform(u_colorDead, deadData);

    // Bind Texture to Slot 0
    bgfx::setTexture(0, s_texState, m_textureGrid);

    // Submit
    bgfx::setVertexBuffer(0, m_vbh);
    bgfx::setIndexBuffer(m_ibh);
    bgfx::setState(BGFX_STATE_WRITE_RGB);
    bgfx::submit(0, m_program);
    bgfx::frame();

    return RC_SUCCESS;
}