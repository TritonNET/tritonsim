#pragma once

struct CircleVertex
{
    float x, y, z;    // Position
    uint32_t Color;   // Color
    float u, v;       // TexCoord (unused but required by shader layout)
};

class RendererBouncingCircle : public RendererBase
{
public:
    RendererBouncingCircle(const SimConfig& cfg);
    ~RendererBouncingCircle();

    ResponseCode Init() override;
    ResponseCode RenderFrame() override;

protected:
    void OnUpdate() override;

private:
    void createCircleGeometry(float centerX, float centerY, float radius, uint32_t color);

    bgfx::ProgramHandle m_program = BGFX_INVALID_HANDLE;
    bgfx::VertexBufferHandle m_vbh = BGFX_INVALID_HANDLE;
    bgfx::IndexBufferHandle  m_ibh = BGFX_INVALID_HANDLE;
    bgfx::VertexLayout m_layout;

    std::vector<CircleVertex> m_vertices;
    std::vector<uint16_t> m_indices;

    bool m_ready = false;
};
