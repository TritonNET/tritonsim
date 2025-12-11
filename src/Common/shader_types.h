#pragma once

#include <stdint.h>
#include <string>

enum class ShaderStage
{
    Unknown = 0,

    Vertex = 1,
    Fragment = 2,
    Compute = 3
};

enum class ShaderType : uint32_t
{
    Unknown = 0,
    DebugLine = 1,       // Uses unlit_primitive shader
    GameOfLife2D = 2,
    BouncingCircle = 3,  // Uses circle_soft shader (distinct form DebugLine)
    ProceduralNeonPulse = 4    
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

    return ShaderType::Unknown;
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

    return ShaderStage::Unknown;
}