#include "pch.h"

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
	std::ifstream fstream(get_full_path(cfg_path));
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

        const auto config = j.get<shader_packer_config>();
        if (!shader_compile_pack(config))
        {
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