#pragma once

#include <stdint.h>
#include <string>

enum class ShaderStage
{
    Vertex,
    Fragment,
    Compute
};

enum class ShaderType : uint32_t
{
    DebugLine,       // Uses unlit_primitive shader
    GameOfLife2D,
    BouncingCircle,  // Uses circle_soft shader (distinct form DebugLine)
    ProceduralNeonPulse,

    Unknown = 0xFFFFFFFF
};

inline std::string to_shader_basename(ShaderType type)
{
    switch (type)
    {
        case ShaderType::DebugLine:             return "debug_line";
        case ShaderType::GameOfLife2D:          return "gameoflife_2d";
        case ShaderType::BouncingCircle:        return "bouncing_circle";
        case ShaderType::ProceduralNeonPulse:   return "procedural_neon_pulse";
        default:                                return "unknown";
    }
}

inline ShaderType from_shader_basename(const std::string& basename)
{
    if (basename == "debug_line")                   return ShaderType::DebugLine;
    if (basename == "gameoflife_2d")                return ShaderType::GameOfLife2D;
    if (basename == "bouncing_circle")              return ShaderType::BouncingCircle;
    if (basename == "procedural_neon_pulse")        return ShaderType::ProceduralNeonPulse;

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