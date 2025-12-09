#include <pch.h>
#include "shader_compiler.h"

bool shader_compile(const ShaderPackerConfig& config)
{
    try 
    {
        fs::create_directories(config.tmp_dir);
    }
    catch (std::exception& e) 
    {
        bx::printf("Error creating output directory: %s\n", e.what());
        return false;
    }

    bool success = true;
    for (const auto& platform : config.platforms)
    {
        for (const auto& shader : config.shaders)
        {
            if (!shader_compile_cli(shader.file_vertex, shader.bin_vertex, shader.file_varying_def, ShaderStage::Vertex, platform, config))
            {
                bx::printf("Failed to compile vertex shader: %s\n", shader.file_vertex.c_str());
                success = false;
            }

            if (!shader_compile_cli(shader.file_fragment, shader.bin_fragment, shader.file_varying_def, ShaderStage::Fragment, platform, config))
            {
                bx::printf("Failed to compile fragment shader: %s\n", shader.file_fragment.c_str());
                success = false;
            }
        }
    }

    return success;
}

bool shader_compile_cli(const str& sourceFile, const str& outputFile, const str& varyingPath, ShaderStage stage, const str& platform, const ShaderPackerConfig& config)
{
    if (sourceFile.empty()) return true;

    std::vector<std::string> args;

    args.push_back("shaderc");

    args.push_back("-f");
    args.push_back(sourceFile);

    args.push_back("-o");
    args.push_back(outputFile);

    args.push_back("--type");
    args.push_back(to_stage_str(stage));

    args.push_back("--platform");
    args.push_back(platform);

    args.push_back("-p");
    args.push_back(config.profile);

    args.push_back("-O");
    args.push_back("3");

    for (const auto& dep : config.dependencies)
    {
        args.push_back("-i");
        args.push_back(dep);
    }

    if (!varyingPath.empty())
    {
        args.push_back("--varyingdef");
        args.push_back(varyingPath);
    }

    std::vector<const char*> c_args;
    c_args.reserve(args.size());
    for (const auto& arg : args)
        c_args.push_back(arg.c_str());

    bx::printf("Compiling %s...\n", sourceFile.c_str());

    int result = bgfx::compileShader((int)c_args.size(), c_args.data());

    return (result == bx::kExitSuccess);
}