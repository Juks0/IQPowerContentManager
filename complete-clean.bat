@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo Kompletne czyszczenie historii git
echo ========================================
echo.
echo To skrypt usunie duże pliki z CAŁEJ historii git.
echo.
echo UWAGA: To może zająć dużo czasu i zmieni historię!
echo.
pause

echo [1/6] Sprawdzanie aktualnego statusu...
git status --short
echo.

echo [2/6] Usuwanie plików z cache...
git rm --cached -r assetofolder/content/ 2>nul
git rm --cached assetofolder/dwrite.dll 2>nul
echo.

echo [3/6] Dodawanie .gitignore...
git add .gitignore
echo.

echo [4/6] Usuwanie plików z historii używając git filter-branch...
echo To może zająć kilka minut, proszę czekać...
git filter-branch --force --index-filter ^
    "git rm --cached --ignore-unmatch -r assetofolder/content/ assetofolder/dwrite.dll" ^
    --prune-empty --tag-name-filter cat -- --all
if errorlevel 1 (
    echo.
    echo BŁĄD: git filter-branch nie jest dostępny lub wystąpił błąd.
    echo Próbuję alternatywnego rozwiązania...
    goto :alternative
)
echo.

echo [5/6] Czyszczenie referencji i optymalizacja...
git for-each-ref --format="%%(refname)" refs/original/ | for /f %%i in ('more') do git update-ref -d %%i 2>nul
git reflog expire --expire=now --all
git gc --prune=now --aggressive
echo.

:alternative
echo [6/6] Tworzenie commita...
git commit -m "Add .gitignore and remove large files from repository" 2>nul
if errorlevel 1 (
    git commit --amend -m "Add .gitignore and remove large files from repository"
)
echo.

echo Gotowe! Teraz możesz zrobić force push:
echo   git push origin main --force
echo.
pause

