@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo Tworzenie nowego repozytorium od zera
echo ========================================
echo.
echo UWAGA: To usunie CAŁĄ historię git i stworzy nowe repozytorium!
echo.
pause

echo [1/6] Usuwanie starego remote...
git remote remove origin 2>nul
echo.

echo [2/6] Usuwanie folderu .git (cała historia)...
if exist ".git" (
    echo Usuwam folder .git...
    rmdir /s /q .git
    echo ✓ Folder .git usunięty.
) else (
    echo Folder .git nie istnieje.
)
echo.

echo [3/6] Inicjalizacja nowego repozytorium git...
git init
echo.

echo [4/6] Dodawanie .gitignore...
git add .gitignore
echo.

echo [5/6] Tworzenie pierwszego commita (tylko .gitignore)...
git commit -m "Initial commit - Add .gitignore"
echo.

echo [6/6] Dodawanie wszystkich plików (bez tych w .gitignore)...
git add .
git commit -m "Add project files (excluding large files)"
echo.

echo ✓ Nowe repozytorium utworzone!
echo.
echo Teraz możesz dodać remote i zrobić push:
echo   git remote add origin https://github.com/Juks0/IQPowerContentManager.git
echo   git branch -M main
echo   git push -u origin main --force
echo.
pause

