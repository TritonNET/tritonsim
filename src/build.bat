@echo off
setlocal enabledelayedexpansion

:: ============================================================================
:: 1. GLOBAL CONFIGURATION & DEFAULTS
:: ============================================================================
set "SRC_ROOT=%~dp0"
set "THIRDPARTY_DIR=%SRC_ROOT%3rdparty"
set "BGFX_DIR=%THIRDPARTY_DIR%\bgfx"
set "BX_DIR=%THIRDPARTY_DIR%\bx"
set "RENDERER_DIR=%SRC_ROOT%TritonSimRenderer"
set "GENIE=%BX_DIR%\tools\bin\windows\genie.exe"

:: Default Argument Values
set "BUILD_CONFIG=Release"
set "BUILD_PLATFORM=win_x64"

:: Paths for Web Build Tools
set "EMSDK_DIR=%THIRDPARTY_DIR%\emsdk"
set "EM_VERSION=3.1.34"

:: VS Environment Path (Adjust if using Professional/Community)
set "VS_VARS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Enterprise\VC\Auxiliary\Build\vcvars64.bat"

:: ============================================================================
:: 2. ARGUMENT PARSING
:: ============================================================================
:parse_loop
if "%~1"=="" goto :args_done
if /i "%~1"=="-Configuration" (
    set "BUILD_CONFIG=%~2"
    shift
    shift
    goto :parse_loop
)
if /i "%~1"=="-Platform" (
    set "BUILD_PLATFORM=%~2"
    shift
    shift
    goto :parse_loop
)
shift
goto :parse_loop
:args_done

echo [INFO] Target Platform: %BUILD_PLATFORM%
echo [INFO] Configuration:   %BUILD_CONFIG%

:: ============================================================================
:: 3. ENVIRONMENT PRE-CHECKS
:: ============================================================================
if not exist "%THIRDPARTY_DIR%" (
    echo [ERROR] Could not find 3rdparty directory at: %THIRDPARTY_DIR%
    exit /b 1
)

if not exist "%GENIE%" (
    echo [ERROR] Could not find GENie build tool at: %GENIE%
    exit /b 1
)

:: Auto-load Visual Studio Environment if tools are missing
where nmake >nul 2>nul
if %errorlevel% neq 0 (
    echo [INFO] MSVC tools not found in PATH. Attempting to load vcvars64.bat...
    if exist "%VS_VARS_PATH%" (
        call "%VS_VARS_PATH%" >nul
        echo [INFO] VS Environment loaded.
    ) else (
        echo [ERROR] Could not find vcvars64.bat and tools are not in PATH.
        echo         Please run from VS Developer Command Prompt or check VS_VARS_PATH.
        exit /b 1
    )
)

:: Branch to specific platform build
if /i "%BUILD_PLATFORM%"=="web_wasm" goto :build_web
if /i "%BUILD_PLATFORM%"=="win_x64"  goto :build_win
if /i "%BUILD_PLATFORM%"=="win_x86"  goto :build_win

echo [ERROR] Unknown Platform: %BUILD_PLATFORM%
exit /b 1

:: ============================================================================
:: 4. WINDOWS BUILD ROUTINE
:: ============================================================================
:build_win
echo.
echo [INFO] Starting Windows Build (%BUILD_PLATFORM%)...
echo --------------------------------------------------------------------------

cd /d "%BGFX_DIR%"

:: 1. Generate Projects
echo [INFO] Generating Visual Studio 2022 Solution...
"%GENIE%" --with-tools vs2022
if %errorlevel% neq 0 exit /b 1

:: 2. Build via MSBuild
set "SLN_FILE=%BGFX_DIR%\.build\projects\vs2022\bgfx.sln"
set "MSBUILD_PLATFORM=x64"
if /i "%BUILD_PLATFORM%"=="win_x86" set "MSBUILD_PLATFORM=x86"

