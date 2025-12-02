@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo Usuwanie plików z historii i push
echo ========================================
echo.

echo [1/4] Usuwanie plików z historii git...
git filter-branch --force --index-filter "git rm --cached --ignore-unmatch -r assetofolder/content/ assetofolder/dwrite.dll" --prune-empty --tag-name-filter cat -- --all

if errorlevel 1 (
    echo.
    echo BŁĄD: git filter-branch nie zadziałał.
    echo Próbuję alternatywnego rozwiązania...
    echo.
    echo Używam git reset do usunięcia ostatniego commita...
    git reset --soft HEAD~1
    git add .gitignore
    git commit -m "Add .gitignore to exclude large files"
    goto :push
)

echo.
echo [2/4] Czyszczenie referencji...
git for-each-ref --format="%%(refname)" refs/original/ | for /f "tokens=*" %%i in ('more') do git update-ref -d %%i 2>nul

echo.
echo [3/4] Optymalizacja repozytorium...
git reflog expire --expire=now --all
git gc --prune=now --aggressive

:push
echo.
echo [4/4] Force push do GitHub...
echo.
echo UWAGA: To nadpisze historię na GitHub!
echo Czy na pewno chcesz kontynuować? (T/N)
set /p confirm=
if /i not "%confirm%"=="T" (
    echo Anulowano.
    pause
    exit /b 0
)

git push origin main --force

if errorlevel 1 (
    echo.
    echo BŁĄD podczas pushowania.
) else (
    echo.
    echo ✓ Sukces! Wszystkie zmiany zostały wypchnięte.
)

echo.
pause

