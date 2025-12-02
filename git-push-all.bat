@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo Pushowanie do GitHub
echo ========================================
echo.

echo [1/5] Sprawdzanie statusu...
git status --short
echo.

echo [2/5] Dodawanie wszystkich plików...
git add -A
echo.

echo [3/5] Tworzenie commita...
git commit -m "Update: IQPower Content Manager" 2>nul
if errorlevel 1 (
    echo Brak zmian do commitowania.
) else (
    echo Commit utworzony.
)
echo.

echo [4/5] Sprawdzanie remote...
git remote set-url origin https://github.com/Juks0/IQPowerContentManager.git
git remote -v
echo.

echo [5/5] Pushowanie do GitHub (main)...
git push -u origin main --force
if errorlevel 1 (
    echo.
    echo BŁĄD: Nie udało się wypchnąć zmian.
    echo Sprawdź czy masz dostęp do repozytorium.
) else (
    echo.
    echo ✓ Sukces! Wszystkie zmiany zostały wypchnięte.
)

echo.
pause

