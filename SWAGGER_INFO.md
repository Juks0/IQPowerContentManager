# Swagger UI - Dokumentacja API

## Dostęp do Swagger UI

Po uruchomieniu API, Swagger UI jest dostępny pod adresem:

```
http://localhost:8080/swagger
```

**Uwaga:** Jeśli powyższy adres nie działa, spróbuj następujące adresy:

1. **Sprawdź czy Swagger JSON działa:**
   ```
   http://localhost:8080/swagger/docs/v1
   ```
   Jeśli widzisz JSON z dokumentacją, to znaczy że Swagger działa, ale UI może mieć problem.

2. **Alternatywne adresy Swagger UI:**
   - `http://localhost:8080/swagger/ui/index`
   - `http://localhost:8080/swagger/index.html`

3. **Jeśli nadal nie działa:**
   - Upewnij się, że projekt został zbudowany po dodaniu Swagger
   - Sprawdź czy pakiety NuGet są zainstalowane (Swashbuckle, Swashbuckle.Core)
   - Sprawdź logi w konsoli po uruchomieniu API
   - Zobacz plik `SWAGGER_TROUBLESHOOTING.md` dla więcej informacji

## Jak używać Swagger UI

1. **Uruchom API** (patrz `HOW_TO_RUN_API.md`)
2. **Otwórz przeglądarkę** i przejdź do: `http://localhost:8080/swagger`
3. **Przeglądaj endpointy** - wszystkie endpointy są pogrupowane według kontrolerów
4. **Testuj endpointy** - możesz kliknąć na dowolny endpoint, zobaczyć szczegóły i przetestować go bezpośrednio z przeglądarki

## Funkcje Swagger UI

### 1. Przeglądanie endpointów
- Wszystkie endpointy są pogrupowane według kontrolerów (Cars, Tracks, Setup, Controls, Video, Content, Game)
- Każdy endpoint ma opis, parametry i przykłady odpowiedzi

### 2. Testowanie endpointów
- Kliknij na endpoint, aby zobaczyć szczegóły
- Kliknij "Try it out" aby przetestować endpoint
- Wypełnij parametry (jeśli wymagane)
- Kliknij "Execute" aby wysłać request
- Zobacz odpowiedź z serwera

### 3. Schematy danych
- Swagger automatycznie generuje schematy dla wszystkich modeli danych
- Możesz zobaczyć strukturę requestów i odpowiedzi

## Przykłady użycia

### Testowanie GET /api/cars
1. Otwórz Swagger UI: `http://localhost:8080/swagger`
2. Znajdź sekcję "Cars"
3. Kliknij na `GET /api/cars`
4. Kliknij "Try it out"
5. Kliknij "Execute"
6. Zobacz odpowiedź z listą samochodów

### Testowanie POST /api/setup/nick
1. Znajdź sekcję "Setup"
2. Kliknij na `POST /api/setup/nick`
3. Kliknij "Try it out"
4. Wypełnij body request:
```json
{
  "nick": "TestPlayer"
}
```
5. Kliknij "Execute"
6. Zobacz odpowiedź z potwierdzeniem

## Uwagi

- Swagger UI działa tylko gdy API jest uruchomione
- Wszystkie endpointy są dostępne do testowania
- Możesz używać Swagger UI jako interaktywnej dokumentacji API
- Swagger automatycznie wykrywa wszystkie endpointy z kontrolerów

## Alternatywne adresy

Jeśli uruchomiłeś API na innym porcie, użyj odpowiedniego adresu:
- Port 8080: `http://localhost:8080/swagger`
- Port 9000: `http://localhost:9000/swagger`
- Port 5000: `http://localhost:5000/swagger`

