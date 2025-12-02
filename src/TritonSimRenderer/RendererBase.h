#pragma once
#include "pch.h"

class RendererBase
{
public:
	RendererBase(const SimConfig& cfg);
	virtual ~RendererBase();

	ResponseCode Init();
	ResponseCode Start();
	ResponseCode Stop();
	virtual ResponseCode RenderFrame();
	virtual ResponseCode Shutdown();

protected:
	virtual void RunAsync();

protected:
	RendererType m_type;

	UINT32 m_width;
	UINT32 m_height;

	void* m_nwh;

	std::atomic<bool> m_running{ false };
	std::thread m_thread;
};

