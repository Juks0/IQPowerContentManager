@echo off
cd /d "C:\Users\kacpe\Documents\! PROGRAMMING PROJECTS\IQPowerContentManager"

echo Sprawdzanie statusu git...
git status

echo.
echo Sprawdzanie remote...
git remote -v

echo.
echo Usuwanie starego remote (jeśli istnieje)...
git remote remove origin 2>nul

echo Dodawanie nowego remote...
git remote add origin https://github.com/Juks0/IQPowerContentManager.git

echo.
echo Dodawanie wszystkich plikow...
git add -A

echo.
echo Sprawdzanie co zostanie dodane...
git status

echo.
echo Tworzenie commita...
git commit -m "Initial commit - IQPower Content Manager"

echo.
echo Pushowanie do GitHub...
git push -u origin main --force

echo.
echo Gotowe!
pause

