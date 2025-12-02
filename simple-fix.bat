@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo Proste rozwiązanie - reset i nowy commit
echo ========================================
echo.
echo To usunie ostatni commit i stworzy nowy bez dużych plików.
echo.
pause

echo [1/3] Sprawdzanie ostatnich commitów...
git log --oneline -3
echo.

echo [2/3] Reset do commita przed dodaniem dużych plików...
echo Wpisz hash commita, do którego chcesz wrócić (lub naciśnij Enter dla HEAD~1):
set /p commit_hash=
if "%commit_hash%"=="" set commit_hash=HEAD~1

git reset --soft %commit_hash%
echo.

echo [3/3] Dodawanie tylko .gitignore i tworzenie nowego commita...
git add .gitignore
git commit -m "Add .gitignore to exclude large files"
echo.

echo Gotowe! Teraz możesz zrobić force push:
echo   git push origin main --force
echo.
pause

