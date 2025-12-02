# Szybka naprawa Swagger

## Problem: Swagger nie działa pod http://localhost:8080/swagger

### ⚠️ WAŻNE: Zbuduj projekt najpierw!

Po dodaniu Swagger, **MUSISZ zbudować projekt**, aby pakiety zostały zainstalowane:

```powershell
# Z głównego folderu projektu
dotnet restore
dotnet build
```

### Krok 1: Sprawdź czy API działa
Otwórz w przeglądarce:
```
http://localhost:8080/api/cars
```

**Jeśli widzisz JSON** - API działa ✅  
**Jeśli widzisz błąd** - API nie jest uruchomione lub nie działa ❌

### Krok 2: Sprawdź Swagger JSON (najważniejsze!)
Otwórz w przeglądarce:
```
http://localhost:8080/swagger/docs/v1
```

**Jeśli widzisz JSON z dokumentacją** - Swagger działa ✅, ale UI może mieć problem  
**Jeśli widzisz 404** - Swagger nie jest poprawnie skonfigurowany ❌

### Krok 3: Alternatywne adresy Swagger UI
Jeśli Swagger JSON działa, spróbuj następujących adresów dla UI:
- `http://localhost:8080/swagger` (domyślny)
- `http://localhost:8080/swagger/ui/index`
- `http://localhost:8080/swagger/index.html`

### Krok 4: Sprawdź logi w konsoli
Po uruchomieniu API, sprawdź konsolę - powinieneś zobaczyć:
```
API uruchomione na: http://localhost:8080
Swagger UI dostępny na: http://localhost:8080/swagger
Swagger JSON: http://localhost:8080/swagger/docs/v1
Przykładowy endpoint: http://localhost:8080/api/cars
```

Jeśli widzisz błędy w konsoli, mogą one wskazywać na problem.

### Krok 5: Sprawdź pakiety NuGet
Upewnij się, że pakiety są zainstalowane:
- ✅ Swashbuckle (5.6.0)
- ✅ Swashbuckle.Core (5.6.0)
- ❌ Swashbuckle.AspNetCore (NIE używaj - to dla .NET Core!)

### Krok 6: Pełna reinstalacja (jeśli nadal nie działa)
1. Zamknij aplikację
2. Usuń foldery `bin` i `obj`:
   ```powershell
   Remove-Item -Recurse -Force bin, obj
   ```
3. Przywróć i zbuduj projekt:
   ```powershell
   dotnet restore
   dotnet clean
   dotnet build
   ```
4. Uruchom API ponownie

### Diagnostyka

**Sprawdź w konsoli przeglądarki (F12):**
- Otwórz DevTools (F12)
- Przejdź do zakładki "Network"
- Spróbuj otworzyć `http://localhost:8080/swagger`
- Sprawdź jakie requesty są wysyłane i jakie błędy występują

**Sprawdź czy port jest zajęty:**
```powershell
netstat -ano | findstr :8080
```

Jeśli port jest zajęty, użyj innego portu:
```powershell
.\start-api.ps1 9000
```

Następnie użyj: `http://localhost:9000/swagger`

