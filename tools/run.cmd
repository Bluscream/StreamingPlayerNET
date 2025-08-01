@echo off

REM Check for debug flag
set "DEBUG_MODE=0"
if "%1"=="/debug" set "DEBUG_MODE=1"
if %DEBUG_MODE%==1 goto debug_build
echo Building and running StreamingPlayerNET...
goto after_debug_check

:debug_build
echo Building and running StreamingPlayerNET (DEBUG MODE)...

:after_debug_check

REM Change to the project root directory
cd /d "%~dp0.."

REM Build the project using the build script
if %DEBUG_MODE%==1 goto debug_build_call
call "tools\build.cmd"
goto after_build_call

:debug_build_call
call "tools\build.cmd" /debug

:after_build_call

if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b %ERRORLEVEL%
)

REM Run the executable
if %DEBUG_MODE%==1 goto debug_start_message
echo Starting StreamingPlayerNET...
goto after_start_message

:debug_start_message
echo Starting StreamingPlayerNET (DEBUG MODE)...

:after_start_message

@REM REM Check if executable exists
@REM if not exist "StreamingPlayerNET\bin\Debug\net9.0-windows\StreamingPlayerNET.exe" (
@REM     echo ERROR: Executable not found at StreamingPlayerNET\bin\Debug\net9.0-windows\StreamingPlayerNET.exe
@REM     pause
@REM     exit /b 1
@REM )

REM Check if yt-dlp.exe exists in the project root
if not exist "yt-dlp.exe" (
    echo yt-dlp.exe not found, downloading with PowerShell...
    powershell -Command "Invoke-WebRequest -Uri 'https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe' -OutFile 'yt-dlp.exe';exit"
    if exist "yt-dlp.exe" (
        echo yt-dlp.exe downloaded successfully.
    ) else (
        echo ERROR: Failed to download yt-dlp.exe
        pause
        exit /b 1
    )
)

if %DEBUG_MODE%==1 goto debug_launch
echo Executable found, launching...
echo Note: Application may take a moment to initialize services...
dotnet run --project StreamingPlayerNET\StreamingPlayerNET.csproj
@REM start /wait "StreamingPlayerNET" "StreamingPlayerNET\bin\Debug\net9.0-windows\StreamingPlayerNET.exe"
echo Application has exited.
goto end_script

:debug_launch
echo Executable found, launching in debug mode...
echo This will show all console output and help diagnose startup issues.
echo Press Ctrl+C to stop the application.
"StreamingPlayerNET\bin\Debug\net9.0-windows\StreamingPlayerNET.exe"
echo Application has exited with code: %ERRORLEVEL%
pause

:end_script 