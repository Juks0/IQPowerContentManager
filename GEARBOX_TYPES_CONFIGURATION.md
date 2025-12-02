# Konfiguracja Typów Skrzyni Biegów - Dokumentacja

## Przegląd

API umożliwia **edycję i konfigurację dostępnych typów skrzyni biegów**, które będą wyświetlane w aplikacji frontendowej. Dzięki temu możesz kontrolować, **ile i jakie opcje** użytkownik może wybrać.

**Kluczowe funkcje:**
- ✅ Możliwość edycji dostępnych opcji skrzyni biegów
- ✅ Kontrola, które opcje są widoczne dla użytkownika w aplikacji frontendowej
- ✅ Elastyczna konfiguracja - możesz ustawić dowolną kombinację typów
- ✅ Domyślne wartości, jeśli nie ustawiono własnych

**Przykłady konfiguracji:**
- Tylko automatyczna i sekwencyjna
- Tylko H-pattern i automatyczna
- Tylko automatyczna
- Wszystkie trzy typy (domyślne)
- Dowolna inna kombinacja

---

## Endpointy API

### 1. Pobieranie dostępnych typów skrzyni

#### GET /api/setup/gearbox-types

Pobiera listę dostępnych typów skrzyni biegów.

**Response (z domyślnymi typami):**
```json
{
  "success": true,
  "data": [
    {
      "id": "automatic",
      "name": "Automatyczna",
      "description": "Automatyczna skrzynia biegów"
    },
    {
      "id": "sequential",
      "name": "Sekwencyjna",
      "description": "Sekwencyjna skrzynia biegów"
    },
    {
      "id": "h-pattern",
      "name": "H-Pattern",
      "description": "Manualna skrzynia biegów H-Pattern"
    }
  ],
  "errorMessage": null
}
```

**Response (z skonfigurowanymi typami):**
```json
{
  "success": true,
  "data": [
    {
      "id": "automatic",
      "name": "Automatyczna",
      "description": "Automatyczna skrzynia biegów"
    },
    {
      "id": "sequential",
      "name": "Sekwencyjna",
      "description": "Sekwencyjna skrzynia biegów"
    },
    {
      "id": "h-pattern",
      "name": "H-Pattern",
      "description": "Manualna skrzynia biegów H-Pattern"
    },
    {
      "id": "custom-type",
      "name": "Własny typ",
      "description": "Własny typ skrzyni"
    }
  ],
  "errorMessage": null
}
```

**Uwagi:**
- Jeśli nie ustawiono własnych typów, API zwraca domyślne typy
- Lista jest zapisywana w `application_state.json`

---

### 2. Ustawianie dostępnych typów skrzyni (EDYCJA OPCJI)

#### POST /api/setup/gearbox-types

**Ustawia listę dostępnych typów skrzyni biegów, które będą wyświetlane w aplikacji frontendowej.** Pozwala na pełną kontrolę nad tym, **ile i jakie opcje** użytkownik może wybrać.

**Request:**
```json
{
  "gearboxTypes": [
    {
      "id": "automatic",
      "name": "Automatyczna",
      "description": "Automatyczna skrzynia biegów"
    },
    {
      "id": "sequential",
      "name": "Sekwencyjna",
      "description": "Sekwencyjna skrzynia biegów"
    },
    {
      "id": "h-pattern",
      "name": "H-Pattern",
      "description": "Manualna skrzynia biegów H-Pattern"
    }
  ]
}
```

**Przykłady konfiguracji dla różnych scenariuszy:**

#### Przykład 1: Tylko automatyczna i sekwencyjna
```json
{
  "gearboxTypes": [
    {
      "id": "automatic",
      "name": "Automatyczna",
      "description": "Automatyczna skrzynia biegów"
    },
    {
      "id": "sequential",
      "name": "Sekwencyjna",
      "description": "Sekwencyjna skrzynia biegów"
    }
  ]
}
```
**Efekt:** Użytkownik w aplikacji frontendowej zobaczy tylko te dwie opcje do wyboru.

