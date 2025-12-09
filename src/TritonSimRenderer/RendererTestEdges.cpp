#include "pch.h"
#include "RendererTestEdges.h"

RendererTestEdges::RendererTestEdges(const SimConfig& cfg)
    : RendererBase(cfg)
{
}

RendererTestEdges::~RendererTestEdges()
{
    if (bgfx::isValid(m_vbh)) bgfx::destroy(m_vbh);
    if (bgfx::isValid(m_ibh)) bgfx::destroy(m_ibh);
    if (bgfx::isValid(m_program)) bgfx::destroy(m_program);
}

ResponseCode RendererTestEdges::Init()
{
    auto rc = RendererBase::Init();
    if ((rc & RC_FAILED))
        return rc;

    rc = LoadProgram(
        "D:\\projects\\tritonnet\\tritonsim\\src\\ShaderBin\\tmp\\unlit_primitive_vs.bin",
        "D:\\projects\\tritonnet\\tritonsim\\src\\ShaderBin\\tmp\\unlit_primitive_fs.bin",
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

void RendererTestEdges::OnUpdate()
{
    m_ready = false;

    m_vertices.clear();
    m_indices.clear();

    createGeometry();

    if (bgfx::isValid(m_vbh))
    {
        bgfx::destroy(m_vbh);
        m_vbh = BGFX_INVALID_HANDLE;
    }

    if (bgfx::isValid(m_ibh))
    {
        bgfx::destroy(m_ibh);
        m_ibh = BGFX_INVALID_HANDLE;
    }

    m_vbh = bgfx::createVertexBuffer(
        bgfx::copy(m_vertices.data(), uint32_t(m_vertices.size() * sizeof(Vertex))),
        m_layout
    );

    m_ibh = bgfx::createIndexBuffer(
        bgfx::copy(m_indices.data(), uint32_t(m_indices.size() * sizeof(uint16_t)))
    );

    m_ready = true;
}

void RendererTestEdges::addLine(float x0, float y0, float x1, float y1, COLOR color)
{
    // 1. Capture the current starting index (e.g., 0, 2, 4...)
    uint16_t baseIndex = (uint16_t)m_vertices.size();

    // 2. Create your vertices
    Vertex v0 = { x0, y0, 0.0f, (uint32_t)color, 0.0f, 0.0f };
    Vertex v1 = { x1, y1, 0.0f, (uint32_t)color, 1.0f, 1.0f };

    // 3. Push Vertices
    m_vertices.push_back(v0);
    m_vertices.push_back(v1);

    // 4. Push Indices relative to the base
    // No matter how many lines you already have, this logic holds true:
    m_indices.push_back(baseIndex);     // Connects 'v0'
    m_indices.push_back(baseIndex + 1); // Connects 'v1'
}

void RendererTestEdges::createGeometry()
{
    // Now you can just call this function repeatedly
    addLine(0.0f, 0, m_width / 4, m_height / 4, COLOR_RED);
    addLine(0, m_height, m_width / 4, (m_height *3) / 4, COLOR_GREEN);
    addLine(m_width, 0.0f, (m_width*3)/4, m_height/4, COLOR_YELLOW);
    addLine(m_width, m_height, (m_width * 3) / 4, (m_height*3) / 4, COLOR_ORANGE);
}

ResponseCode RendererTestEdges::RenderFrame()
{
    if (!m_ready) return RC_SUCCESS;

    // 1. Setup View Matrix (Identity means camera is at 0,0)
    float view[16];
    bx::mtxIdentity(view);

    // 2. Setup Projection Matrix (Ortho maps pixels to -1..1 space)
    float proj[16];
    bx::mtxOrtho(
        proj,
        0.0f, float(m_width),
        float(m_height), 0.0f,
        0.0f, 100.0f,
        0.0f,
        bgfx::getCaps()->homogeneousDepth);

    // 3. APPLY MATRICES HERE
    // bgfx will calculate u_modelViewProj = Proj * View * Model
    bgfx::setViewTransform(0, view, proj);

    // REMOVED: bgfx::setUniform(u_mvp, proj); 
    // Reason: setViewTransform handles this data path now.

    bgfx::setViewRect(0, 0, 0, m_width, m_height);
    bgfx::setViewClear(0, BGFX_CLEAR_COLOR | BGFX_CLEAR_DEPTH, 0x303030FF);

    bgfx::setVertexBuffer(0, m_vbh);
    bgfx::setIndexBuffer(m_ibh);

    bgfx::setState(BGFX_STATE_WRITE_RGB | BGFX_STATE_PT_LINES | BGFX_STATE_MSAA);

    bgfx::submit(0, m_program);
    bgfx::frame();

    return RC_SUCCESS;
}