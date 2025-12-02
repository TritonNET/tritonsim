#pragma once
#include "defs.h"
#include "ResponseCode.h"


class RendererFactory
{
public:
	static ResponseCode CreateRenderer(const SimConfig& config, SimContext& ctx);
};

