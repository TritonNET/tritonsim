#pragma once
#include "RendererBase.h"

class RendererTest: public RendererBase
{
public:
	RendererTest(const SimConfig& cfg);
	~RendererTest();

	ResponseCode RenderFrame() override;
protected:
	void RunAsync() override;

private:
	uint32_t m_color;
};

