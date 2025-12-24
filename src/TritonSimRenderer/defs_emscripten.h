#pragma once
#include <emscripten.h>
#include <emscripten/html5.h>
#include <emscripten/threading.h>
#include "..\ShaderBin\tritonsim_asm.h"
#include <emscripten/wasm_worker.h>

#ifndef EMSCRIPTEN_KEEPALIVE
#error EMSCRIPTEN_KEEPALIVE macro isnt defined. 
#endif // !EMSCRIPTEN_KEEPALIVE

// EMSCRIPTEN_KEEPALIVE ensures the function is exported and not removed by the optimizer/linker.
#define TRITON_EXPORT EMSCRIPTEN_KEEPALIVE
#define TRITONSIM_EMSCRIPTEN

#define MAX_CANVAS_ID_LENGTH 20