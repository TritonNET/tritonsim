#pragma once

#include <string>
#include <vector>
#include <iostream>
#include "nlohmann/json.hpp"

using json = nlohmann::json;

struct shader_def
{
    std::string type;
    std::string file_vertex;
    std::string file_fragment;
    std::string file_varying_def;
};

struct shader_platform_def
{
    std::string profile;
    std::string output_file;
};

struct shader_packer_config
{
    std::string tmp_dir;
    std::vector<std::string> dependencies;

    std::map<std::string, shader_platform_def> platforms;

    std::vector<shader_def> shaders;
};

NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(shader_def, type, file_vertex, file_fragment, file_varying_def)
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(shader_platform_def, profile, output_file)
NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE(shader_packer_config, tmp_dir, dependencies, platforms, shaders)