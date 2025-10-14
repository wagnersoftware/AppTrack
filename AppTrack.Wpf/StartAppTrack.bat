@echo off
rem ======================================================
rem  Start-AppTrack.bat
rem  Starts AppTrack.Api.exe and AppTrack.WpfUi.exe
rem Waits until the API started 
rem ======================================================

rem === CONFIGURATION ===
set "APIPATH={enter your api path}"
set "UIPATH={enter your ui path}"
set "APIURL=http://localhost:5000/swagger"   rem Health URL
set "MAX_RETRIES=30"                        
set "DELAY_SECONDS=1"                      

rem === OPTIONAL PARAMETER (API/UI-Paths) ===
if not "%~1"=="" set "APIPATH=%~1"
if not "%~2"=="" set "UIPATH=%~2"

set "APIEXE=%APIPATH%\AppTrack.Api.exe"
set "UIEXE=%UIPATH%\AppTrack.WpfUi.exe"

rem === Check if exists ===
if not exist "%APIEXE%" (
  echo [Error] File not found %APIEXE%
  goto :eof
)
if not exist "%UIEXE%" (
  echo [Error] File not found: %UIEXE%
  goto :eof
)

rem === START API ===
echo [INFO] Starting API: %APIEXE%
set ASPNETCORE_ENVIRONMENT=Production
start "AppTrack API" cmd /k "cd /d %APIPATH% && AppTrack.Api.exe"
echo [INFO] API is starting ...

rem === HEALTHCHECK-LOOP ===
setlocal enabledelayedexpansion
set /a COUNT=0
:WAIT_LOOP
set /a COUNT+=1
rem Uses PowerShell, for getting HTTP status
for /f "usebackq delims=" %%A in (`powershell -NoLogo -Command "(Invoke-WebRequest -Uri '%APIURL%' -UseBasicParsing -TimeoutSec 2).StatusCode" 2^>nul`) do set STATUS=%%A

if "!STATUS!"=="200" (
    echo [OK] API has started. (Statuscode 200)
    goto START_UI
)

if !COUNT! geq %MAX_RETRIES% (
    echo [Error] API not reachable after %MAX_RETRIES% attempts.
    echo Aborting.
    goto :eof
)

echo [INFO] API not ready yet (Attempt !COUNT!/ %MAX_RETRIES%) ...
timeout /t %DELAY_SECONDS% >nul
goto WAIT_LOOP

:START_UI
rem === STARTING UI ===
echo [INFO] Starting UI: %UIEXE%
start "" "%UIEXE%"
echo [READY] APPTRACK is running.
endlocal
