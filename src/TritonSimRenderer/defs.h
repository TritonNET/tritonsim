#pragma once
#include "ResponseCode.h"
#include "RendererType.h"

class RendererBase;  // forward declare

struct SimConfig
{
    void* handle;
    int width;
    int height;
    RendererType Type;
};

struct SimContext
{
    RendererBase* Renderer{};
};