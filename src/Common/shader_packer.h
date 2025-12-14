#pragma once
#include <pch.h>

struct ShaderBin
{
	ShaderType type;
};

class ShaderPacker
{
public:
	ShaderPacker(const std::string& packBin) 
		: m_storageType(stFile)
		, m_bin(packBin)
		, m_data(nullptr)
		, m_size(0)
	{ }

	ShaderPacker(const void* data, size_t size)
		: m_storageType(stResource)		
		, m_bin()
		, m_data(static_cast<const uint8_t*>(data))
		, m_size(size)
	{

	}

	bool Add(const std::string& typen, const std::string& bin_fragment, const std::string& bin_vertex);
	bool Pack();

#ifndef SHADER_PACKER_TOOL
	// SHADER_PACKER_TOOL defines to limit the inclusion of GPU context.
	bool Unpack(ShaderType type, ShaderStage stage, bgfx::ShaderHandle& handle/*out*/);
#endif // !SHADER_PACKER_TOOL

private:
	bool EnsureHeaderLoaded();

private:
	enum StorageType { stResource, stFile };

	const StorageType m_storageType;

	/*For File storage type*/
	std::string m_bin{};

	/*For resource storage type*/
	const uint8_t* m_data;
	const size_t m_size;

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