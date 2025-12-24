#include "pch.h"
#include "RendererBouncingCircle.h"

RendererBouncingCircle::RendererBouncingCircle(ShaderPacker* sp, const SimConfig& cfg)
    : RendererBase(sp, ShaderType::BouncingCircle, cfg)
{
}

RendererBouncingCircle::~RendererBouncingCircle()
{
    if (bgfx::isValid(m_vbh)) bgfx::destroy(m_vbh);
    if (bgfx::isValid(m_ibh)) bgfx::destroy(m_ibh);
    if (bgfx::isValid(m_program)) bgfx::destroy(m_program);
}

ResponseCode RendererBouncingCircle::Init()
{
    auto rc = RendererBase::Init();
    if ((rc & RC_FAILED)) return rc;

    rc = LoadProgram(&m_program);
    if ((rc & RC_FAILED))
        return rc;

    m_layout.begin()
        .add(bgfx::Attrib::Position, 3, bgfx::AttribType::Float)
        .add(bgfx::Attrib::Color0, 4, bgfx::AttribType::Uint8, true)
        .add(bgfx::Attrib::TexCoord0, 2, bgfx::AttribType::Float)
        .end();

    return RC_SUCCESS;
}

void RendererBouncingCircle::OnUpdate()
{
    std::lock_guard<std::mutex> lock(m_dataMutex);

    m_vertices.clear();
    m_indices.clear();

    float centerX = (float)m_width / 2.0f;
    float centerY = (float)m_height / 2.0f;
    float radius = 100.0f;
    uint32_t color = 0xFF00FFFF; // Yellow (ABGR)

    createCircleGeometry(centerX, centerY, radius, color);

    m_dataDirty = true;
}

void RendererBouncingCircle::uploadResources()
{
    std::lock_guard<std::mutex> lock(m_dataMutex);

    if (!m_dataDirty) return;

    // Destroy old buffers
    if (bgfx::isValid(m_vbh)) bgfx::destroy(m_vbh);
    if (bgfx::isValid(m_ibh)) bgfx::destroy(m_ibh);

    if (!m_vertices.empty())
    {
        // Upload new buffers
        // BGFX copies the memory immediately here, so it is safe to release the lock after this block
        m_vbh = bgfx::createVertexBuffer(
            bgfx::copy(m_vertices.data(), uint32_t(m_vertices.size() * sizeof(CircleVertex))),
            m_layout
        );

        m_ibh = bgfx::createIndexBuffer(
            bgfx::copy(m_indices.data(), uint32_t(m_indices.size() * sizeof(uint16_t)))
        );

        m_ready = true;
    }
    else
    {
        m_ready = false;
    }

    m_dataDirty = false;
}

void RendererBouncingCircle::createCircleGeometry(float cx, float cy, float r, uint32_t color)
{
    const int segments = 64;
    const float angleStep = (bx::kPi2) / segments;

    m_vertices.push_back({ cx, cy, 0.0f, color, 0.5f, 0.5f });

    for (int i = 0; i <= segments; ++i)
    {
        float angle = i * angleStep;
        float x = cx + r * cosf(angle);
        float y = cy + r * sinf(angle);

        m_vertices.push_back({ x, y, 0.0f, color, 0.0f, 0.0f });
    }

    for (int i = 1; i <= segments; ++i)
    {
        m_indices.push_back(0);      // Center
        m_indices.push_back(i);      // Current Point
        m_indices.push_back(i + 1);  // Next Point
    }
}

ResponseCode RendererBouncingCircle::RenderFrame()
{
    uploadResources();

    if (!m_ready || !bgfx::isValid(m_vbh)) return RC_SUCCESS;

    float view[16];
    bx::mtxIdentity(view);

    float proj[16];
    bx::mtxOrtho(proj, 0.0f, float(m_width), float(m_height), 0.0f, 0.0f, 100.0f, 0.0f, bgfx::getCaps()->homogeneousDepth);

    bgfx::setViewTransform(0, view, proj);
    bgfx::setViewRect(0, 0, 0, m_width, m_height);

    bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH, 0x303030FF);

    bgfx::setVertexBuffer(0, m_vbh);
    bgfx::setIndexBuffer(m_ibh);

    bgfx::setState(BGFX_STATE_WRITE_RGB | BGFX_STATE_MSAA);

    bgfx::submit(0, m_program);
    bgfx::frame();

    return RC_SUCCESS;
}