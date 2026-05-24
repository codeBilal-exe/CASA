@echo off
echo ==================================================
echo CASA - Server Compilation Script
echo ==================================================

echo.
echo [1/8] Compiling DNS Server...
gcc Servers\DNS_Server\DNS_Server.c -o Servers\DNS_Server\DNS_Server.exe -lws2_32
if %errorlevel% neq 0 (echo ERROR: DNS compilation failed!) else (echo Success!)

echo.
echo [2/8] Compiling Load Balancer...
gcc Servers\Load_Balancer\Load_Balancer.c -o Servers\Load_Balancer\Load_Balancer.exe -lws2_32
if %errorlevel% neq 0 (echo ERROR: Load Balancer compilation failed!) else (echo Success!)

echo.
echo [3/9] Compiling Apple Website Server...
gcc Websites\Apple\apple_Servers\apple_server.c -o Websites\Apple\apple_Servers\apple_server.exe -lws2_32
if %errorlevel% neq 0 (echo ERROR: Apple compilation failed!) else (echo Success!)

echo.
echo [4/9] Compiling Apple2 Website Server...
gcc Websites\Apple\apple_Servers\apple2_server.c -o Websites\Apple\apple_Servers\apple2_server.exe -lws2_32
if %errorlevel% neq 0 (echo ERROR: Apple2 compilation failed!) else (echo Success!)

echo.
echo [5/9] Compiling Apple3 Website Server...
gcc Websites\Apple\apple_Servers\apple3_server.c -o Websites\Apple\apple_Servers\apple3_server.exe -lws2_32
if %errorlevel% neq 0 (echo ERROR: Apple3 compilation failed!) else (echo Success!)

echo.
echo [6/9] Compiling Google Website Server...
gcc Websites\Google\google-Servers\google_server.c -o Websites\Google\google-Servers\google_server.exe -lws2_32
if %errorlevel% neq 0 (echo ERROR: Google compilation failed!) else (echo Success!)

echo.
echo [7/9] Compiling Google2 Website Server...
gcc Websites\Google\google-Servers\google2_server.c -o Websites\Google\google-Servers\google2_server.exe -lws2_32
if %errorlevel% neq 0 (echo ERROR: Google2 compilation failed!) else (echo Success!)

echo.
echo [8/9] Compiling GitHub Website Server...
gcc Websites\Github\github_server.c -o Websites\Github\github_server.exe -lws2_32
if %errorlevel% neq 0 (echo ERROR: GitHub compilation failed!) else (echo Success!)

echo.
echo [9/9] Compiling Youtube Website Server...
gcc Websites\Youtube\yoututbe-Servers\youtube_server.c -o Websites\Youtube\yoututbe-Servers\youtube_server.exe -lws2_32
if %errorlevel% neq 0 (echo ERROR: Youtube compilation failed!) else (echo Success!)

echo.
echo ==================================================
echo Compilation Finished!
echo ==================================================