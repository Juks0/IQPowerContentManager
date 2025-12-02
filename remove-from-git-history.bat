@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo Usuwanie dużych plików z historii git
echo ========================================
echo.
echo To usunie pliki z CAŁEJ historii commitów.
echo To może zająć kilka minut...
echo.
pause

echo [1/4] Usuwanie plików z historii używając git filter-branch...
git filter-branch --force --index-filter ^
    "git rm --cached --ignore-unmatch -r assetofolder/content/ assetofolder/dwrite.dll" ^
    --prune-empty --tag-name-filter cat -- --all

if errorlevel 1 (
    echo.
    echo BŁĄD: git filter-branch nie zadziałał.
    echo Sprawdzam czy git filter-repo jest dostępny...
    git filter-repo --path assetofolder/content/ --path assetofolder/dwrite.dll --invert-paths 2>nul
    if errorlevel 1 (
        echo.
        echo BŁĄD: Ani git filter-branch ani git filter-repo nie są dostępne.
        echo Musisz zainstalować git filter-repo lub użyć BFG Repo-Cleaner.
        pause
        exit /b 1
    )
)
echo.

echo [2/4] Czyszczenie referencji...
git for-each-ref --format="%%(refname)" refs/original/ | for /f "tokens=*" %%i in ('more') do git update-ref -d %%i 2>nul
echo.

echo [3/4] Czyszczenie reflog i optymalizacja...
git reflog expire --expire=now --all
git gc --prune=now --aggressive
echo.

echo [4/4] Sprawdzanie czy pliki zostały usunięte...
git log --all --full-history -- assetofolder/content/ assetofolder/dwrite.dll 2>nul | findstr /C:"assetofolder" >nul
if errorlevel 1 (
    echo ✓ Pliki zostały usunięte z historii!
) else (
    echo ⚠ Pliki mogą nadal być w historii. Sprawdzam...
    git log --all --full-history -- assetofolder/content/ assetofolder/dwrite.dll
)
echo.

echo Gotowe! Teraz możesz zrobić force push:
echo   git push origin main --force
echo.
pause

