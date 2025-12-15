@echo off
echo Generating Enums...

:: Run Python
python enum_compile.py

:: Check if Python failed
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Generation failed.
    exit /b 1
)

:: If we got here, it succeeded
echo.
echo [SUCCESS] Code generation complete.
exit /b 0