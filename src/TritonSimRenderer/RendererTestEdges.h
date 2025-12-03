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
    // Format: 0xAABBGGRR
    COLOR_BLACK = 0xFF000000,
    COLOR_WHITE = 0xFFFFFFFF,

    // R and B are swapped compared to your list
    COLOR_RED = 0xFF0000FF, // R=FF (LSB)
    COLOR_GREEN = 0xFF00FF00,
    COLOR_BLUE = 0xFFFF0000, // B=FF (High byte of low word)

    // Yellow (R+G) and Cyan (G+B)
    COLOR_YELLOW = 0xFF00FFFF, // R=FF, G=FF, B=00
    COLOR_CYAN = 0xFFFF00FF, // R=00, G=FF, B=FF
    COLOR_MAGENTA = 0xFFFF00FF, // R=FF, B=FF (Symmetric, same as before)

    COLOR_GRAY = 0xFF808080,

    // Orange: Red=FF, Green=A5, Blue=00
    // ABGR: 0xFF(Alpha) 00(Blue) A5(Green) FF(Red)
    COLOR_ORANGE = 0xFF00A5FF,
};

class RendererTestEdges : public RendererBase
{
public:
    RendererTestEdges(const SimConfig& cfg);
    ~RendererTestEdges();

    ResponseCode Init() override;
    ResponseCode RenderFrame() override;

protected:
    void OnUpdate() override;

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
