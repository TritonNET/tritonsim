#pragma once
#include "pch.h"

#ifndef TRITONSIM_EMSCRIPTEN
#define EM_FUNC_SIG_V   0
#define EM_FUNC_SIG_VI  0
#define EM_FUNC_SIG_II  0
#define EM_FUNC_SIG_III 0
#endif

template <typename Sig, typename Func, typename... Args>
ResponseCode Dispatch(Sig emscriptenSig, Func func, Args... args)
{
#ifdef TRITONSIM_EMSCRIPTEN
    int rc = emscripten_sync_run_in_main_runtime_thread(emscriptenSig, func, args...);
    return static_cast<ResponseCode>(rc);
#else
    return func(args...);
#endif
}