#pragma once
#include "RendererBase.h"
#include <vector>
#include <glm/glm.hpp>

struct Vertex
{
    float x, y, z;    // Position (12 bytes)
    uint32_t Color;   // Color    (4 bytes)
    float u, v;       // TexCoord (8 bytes)
};

enum COLOR : uint32_t
{
    COLOR_BLACK = 0xFF000000,
    COLOR_WHITE = 0xFFFFFFFF,
    COLOR_RED = 0xFFFF0000,
    COLOR_GREEN = 0xFF00FF00,
    COLOR_BLUE = 0xFF0000FF,
    COLOR_YELLOW = 0xFFFFFF00,
    COLOR_CYAN = 0xFF00FFFF,
    COLOR_MAGENTA = 0xFFFF00FF,
    COLOR_GRAY = 0xFF808080,
    COLOR_ORANGE = 0xFFFFA500,
};

class RendererTestEdges : public RendererBase
{
public:
    RendererTestEdges(const SimConfig& cfg);
    ~RendererTestEdges();

    ResponseCode Init() override;
    ResponseCode RenderFrame() override;

protected:
    void RunAsync() override;

private:
    void createGeometry();
    void addLine(float x0, float y0, float x1, float y1, COLOR color);

    bgfx::ProgramHandle m_program = BGFX_INVALID_HANDLE;

    bgfx::VertexBufferHandle m_vbh = BGFX_INVALID_HANDLE;
    bgfx::IndexBufferHandle  m_ibh = BGFX_INVALID_HANDLE;

    bgfx::VertexLayout m_layout;

    std::vector<Vertex>  m_vertices;
    std::vector<uint16_t> m_indices;

    bgfx::UniformHandle u_mvp;

    bool m_ready = false;
};
