@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo Czyszczenie i push do GitHub
echo ========================================
echo.

echo [1/4] Usuwanie plików z cache...
git rm --cached -r assetofolder/content/ 2>nul
git rm --cached assetofolder/dwrite.dll 2>nul
echo.

echo [2/4] Dodawanie .gitignore...
git add .gitignore
echo.

echo [3/4] Tworzenie commita (amend ostatniego commita)...
git commit --amend -m "Add .gitignore and remove large files from repository"
echo.

echo [4/4] Force push do GitHub (UWAGA: nadpisze historię)...
echo.
echo Czy na pewno chcesz kontynuować? (T/N)
set /p confirm=
if /i "%confirm%"=="T" (
    git push origin main --force
    if errorlevel 1 (
        echo.
        echo BŁĄD podczas pushowania.
        echo Może być potrzebne użycie git filter-branch.
    ) else (
        echo.
        echo ✓ Sukces! Wszystkie zmiany zostały wypchnięte.
    )
) else (
    echo Anulowano.
)

echo.
pause

