#include "pch.h"
#include "RendererNeonPulse.h"

RendererNeonPulse::RendererNeonPulse(ShaderPacker* sp, const SimConfig& cfg)
    : RendererBase(sp, ShaderType::ProceduralNeonPulse, cfg)
{
}

RendererNeonPulse::~RendererNeonPulse()
{
    if (bgfx::isValid(m_vbh)) bgfx::destroy(m_vbh);
    if (bgfx::isValid(m_ibh)) bgfx::destroy(m_ibh);
    if (bgfx::isValid(m_program)) bgfx::destroy(m_program);

    if (bgfx::isValid(u_time)) bgfx::destroy(u_time);
    if (bgfx::isValid(u_screenParams)) bgfx::destroy(u_screenParams);
}

ResponseCode RendererNeonPulse::Init()
{
    auto rc = RendererBase::Init();
    if ((rc & RC_FAILED)) 
        return rc;

    rc = LoadProgram(&m_program);
    if ((rc & RC_FAILED)) 
        return rc;

    u_time = bgfx::createUniform("u_time", bgfx::UniformType::Vec4);
    u_screenParams = bgfx::createUniform("u_screenParams", bgfx::UniformType::Vec4);

    m_layout.begin()
        .add(bgfx::Attrib::Position, 3, bgfx::AttribType::Float)
        .add(bgfx::Attrib::TexCoord0, 2, bgfx::AttribType::Float)
        .end();

    createFullscreenQuad();

    m_ready = true;

    return RC_SUCCESS;
}

void RendererNeonPulse::createFullscreenQuad()
{
    // Define a Quad covering the screen size defined in m_config/m_width
    float w = (float)m_width;
    float h = (float)m_height;

    // 4 Vertices
    QuadVertex vertices[] = {
        { 0.0f, 0.0f, 0.0f,  0.0f, 0.0f }, // Top-Left
        { w,    0.0f, 0.0f,  1.0f, 0.0f }, // Top-Right
        { 0.0f, h,    0.0f,  0.0f, 1.0f }, // Bottom-Left
        { w,    h,    0.0f,  1.0f, 1.0f }  // Bottom-Right
    };

    // 6 Indices (Two Triangles)
    uint16_t indices[] = {
        0, 1, 2, // First Tri
        1, 3, 2  // Second Tri
    };

    // Create Buffers
    m_vbh = bgfx::createVertexBuffer(
        bgfx::copy(vertices, sizeof(vertices)),
        m_layout
    );

    m_ibh = bgfx::createIndexBuffer(
        bgfx::copy(indices, sizeof(indices))
    );
}

void RendererNeonPulse::OnUpdate()
{
    // Update logic (increase time)
    // Assuming 60fps roughly, or pass delta time from main loop if available
    m_timeAccumulator += 0.016f;
}

ResponseCode RendererNeonPulse::RenderFrame()
{
    if (!m_ready) return RC_SUCCESS;

    // 1. Setup View/Proj (Standard Ortho)
    float view[16];
    bx::mtxIdentity(view);

    float proj[16];
    bx::mtxOrtho(proj, 0.0f, float(m_width), float(m_height), 0.0f, 0.0f, 100.0f, 0.0f, bgfx::getCaps()->homogeneousDepth);

    bgfx::setViewTransform(0, view, proj);
    bgfx::setViewRect(0, 0, 0, m_width, m_height);

    // 2. Update Uniforms
    // Time
    float timeData[4] = { m_timeAccumulator, 0.0f, 0.0f, 0.0f };
    bgfx::setUniform(u_time, timeData);

    // Resolution
    float screenData[4] = { (float)m_width, (float)m_height, 0.0f, 0.0f };
    bgfx::setUniform(u_screenParams, screenData);

    // 3. Submit
    bgfx::setVertexBuffer(0, m_vbh);
    bgfx::setIndexBuffer(m_ibh);

    // Write RGB, no Depth check needed for fullscreen effects usually
    bgfx::setState(BGFX_STATE_WRITE_RGB);

    bgfx::submit(0, m_program);
    bgfx::frame();

    return RC_SUCCESS;
}