#### Przykład 2: Tylko H-pattern i automatyczna
```json
{
  "gearboxTypes": [
    {
      "id": "h-pattern",
      "name": "H-Pattern",
      "description": "Manualna skrzynia biegów H-Pattern"
    },
    {
      "id": "automatic",
      "name": "Automatyczna",
      "description": "Automatyczna skrzynia biegów"
    }
  ]
}
```
**Efekt:** Użytkownik w aplikacji frontendowej zobaczy tylko te dwie opcje do wyboru.

#### Przykład 3: Tylko automatyczna
```json
{
  "gearboxTypes": [
    {
      "id": "automatic",
      "name": "Automatyczna",
      "description": "Automatyczna skrzynia biegów"
    }
  ]
}
```
**Efekt:** Użytkownik w aplikacji frontendowej zobaczy tylko jedną opcję do wyboru.

#### Przykład 4: Wszystkie trzy typy (domyślne)
```json
{
  "gearboxTypes": [
    {
      "id": "automatic",
      "name": "Automatyczna",
      "description": "Automatyczna skrzynia biegów"
    },
    {
      "id": "sequential",
      "name": "Sekwencyjna",
      "description": "Sekwencyjna skrzynia biegów"
    },
    {
      "id": "h-pattern",
      "name": "H-Pattern",
      "description": "Manualna skrzynia biegów H-Pattern"
    }
  ]
}
```
**Efekt:** Użytkownik w aplikacji frontendowej zobaczy wszystkie trzy opcje do wyboru.

**Response (sukces):**
```json
{
  "success": true,
  "message": "Ustawiono 3 dostępnych typów skrzyni",
  "data": "Ustawiono 3 dostępnych typów skrzyni",
  "errorMessage": null
}
```

**Response (błąd - pusta lista):**
```json
{
  "success": false,
  "message": null,
  "data": null,
  "errorMessage": "Lista typów skrzyni nie może być pusta"
}
```

**Response (błąd - brakujące pole):**
```json
{
  "success": false,
  "message": null,
  "data": null,
  "errorMessage": "Wszystkie typy skrzyni muszą mieć ID"
}
```

**Walidacja:**
- Lista nie może być pusta (musi zawierać przynajmniej jeden typ)
- Każdy typ musi mieć `id` (wymagane, nie może być puste)
- Każdy typ musi mieć `name` (wymagane, nie może być puste)
- `description` jest opcjonalne

**Zapis:**
- Konfiguracja jest zapisywana w `application_state.json`
- Lokalizacja: `%AppData%\IQPowerContentManager\application_state.json`
- Po zapisaniu, endpoint `GET /api/setup/gearbox-types` zwróci tylko skonfigurowane typy

**Ważne:**
- Po ustawieniu konfiguracji, użytkownik w aplikacji frontendowej będzie mógł wybrać **tylko** z listy skonfigurowanych typów
- Jeśli nie ustawiono własnych typów, używane są domyślne (automatic, sequential, h-pattern)
- Aby przywrócić domyślne wartości, wyślij wszystkie trzy typy w liście

---

### 3. Ustawianie wybranego typu skrzyni

#### POST /api/setup/gearbox

Ustawia wybrany typ skrzyni biegów.

**Request:**
```json
{
  "shifterType": "sequential"
}
```

**Walidacja:**
- Typ musi być na liście dostępnych typów (ustawionych przez `POST /api/setup/gearbox-types`)
- Jeśli nie ustawiono własnych typów, dozwolone są domyślne: `automatic`, `sequential`, `h-pattern`

**Response (sukces):**
```json
{
  "success": true,
  "message": "Typ skrzyni ustawiony: sequential",
  "data": "Typ skrzyni ustawiony: sequential",
  "errorMessage": null
}
```

**Response (błąd - nieprawidłowy typ):**
```json
{
  "success": false,
  "message": null,
  "data": null,
  "errorMessage": "Nieprawidłowy typ skrzyni. Dozwolone: automatic, sequential, h-pattern"
}
```

