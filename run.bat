@echo off
echo ==================================================
echo MISS - Mini Internet Simulation System Launcher
echo ==================================================

cd MISS

echo.
echo [STEP 1] Checking for server binaries...
set "all_compiled=true"
if not exist "Servers\DNS_Server\DNS_Server.exe" set "all_compiled=false"
if not exist "Websites\apple\apple_server.exe" set "all_compiled=false"
if not exist "Websites\google\google_server.exe" set "all_compiled=false"
if not exist "Websites\github\github_server.exe" set "all_compiled=false"

if "%all_compiled%"=="false" (
    echo Binaries missing. Compiling all C servers...
    call compile_servers.bat
) else (
    echo All binaries found. Skipping compilation.
)

echo.
echo [STEP 2] Launching Browser Application...
cd Browser
dotnet run

echo.
echo ==================================================
echo Application Closed.
echo ==================================================
pause
