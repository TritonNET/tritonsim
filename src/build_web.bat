@echo off
setlocal enabledelayedexpansion

:: ============================================================================
:: CONFIGURATION
:: ============================================================================
set "SRC_ROOT=%~dp0"
set "THIRDPARTY_DIR=%SRC_ROOT%3rdparty"
set "BGFX_DIR=%THIRDPARTY_DIR%\bgfx\"
set "RENDERER_DIR=%SRC_ROOT%TritonSimRenderer\"
set "RENDERER_DIR_WASM_BUILD_DIR=%RENDERER_DIR%build_wasm"
set "GENIE=%THIRDPARTY_DIR%\bx\tools\bin\windows\genie.exe"
set "EMSDK_DIR=%THIRDPARTY_DIR%\emsdk"
set "EM_VERSION=3.1.34"
set "VS_VARS=C:\Program Files\Microsoft Visual Studio\2022\Enterprise\VC\Auxiliary\Build\vcvars64.bat"

set "MAKE_PATH=C:\ProgramData\chocolatey\bin\make.exe"
set "CMAKE_PATH=C:\Program Files\CMake\bin\cmake.exe"
set "WASM_PROJECT_DIR=%BGFX_DIR%.build/projects/gmake-wasm"

:: ============================================================================
:: PRE-CHECKS
:: ============================================================================
call "%VS_VARS%"

echo [INFO] Checking environment...
where nmake >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] nmake not found.
    exit /b 1
)

set MAKE_CMD=make
where make >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] GNU make not found.
    exit /b 1
)

:: ============================================================================
:: STEP 1: SETUP EMSCRIPTEN
:: ============================================================================
echo [INFO] Setting up Emscripten...
pushd "%EMSDK_DIR%"
call emsdk.bat install %EM_VERSION%
call emsdk.bat activate %EM_VERSION%
call emsdk_env.bat
popd

:: ============================================================================
:: STEP 2: COMPILE BGFX (WASM) - MODERN EXCEPTIONS
:: ============================================================================
echo.
echo [INFO] Building BGFX (Native Wasm Exceptions)...

pushd "%BGFX_DIR%"
"%GENIE%" --gcc=wasm gmake
popd

pushd "%WASM_PROJECT_DIR%"

:: FIX: Use -fwasm-exceptions to match .NET 8
set "CXXFLAGS=-fwasm-exceptions"
set "CFLAGS=-fwasm-exceptions"

call emmake make config=release
if %errorlevel% neq 0 (
    echo [ERROR] BGFX Web build failed.
    exit /b 1
)

set "CXXFLAGS="
set "CFLAGS="
popd

:: ============================================================================
:: STEP 3: COMPILE TRITONSIM RENDERER (WASM) - MODERN EXCEPTIONS
:: ============================================================================
echo.
echo [INFO] Building TritonSimRenderer...

if exist "%RENDERER_DIR_WASM_BUILD_DIR%" rmdir /s /q "%RENDERER_DIR_WASM_BUILD_DIR%"
mkdir "%RENDERER_DIR_WASM_BUILD_DIR%"
cd "%RENDERER_DIR_WASM_BUILD_DIR%"

call emcmake cmake .. -G "NMake Makefiles" -DCMAKE_BUILD_TYPE=Debug -DCMAKE_CXX_FLAGS="-fwasm-exceptions"
if %errorlevel% neq 0 (
    echo [ERROR] CMake Configuration failed.
    exit /b 1
)

call emmake nmake
if %errorlevel% neq 0 (
    echo [ERROR] TritonSimRenderer Web build failed.
    exit /b 1
)

popd

echo.
echo [SUCCESS] Build Complete.
