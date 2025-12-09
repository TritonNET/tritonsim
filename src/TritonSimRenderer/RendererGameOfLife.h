#pragma once
#include "RendererBase.h"

class RendererGameOfLife: public RendererBase
{
public:
	RendererGameOfLife(ShaderPacker* sp, const SimConfig& cfg);
};

