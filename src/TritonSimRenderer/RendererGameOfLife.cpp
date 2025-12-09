#include "pch.h"
#include "RendererGameOfLife.h"

RendererGameOfLife::RendererGameOfLife(ShaderPacker* sp, const SimConfig& cfg)
	: RendererBase(sp, ShaderType::UnlitPrimitive, cfg)
{

}