@echo off
REM Skrypt do uruchamiania API
REM Użycie: start-api.bat [port]

setlocal

set PORT=%1
if "%PORT%"=="" set PORT=8080

set SCRIPT_DIR=%~dp0
set EXE_PATH=%SCRIPT_DIR%bin\Debug\net48\IQPowerContentManager.exe

if not exist "%EXE_PATH%" (
    echo Błąd: Nie znaleziono pliku wykonywalnego w: %EXE_PATH%
    echo Upewnij się, że projekt został zbudowany.
    exit /b 1
)

echo Uruchamianie API na porcie %PORT%...
echo Adres: http://localhost:%PORT%
echo.

"%EXE_PATH%" --api http://localhost:%PORT%

endlocal

