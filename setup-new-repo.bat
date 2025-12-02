@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo Konfiguracja nowego repozytorium
echo ========================================
echo.

echo [1/3] Dodawanie remote...
git remote add origin https://github.com/Juks0/IQPowerContentManager.git
echo.

echo [2/3] Ustawianie brancha na main...
git branch -M main
echo.

echo [3/3] Pushowanie do GitHub (nadpisze istniejące repozytorium)...
echo.
echo UWAGA: To nadpisze repozytorium na GitHub!
echo Czy na pewno chcesz kontynuować? (T/N)
set /p confirm=
if /i not "%confirm%"=="T" (
    echo Anulowano.
    pause
    exit /b 0
)

git push -u origin main --force

if errorlevel 1 (
    echo.
    echo BŁĄD podczas pushowania.
    echo Sprawdź czy masz dostęp do repozytorium.
) else (
    echo.
    echo ✓ Sukces! Nowe repozytorium zostało utworzone na GitHub.
)

echo.
pause

