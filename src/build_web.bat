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
set "EMSDK_DIR=%THIRDPARTY_DIR%\emsdk"
set "EM_VERSION=3.1.34"

:: ============================================================================
:: PRE-CHECKS
:: ============================================================================
echo [INFO] Checking environment...

:: 1. Check for Make
where make >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] 'make' not found.
    echo         BGFX requires 'make' to build for Web. 
    echo         Install it via Chocolatey: 'choco install make'
    pause
    exit /b 1
)

:: ============================================================================
:: STEP 1: SETUP EMSCRIPTEN 3.1.51
:: ============================================================================
echo.
echo [INFO] Setting up Emscripten %EM_VERSION%...
echo --------------------------------------------------------------------------

if not exist "%EMSDK_DIR%\emsdk.bat" (
    echo [ERROR] Emscripten submodule missing. Run: git submodule update --init --recursive
    pause
    exit /b 1
)

pushd "%EMSDK_DIR%"

:: 1. Install specific version
echo [INFO] Installing Emscripten %EM_VERSION%...
call emsdk.bat install %EM_VERSION%
if %errorlevel% neq 0 (
    echo [ERROR] Failed to install Emscripten %EM_VERSION%.
    popd
    pause
    exit /b 1
)

:: 2. Activate specific version
echo [INFO] Activating Emscripten %EM_VERSION%...
call emsdk.bat activate %EM_VERSION%
if %errorlevel% neq 0 (
    echo [ERROR] Failed to activate Emscripten %EM_VERSION%.
    popd
    pause
    exit /b 1
)

:: 3. Set Environment Variables for this session
call emsdk_env.bat
popd

:: Verify 'emcc' is now available
where emcc >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] 'emcc' not found after activation. Setup failed.
    pause
    exit /b 1
)

echo [INFO] Emscripten %EM_VERSION% is active.

:: ============================================================================
:: STEP 2: COMPILE BGFX (WASM)
:: ============================================================================
echo.
echo [INFO] Building BGFX for WebAssembly...
echo --------------------------------------------------------------------------

cd /d "%BGFX_DIR%"

:: 1. Generate Makefiles
"%GENIE%" --gcc=wasm gmake
if %errorlevel% neq 0 (
    echo [ERROR] GENie failed to generate WebMakefiles.
    pause
    exit /b 1
)

:: 2. Enter directory
cd .build/projects/gmake-wasm
if %errorlevel% neq 0 (
    echo [ERROR] Project directory not found: .build/projects/gmake-wasm
    pause
    exit /b 1
)

:: 3. Build Release
call emmake make config=release
if %errorlevel% neq 0 (
    echo [ERROR] BGFX Web build failed.
    pause
    exit /b 1
)

:: ============================================================================
:: STEP 3: COMPILE TRITONSIM RENDERER (WASM)
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

:: 1. Configure CMake
call emcmake cmake .. -G "Unix Makefiles" -DCMAKE_BUILD_TYPE=Debug
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
echo           Library: %RENDERER_DIR%\build_wasm\lib\libTritonSimRenderer.a
echo.
pause