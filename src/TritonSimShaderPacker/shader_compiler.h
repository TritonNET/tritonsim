#pragma once
#include "pch.h"

namespace fs = std::filesystem;
using str = std::string;

bool shader_compile(const ShaderPackerConfig& config);
bool shader_compile_cli(const str& s, const str& of, const str& v, ShaderStage stage, const str& p, const ShaderPackerConfig& c);

namespace bgfx
{
    // impl in ~\bgfx\tools\shaderc\shaderc.cpp
    int compileShader(int _argc, const char* _argv[]);
}