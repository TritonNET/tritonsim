#pragma once
#include "pch.h"

class RendererBase
{
public:
	RendererBase(const SimConfig& cfg);
	virtual ~RendererBase();

	virtual ResponseCode Init();
	virtual ResponseCode UpdateConfig(const SimConfig& cfg);

	ResponseCode Start();
	ResponseCode Stop();
	virtual ResponseCode RenderFrame();
	virtual ResponseCode Shutdown();

protected:
	virtual void WorkerLoop();
	virtual void OnUpdate() {}
	virtual ResponseCode LoadFile(const char* path, const bgfx::Memory** mem);
	virtual ResponseCode LoadProgram(const char* vs, const char* fs, bgfx::ProgramHandle* handle);

protected:
	RendererType m_type;

	UINT32 m_width;
	UINT32 m_height;

	void* m_nwh;

	std::atomic<bool> m_running{ false };
	std::thread m_thread;

	const uint32_t m_resetFlags = BGFX_RESET_VSYNC | BGFX_RESET_MSAA_X16;
};

