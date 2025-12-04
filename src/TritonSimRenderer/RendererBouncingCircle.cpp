#include "pch.h"
#include "RendererBouncingCircle.h"

#include "pch.h"
#include "RendererTestEdges.h"

RendererBouncingCircle::RendererBouncingCircle(const SimConfig& cfg)
    : RendererBase(cfg)
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

    // Reuse the exact same shaders as the Line test
    rc = LoadProgram(
        "D:\\projects\\tritonnet\\tritonsim\\src\\TritonSimRenderer\\vertex.bin",
        "D:\\projects\\tritonnet\\tritonsim\\src\\TritonSimRenderer\\frag.bin",
        &m_program);

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
    // 1. Thread Safety: Lock rendering while we update
    m_ready = false;

    // 2. Clear old geometry
    m_vertices.clear();
    m_indices.clear();

    // 3. Define Circle Properties
    float centerX = (float)m_width / 2.0f;
    float centerY = (float)m_height / 2.0f;
    float radius = 100.0f;
    uint32_t color = 0xFF00FFFF; // Yellow (ABGR)

    // 4. Generate new geometry
    createCircleGeometry(centerX, centerY, radius, color);

    // 5. Cleanup old GPU buffers
    if (bgfx::isValid(m_vbh)) bgfx::destroy(m_vbh);
    if (bgfx::isValid(m_ibh)) bgfx::destroy(m_ibh);

    // 6. Upload new buffers
    m_vbh = bgfx::createVertexBuffer(
        bgfx::copy(m_vertices.data(), uint32_t(m_vertices.size() * sizeof(CircleVertex))),
        m_layout
    );

    m_ibh = bgfx::createIndexBuffer(
        bgfx::copy(m_indices.data(), uint32_t(m_indices.size() * sizeof(uint16_t)))
    );

    // 7. Resume rendering
    m_ready = true;
}

void RendererBouncingCircle::createCircleGeometry(float cx, float cy, float r, uint32_t color)
{
    const int segments = 64; // Higher = smoother circle
    const float angleStep = (bx::kPi2) / segments; // 2*PI / segments

    // Center Vertex (Index 0)
    m_vertices.push_back({ cx, cy, 0.0f, color, 0.5f, 0.5f });

    // Perimeter Vertices (Indices 1 to segments)
    for (int i = 0; i <= segments; ++i)
    {
        float angle = i * angleStep;
        float x = cx + r * cosf(angle);
        float y = cy + r * sinf(angle);

        m_vertices.push_back({ x, y, 0.0f, color, 0.0f, 0.0f });
    }

    // Indices (Triangle Fan)
    // Connect Center(0) -> Current(i) -> Next(i+1)
    for (int i = 1; i <= segments; ++i)
    {
        m_indices.push_back(0);     // Center
        m_indices.push_back(i);     // Current Point
        m_indices.push_back(i + 1); // Next Point
    }
}

ResponseCode RendererBouncingCircle::RenderFrame()
{
    if (!m_ready) return RC_SUCCESS;

    // Standard Orthographic Setup
    float view[16];
    bx::mtxIdentity(view);

    float proj[16];
    bx::mtxOrtho(proj, 0.0f, float(m_width), float(m_height), 0.0f, 0.0f, 100.0f, 0.0f, bgfx::getCaps()->homogeneousDepth);

    bgfx::setViewTransform(0, view, proj);
    bgfx::setViewRect(0, 0, 0, m_width, m_height);

    // Clear background to dark gray
    bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH, 0x303030FF);

    bgfx::setVertexBuffer(0, m_vbh);
    bgfx::setIndexBuffer(m_ibh);

    // Draw triangles (filled circle) with MSAA for smooth edges
    bgfx::setState(BGFX_STATE_WRITE_RGB | BGFX_STATE_MSAA);

    bgfx::submit(0, m_program);
    bgfx::frame();

    return RC_SUCCESS;
}