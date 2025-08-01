@echo off
echo Terminating any running instances of StreamingPlayerNET...

REM Kill any running StreamingPlayerNET processes
taskkill /f /im "StreamingPlayerNET.exe" >nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo Successfully terminated running StreamingPlayerNET processes.
) else (
    echo No running StreamingPlayerNET processes found.
)

REM Also kill any dotnet processes that might be holding onto the executable
taskkill /f /im "dotnet.exe" >nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo Terminated dotnet processes that might be holding file locks.
) else (
    echo No dotnet processes found.
)

echo.
echo Kill operation completed.
echo. 