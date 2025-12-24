#pragma once
#include "RendererBase.h"

#include <glm/glm.hpp>

struct Vertex
{
    float x, y, z;    // Position (12 bytes)
    uint32_t Color;   // Color    (4 bytes)
    float u, v;       // TexCoord (8 bytes)
};

class RendererTestEdges : public RendererBase
{
public:
    RendererTestEdges(ShaderPacker* sp, const SimConfig& cfg);
    ~RendererTestEdges();

    ResponseCode Init() override;
    ResponseCode RenderFrame() override;

protected:
    void OnUpdate() override;

private:
    void createGeometry();
    void addLine(float x0, float y0, float x1, float y1, COLOR color);
    void uploadResources();

    bgfx::ProgramHandle m_program = BGFX_INVALID_HANDLE;

    bgfx::VertexBufferHandle m_vbh = BGFX_INVALID_HANDLE;
    bgfx::IndexBufferHandle  m_ibh = BGFX_INVALID_HANDLE;

    bgfx::VertexLayout m_layout;

    std::vector<Vertex>  m_vertices;
    std::vector<uint16_t> m_indices;

    bool m_ready = false;
};
