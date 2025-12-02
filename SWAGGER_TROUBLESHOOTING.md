# Rozwiązywanie problemów ze Swagger

## Problem: Swagger nie działa pod http://localhost:8080/swagger

### Sprawdź czy API działa

Najpierw sprawdź czy API w ogóle działa:

1. Otwórz w przeglądarce: `http://localhost:8080/api/cars`
2. Jeśli widzisz odpowiedź JSON - API działa
3. Jeśli nie widzisz odpowiedzi - API nie jest uruchomione

### Sprawdź czy Swagger jest poprawnie skonfigurowany

1. **Sprawdź czy pakiety są zainstalowane:**
   - `Swashbuckle` (5.6.0)
   - `Swashbuckle.Core` (5.6.0)

2. **Sprawdź konfigurację w `Api/Startup.cs`:**
   - Swagger musi być skonfigurowany PRZED `app.UseWebApi(config)`
   - Routing musi być poprawnie ustawiony

3. **Sprawdź czy projekt został zbudowany:**
   - Upewnij się, że projekt został zbudowany po dodaniu Swagger
   - Sprawdź czy pliki DLL są w folderze `bin/Debug/net48`

### Alternatywne adresy Swagger

Swagger może być dostępny pod różnymi adresami:

- `http://localhost:8080/swagger` (domyślny)
- `http://localhost:8080/swagger/ui/index` (alternatywny)
- `http://localhost:8080/swagger/docs/v1` (tylko JSON spec)

### Sprawdź logi

Po uruchomieniu API, sprawdź czy są jakieś błędy w konsoli. Jeśli widzisz błędy związane ze Swagger, mogą one wskazywać na problem.

### Rozwiązania

#### Rozwiązanie 1: Przeładuj stronę
Spróbuj odświeżyć stronę (Ctrl+F5) lub wyczyścić cache przeglądarki.

#### Rozwiązanie 2: Sprawdź port
Upewnij się, że używasz poprawnego portu. Jeśli uruchomiłeś API na porcie 9000, użyj:
```
http://localhost:9000/swagger
```

#### Rozwiązanie 3: Sprawdź firewall
Firewall może blokować połączenia. Sprawdź czy port jest otwarty.

#### Rozwiązanie 4: Sprawdź czy nie ma konfliktów
Upewnij się, że nie ma innej aplikacji używającej tego samego portu.

#### Rozwiązanie 5: Zbuduj projekt ponownie
```powershell
dotnet clean
dotnet build
```

#### Rozwiązanie 6: Sprawdź czy wszystkie pakiety są zainstalowane
```powershell
dotnet restore
```

### Testowanie bez Swagger UI

Jeśli Swagger UI nie działa, możesz przetestować API bezpośrednio:

1. **Używając curl:**
```bash
curl http://localhost:8080/api/cars
```

2. **Używając PowerShell:**
```powershell
Invoke-RestMethod -Uri "http://localhost:8080/api/cars" -Method Get
```

3. **Używając przeglądarki:**
```
http://localhost:8080/api/cars
```

### Sprawdź dokumentację Swagger JSON

Jeśli Swagger UI nie działa, możesz sprawdzić czy dokumentacja JSON jest dostępna:

```
http://localhost:8080/swagger/docs/v1
```

Jeśli widzisz JSON z dokumentacją API, to znaczy że Swagger działa, ale UI może mieć problem.

### Kontakt

Jeśli problem nadal występuje, sprawdź:
1. Czy wszystkie pakiety NuGet są zainstalowane
2. Czy projekt został poprawnie zbudowany
3. Czy nie ma błędów w konsoli po uruchomieniu API
4. Czy port nie jest zajęty przez inną aplikację

