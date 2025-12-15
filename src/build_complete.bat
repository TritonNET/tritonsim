@echo off
setlocal enabledelayedexpansion

set "PROJECT_DIR=%~dp0TritonSim.GUI.Browser"
set "BUILD_CONFIG=Debug"
set "TEMP_TARGETS=%PROJECT_DIR%\Directory.Build.targets"
set "WASM_FILE=%PROJECT_DIR%\bin\%BUILD_CONFIG%\net8.0-browser\wwwroot\_framework\dotnet.native.wasm"
set "LIB_FILE=%~dp0TritonSimRenderer\build_wasm\lib\libTritonSimRenderer.a"
set "OBJ_EXTRACT_DIR=%PROJECT_DIR%\obj\native_refs"
set "LLVM_AR=%~dp03rdparty\emsdk\upstream\bin\llvm-ar.exe"

echo [1/7] Checking Paths...
if not exist "%PROJECT_DIR%" (echo ERROR: Project missing & pause & exit /b 1)
if not exist "%LIB_FILE%" (echo ERROR: Library missing: %LIB_FILE% & pause & exit /b 1)
if not exist "%LLVM_AR%" (echo ERROR: llvm-ar missing & pause & exit /b 1)

echo [2/7] Cleaning old builds...
if exist "%PROJECT_DIR%\bin" rmdir /s /q "%PROJECT_DIR%\bin"
if exist "%PROJECT_DIR%\obj" rmdir /s /q "%PROJECT_DIR%\obj"
if exist "%TEMP_TARGETS%" del "%TEMP_TARGETS%"

echo [3/7] Extracting Object Files (The Nuclear Fix)...
if not exist "%OBJ_EXTRACT_DIR%" mkdir "%OBJ_EXTRACT_DIR%"
pushd "%OBJ_EXTRACT_DIR%"
"%LLVM_AR%" x "%LIB_FILE%"
if %errorlevel% neq 0 (echo ERROR: Extraction failed & popd & pause & exit /b 1)
popd

echo [4/7] Generating Build Config...
(
echo ^<Project^>
echo   ^<ItemGroup^>
echo      ^<NativeFileReference Include="%OBJ_EXTRACT_DIR%\*.o" /^>
echo   ^</ItemGroup^>
echo   ^<PropertyGroup^>
echo     ^<EmccFlags^>$^(EmccFlags^) -s EXPORTED_FUNCTIONS="['_tritonsim_test','tritonsim_test']" -s ERROR_ON_UNDEFINED_SYMBOLS=0^</EmccFlags^>
echo     ^<WasmBuildNative^>true^</WasmBuildNative^>
echo   ^</PropertyGroup^>
echo ^</Project^>
) > "%TEMP_TARGETS%"

echo [5/7] Building...
cd "%PROJECT_DIR%"
call dotnet build -c %BUILD_CONFIG%
set BUILD_STATUS=%errorlevel%

echo [6/7] Cleanup...
if exist "%TEMP_TARGETS%" del "%TEMP_TARGETS%"
if %BUILD_STATUS% neq 0 (echo ERROR: Build Failed & pause & exit /b 1)

echo [7/7] Verifying Result...
if not exist "%WASM_FILE%" (echo ERROR: WASM output missing & pause & exit /b 1)

python -c "path = r'%WASM_FILE%'; content = open(path, 'rb').read(); print('\n[CHECK RESULT]: ' + ('SUCCESS! Function Found.' if b'tritonsim_test' in content else 'FAILURE. Function Missing.'))"

echo.
pause