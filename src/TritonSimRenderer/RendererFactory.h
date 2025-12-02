#pragma once
#include "pch.h"

class RendererFactory
{
public:
	static ResponseCode CreateRenderer(const SimConfig& config, SimContext& ctx);
};

