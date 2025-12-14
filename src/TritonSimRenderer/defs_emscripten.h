#pragma once
#include <emscripten.h>

#ifndef EMSCRIPTEN_KEEPALIVE
#error EMSCRIPTEN_KEEPALIVE macro isnt defined. 
#endif // !EMSCRIPTEN_KEEPALIVE

// EMSCRIPTEN_KEEPALIVE ensures the function is exported and not removed by the optimizer/linker.
#define TRITON_EXPORT EMSCRIPTEN_KEEPALIVE