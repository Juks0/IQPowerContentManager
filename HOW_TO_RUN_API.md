# Jak uruchomić API

**API uruchamia się automatycznie przy starcie aplikacji!**

## Automatyczne uruchomienie (domyślne)

1. Uruchom aplikację `IQPowerContentManager.exe`
2. API zostanie automatycznie uruchomione na adresie: `http://localhost:8080`
3. Aby zatrzymać API, naciśnij **Enter** w konsoli

## Niestandardowy port

Możesz uruchomić API na innym porcie podając URL jako argument:

### Z linii poleceń (najprostsza - użyj skryptu)

### PowerShell:
```powershell
# Z głównego folderu projektu
.\start-api.ps1

# Z niestandardowym portem
.\start-api.ps1 9000
```

### CMD (Command Prompt):
```cmd
# Z głównego folderu projektu
start-api.bat

# Z niestandardowym portem
start-api.bat 9000
```

### Bezpośrednio z linii poleceń:

#### PowerShell (Windows):
```powershell
# Domyślny port (8080)
.\IQPowerContentManager.exe

# Niestandardowy port
.\IQPowerContentManager.exe http://localhost:9000
```

#### CMD (Command Prompt):
```cmd
# Domyślny port (8080)
IQPowerContentManager.exe

# Niestandardowy port
IQPowerContentManager.exe http://localhost:9000
```

## Swagger UI - Interaktywna dokumentacja API

Po uruchomieniu API, możesz przeglądać i testować wszystkie endpointy w Swagger UI:

```
http://localhost:8080/swagger
```

Swagger UI pozwala na:
- Przeglądanie wszystkich endpointów
- Testowanie endpointów bezpośrednio z przeglądarki
- Zobaczenie schematów danych
- Interaktywną dokumentację API

Więcej informacji w pliku `SWAGGER_INFO.md`

## Weryfikacja, że API działa

Po uruchomieniu API, możesz sprawdzić czy działa na kilka sposobów:

### 1. W przeglądarce:
Otwórz w przeglądarce:
```
http://localhost:8080/api/cars
```

Powinieneś zobaczyć odpowiedź JSON z listą samochodów.

### 2. Używając curl:
```bash
curl http://localhost:8080/api/cars
```

### 3. Używając PowerShell:
```powershell
Invoke-RestMethod -Uri "http://localhost:8080/api/cars" -Method Get
```

### 4. Używając JavaScript (w konsoli przeglądarki):
```javascript
fetch('http://localhost:8080/api/cars')
  .then(response => response.json())
  .then(data => console.log(data));
```

## Dostępne endpointy

Po uruchomieniu API, wszystkie endpointy są dostępne pod adresem bazowym:
```
http://localhost:8080/api
```

Przykłady:
- `http://localhost:8080/api/cars` - lista samochodów
- `http://localhost:8080/api/tracks` - lista torów
- `http://localhost:8080/api/setup/nick` - ustawienie nicku
- `http://localhost:8080/api/controls/devices` - lista kontrolerów
- itd.

Pełna lista endpointów znajduje się w pliku `API_ENDPOINTS.md`

## Rozwiązywanie problemów

### Problem: "Błąd uruchamiania API: Address already in use"
**Rozwiązanie:** Port 8080 jest już zajęty. Użyj innego portu:
```bash
IQPowerContentManager.exe --api http://localhost:9000
```

### Problem: "Błąd uruchamiania API: Access denied"
**Rozwiązanie:** Uruchom aplikację jako Administrator (porty < 1024 wymagają uprawnień administratora)

### Problem: API nie odpowiada
**Rozwiązanie:** 
1. Sprawdź czy aplikacja jest uruchomiona
2. Sprawdź czy port nie jest zablokowany przez firewall
3. Sprawdź czy nie ma innej aplikacji używającej tego samego portu

## Automatyczne zapisywanie stanu

API automatycznie zapisuje wszystkie zmiany do pliku:
- **Lokalizacja**: `%AppData%\IQPowerContentManager\application_state.json`
- **Co jest zapisywane**: 
  - Wszystkie bindy kontrolerów (włącznie z wieloma bindami dla GEARUP/GEARDN)
  - Biegi H-shiftera (GEAR_1 do GEAR_7 i GEAR_R)
  - Ustawienia wideo (rozdzielczość, tryb wyświetlania)
  - Ostatnio wybrane samochody, tory, nick, etc.
- **Przywracanie**: Stan jest automatycznie wczytywany przy każdym starcie API

**Nie musisz ręcznie zapisywać ustawień - wszystko dzieje się automatycznie!**

## Uwagi

- API działa lokalnie na twoim komputerze
- CORS jest włączony, więc frontend może komunikować się z API
- API działa dopóki aplikacja jest uruchomiona
- Aby zatrzymać API, naciśnij Enter w konsoli
- Wszystkie zmiany są automatycznie zapisywane i przywracane przy starcie

