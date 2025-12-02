@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo Naprawianie git i push do GitHub
echo ========================================
echo.

echo [1/4] Usuwanie dużych plików z cache...
git rm --cached -r assetofolder/content/ 2>nul
git rm --cached assetofolder/dwrite.dll 2>nul

echo.
echo [2/4] Dodawanie .gitignore...
git add .gitignore

echo.
echo [3/4] Tworzenie commita...
git commit -m "Add .gitignore and remove large files from repository"

echo.
echo [4/4] Pushowanie do GitHub...
git push origin main

if errorlevel 1 (
    echo.
    echo BŁĄD podczas pushowania.
) else (
    echo.
    echo ✓ Sukces! Wszystkie zmiany zostały wypchnięte.
)

echo.
pause

