#pragma once
#include <filesystem>
#include <fstream>
#include <vector>
#include <iostream>

#include "shader_def.h"
#include "shaderc.h"
#include <bx/commandline.h>
#include <bx/file.h>
#include <bx/string.h>

namespace fs = std::filesystem;

bool shader_compile(const ShaderPackerConfig& config);

bool shader_compile_cli(
    const std::string& sourceFile,
    const std::string& outputDir,
    const std::string& varyingPath,
    const std::string& type, // "vertex", "fragment", "compute"
    const std::string& platform,
    const ShaderPackerConfig& config);

bool shader_compile_read_file(const fs::path& filePath, std::vector<char>& outData);

namespace bgfx
{
    // impl in ~\bgfx\tools\shaderc\shaderc.cpp
    int compileShader(int _argc, const char* _argv[]);
}