#include "pch.h"
#include "utils.h"

std::filesystem::path get_executable_dir()
{
    char buffer[MAX_PATH];
    GetModuleFileNameA(NULL, buffer, MAX_PATH);

    std::filesystem::path exePath(buffer);

    return exePath.parent_path();
}

std::string get_full_path(const std::string& f)
{
    return (get_executable_dir() / std::filesystem::path(f)).lexically_normal().string();
}