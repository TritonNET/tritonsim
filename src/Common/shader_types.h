#pragma once
#include <stdint.h>
#include <string>

enum ShaderType : uint32_t
{
    // 1. Basic Geometry (Lines, Circles, Debugging)
    // No lighting, no textures. Uses vertex colors.
    UnlitPrimitive,

    // 2. 2D Elements (UI, Sprites, Agents)
    // No lighting. Renders an image/texture directly.
    UnlitTexture,

    // 3. 3D Objects (Meshes, Spheres, Cubes)
    // Uses lighting calculations (normals, light direction).
    LitStandard,

    // 4. Simulations (Smoke, Fire, Swarms)
    // Billboarded quads that always face the camera.
    Particle,

    // 5. Fullscreen Effects (Fluids, Clouds, "ShaderToy")
    // Procedural generation on a fullscreen quad.
    Procedural
};

std::string GetShaderBaseName(ShaderType type)
{
    switch (type)
    {
        case ShaderType::UnlitPrimitive: return "unlit_primitive";
        case ShaderType::UnlitTexture:   return "unlit_texture";
        case ShaderType::LitStandard:    return "lit_standard";
        case ShaderType::Particle:       return "particle_billboard";
        case ShaderType::Procedural:     return "procedural_quad";
        default:                         return "unknown";
    }
}