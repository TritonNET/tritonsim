@echo off
setlocal enabledelayedexpansion

:: ============================================================================
:: CONFIGURATION
:: ============================================================================
set "PROJECT_NAME=TritonSim.GUI.Browser"
set "PROJECT_DIR=%~dp0%PROJECT_NAME%"
set "BUILD_CONFIG=Debug"
set "TEMP_TARGETS=%PROJECT_DIR%\Directory.Build.targets"

echo ============================================================================
echo  BUILDING C# WASM GUI (INJECTION METHOD)
echo ============================================================================

:: 1. CHECK PATHS
if not exist "%PROJECT_DIR%" (
    echo [ERROR] Could not find project directory: %PROJECT_DIR%
    pause
    exit /b 1
)

:: 2. AGGRESSIVE CLEAN
echo.
echo [INFO] Performing Hard Clean...
if exist "%PROJECT_DIR%\bin" rmdir /s /q "%PROJECT_DIR%\bin"
if exist "%PROJECT_DIR%\obj" rmdir /s /q "%PROJECT_DIR%\obj"

:: 3. INJECT FLAGS VIA TEMPORARY TARGETS FILE
:: This bypasses command-line quoting issues by writing valid XML directly.
echo.
echo [INFO] Generaring temporary build config...

(
echo ^<Project^>
echo   ^<PropertyGroup^>
echo     ^<EmccFlags^>$^(EmccFlags^) -s EXPORTED_FUNCTIONS="['_tritonsim_test','tritonsim_test']" -s ERROR_ON_UNDEFINED_SYMBOLS=0^</EmccFlags^>
echo   ^</PropertyGroup^>
echo ^</Project^>
) > "%TEMP_TARGETS%"

:: 4. BUILD
echo.
echo [INFO] Building Project...
cd "%PROJECT_DIR%"

call dotnet build -c %BUILD_CONFIG% -p:WasmBuildNative=true

set BUILD_STATUS=%errorlevel%

:: 5. CLEANUP
:: Delete the temp file so it doesn't mess up future builds
if exist "%TEMP_TARGETS%" del "%TEMP_TARGETS%"

if %BUILD_STATUS% neq 0 (
    echo.
    echo [ERROR] Build failed.
    pause
    exit /b 1
)

:: 6. SUCCESS
echo.
echo [SUCCESS] Build Complete!
pause