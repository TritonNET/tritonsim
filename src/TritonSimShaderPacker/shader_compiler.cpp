#include <pch.h>
#include "shader_compiler.h"

bool shader_compile_pack(const shader_packer_config& config)
{
    scitems items;
    if (!shader_compute_compile_items(config, items))
    {
        bx::printf("Error: Shader compiler error in computing items.\n");
        return bx::kExitFailure;
    }

    for (const auto& [platform, shaders] : items)
    {
        for (const auto& [tyname, spair] : shaders)
        {
            if (!shader_compile_cli(spair.vertex))
                return false;

            if (!shader_compile_cli(spair.fragment))
                return false;
        }
    }

    for (const auto& [platform, shaders] : items)
    {
        ShaderPacker packer(get_full_path(platform.output_file));

        for (const auto& [tyname, spair] : shaders)
            packer.Add(tyname, spair.fragment.bin_path, spair.vertex.bin_path);

        if (!packer.Pack())
            return false;
    }

    return true;
}

bool shader_compute_compile_items(const shader_packer_config& config, scitems& items)
{
    const auto tmp_dir = get_full_path(config.tmp_dir);

    try
    {
        fs::remove_all(tmp_dir);
        fs::create_directories(tmp_dir);
    }
    catch (std::exception& e)
    {
        bx::printf("Error creating output directory: %s\n", e.what());
        return false;
    }

    for (const auto& [platform, platform_config] : config.platforms)
    {
        for (const auto& shader : config.shaders)
        {
            shader_compile_item v;
            v.source_path = get_full_path(shader.file_vertex);
            v.varying_path = get_full_path(shader.file_varying_def);
            v.bin_path = (fpath(tmp_dir) / fpath(shader.file_vertex).filename().replace_extension("." + platform + ".bin")).string();
            v.stage = ShaderStage::Vertex;
            v.platform = platform;
            v.profile = platform_config.profile;
            v.dependencies = config.dependencies;

            shader_compile_item f;
            f.source_path = get_full_path(shader.file_fragment);
            f.varying_path = get_full_path(shader.file_varying_def);
            f.bin_path = (fpath(tmp_dir) / fpath(shader.file_fragment).filename().replace_extension("." + platform + ".bin")).string();
            f.stage = ShaderStage::Fragment;
            f.platform = platform;
            f.profile = platform_config.profile;
            f.dependencies = config.dependencies;

            shader_compile_platform scp;
            scp.platform = platform;
            scp.output_file = get_full_path(platform_config.output_file);

            fs::remove(scp.output_file);

            items[scp][shader.type] = { v, f };
        }
    }
}

bool shader_compile_cli(const shader_compile_item& item)
{
    if (item.source_path.empty()) return true;

    std::vector<std::string> args;

    args.push_back("shaderc");

    args.push_back("-f");
    args.push_back(item.source_path);

    args.push_back("-o");
    args.push_back(item.bin_path);

    args.push_back("--type");
    args.push_back(to_stage_str(item.stage));

    args.push_back("--platform");
    args.push_back(item.platform);

    args.push_back("-p");
    args.push_back(item.profile);

    args.push_back("-O");
    args.push_back("3");

    for (const auto& dep : item.dependencies)
    {
        args.push_back("-i");
        args.push_back(dep);
    }

    if (!item.varying_path.empty())
    {
        args.push_back("--varyingdef");
        args.push_back(item.varying_path);
    }

    std::vector<const char*> c_args;
    c_args.reserve(args.size());
    for (const auto& arg : args)
        c_args.push_back(arg.c_str());

    bx::printf("Compiling %s...\n", item.source_path.c_str());

    int result = bgfx::compileShader((int)c_args.size(), c_args.data());

    return (result == bx::kExitSuccess);
}