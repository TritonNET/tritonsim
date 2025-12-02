#pragma once
#include "defs.h"
#include "RendererType.h"
#include "ResponseCode.h"

class RendererBase
{
public:
	RendererBase(const SimConfig& cfg);

	ResponseCode Init();
	ResponseCode RenderFrame();
	ResponseCode Shutdown();
protected:
	RendererType m_type;

	UINT32 m_width;
	UINT32 m_height;

	void* m_nwh;
};

