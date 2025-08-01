@echo off
echo Cleaning all .NET projects and removing build artifacts...

REM Clean all projects using dotnet clean
echo.
echo Running 'dotnet clean' on all projects...
dotnet clean

REM Remove all bin/ directories
echo.
echo Removing all bin/ directories...
for /d /r . %%d in (bin) do @if exist "%%d" (
    echo Removing: %%d
    rmdir /s /q "%%d" 2>nul
)

REM Remove all obj/ directories
echo.
echo Removing all obj/ directories...
for /d /r . %%d in (obj) do @if exist "%%d" (
    echo Removing: %%d
    rmdir /s /q "%%d" 2>nul
)

echo.
echo Clearing log files in .\AppData\logs\ ...
if exist "AppData\logs" (
    del /q /s "AppData\logs\*.*" >nul 2>&1
)


echo.
echo Cleanup completed successfully!
echo All build artifacts have been removed.