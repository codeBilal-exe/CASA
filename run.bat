    @echo off
setlocal enabledelayedexpansion
echo ==================================================
echo CASA - Client And Server Architecture Launcher
echo ==================================================

echo.
echo [STEP 0] Verifying Project Requirements...
set "requirements_met=true"

REM Check for .NET SDK
echo.
echo Checking for .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK is not installed!
    echo Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download
    set "requirements_met=false"
) else (
    for /f "tokens=*" %%i in ('dotnet --version') do (
        echo [OK] .NET SDK found: %%i
    )
)

REM Check for C compiler (gcc or clang) for compiling C servers
echo.
echo Checking for C compiler...
gcc --version >nul 2>&1
if errorlevel 1 (
    clang --version >nul 2>&1
    if errorlevel 1 (
        echo [WARNING] C compiler not found. C servers won't compile.
        echo Please install MinGW or LLVM from: https://mingw-w64.org or https://releases.llvm.org
        set "requirements_met=false"
    ) else (
        for /f "tokens=*" %%i in ('clang --version') do (
            echo [OK] Clang found: %%i
            goto :compiler_ok
        )
    )
) else (
    for /f "tokens=*" %%i in ('gcc --version') do (
        echo [OK] GCC found: %%i
        goto :compiler_ok
    )
)

:compiler_ok
REM Check for required project files
echo.
echo Checking for project files...
if not exist "CASA-Client\CASA-Client.csproj" (
    echo [ERROR] CASA-Client project file not found!
    set "requirements_met=false"
) else (
    echo [OK] CASA-Client project found
)

if "%requirements_met%"=="false" (
    echo.
    echo ==================================================
    echo [ERROR] Some requirements are missing!
    echo Please install the missing dependencies and try again.
    echo ==================================================
    pause
    exit /b 1
)

echo.
echo [OK] All requirements verified successfully!
echo.
echo [STEP 1] Checking for server binaries...
set "all_compiled=true"
if not exist "Servers\DNS_Server\DNS_Server.exe" set "all_compiled=false"
if not exist "Servers\Load_Balancer\Load_Balancer.exe" set "all_compiled=false"
if not exist "Websites\Apple\apple_Servers\apple_server.exe" set "all_compiled=false"
if not exist "Websites\Apple\apple_Servers\apple2_server.exe" set "all_compiled=false"
if not exist "Websites\Apple\apple_Servers\apple3_server.exe" set "all_compiled=false"
if not exist "Websites\Google\google-Servers\google_server.exe" set "all_compiled=false"
if not exist "Websites\Google\google-Servers\google2_server.exe" set "all_compiled=false"
if not exist "Websites\Github\github_server.exe" set "all_compiled=false"
if not exist "Websites\Youtube\yoututbe-Servers\youtube_server.exe" set "all_compiled=false"

if "%all_compiled%"=="false" (
    echo Binaries missing. Compiling all C servers...
    call Servers\DNS_Server\compile_servers.bat
) else (
    echo All binaries found. Skipping compilation.
)

echo.
echo [STEP 2] Restoring NuGet packages...
cd CASA-Client
dotnet restore

echo.
echo [STEP 3] Building project...
dotnet build

echo.
echo [STEP 4] Launching Browser Application...
dotnet run

echo.
echo ==================================================
echo Application Closed.
echo ==================================================
pause
