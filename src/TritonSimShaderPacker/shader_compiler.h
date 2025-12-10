#pragma once
#include "pch.h"

namespace fs = std::filesystem;
using str = std::string;
using fpath = std::filesystem::path;
using json = nlohmann::json;

struct shader_compile_item
{
    std::string source_path;
    std::string varying_path;
    std::string bin_path;
    ShaderStage stage;
    std::string platform;
    std::string profile;
    std::vector<std::string> dependencies;
};

struct shader_pair
{
    shader_compile_item vertex;
    shader_compile_item fragment;
};

struct shader_compile_platform
{
    std::string platform;
    std::string output_file;

    bool operator<(const shader_compile_platform& other) const
    {
        if (platform != other.platform)
            return platform < other.platform;

        return output_file < other.output_file;
    }
};

/* {platform: [ typename: {vertex, frag}]}*/
typedef std::map<shader_compile_platform, std::map<std::string, shader_pair>> scitems;

bool shader_compile_pack(const shader_packer_config& config);

bool shader_compute_compile_items(const shader_packer_config& config, scitems& items);
bool shader_compile_cli(const shader_compile_item& item);


namespace bgfx
{
    // impl in ~\bgfx\tools\shaderc\shaderc.cpp
    int compileShader(int _argc, const char* _argv[]);
}