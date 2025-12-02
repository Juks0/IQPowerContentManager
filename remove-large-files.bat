@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ========================================
echo Usuwanie dużych plików z git cache
echo ========================================
echo.

echo [1/3] Usuwanie plików z cache git...
git rm --cached assetofolder/dwrite.dll
git rm --cached assetofolder/content/cars/cky_porsche992_gt3rs_2023/cky_porsche992_gt3rs_2023.kn5
git rm --cached assetofolder/content/tracks/ks_nordschleife/18.kn5
git rm --cached assetofolder/content/tracks/ks_nordschleife/ks_nordschleife.kn5
git rm --cached assetofolder/content/tracks/ks_nurburgring/ks_nurburgring.kn5
git rm --cached assetofolder/content/cars/cky_porsche992_gt3rs_2023/sfx/cky_porsche992_gt3rs_2023.bank

echo.
echo [2/3] Usuwanie wszystkich plików .kn5 i .bank z cache...
git rm --cached -r assetofolder/content/ 2>nul

echo.
echo [3/3] Sprawdzanie statusu...
git status --short

echo.
echo ✓ Gotowe! Teraz możesz zrobić commit i push.
echo.
pause

