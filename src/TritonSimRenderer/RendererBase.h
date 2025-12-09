#pragma once
#include "pch.h"

class RendererBase
{
public:
	RendererBase(ShaderPacker* sp/*takes ownership*/, ShaderType st, const SimConfig& cfg);
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
	virtual ResponseCode LoadProgram(bgfx::ProgramHandle* handle);

protected:
	RendererType m_type;
	ShaderType m_st;

	ShaderPacker* m_sp;

	UINT32 m_width;
	UINT32 m_height;

	void* m_nwh;

	std::atomic<bool> m_running{ false };
	std::thread m_thread;

	const uint32_t m_resetFlags = BGFX_RESET_VSYNC | BGFX_RESET_MSAA_X16;

	uint32_t m_backgroundColor = COLOR_BLACK; // black
};

