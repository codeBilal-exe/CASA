@echo off
echo ==================================================
echo MISS - Server Compilation Script
echo ==================================================

echo.
echo [1/4] Compiling DNS Server...
gcc Servers\DNS_Server\DNS_Server.c -o Servers\DNS_Server\DNS_Server.exe -lws2_32
if %errorlevel% neq 0 (echo ERROR: DNS compilation failed!) else (echo Success!)

echo.
echo [2/4] Compiling Apple Website Server...
gcc Websites\apple\apple_server.c -o Websites\apple\apple_server.exe -lws2_32
if %errorlevel% neq 0 (echo ERROR: Apple compilation failed!) else (echo Success!)

echo.
echo [3/4] Compiling Google Website Server...
gcc Websites\google\google_server.c -o Websites\google\google_server.exe -lws2_32
if %errorlevel% neq 0 (echo ERROR: Google compilation failed!) else (echo Success!)

echo.
echo [4/4] Compiling GitHub Website Server...
gcc Websites\github\github_server.c -o Websites\github\github_server.exe -lws2_32
if %errorlevel% neq 0 (echo ERROR: GitHub compilation failed!) else (echo Success!)

echo.
echo ==================================================
echo Compilation Finished!
echo ==================================================
pause