---

## Model danych

### GearboxTypeInfo

```typescript
interface GearboxTypeInfo {
  id: string;          // Unikalny identyfikator typu (np. "automatic", "sequential")
  name: string;        // Nazwa wyświetlana (np. "Automatyczna", "Sekwencyjna")
  description: string; // Opis typu skrzyni (opcjonalne)
}
```

### SetGearboxTypesRequest

```typescript
interface SetGearboxTypesRequest {
  gearboxTypes: GearboxTypeInfo[];
}
```

---

## Przykłady użycia

### JavaScript (Fetch API)

#### Pobranie dostępnych typów skrzyni
```javascript
fetch('http://localhost:8080/api/setup/gearbox-types')
  .then(response => response.json())
  .then(data => {
    console.log('Dostępne typy skrzyni:', data.data);
    // data.data zawiera tablicę GearboxTypeInfo
  });
```

#### Ustawienie dostępnych typów skrzyni
```javascript
const customGearboxTypes = [
  {
    id: "automatic",
    name: "Automatyczna",
    description: "Automatyczna skrzynia biegów"
  },
  {
    id: "sequential",
    name: "Sekwencyjna",
    description: "Sekwencyjna skrzynia biegów"
  },
  {
    id: "h-pattern",
    name: "H-Pattern",
    description: "Manualna skrzynia biegów H-Pattern"
  },
  {
    id: "paddle-shift",
    name: "Paddle Shift",
    description: "Skrzynia z łopatkami"
  }
];

fetch('http://localhost:8080/api/setup/gearbox-types', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    gearboxTypes: customGearboxTypes
  })
})
  .then(response => response.json())
  .then(data => {
    console.log('Typy skrzyni ustawione:', data);
  });
```

#### Ustawienie wybranego typu skrzyni
```javascript
fetch('http://localhost:8080/api/setup/gearbox', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    shifterType: 'sequential'
  })
})
  .then(response => response.json())
  .then(data => {
    console.log('Typ skrzyni ustawiony:', data);
  });
```

### cURL

#### Pobranie dostępnych typów skrzyni
```bash
curl http://localhost:8080/api/setup/gearbox-types
```

#### Ustawienie dostępnych typów skrzyni
```bash
curl -X POST http://localhost:8080/api/setup/gearbox-types \
  -H "Content-Type: application/json" \
  -d '{
    "gearboxTypes": [
      {
        "id": "automatic",
        "name": "Automatyczna",
        "description": "Automatyczna skrzynia biegów"
      },
      {
        "id": "sequential",
        "name": "Sekwencyjna",
        "description": "Sekwencyjna skrzynia biegów"
      },
      {
        "id": "h-pattern",
        "name": "H-Pattern",
        "description": "Manualna skrzynia biegów H-Pattern"
      }
    ]
  }'
```

#### Ustawienie wybranego typu skrzyni
```bash
curl -X POST http://localhost:8080/api/setup/gearbox \
  -H "Content-Type: application/json" \
  -d '{"shifterType":"sequential"}'
```

---

## Domyślne typy skrzyni

Jeśli nie ustawiono własnych typów, API zwraca następujące domyślne typy:

1. **automatic** - Automatyczna skrzynia biegów
2. **sequential** - Sekwencyjna skrzynia biegów
3. **h-pattern** - Manualna skrzynia biegów H-Pattern

---

## Przechowywanie konfiguracji

### Lokalizacja pliku
```
%AppData%\IQPowerContentManager\application_state.json
```

### Format w pliku JSON
```json
{
  "Controls": { ... },
  "LastSelectedCar": "...",
  "LastSelectedTrack": "...",
  "LastNick": "...",
  "LastShifterType": "sequential",
  "AvailableGearboxTypes": [
    {
      "Id": "automatic",
      "Name": "Automatyczna",
      "Description": "Automatyczna skrzynia biegów"
    },
    {
      "Id": "sequential",
      "Name": "Sekwencyjna",
      "Description": "Sekwencyjna skrzynia biegów"
    },
    {
      "Id": "h-pattern",
      "Name": "H-Pattern",
      "Description": "Manualna skrzynia biegów H-Pattern"
    }
  ],
  ...
}
```

