#pragma once
#include "pch.h"

struct ShaderBin
{
	ShaderType type;
};

class ShaderPacker
{
public:
	ShaderPacker(const std::string& packBin) : m_bin(packBin) {}

	bool Add(const ShaderDefinition& sdef);
	bool Pack();

#ifndef SHADER_PACKER_TOOL
	// SHADER_PACKER_TOOL defines to limit the inclusion of GPU context.
	bool Unpack(ShaderType type, ShaderStage stage, bgfx::ShaderHandle& handle/*out*/);
#endif // !SHADER_PACKER_TOOL

private:
	bool EnsureHeaderLoaded();

private:
	std::string m_bin{};

	struct Entry {
		ShaderType type;
		std::string fsPath;
		std::string vsPath;
	};
	std::vector<Entry> m_entries;

	struct PackIndex {
		ShaderType type;
		ShaderStage stage;
		uint32_t offset;
		uint32_t size;
	};

	struct FileLocation {
		uint32_t offset;
		uint32_t size;
	};
	std::map<std::pair<ShaderType, ShaderStage>, FileLocation> m_lookup;
	bool m_headerLoaded = false;
};

bool shader_pack(const ShaderPackerConfig& config);