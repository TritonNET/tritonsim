@echo off
setlocal enabledelayedexpansion

set "SRC_ROOT=%~dp0"
set "THIRDPARTY_DIR=%SRC_ROOT%3rdparty"
set "BGFX_DIR=%THIRDPARTY_DIR%\bgfx"
set "BX_DIR=%THIRDPARTY_DIR%\bx"
set "GENIE=%BX_DIR%\tools\bin\windows\genie.exe"

echo [INFO] Checking environment...

:: Check if 3rdparty directory exists
if not exist "%THIRDPARTY_DIR%" (
    echo [ERROR] Could not find 3rdparty directory at: %THIRDPARTY_DIR%
    echo         Please ensure this script is placed in: D:\projects\tritonnet\tritonsim\src
    pause
    exit /b 1
)

:: Check if GENie tool exists
if not exist "%GENIE%" (
    echo [ERROR] Could not find GENie build tool at: %GENIE%
    echo         Ensure you have cloned 'bx' correctly.
    pause
    exit /b 1
)

:: Check if MSBuild is in path (User must run from Dev Command Prompt)
where msbuild >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] 'msbuild' command not found.
    echo         Please run this script from the "x64 Native Tools Command Prompt for VS 2022".
    pause
    exit /b 1
)

:: ============================================================================
:: STEP 1: GENERATE PROJECT FILES (GENie)
:: ============================================================================
echo.
echo [INFO] Generating Visual Studio 2022 Solution...
echo --------------------------------------------------------------------------

cd /d "%BGFX_DIR%"

:: --with-tools: Builds shaderc.exe, geometryc.exe, texturec.exe
"%GENIE%" vs2022

if %errorlevel% neq 0 (
    echo [ERROR] GENie failed to generate project files.
    pause
    exit /b 1
)

:: ============================================================================
:: STEP 2: BUILD LIBRARIES (Debug & Release)
:: ============================================================================
set "SLN_FILE=%BGFX_DIR%\.build\projects\vs2022\bgfx.sln"

echo.
echo [INFO] Building Debug Configuration...
echo --------------------------------------------------------------------------
msbuild "%SLN_FILE%" /p:Configuration=Debug /p:Platform=x64 /t:Build /v:minimal /maxcpucount
if %errorlevel% neq 0 (
    echo [ERROR] Debug build failed.
    pause
    exit /b 1
)

echo.
echo [INFO] Building Release Configuration...
echo --------------------------------------------------------------------------
msbuild "%SLN_FILE%" /p:Configuration=Release /p:Platform=x64 /t:Build /v:minimal /maxcpucount
if %errorlevel% neq 0 (
    echo [ERROR] Release build failed.
    pause
    exit /b 1
)

:: ============================================================================
:: SUCCESS
:: ============================================================================
echo.
echo [SUCCESS] All BGFX libraries compiled successfully!
echo           Binaries located in: %BGFX_DIR%\.build\win64_vs2022\bin\
echo.
pause