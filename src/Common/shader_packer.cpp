#include <pch.h>
#include "shader_packer.h"
#include <shader_def.h>
#include <shader_types.h>

namespace fs = std::filesystem;

// Magic number to identify our specific file format (e.g., "TSPK" in hex)
static const uint32_t kPackMagic = 0x5453504B;

bool ShaderPacker::Add(const ShaderDefinition& sdef)
{
    if (!fs::exists(sdef.bin_fragment)) {
        bx::printf("Error: Fragment shader not found: %s\n", sdef.bin_fragment.c_str());
        return false;
    }

    if (!fs::exists(sdef.bin_vertex)) {
        bx::printf("Error: Vertex shader not found: %s\n", sdef.bin_vertex.c_str());
        return false;
    }

    m_entries.push_back({from_shader_basename(sdef.type), sdef.bin_fragment, sdef.bin_vertex});
    return true;
}

bool ShaderPacker::Pack()
{
    std::ofstream outFile(m_bin, std::ios::binary);
    if (!outFile.is_open()) 
    {
        bx::printf("Error: Could not open pack file for writing: %s\n", m_bin.c_str());
        return false;
    }

    // Prepare Metadata
    // We have 2 shaders (VS + FS) per added Entry.
    uint32_t totalShaders = (uint32_t)m_entries.size() * 2;

    // Write Header: Magic + Count
    outFile.write(reinterpret_cast<const char*>(&kPackMagic), sizeof(kPackMagic));
    outFile.write(reinterpret_cast<const char*>(&totalShaders), sizeof(totalShaders));

    // Calculate where raw data starts
    // Start = HeaderSize + (NumberOfIndices * SizeOfIndex)
    uint32_t currentOffset = sizeof(uint32_t) * 2 + (totalShaders * sizeof(PackIndex));

    // We need to store data in a buffer to write it after the header
    std::vector<PackIndex> indices;
    std::vector<char> dataBlob;

    for (const auto& entry : m_entries)
    {
        // Helper lambda to process one file
        auto ProcessFile = [&](std::string path, ShaderStage stage) -> bool {
            std::ifstream inFile(path, std::ios::binary | std::ios::ate);
            if (!inFile) return false;

            std::streamsize size = inFile.tellg();
            inFile.seekg(0, std::ios::beg);

            if (size > 0)
            {
                // Create Index
                PackIndex idx;
                idx.type = entry.type;
                idx.stage = stage;
                idx.offset = currentOffset;
                idx.size = (uint32_t)size;
                indices.push_back(idx);

                // Read data into blob
                size_t oldSize = dataBlob.size();
                dataBlob.resize(oldSize + size);
                inFile.read(dataBlob.data() + oldSize, size);

                // Advance offset
                currentOffset += (uint32_t)size;
            }
            return true;
            };

        if (!ProcessFile(entry.vsPath, ShaderStage::Vertex)) return false;
        if (!ProcessFile(entry.fsPath, ShaderStage::Fragment)) return false;
    }

    // 2. Write Index Table
    for (const auto& idx : indices) {
        outFile.write(reinterpret_cast<const char*>(&idx), sizeof(PackIndex));
    }

    // 3. Write Data Blob
    outFile.write(dataBlob.data(), dataBlob.size());

    outFile.close();

    bx::printf("Successfully packed %d shaders into %s\n", totalShaders, m_bin.c_str());

    return true;
}

#ifndef SHADER_PACKER_TOOL // Unpack is only available outside the packer tool

bool ShaderPacker::Unpack(ShaderType type, ShaderStage stage, bgfx::ShaderHandle& handle/*out*/)
{
    if (!EnsureHeaderLoaded()) {
        return false;
    }

    // 1. Find the location in our lookup map
    auto key = std::make_pair(type, stage);
    auto it = m_lookup.find(key);

    if (it == m_lookup.end()) {
        bx::printf("Error: Shader not found in pack (Type: %d, Stage: %d)\n", (int)type, (int)stage);
        return false;
    }

    FileLocation loc = it->second;

    // 2. Open file and seek to offset
    std::ifstream inFile(m_bin, std::ios::binary);
    if (!inFile.is_open()) return false;

    inFile.seekg(loc.offset);

    // 3. Allocate BGFX memory
    // bgfx::alloc allocates memory that BGFX manages. 
    // It will automatically free this when the shader is destroyed or reference count hits zero.
    const bgfx::Memory* mem = bgfx::alloc(loc.size);

    // 4. Read directly into BGFX memory
    inFile.read(reinterpret_cast<char*>(mem->data), loc.size);

    if (!inFile) {
        bx::printf("Error: Failed to read shader data at offset %u\n", loc.offset);
        return false;
    }

    // 5. Create Shader
    handle = bgfx::createShader(mem);

    if (!bgfx::isValid(handle))
    {
        bx::printf("Error: BGFX failed to create shader from loaded memory.\n");
        return false;
    }

    // Set debug name for convenience
    bgfx::setName(handle, "UnpackedShader");

    return true;
}
#endif // !#ifndef SHADER_PACKER_TOOL

bool ShaderPacker::EnsureHeaderLoaded()
{
    if (m_headerLoaded) return true;

    std::ifstream inFile(m_bin, std::ios::binary);
    if (!inFile.is_open()) {
        bx::printf("Error: Could not open pack file: %s\n", m_bin.c_str());
        return false;
    }

    uint32_t magic = 0;
    uint32_t count = 0;

    inFile.read(reinterpret_cast<char*>(&magic), sizeof(magic));
    inFile.read(reinterpret_cast<char*>(&count), sizeof(count));

    if (magic != kPackMagic) {
        bx::printf("Error: Invalid pack file magic number.\n");
        return false;
    }

    // Read all indices
    for (uint32_t i = 0; i < count; ++i)
    {
        PackIndex idx;
        inFile.read(reinterpret_cast<char*>(&idx), sizeof(PackIndex));

        // Store in lookup map for fast runtime access
        m_lookup[{idx.type, idx.stage}] = { idx.offset, idx.size };
    }

    m_headerLoaded = true;
    return true;
}

bool shader_pack(const ShaderPackerConfig& config)
{
    ShaderPacker packer(config.output_file);

    for (const auto& shader : config.shaders)
        packer.Add(shader);

    return packer.Pack();
}