**Uwaga:** W pliku JSON pola są pisane z wielką literą (PascalCase), podczas gdy w API używamy camelCase.

---

## Walidacja

### Przy ustawianiu typów skrzyni (POST /api/setup/gearbox-types)
- Lista nie może być pusta
- Każdy typ musi mieć `id` (nie może być puste)
- Każdy typ musi mieć `name` (nie może być puste)
- `description` jest opcjonalne

### Przy wyborze typu skrzyni (POST /api/setup/gearbox)
- Typ musi być na liście dostępnych typów
- Jeśli nie ustawiono własnych typów, sprawdzane są domyślne typy
- Typ jest porównywany case-insensitive (automatyczna konwersja na lowercase)

---

## Przykładowe scenariusze użycia

### Scenariusz 1: Domyślna konfiguracja
1. Użytkownik wywołuje `GET /api/setup/gearbox-types`
2. API zwraca domyślne typy: `automatic`, `sequential`, `h-pattern`
3. W aplikacji frontendowej użytkownik widzi wszystkie trzy opcje
4. Użytkownik wybiera jeden z typów przez `POST /api/setup/gearbox`

### Scenariusz 2: Ograniczenie do dwóch opcji (automatyczna i sekwencyjna)
1. Administrator wywołuje `POST /api/setup/gearbox-types` z listą zawierającą tylko `automatic` i `sequential`
2. Konfiguracja jest zapisywana w `application_state.json`
3. Użytkownik wywołuje `GET /api/setup/gearbox-types` i widzi tylko te dwie opcje
4. W aplikacji frontendowej użytkownik widzi tylko automatyczną i sekwencyjną
5. Użytkownik wybiera jeden z dostępnych typów przez `POST /api/setup/gearbox`

### Scenariusz 3: Ograniczenie do jednej opcji (tylko automatyczna)
1. Administrator wywołuje `POST /api/setup/gearbox-types` z listą zawierającą tylko `automatic`
2. Konfiguracja jest zapisywana w `application_state.json`
3. Użytkownik wywołuje `GET /api/setup/gearbox-types` i widzi tylko automatyczną
4. W aplikacji frontendowej użytkownik widzi tylko jedną opcję
5. Użytkownik wybiera automatyczną przez `POST /api/setup/gearbox`

### Scenariusz 4: Dodanie nowego typu
1. Administrator pobiera aktualne typy: `GET /api/setup/gearbox-types`
2. Dodaje nowy typ do listy (np. `paddle-shift`)
3. Wysyła zaktualizowaną listę: `POST /api/setup/gearbox-types`
4. Nowy typ jest dostępny dla użytkowników w aplikacji frontendowej

### Scenariusz 5: Zmiana dostępnych opcji w trakcie działania aplikacji
1. Administrator zmienia konfigurację: `POST /api/setup/gearbox-types` z nową listą typów
2. Konfiguracja jest natychmiast zapisywana
3. Użytkownik wywołuje `GET /api/setup/gearbox-types` i widzi zaktualizowaną listę
4. W aplikacji frontendowej użytkownik widzi nowe opcje
5. Użytkownik może wybrać tylko z nowej listy dostępnych typów

---

## Logowanie

Wszystkie operacje są logowane w konsoli:

```
[2024-01-15 10:30:00.123] [SETUP] Pobrano listę typów skrzyni: 3 dostępnych
[2024-01-15 10:30:05.456] [SETUP] Ustawiono dostępne typy skrzyni: 4 typów
[2024-01-15 10:30:10.789] [SETUP] Typ skrzyni ustawiony: sequential
```

---

## Błędy i obsługa

### Błąd: Pusta lista typów
```json
{
  "success": false,
  "errorMessage": "Lista typów skrzyni nie może być pusta"
}
```
**Rozwiązanie:** Upewnij się, że lista zawiera przynajmniej jeden typ.

