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
            // Compile Vertex Shader
            if (!shader_compile_cli(shader.file_vertex, config.tmp_dir, shader.file_varying_def, "vertex", platform, config))
            {
                bx::printf("Failed to compile vertex shader: %s\n", shader.file_vertex.c_str());
                success = false;
            }

            // Compile Fragment Shader
            if (!shader_compile_cli(shader.file_fragment, config.tmp_dir, shader.file_varying_def, "fragment", platform, config))
            {
                bx::printf("Failed to compile fragment shader: %s\n", shader.file_fragment.c_str());
                success = false;
            }
        }
    }

    return success;
}

bool shader_compile_cli(
    const std::string& sourceFile,
    const std::string& outputDir,
    const std::string& varyingPath,
    const std::string& type, // "vertex", "fragment", "compute"
    const std::string& platform,
    const ShaderPackerConfig& config)
{
    if (sourceFile.empty()) return true;

    fs::path inPath(sourceFile);

    // Construct Output Path: output_dir / filename.bin
    fs::path outPath = fs::path(outputDir) / inPath.filename().replace_extension(".bin");

    // --- Build Argument List (std::string) ---
    std::vector<std::string> args;

    args.push_back("shaderc");

    args.push_back("-f");
    args.push_back(inPath.string());

    args.push_back("-o");
    args.push_back(outPath.string());

    args.push_back("--type");
    args.push_back(type);

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

    args.push_back("-i");
    args.push_back(inPath.parent_path().string());

    if (!varyingPath.empty())
    {
        args.push_back("--varyingdef");
        args.push_back(varyingPath);
    }

    std::vector<const char*> c_args;
    c_args.reserve(args.size());
    for (const auto& arg : args)
        c_args.push_back(arg.c_str());

    bx::printf("Compiling %s...\n", inPath.filename().string().c_str());

    int result = bgfx::compileShader((int)c_args.size(), c_args.data());

    return (result == bx::kExitSuccess);
}

bool shader_compile_read_file(const fs::path& filePath, std::vector<char>& outData)
{
    std::ifstream file(filePath, std::ios::binary | std::ios::ate);
    if (!file.is_open()) return false;

    std::streamsize size = file.tellg();
    file.seekg(0, std::ios::beg);

    if (size <= 0) return false;

    // Allocate size + padding (original code added 16384 padding)
    outData.resize(size + 16384);

    // Read directly into the vector
    if (!file.read(outData.data(), size)) return false;

    // Handle BOM (Byte Order Mark)
    char* data = outData.data();
    if (size >= 3 && data[0] == '\xef' && data[1] == '\xbb' && data[2] == '\xbf')
    {
        // Shift data to remove BOM
        memmove(data, data + 3, size - 3);
        size -= 3;
    }

    // Ensure null termination and empty line at EOF (required by glslang)
    data[size] = '\n';
    memset(&data[size + 1], 0, outData.size() - size - 1);

    return true;
}