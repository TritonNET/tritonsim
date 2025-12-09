#pragma once

#include <stdint.h>
#include <string>

enum class ShaderStage
{
    Vertex,
    Fragment,
    Compute
};

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

inline std::string to_shader_basename(ShaderType type)
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

inline ShaderType from_shader_basename(const std::string& basename)
{
    if (basename == "unlit_primitive")    return ShaderType::UnlitPrimitive;
    if (basename == "unlit_texture")      return ShaderType::UnlitTexture;
    if (basename == "lit_standard")       return ShaderType::LitStandard;
    if (basename == "particle_billboard") return ShaderType::Particle;
    if (basename == "procedural_quad")    return ShaderType::Procedural;

    throw std::runtime_error("Invalid shader basename: " + basename);
}

inline std::string to_stage_str(ShaderStage stage)
{
    switch (stage)
    {
    case ShaderStage::Vertex:   return "vertex";
    case ShaderStage::Fragment: return "fragment";
    case ShaderStage::Compute:  return "compute";
    default: return "unknown";
    }
}

inline ShaderStage from_stage_str(const std::string& str)
{
    if (str == "vertex")   return ShaderStage::Vertex;
    if (str == "fragment") return ShaderStage::Fragment;
    if (str == "compute")  return ShaderStage::Compute;

    // Throwing ensures the packer doesn't silently fail or produce invalid binaries
    throw std::runtime_error("Invalid ShaderStage string: " + str);
}