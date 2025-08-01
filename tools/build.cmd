@echo off

REM Check for debug flag
set "DEBUG_MODE=0"
if "%1"=="/debug" set "DEBUG_MODE=1"

if %DEBUG_MODE%==1 goto debug_build_msg
echo Building StreamingPlayerNET solution...
goto after_build_msg

:debug_build_msg
echo Building StreamingPlayerNET solution (DEBUG MODE)...

:after_build_msg

REM Kill any running instances first
call "%~dp0kill.cmd"

REM Build Debug configuration
if %DEBUG_MODE%==1 goto debug_build_start
echo.
echo Building project...
goto after_build_start

:debug_build_start
echo Building project...

:after_build_start

dotnet build StreamingPlayerNET.sln --configuration Debug

if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Build completed successfully!
echo. 