### Błąd: Brakujące pole ID
```json
{
  "success": false,
  "errorMessage": "Wszystkie typy skrzyni muszą mieć ID"
}
```
**Rozwiązanie:** Upewnij się, że każdy typ ma pole `id`.

### Błąd: Brakujące pole Name
```json
{
  "success": false,
  "errorMessage": "Wszystkie typy skrzyni muszą mieć nazwę"
}
```
**Rozwiązanie:** Upewnij się, że każdy typ ma pole `name`.

### Błąd: Nieprawidłowy typ skrzyni
```json
{
  "success": false,
  "errorMessage": "Nieprawidłowy typ skrzyni. Dozwolone: automatic, sequential, h-pattern"
}
```
**Rozwiązanie:** Upewnij się, że wybrany typ jest na liście dostępnych typów.

---

## Uwagi techniczne

1. **Case-insensitive:** Typy są porównywane case-insensitive (automatyczna konwersja na lowercase)
2. **Persystencja:** Konfiguracja jest zapisywana w `application_state.json` i przywracana przy starcie aplikacji
3. **Domyślne wartości:** Jeśli nie ustawiono własnych typów, używane są domyślne typy (automatic, sequential, h-pattern)
4. **Walidacja:** API waliduje zarówno przy ustawianiu typów, jak i przy wyborze typu
5. **Logowanie:** Wszystkie operacje są logowane w konsoli z timestampem
6. **Kontrola opcji:** Endpoint `POST /api/setup/gearbox-types` pozwala na pełną kontrolę nad tym, które opcje są dostępne w aplikacji frontendowej
7. **Elastyczność:** Możesz ustawić dowolną kombinację typów - od jednej opcji do wszystkich trzech (lub więcej, jeśli dodasz własne)
8. **Natychmiastowe zastosowanie:** Zmiany w konfiguracji są natychmiast widoczne dla użytkowników przez endpoint `GET /api/setup/gearbox-types`

---

## Migracja

### Przed zmianami
- Typy skrzyni były hardcoded w kodzie
- Nie było możliwości konfiguracji

### Po zmianach
- Typy skrzyni są konfigurowalne przez API
- Konfiguracja jest zapisywana i przywracana
- Domyślne typy są używane, jeśli nie ustawiono własnych

### Kompatybilność wsteczna
- ✅ Istniejące endpointy działają bez zmian
- ✅ Domyślne typy są zachowane
- ✅ Jeśli nie ustawiono własnych typów, używane są domyślne

---

## Podsumowanie - Edycja dostępnych opcji

**Główna funkcja:** Endpoint `POST /api/setup/gearbox-types` pozwala na **edycję dostępnych opcji skrzyni biegów**, które będą wyświetlane w aplikacji frontendowej.

**Możliwości:**
- ✅ Kontrola, **ile opcji** użytkownik może wybrać (1, 2, 3 lub więcej)
- ✅ Kontrola, **jakie opcje** użytkownik może wybrać (dowolna kombinacja)
- ✅ Elastyczna konfiguracja - możesz ustawić np. tylko automatyczną, tylko automatyczną i sekwencyjną, itp.
- ✅ Natychmiastowe zastosowanie zmian - po zapisaniu, użytkownik widzi tylko skonfigurowane opcje

**Przepływ pracy:**
1. Administrator wywołuje `POST /api/setup/gearbox-types` z listą dostępnych typów
2. Konfiguracja jest zapisywana w `application_state.json`
3. Aplikacja frontendowa wywołuje `GET /api/setup/gearbox-types`
4. Użytkownik widzi tylko skonfigurowane opcje w interfejsie
5. Użytkownik wybiera jeden z dostępnych typów przez `POST /api/setup/gearbox`

---

## Data ostatniej aktualizacji

**Wersja:** Aktualna  
**Data:** 2024-01-15  
**Ostatnia zmiana:** Dodano możliwość edycji dostępnych opcji skrzyni biegów dla aplikacji frontendowej


