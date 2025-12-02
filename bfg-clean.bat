@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo Czyszczenie używając BFG Repo-Cleaner
echo ========================================
echo.
echo BFG Repo-Cleaner jest szybszy niż git filter-branch.
echo Musisz najpierw pobrać: https://rtyley.github.io/bfg-repo-cleaner/
echo.
pause

echo [1/3] Tworzenie kopii zapasowej...
git clone --mirror . ..\IQPowerContentManager-backup.git
echo.

echo [2/3] Usuwanie plików używając BFG...
if exist "bfg.jar" (
    java -jar bfg.jar --delete-folders assetofolder/content --delete-files dwrite.dll
    echo.
    echo [3/3] Czyszczenie i optymalizacja...
    git reflog expire --expire=now --all
    git gc --prune=now --aggressive
    echo.
    echo Gotowe! Teraz możesz zrobić force push:
    echo   git push origin main --force
) else (
    echo BŁĄD: Nie znaleziono bfg.jar
    echo Pobierz z: https://rtyley.github.io/bfg-repo-cleaner/
    echo I umieść w katalogu projektu.
)
echo.
pause

