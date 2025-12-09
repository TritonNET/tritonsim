// pch.h: This is a precompiled header file.
// Files listed below are compiled only once, improving build performance for future builds.
// This also affects IntelliSense performance, including code completion and many code browsing features.
// However, files listed here are ALL re-compiled if any one of them is updated between builds.
// Do not add files here that you will be updating frequently as this negates the performance advantage.

#ifndef PCH_H
#define PCH_H

#include <atomic>
#include <thread>
#include <fstream>
#include <vector>

#include <bgfx/bgfx.h>
#include <bgfx/platform.h>
#include <bx/commandline.h>
#include <bx/math.h>

// add headers that you want to pre-compile here
#include "framework.h"

#include <shader_types.h>
#include <shader_def.h>
#include <shader_packer.h>

#include "defs.h"
#include "RendererType.h"
#include "ResponseCode.h"
#include "RendererBase.h"

#endif //PCH_H
