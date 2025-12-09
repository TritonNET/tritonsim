#pragma once

#include <string>
#include <vector>
#include <iostream>
#include "nlohmann/json.hpp"

using json = nlohmann::json;

struct ShaderDefinition 
{
    std::string type;
    std::string file_vertex;
    std::string file_fragment;
    std::string file_varying_def;

    // Following properties to be populated
    std::string bin_vertex;
    std::string bin_fragment;
};

struct ShaderPackerConfig 
{
    std::string output_file;
    std::string tmp_dir;
    std::string profile;
    std::vector<std::string> dependencies;
    std::vector<std::string> platforms;
    std::vector<ShaderDefinition> shaders;
};

NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(ShaderDefinition, type, file_vertex, file_fragment, file_varying_def)
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(ShaderPackerConfig, output_file, tmp_dir, profile, dependencies, platforms, shaders)