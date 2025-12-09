#include "pch.h"

using json = nlohmann::json;
using fpath = std::filesystem::path;

fpath GetExecutableDir()
{
    char buffer[MAX_PATH];
    GetModuleFileNameA(NULL, buffer, MAX_PATH);

    fpath exePath(buffer);

    return exePath.parent_path();
}

int main(int _argc, const char* _argv[])
{
    bx::CommandLine cmdLine(_argc, _argv);

	if (!cmdLine.hasArg('c', "config"))
	{
		bx::printf("Shader packer error: Please provide the configuration file using -c <file_path> or -config <file_path>.\n");
		return bx::kExitSuccess;
	}

	const char* cfg_path = cmdLine.findOption('c', "config");
	if (cfg_path == nullptr)
	{
		bx::printf("Error: Configuration argument found, but no file path was provided.\n");
		return bx::kExitFailure;
	}

    // All paths are relative to the exe
    const auto _fs_full = [&](const std::string& f) { return (GetExecutableDir() / fpath(f)).lexically_normal().string(); };

	std::ifstream fstream(_fs_full(cfg_path));
	if (!fstream.is_open())
	{
		bx::printf("Error: Could not open configuration file at: %s\n", cfg_path);
		return bx::kExitFailure;
	}

    std::string str((std::istreambuf_iterator<char>(fstream)), std::istreambuf_iterator<char>());
    try
    {
        json j = json::parse(str, nullptr, false);
        if (j.is_discarded())
        {
            bx::printf("Error: JSON parsing failed. The file likely has a syntax error.\n");
            return bx::kExitFailure;
        }

        auto config = j.get<ShaderPackerConfig>();
        config.output_file = _fs_full(config.output_file);
        config.tmp_dir = _fs_full(config.tmp_dir);

        for (auto& shader : config.shaders)
        {
            shader.file_fragment = _fs_full(shader.file_fragment);
            shader.file_vertex = _fs_full(shader.file_vertex);
            shader.file_varying_def = _fs_full(shader.file_varying_def);

            shader.bin_vertex = (fs::path(config.tmp_dir) / fpath(shader.file_vertex).filename().replace_extension(".bin")).string();
            shader.bin_fragment = (fs::path(config.tmp_dir) / fpath(shader.file_fragment).filename().replace_extension(".bin")).string();
        }

        if (!shader_compile(config))
        {
            bx::printf("Error: Shader compiler failed.\n");
            return bx::kExitFailure;
        }

        if (!shader_pack(config))
        {
            bx::printf("Error: Shader pack failed.\n");
            return bx::kExitFailure;
        }

        return bx::kExitSuccess;
    }
    catch (const json::parse_error& e)
    {
        bx::printf("JSON Parse Error: %s\n", e.what());
        return bx::kExitFailure;
    }
    catch (const std::exception& e)
    {
        bx::printf("General Error: %s\n", e.what());
        return bx::kExitFailure;
    }

    return bx::kExitSuccess;
}