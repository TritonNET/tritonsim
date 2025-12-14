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
where nmake >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] nmake not found.
    pause
    exit /b 1
)

set MAKE_CMD=make
where make >nul 2>nul
if %errorlevel% neq 0 (
    where mingw32-make >nul 2>nul
    if !errorlevel! equ 0 (
        set MAKE_CMD=mingw32-make
    ) else (
        echo [ERROR] GNU make not found.
        pause
        exit /b 1
    )
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

cd /d "%BGFX_DIR%"
"%GENIE%" --gcc=wasm gmake
cd .build/projects/gmake-wasm

:: FIX: Use -fwasm-exceptions to match .NET 8
set "CXXFLAGS=-fwasm-exceptions"
set "CFLAGS=-fwasm-exceptions"

call emmake %MAKE_CMD% config=release
if %errorlevel% neq 0 (
    echo [ERROR] BGFX Web build failed.
    pause
    exit /b 1
)

set "CXXFLAGS="
set "CFLAGS="

:: ============================================================================
:: STEP 3: COMPILE TRITONSIM RENDERER (WASM) - MODERN EXCEPTIONS
:: ============================================================================
echo.
echo [INFO] Building TritonSimRenderer...

cd /d "%RENDERER_DIR%"
if exist build_wasm rmdir /s /q build_wasm
mkdir build_wasm
cd build_wasm

:: FIX: Pass -fwasm-exceptions to CMake
call emcmake cmake .. -G "NMake Makefiles" ^
    -DCMAKE_BUILD_TYPE=Debug ^
    -DCMAKE_CXX_FLAGS="-fwasm-exceptions"

if %errorlevel% neq 0 (
    echo [ERROR] CMake Configuration failed.
    pause
    exit /b 1
)

call emmake nmake
if %errorlevel% neq 0 (
    echo [ERROR] TritonSimRenderer Web build failed.
    pause
    exit /b 1
)

echo.
echo [SUCCESS] Build Complete.
pause