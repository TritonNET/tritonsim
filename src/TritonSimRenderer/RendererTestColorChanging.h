#pragma once
#include "RendererBase.h"

class RendererTestColorChanging : public RendererBase
{
public:
	RendererTestColorChanging(const SimConfig& cfg) : RendererBase(cfg) {}
	~RendererTestColorChanging() = default;

	ResponseCode RenderFrame() override;
protected:
	void OnUpdate() override;

private:
	uint32_t m_color;
};

