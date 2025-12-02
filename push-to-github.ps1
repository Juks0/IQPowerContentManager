# Skrypt do pushowania zmian do GitHub
cd "C:\Users\kacpe\Documents\! PROGRAMMING PROJECTS\IQPowerContentManager"

Write-Host "Sprawdzanie statusu git..." -ForegroundColor Cyan
git status

Write-Host "`nSprawdzanie remote..." -ForegroundColor Cyan
git remote -v

Write-Host "`nUsuwanie starego remote (jeśli istnieje)..." -ForegroundColor Yellow
git remote remove origin 2>$null

Write-Host "Dodawanie nowego remote..." -ForegroundColor Cyan
git remote add origin https://github.com/Juks0/IQPowerContentManager.git

Write-Host "`nDodawanie wszystkich plików..." -ForegroundColor Cyan
git add -A

Write-Host "`nSprawdzanie co zostanie dodane..." -ForegroundColor Cyan
git status

Write-Host "`nCzy chcesz kontynuować z commit i push? (T/N)" -ForegroundColor Yellow
$response = Read-Host

if ($response -eq "T" -or $response -eq "t" -or $response -eq "Y" -or $response -eq "y") {
    Write-Host "`nTworzenie commita..." -ForegroundColor Cyan
    git commit -m "Initial commit - IQPower Content Manager"
    
    Write-Host "`nPushowanie do GitHub..." -ForegroundColor Cyan
    git push -u origin main --force
    
    Write-Host "`nGotowe!" -ForegroundColor Green
}
else {
    Write-Host "`nAnulowano." -ForegroundColor Yellow
}