echo [INFO] Building %BUILD_CONFIG% Configuration...
msbuild "%SLN_FILE%" /p:Configuration=%BUILD_CONFIG% /p:Platform=%MSBUILD_PLATFORM% /t:Build /v:minimal /maxcpucount
if %errorlevel% neq 0 (
    echo [ERROR] Windows build failed.
    exit /b 1
)

echo.
echo [SUCCESS] Windows Build Complete.
echo           Binaries in: %BGFX_DIR%\.build\win64_vs2022\bin\
goto :eof


:: ============================================================================
:: 5. WEB (WASM) BUILD ROUTINE
:: ============================================================================
:build_web
echo.
echo [INFO] Starting Web Assembly Build...
echo --------------------------------------------------------------------------

:: Check for Make (required for bgfx gmake)
where make >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] GNU 'make' not found in PATH.
    exit /b 1
)

:: 1. Setup Emscripten
echo [INFO] Activating Emscripten %EM_VERSION%...
pushd "%EMSDK_DIR%"
call emsdk.bat install %EM_VERSION% >nul
call emsdk.bat activate %EM_VERSION% >nul
call emsdk_env.bat
popd

:: FIX: GENie requires 'EMSCRIPTEN' env var to be set explicitly
if not defined EMSCRIPTEN (
    set "EMSCRIPTEN=%EMSDK_DIR%\upstream\emscripten"
    echo [INFO] Force-setting EMSCRIPTEN=!EMSCRIPTEN!
)

:: 2. Build BGFX
echo [INFO] Building BGFX (Native Wasm Exceptions)...
pushd "%BGFX_DIR%"
"%GENIE%" --gcc=wasm gmake
if %errorlevel% neq 0 (
    echo [ERROR] GENie generation failed.
    popd
    exit /b 1
)
popd

set "WASM_PROJECT_DIR=%BGFX_DIR%\.build\projects\gmake-wasm"

:: Verify directory was created
if not exist "%WASM_PROJECT_DIR%" (
    echo [ERROR] Project directory not found: %WASM_PROJECT_DIR%
    exit /b 1
)

pushd "%WASM_PROJECT_DIR%"

:: Convert Configuration to lowercase for gmake (Debug -> debug)
set "GMAKE_CONFIG=%BUILD_CONFIG%"
if /i "%GMAKE_CONFIG%"=="Release" set "GMAKE_CONFIG=release"
if /i "%GMAKE_CONFIG%"=="Debug"   set "GMAKE_CONFIG=debug"

set "CXXFLAGS=-fwasm-exceptions"
set "CFLAGS=-fwasm-exceptions"

call emmake make config=%GMAKE_CONFIG%
if %errorlevel% neq 0 (
    echo [ERROR] BGFX Web build failed.
    popd
    exit /b 1
)
set "CXXFLAGS="
set "CFLAGS="
popd

:: 3. Build TritonSimRenderer
echo [INFO] Building TritonSimRenderer...
set "RENDERER_DIR_WASM_BUILD_DIR=%RENDERER_DIR%\build_wasm"

if exist "%RENDERER_DIR_WASM_BUILD_DIR%" rmdir /s /q "%RENDERER_DIR_WASM_BUILD_DIR%"
mkdir "%RENDERER_DIR_WASM_BUILD_DIR%"
cd /d "%RENDERER_DIR_WASM_BUILD_DIR%"

call emcmake cmake .. -G "NMake Makefiles" -DCMAKE_BUILD_TYPE=%BUILD_CONFIG% -DCMAKE_CXX_FLAGS="-fwasm-exceptions"
if %errorlevel% neq 0 (
    echo [ERROR] CMake Configuration failed.
    exit /b 1
)

call emmake nmake
if %errorlevel% neq 0 (
    echo [ERROR] TritonSimRenderer Web build failed.
    exit /b 1
)

echo.
echo [SUCCESS] Web Build Complete.
goto :eof