@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo Usuwanie dużych plików z historii git
echo ========================================
echo.
echo UWAGA: To usunie pliki z całej historii git!
echo.
pause

echo [1/5] Sprawdzanie aktualnego statusu...
git status --short
echo.

echo [2/5] Usuwanie plików z cache (jeśli jeszcze są)...
git rm --cached -r assetofolder/content/ 2>nul
git rm --cached assetofolder/dwrite.dll 2>nul
echo.

echo [3/5] Usuwanie plików z historii używając git filter-branch...
echo To może zająć kilka minut...
git filter-branch --force --index-filter ^
    "git rm --cached --ignore-unmatch -r assetofolder/content/ assetofolder/dwrite.dll" ^
    --prune-empty --tag-name-filter cat -- --all
echo.

echo [4/5] Czyszczenie referencji...
git for-each-ref --format="%%(refname)" refs/original/ | for /f %%i in ('more') do git update-ref -d %%i
git reflog expire --expire=now --all
git gc --prune=now --aggressive
echo.

echo [5/5] Sprawdzanie czy pliki zostały usunięte...
git log --all --full-history -- assetofolder/content/ assetofolder/dwrite.dll
if errorlevel 1 (
    echo ✓ Pliki zostały usunięte z historii!
) else (
    echo ⚠ Pliki nadal mogą być w historii.
)
echo.

echo Gotowe! Teraz możesz zrobić push z --force
echo.
pause

