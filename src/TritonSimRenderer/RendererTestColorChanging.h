#pragma once
#include "RendererBase.h"

class RendererTestColorChanging : public RendererBase
{
public:
	RendererTestColorChanging(ShaderPacker* sp, const SimConfig& cfg) : RendererBase(sp, ShaderType::UnlitPrimitive, cfg) {}
	~RendererTestColorChanging() = default;

	ResponseCode RenderFrame() override;
protected:
	void OnUpdate() override;

private:
	uint32_t m_color;
};

