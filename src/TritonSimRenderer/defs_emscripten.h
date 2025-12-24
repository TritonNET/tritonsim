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

#define ASSERT_MAIN_THREAD() \
	if (!emscripten_is_main_browser_thread()) { \
		LOG_DEBUG("Function must be called from the main browser thread!"); \
		return RC_INVALID_CALLER_THREAD_NOTMAIN; \
	}