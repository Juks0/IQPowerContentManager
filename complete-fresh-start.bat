@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo Kompletny restart - nowe repozytorium
echo ========================================
echo.
echo To skrypt:
echo 1. Usunie całą historię git
echo 2. Stworzy nowe repozytorium
echo 3. Doda .gitignore
echo 4. Utworzy commity
echo 5. Wypchnie do GitHub
echo.
echo UWAGA: To nadpisze repozytorium na GitHub!
echo.
pause

echo [1/7] Usuwanie starego remote...
git remote remove origin 2>nul
echo.

echo [2/7] Usuwanie folderu .git (cała historia)...
if exist ".git" (
    echo Usuwam folder .git...
    rmdir /s /q .git
    echo ✓ Folder .git usunięty.
) else (
    echo Folder .git nie istnieje.
)
echo.

echo [3/7] Inicjalizacja nowego repozytorium git...
git init
echo.

echo [4/7] Dodawanie .gitignore...
git add .gitignore
git commit -m "Initial commit - Add .gitignore"
echo.

echo [5/7] Dodawanie wszystkich plików (bez tych w .gitignore)...
git add .
git commit -m "Add project files (excluding large files)"
echo.

echo [6/7] Konfiguracja remote i brancha...
git remote add origin https://github.com/Juks0/IQPowerContentManager.git
git branch -M main
echo.

echo [7/7] Pushowanie do GitHub...
git push -u origin main --force

if errorlevel 1 (
    echo.
    echo BŁĄD podczas pushowania.
    echo Sprawdź czy masz dostęp do repozytorium.
) else (
    echo.
    echo ✓ Sukces! Nowe repozytorium zostało utworzone na GitHub.
    echo.
    echo Repozytorium jest teraz czyste i nie zawiera dużych plików.
)

echo.
pause

