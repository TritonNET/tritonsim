@echo off
setlocal enabledelayedexpansion

:: ============================================================================
:: CONFIGURATION
:: ============================================================================
set "SRC_ROOT=%~dp0"
set "THIRDPARTY_DIR=%SRC_ROOT%3rdparty"
set "BGFX_DIR=%THIRDPARTY_DIR%\bgfx"
set "RENDERER_DIR=%SRC_ROOT%TritonSimRenderer"
set "GENIE=%THIRDPARTY_DIR%\bx\tools\bin\windows\genie.exe"

:: ============================================================================
:: PRE-CHECKS
:: ============================================================================
echo [INFO] Checking environment...

:: 1. Check for Emscripten
where emcc >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] 'emcc' not found. 
    echo         Please activate Emscripten SDK (emsdk_env.bat) before running this script.
    pause
    exit /b 1
)

:: 2. Check for Make
where make >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] 'make' not found.
    echo         BGFX requires 'make' to build for Web. 
    echo         Install it via Chocolatey: 'choco install make'
    pause
    exit /b 1
)

:: ============================================================================
:: STEP 1: COMPILE BGFX (WASM)
:: ============================================================================
echo.
echo [INFO] Building BGFX for WebAssembly...
echo --------------------------------------------------------------------------

cd /d "%BGFX_DIR%"

:: 1. Generate Makefiles for Emscripten
"%GENIE%" --gcc=wasm-emscripten gmake
if %errorlevel% neq 0 (
    echo [ERROR] GENie failed to generate WebMakefiles.
    pause
    exit /b 1
)

:: 2. Build Release Config using Emscripten's Make wrapper
cd .build/projects/gmake-wasm-emscripten
call emmake make config=release
if %errorlevel% neq 0 (
    echo [ERROR] BGFX Web build failed.
    pause
    exit /b 1
)

:: ============================================================================
:: STEP 2: COMPILE TRITONSIM RENDERER (WASM)
:: ============================================================================
echo.
echo [INFO] Building TritonSimRenderer for WebAssembly...
echo --------------------------------------------------------------------------

cd /d "%RENDERER_DIR%"

if exist build_wasm (
    echo [INFO] Cleaning old Web build...
    rmdir /s /q build_wasm
)
mkdir build_wasm
cd build_wasm

:: 1. Configure CMake with Emscripten Toolchain
call emcmake cmake .. -DCMAKE_BUILD_TYPE=Release
if %errorlevel% neq 0 (
    echo [ERROR] CMake Configuration failed.
    pause
    exit /b 1
)

:: 2. Build
call emmake make
if %errorlevel% neq 0 (
    echo [ERROR] TritonSimRenderer Web build failed.
    pause
    exit /b 1
)

:: ============================================================================
:: SUCCESS
:: ============================================================================
echo.
echo [SUCCESS] Web Build Complete!
echo           Library: %RENDERER_DIR%\build_wasm\libTritonSimRenderer.a
echo.
pause