# API Documentation - IQPower Content Manager

## Base URL
```
http://localhost:8080/api
```

## Format odpowiedzi

Wszystkie endpointy zwracają odpowiedzi w formacie `ApiResponse<T>`:

```json
{
  "success": true,
  "message": "Opcjonalna wiadomość",
  "data": { /* dane odpowiedzi */ },
  "errorMessage": null
}
```

W przypadku błędu:
```json
{
  "success": false,
  "message": null,
  "data": null,
  "errorMessage": "Szczegóły błędu"
}
```

---

## 1. Setup & Configuration (Konfiguracja przed uruchomieniem gry)

### 1.1 Nick (Nazwa gracza)

#### POST /api/setup/nick
Ustawia nick gracza.

**Request:**
```json
{
  "nick": "Player1"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Nick ustawiony: Player1",
  "data": "Nick ustawiony: Player1",
  "errorMessage": null
}
```

#### GET /api/setup/nick
Pobiera aktualny nick.

**Response:**
```json
{
  "success": true,
  "data": "Player1",
  "errorMessage": null
}
```

---

### 1.2 Samochody (Cars)

#### GET /api/setup/cars
Pobiera listę dostępnych samochodów z czytelnymi nazwami.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "ks_porsche_991_turbo_s",
      "name": "Porsche 911 Turbo S"
    },
    {
      "id": "cky_porsche992_gt3rs_2023",
      "name": "Porsche 992 GT3 RS"
    },
    {
      "id": "ks_nissan_gtr",
      "name": "Nissan GT-R NISMO"
    }
  ],
  "errorMessage": null
}
```

#### POST /api/setup/car
Wybiera samochód do użycia w grze.

**Request:**
```json
{
  "carId": "ks_porsche_991_turbo_s"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Samochód ustawiony: ks_porsche_991_turbo_s",
  "data": "Samochód ustawiony: ks_porsche_991_turbo_s",
  "errorMessage": null
}
```

#### GET /api/setup/car
Pobiera aktualnie wybrany samochód.

**Response:**
```json
{
  "success": true,
  "data": "ks_porsche_991_turbo_s",
  "errorMessage": null
}
```

---

### 1.3 Tory (Tracks)

#### GET /api/setup/tracks
Pobiera listę dostępnych torów.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "ks_nordschleife",
      "name": "ks_nordschleife"
    },
    {
      "id": "ks_nurburgring",
      "name": "ks_nurburgring"
    }
  ],
  "errorMessage": null
}
```

#### POST /api/setup/track
Wybiera tor do użycia w grze.

**Request:**
```json
{
  "trackId": "ks_nordschleife"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Tor ustawiony: ks_nordschleife",
  "data": "Tor ustawiony: ks_nordschleife",
  "errorMessage": null
}
```

#### GET /api/setup/track
Pobiera aktualnie wybrany tor.

**Response:**
```json
{
  "success": true,
  "data": "ks_nordschleife",
  "errorMessage": null
}
```

---

### 1.4 Skrzynia biegów (Gearbox)

#### GET /api/setup/gearbox-types
Pobiera listę dostępnych typów skrzyni biegów.

**Response:**
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

**Uwaga:** Jeśli lista nie została jeszcze skonfigurowana, zwracane są domyślne wartości (automatic, sequential, h-pattern).

#### POST /api/setup/gearbox-types
Ustawia dostępne typy skrzyni biegów, które będą wyświetlane w aplikacji frontendowej. Pozwala na konfigurację, które opcje są dostępne do wyboru (np. tylko automatyczna i sekwencyjna, tylko H-pattern i automatyczna, tylko automatyczna, itp.).

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
    }
  ]
}
```

**Przykłady konfiguracji:**

1. **Tylko automatyczna i sekwencyjna:**
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

2. **Tylko H-pattern i automatyczna:**
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

3. **Tylko automatyczna:**
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

**Walidacja:**
- Lista nie może być pusta
- Każdy typ musi mieć `id` (nie może być pusty)
- Każdy typ musi mieć `name` (nie może być pusty)
- `description` jest opcjonalne

**Response (sukces):**
```json
{
  "success": true,
  "message": "Ustawiono 2 dostępnych typów skrzyni",
  "data": "Ustawiono 2 dostępnych typów skrzyni",
  "errorMessage": null
}
```

**Response (błąd):**
```json
{
  "success": false,
  "message": null,
  "data": null,
  "errorMessage": "Lista typów skrzyni nie może być pusta"
}
```

**Uwaga:** Po ustawieniu dostępnych typów, endpoint `GET /api/setup/gearbox-types` będzie zwracał tylko te skonfigurowane typy. Aby przywrócić domyślne wartości (automatic, sequential, h-pattern), wyślij wszystkie trzy typy w liście.

#### POST /api/setup/gearbox
Ustawia typ skrzyni biegów.

**Request:**
```json
{
  "shifterType": "sequential"
}
```

**Dozwolone wartości:**
- `"automatic"` - Automatyczna skrzynia biegów
- `"sequential"` - Sekwencyjna skrzynia biegów
- `"h-pattern"` - Manualna skrzynia biegów H-Pattern

**Response:**
```json
{
  "success": true,
  "message": "Typ skrzyni ustawiony: sequential",
  "data": "Typ skrzyni ustawiony: sequential",
  "errorMessage": null
}
```

#### GET /api/setup/gearbox
Pobiera aktualny typ skrzyni biegów.

**Response:**
```json
{
  "success": true,
  "data": "sequential",
  "errorMessage": null
}
```

---

### 1.5 Podsumowanie konfiguracji

#### GET /api/setup/summary
Pobiera podsumowanie aktualnej konfiguracji setupu.

**Response:**
```json
{
  "success": true,
  "data": {
    "nick": "Player1",
    "shifterType": "sequential",
    "trackId": "ks_nordschleife",
    "carId": "ks_porsche_991_turbo_s",
    "isComplete": true
  },
  "errorMessage": null
}
```

**Pole `isComplete`** wskazuje, czy wszystkie wymagane pola są ustawione (nick, shifterType, trackId, carId).

---

### 1.6 Uruchomienie gry

#### POST /api/setup/launch
Uruchamia grę z aktualną konfiguracją setupu.

**Request:** Brak (używa zapisanej konfiguracji)

**Response (sukces):**
```json
{
  "success": true,
  "message": "Gra uruchomiona pomyślnie",
  "data": "Gra uruchomiona pomyślnie",
  "errorMessage": null
}
```

**Response (błąd - brakuje konfiguracji):**
```json
{
  "success": false,
  "message": null,
  "data": null,
  "errorMessage": "Nick nie jest ustawiony. Użyj POST /api/setup/nick"
}
```

**Uwagi:**
- Endpoint wymaga ustawienia wszystkich pól: nick, shifterType, trackId, carId
- Automatycznie znajduje ścieżkę do Assetto Corsa (sprawdza typowe lokalizacje Steam)
- Konwertuje typ skrzyni na odpowiednie ustawienia gry (`automatic` → `autoShifter: true`, inne → `autoShifter: false`)
- Wszystkie operacje są logowane w konsoli

---

## 2. Controls & Input Binding (Kontrolery i bindowanie)

### 2.1 Urządzenia (Devices)

#### GET /api/controls/devices
Pobiera listę ustawionych urządzeń (te które będą zapisane w controls.ini).

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "index": 0,
      "name": "SGP Shifter",
      "guid": "{82BA0960-C150-11F0-8002-444553540000}",
      "productGuid": "{0023346E-0000-0000-0000-504944564944}"
    },
    {
      "index": 1,
      "name": "MOZA CRP pedals",
      "guid": "{82BA0960-C150-11F0-8001-444553540000}",
      "productGuid": "{0001346E-0000-0000-0000-504944564944}"
    }
  ],
  "errorMessage": null
}
```

#### GET /api/controls/devices/available
Pobiera listę wszystkich dostępnych urządzeń wykrytych w systemie.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "index": 0,
      "name": "SGP Shifter",
      "guid": "{82BA0960-C150-11F0-8002-444553540000}",
      "productGuid": "{0023346E-0000-0000-0000-504944564944}"
    }
  ],
  "errorMessage": null
}
```

#### POST /api/controls/devices
Ustawia listę urządzeń do zapisania w pliku controls.ini.

**Request:**
```json
{
  "devices": [
    {
      "index": 0,
      "name": "SGP Shifter",
      "guid": "{82BA0960-C150-11F0-8002-444553540000}",
      "productGuid": "{0023346E-0000-0000-0000-504944564944}"
    },
    {
      "index": 1,
      "name": "MOZA CRP pedals",
      "guid": "{82BA0960-C150-11F0-8001-444553540000}",
      "productGuid": "{0001346E-0000-0000-0000-504944564944}"
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "message": "Ustawiono 2 urządzeń",
  "data": null,
  "errorMessage": null
}
```

**Uwaga:** Lista urządzeń jest automatycznie zapisywana do stanu aplikacji i będzie dostępna po restarcie. Urządzenia są zapisywane do pliku controls.ini przy każdym zapisie.

---

### 2.2 Bindowanie (Binding)

#### POST /api/controls/bind
Przypisuje bind do akcji. Obsługuje wiele bindów dla GEARUP/GEARDN oraz bindowanie biegów H-shiftera.

**Request:**
```json
{
  "action": "STEER",
  "controllerIndex": 0,
  "buttonIndex": 2,  // opcjonalne, dla przycisków (wymagane dla GEARUP, GEARDN, GEAR_*)
  "axleIndex": 1     // opcjonalne, dla osi (1-indexed w API, automatycznie konwertowane na 0-indexed wewnętrznie)
}
```

**Dostępne akcje:**
- `"STEER"` - Kierownica (wymaga `axleIndex`)
- `"THROTTLE"` - Gaz (wymaga `axleIndex`)
- `"BRAKES"` - Hamulce (wymaga `axleIndex`)
- `"CLUTCH"` - Sprzęgło (wymaga `axleIndex`)
- `"HANDBRAKE"` - Hamulec ręczny (wymaga `buttonIndex` lub `axleIndex`)
- `"GEARUP"` - Zmiana biegów w górę (wymaga `buttonIndex`, może mieć 2 bindy)
- `"GEARDN"` - Zmiana biegów w dół (wymaga `buttonIndex`, może mieć 2 bindy)
- `"CAMERA"` - Zmiana kamery (wymaga `buttonIndex`)
- `"GEAR_1"` do `"GEAR_7"` - Biegi H-shiftera 1-7 (wymaga `buttonIndex`)
- `"GEAR_R"` - Bieg wsteczny H-shiftera (wymaga `buttonIndex`)

**Uwagi:**
- **Osi są indeksowane od 1 w API** (np. oś 1, 2, 3), ale zapisywane jako 0-indexed w pliku controls.ini (0, 1, 2)
- **GEARUP** i **GEARDN** mogą mieć **2 bindy** każdy. API automatycznie wybiera wolny slot (pierwszy lub drugi)
- **Biegi H-shiftera**: Możesz zbindować każdy bieg osobno używając akcji `GEAR_1`, `GEAR_2`, `GEAR_3`, `GEAR_4`, `GEAR_5`, `GEAR_6`, `GEAR_7`, `GEAR_R`
- Jeśli oba sloty dla GEARUP/GEARDN są zajęte, API zwróci błąd z informacją o konieczności usunięcia jednego z bindów

**Przykłady:**

Bindowanie kierownicy (oś 1):
```json
{
  "action": "STEER",
  "controllerIndex": 0,
  "axleIndex": 1
}
```

Dodanie pierwszego binda dla GEARUP:
```json
{
  "action": "GEARUP",
  "controllerIndex": 0,
  "buttonIndex": 5
}
```

Dodanie drugiego binda dla GEARUP:
```json
{
  "action": "GEARUP",
  "controllerIndex": 0,
  "buttonIndex": 6
}
```

Dodanie biegów H-shiftera:
```json
{
  "action": "GEAR_1",
  "controllerIndex": 0,
  "buttonIndex": 10
}
```

**Response:**
```json
{
  "success": true,
  "message": "Bind przypisany pomyślnie",
  "data": null,
  "errorMessage": null
}
```

---

#### POST /api/controls/bind/h-shifter
Przypisuje bindy dla skrzyni H-pattern w jednym wywołaniu.

**Request:**
```json
{
  "controllerIndex": 0,
  "gears": {
    "GEAR_1": 0,
    "GEAR_2": 1,
    "GEAR_3": 2,
    "GEAR_4": 3,
    "GEAR_5": 4,
    "GEAR_6": 5,
    "GEAR_7": 6,
    "GEAR_R": 7
  }
}
```

**Response:**
```json
{
  "success": true,
  "message": "H-shifter bindy przypisane pomyślnie",
  "data": null,
  "errorMessage": null
}
```

---

#### POST /api/controls/bind/sequential
Przypisuje bindy dla skrzyni sekwencyjnej.

**Request:**
```json
{
  "controllerIndex": 0,
  "gearUpButton": 2,
  "gearDownButton": 3
}
```

**Response:**
```json
{
  "success": true,
  "message": "Sequential bindy przypisane pomyślnie",
  "data": null,
  "errorMessage": null
}
```

---

### 2.3 Wykrywanie inputów (Input Detection)

API umożliwia wykrywanie ruchów osi i wciśnięć przycisków z wybranego urządzenia. Proces składa się z dwóch kroków:

1. **Rozpoczęcie nasłuchiwania** - POST do endpointu `/api/controls/bind/{action}/start`
2. **Sprawdzanie statusu** - GET do endpointu `/api/controls/bind/{action}`

#### POST /api/controls/bind/steer/start
Rozpoczyna nasłuchiwanie na ruch osi kierownicy.

**Request:**
```json
{
  "controllerIndex": 0,
  "timeoutSeconds": 15
}
```

#### POST /api/controls/bind/throttle/start
Rozpoczyna nasłuchiwanie na ruch osi gazu.

#### POST /api/controls/bind/brakes/start
Rozpoczyna nasłuchiwanie na ruch osi hamulców.

#### POST /api/controls/bind/clutch/start
Rozpoczyna nasłuchiwanie na ruch osi sprzęgła.

#### POST /api/controls/bind/handbrake/start
Rozpoczyna nasłuchiwanie na ruch osi lub wciśnięcie przycisku hamulca ręcznego.

#### POST /api/controls/bind/gearup/start
Rozpoczyna nasłuchiwanie na wciśnięcie przycisku zmiany biegów w górę.

#### POST /api/controls/bind/geardn/start
Rozpoczyna nasłuchiwanie na wciśnięcie przycisku zmiany biegów w dół.

#### POST /api/controls/bind/camera/start
Rozpoczyna nasłuchiwanie na wciśnięcie przycisku zmiany kamery.

**Request (dla wszystkich start endpoints):**
```json
{
  "controllerIndex": 0,
  "timeoutSeconds": 15  // opcjonalne, domyślnie 15 sekund
}
```

**Response:**
```json
{
  "success": true,
  "message": "Nasłuchiwanie rozpoczęte",
  "data": null,
  "errorMessage": null
}
```

---

#### GET /api/controls/bind/steer
Sprawdza status wykrywania osi kierownicy.

#### GET /api/controls/bind/throttle
Sprawdza status wykrywania osi gazu.

#### GET /api/controls/bind/brakes
Sprawdza status wykrywania osi hamulców.

#### GET /api/controls/bind/clutch
Sprawdza status wykrywania osi sprzęgła.

#### GET /api/controls/bind/handbrake
Sprawdza status wykrywania hamulca ręcznego.

#### GET /api/controls/bind/gearup
Sprawdza status wykrywania przycisku zmiany biegów w górę.

#### GET /api/controls/bind/geardn
Sprawdza status wykrywania przycisku zmiany biegów w dół.

#### GET /api/controls/bind/camera
Sprawdza status wykrywania przycisku zmiany kamery.

**Response (nasłuchiwanie w toku):**
```json
{
  "success": true,
  "data": {
    "action": "STEER",
    "isListening": true,
    "isCompleted": false,
    "isCancelled": false,
    "statusMessage": "Nasłuchiwanie... Porusz osią",
    "detectedAxis": null,
    "detectedButton": null,
    "detectedControllerIndex": null,
    "detectedControllerName": null,
    "success": false,
    "startTime": "2024-01-15T10:30:00.000Z",
    "timeoutSeconds": 15
  },
  "errorMessage": null
}
```

**Response (wykryto input):**
```json
{
  "success": true,
  "data": {
    "action": "STEER",
    "isListening": false,
    "isCompleted": true,
    "isCancelled": false,
    "statusMessage": "Wykryto oś 1",
    "detectedAxis": 1,
    "detectedButton": null,
    "detectedControllerIndex": 0,
    "detectedControllerName": "Logitech G29",
    "success": true,
    "startTime": "2024-01-15T10:30:00.000Z",
    "timeoutSeconds": 15
  },
  "errorMessage": null
}
```

**Uwagi:**
- **Osi są zwracane jako 1-indexed** (oś 1, 2, 3, itd.)
- Frontend powinien regularnie sprawdzać status (np. co 500ms) używając GET endpointu
- Po wykryciu inputu, frontend powinien użyć POST `/api/controls/bind` z wykrytymi wartościami
- Wszystkie operacje są logowane w konsoli

---

### 2.4 Zarządzanie bindami

#### GET /api/controls/bindings
Pobiera listę wszystkich akcji z ich aktualnymi bindami. Każda akcja może mieć wiele bindów wyświetlanych jeden pod drugim.

**Response:**
```json
{
  "success": true,
  "data": {
    "actions": [
      {
        "name": "STEER",
        "type": "axis",
        "description": "Kierownica",
        "bindings": [
          {
            "id": "STEER_1",
            "controllerIndex": 0,
            "controllerName": "Logitech G29",
            "inputType": "axis",
            "inputIndex": 1,
            "displayName": "Axis 1"
          }
        ]
      },
      {
        "name": "GEARUP",
        "type": "button",
        "description": "Gear up",
        "bindings": [
          {
            "id": "GEARUP_1",
            "controllerIndex": 0,
            "controllerName": "Logitech G29",
            "inputType": "button",
            "inputIndex": 5,
            "displayName": "Button 5"
          },
          {
            "id": "GEARUP_2",
            "controllerIndex": 0,
            "controllerName": "Logitech G29",
            "inputType": "button",
            "inputIndex": 6,
            "displayName": "Button 6"
          }
        ]
      },
      {
        "name": "GEARS",
        "type": "button",
        "description": "H-Shifter gears (1-7 and R)",
        "bindings": [
          {
            "id": "GEAR_1",
            "controllerIndex": 0,
            "controllerName": "Logitech G29",
            "inputType": "button",
            "inputIndex": 10,
            "displayName": "Button 10"
          },
          {
            "id": "GEAR_2",
            "controllerIndex": 0,
            "controllerName": "Logitech G29",
            "inputType": "button",
            "inputIndex": 11,
            "displayName": "Button 11"
          }
        ]
      }
    ]
  },
  "errorMessage": null
}
```

**Uwagi:**
- Wszystkie bindy dla danej akcji są wyświetlane w tablicy `bindings` jeden pod drugim
- Każdy bind ma unikalny `id`, który można użyć do usunięcia konkretnego binda
- Format ID: `{ACTION}_{INDEX}` (np. `GEARUP_1`, `GEARUP_2`, `GEAR_1`, `GEAR_R`)
- **Osi są wyświetlane jako 1-indexed** (Axis 1, 2, 3, itd.)

---

#### DELETE /api/controls/bind/{actionName}
Usuwa wszystkie bindy dla określonej akcji.

**Parametry:**
- `actionName` - nazwa akcji (np. "STEER", "THROTTLE", "GEARUP")

**Przykład:**
```
DELETE /api/controls/bind/GEARUP
```

**Response:**
```json
{
  "success": true,
  "message": "Usunięto wszystkie bindy dla akcji: GEARUP",
  "data": null,
  "errorMessage": null
}
```

---

#### DELETE /api/controls/bind
Usuwa konkretny bind na podstawie jego ID.

**Request:**
```json
{
  "bindingId": "GEARUP_1"
}
```

**Przykłady ID bindów:**
- `GEARUP_1` - pierwszy bind dla GEARUP
- `GEARUP_2` - drugi bind dla GEARUP
- `GEARDN_1` - pierwszy bind dla GEARDN
- `GEARDN_2` - drugi bind dla GEARDN
- `GEAR_1` do `GEAR_7` - biegi 1-7
- `GEAR_R` - bieg wsteczny
- `STEER_1`, `THROTTLE_1`, `BRAKES_1`, etc. - standardowe bindy

**Response:**
```json
{
  "success": true,
  "message": "Bind usunięty pomyślnie",
  "data": null,
  "errorMessage": null
}
```

---

### 2.5 Stan i zapis/odczyt

#### GET /api/controls/state
Pobiera aktualny stan ustawień kontrolerów.

**Response:**
```json
{
  "success": true,
  "data": {
    "devices": [
      {
        "index": 0,
        "name": "Logitech G29",
        "guid": "{C24F046D-0000-0000-0000-504944564944}",
        "productGuid": "{C24F046D-0000-0000-0000-504944564944}"
      }
    ],
    "steer": {
      "controllerIndex": 0,
      "axleIndex": 1,
      "degreesOfRotation": 900,
      "scale": 100
    },
    "throttle": {
      "controllerIndex": 0,
      "axleIndex": 2,
      "rangeFrom": 0,
      "rangeTo": 100
    },
    "brakes": {
      "controllerIndex": 0,
      "axleIndex": 3,
      "rangeFrom": 0,
      "rangeTo": 100
    },
    "clutch": {
      "controllerIndex": 0,
      "axleIndex": 4,
      "rangeFrom": 0,
      "rangeTo": 100
    },
    "handbrake": {
      "controllerIndex": 0,
      "buttonIndex": 4,
      "axleIndex": null
    },
    "gearUp": {
      "controllerIndex": 0,
      "buttonIndex": 2
    },
    "gearDown": {
      "controllerIndex": 0,
      "buttonIndex": 3
    },
    "camera": {
      "controllerIndex": 0,
      "buttonIndex": 8
    },
    "hShifter": {
      "active": false,
      "controllerIndex": -1,
      "gears": {}
    }
  },
  "errorMessage": null
}
```

**Uwaga:** Osi są zwracane jako 1-indexed (axleIndex: 1, 2, 3, itd.)

---

#### POST /api/controls/save
Zapisuje ustawienia kontrolerów do pliku controls.ini.

**Response:**
```json
{
  "success": true,
  "message": "Ustawienia zapisane pomyślnie",
  "data": null,
  "errorMessage": null
}
```

---

#### POST /api/controls/load
Wczytuje ustawienia kontrolerów z pliku controls.ini.

**Response:**
```json
{
  "success": true,
  "message": "Ustawienia wczytane pomyślnie",
  "data": null,
  "errorMessage": null
}
```

---

## 3. Video Settings (Ustawienia wideo)

### POST /api/video/display-mode
Ustawia tryb wyświetlania.

**Request:**
```json
{
  "mode": "DEFAULT"
}
```

**Dozwolone wartości:**
- `"DEFAULT"` - Pojedynczy ekran
- `"TRIPLE"` - Tryb potrójny
- `"OPENVR"` - OpenVR
- `"OCULUS"` - Oculus

**Response:**
```json
{
  "success": true,
  "message": "Tryb wyświetlania ustawiony: DEFAULT",
  "data": null,
  "errorMessage": null
}
```

---

### POST /api/video/resolution
Ustawia rozdzielczość ekranu.

**Request:**
```json
{
  "width": 1920,
  "height": 1080,
  "refresh": 60,
  "index": 0
}
```

**Response:**
```json
{
  "success": true,
  "message": "Rozdzielczość ustawiona: 1920x1080@60Hz",
  "data": null,
  "errorMessage": null
}
```

---

### POST /api/video/save
Zapisuje ustawienia wideo do pliku.

**Response:**
```json
{
  "success": true,
  "message": "Ustawienia wideo zapisane pomyślnie",
  "data": null,
  "errorMessage": null
}
```

---

### GET /api/video/state
Pobiera aktualne ustawienia wideo.

**Response:**
```json
{
  "success": true,
  "data": {
    "width": 1920,
    "height": 1080,
    "refresh": 60,
    "displayMode": "DEFAULT",
    "displayModeName": "Single screen"
  },
  "errorMessage": null
}
```

---

### POST /api/video/load
Wczytuje ustawienia wideo z pliku.

**Response:**
```json
{
  "success": true,
  "message": "Ustawienia wideo wczytane pomyślnie",
  "data": null,
  "errorMessage": null
}
```

---

## 4. Content Management (Zarządzanie zawartością)

### POST /api/content/upload
Wgrywa zawartość do Assetto Corsa w jednym wywołaniu (assetofolder, samochód, tor). Można wybrać dowolną kombinację operacji.

**Request:**
```json
{
  "uploadAssetofolder": true,  // opcjonalne, wgraj cały folder assetofolder
  "carId": "ks_porsche_991_turbo_s",  // opcjonalne, ID samochodu do wgrania
  "trackId": "ks_nordschleife"  // opcjonalne, ID toru do wgrania
}
```

**Response (sukces):**
```json
{
  "success": true,
  "message": "Assetofolder wgrany pomyślnie; Samochód 'ks_porsche_991_turbo_s' wgrany pomyślnie; Tor 'ks_nordschleife' wgrany pomyślnie",
  "data": null,
  "errorMessage": null
}
```

**Response (częściowy sukces z błędami):**
```json
{
  "success": false,
  "message": "Samochód 'ks_porsche_991_turbo_s' wgrany pomyślnie. Błędy: Błąd wgrywania toru 'ks_nordschleife': Tor nie istnieje",
  "data": null,
  "errorMessage": "Samochód 'ks_porsche_991_turbo_s' wgrany pomyślnie. Błędy: Błąd wgrywania toru 'ks_nordschleife': Tor nie istnieje"
}
```

**Uwagi:**
- Musisz wybrać przynajmniej jedną operację (uploadAssetofolder, carId lub trackId)
- Wszystkie wybrane operacje są wykonywane sekwencyjnie
- Endpoint zwraca szczegółowy raport z wynikami wszystkich operacji

---

### POST /api/content/upload-assetofolder
Wgrywa całą zawartość folderu assetofolder do Assetto Corsa.

**Request:** Brak

**Response:**
```json
{
  "success": true,
  "message": "Assetofolder wgrany pomyślnie",
  "data": null,
  "errorMessage": null
}
```

---

### POST /api/content/upload-car
Wgrywa wybrany samochód do Assetto Corsa.

**Request:**
```json
{
  "carId": "ks_porsche_991_turbo_s"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Samochód 'ks_porsche_991_turbo_s' wgrany pomyślnie",
  "data": null,
  "errorMessage": null
}
```

---

### POST /api/content/upload-track
Wgrywa wybrany tor do Assetto Corsa.

**Request:**
```json
{
  "trackId": "ks_nordschleife"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Tor 'ks_nordschleife' wgrany pomyślnie",
  "data": null,
  "errorMessage": null
}
```

---

## 5. Automatyczne zapisywanie stanu

API automatycznie zapisuje stan aplikacji po każdej zmianie:
- **Lokalizacja pliku**: `%AppData%\IQPowerContentManager\application_state.json`
- **Zapisuje**: 
  - Wszystkie bindy kontrolerów (włącznie z wieloma bindami dla GEARUP/GEARDN)
  - Biegi H-shiftera (GEAR_1 do GEAR_7 i GEAR_R)
  - Ustawienia wideo (rozdzielczość, tryb wyświetlania)
  - Ostatnio wybrane samochody, tory, nick, typ skrzyni, etc.
- **Przywracanie**: Stan jest automatycznie wczytywany przy starcie API

**Nie musisz ręcznie zapisywać ustawień - wszystko dzieje się automatycznie!**

---

## 6. Indeksowanie osi

**Ważne:** API używa **1-indexed** indeksowania osi dla użytkownika, ale zapisuje je jako **0-indexed** w pliku controls.ini.

- **W API (request/response)**: Osi są numerowane od 1 (oś 1, 2, 3, itd.)
- **W pliku controls.ini**: Osi są zapisywane jako 0-indexed (0, 1, 2, itd.)
- **Konwersja**: API automatycznie konwertuje między formatami

**Przykład:**
- Użytkownik wybiera "oś 1" w frontendzie
- API otrzymuje `axleIndex: 1`
- API konwertuje na `axleIndex: 0` wewnętrznie
- W pliku controls.ini zapisuje się jako `AXLE=0`
- W odpowiedziach API zwraca `axleIndex: 1`

---

## 7. Uruchomienie API

### Automatyczne uruchomienie:
Aplikacja automatycznie uruchamia API przy starcie.

### Z linii poleceń:
```bash
IQPowerContentManager.exe
```

### Z niestandardowym portem:
```bash
IQPowerContentManager.exe http://localhost:9000
```

**Uwaga:** Aplikacja uruchamia API automatycznie. Naciśnij Enter w konsoli, aby zatrzymać API.

### Swagger UI:
Po uruchomieniu API, Swagger UI jest dostępny pod adresem:
```
http://localhost:8080/swagger
```

---

## 8. Przykłady użycia

### JavaScript (Fetch API)

```javascript
// Pobierz listę samochodów
fetch('http://localhost:8080/api/setup/cars')
  .then(response => response.json())
  .then(data => console.log(data));

// Ustaw nick
fetch('http://localhost:8080/api/setup/nick', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ nick: 'Player1' })
})
  .then(response => response.json())
  .then(data => console.log(data));

// Wybierz samochód
fetch('http://localhost:8080/api/setup/car', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ carId: 'ks_porsche_991_turbo_s' })
})
  .then(response => response.json())
  .then(data => console.log(data));

// Wybierz tor
fetch('http://localhost:8080/api/setup/track', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ trackId: 'ks_nordschleife' })
})
  .then(response => response.json())
  .then(data => console.log(data));

// Ustaw typ skrzyni
fetch('http://localhost:8080/api/setup/gearbox', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ shifterType: 'sequential' })
})
  .then(response => response.json())
  .then(data => console.log(data));

// Uruchom grę
fetch('http://localhost:8080/api/setup/launch', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  }
})
  .then(response => response.json())
  .then(data => console.log(data));

// Rozpocznij wykrywanie osi kierownicy
fetch('http://localhost:8080/api/controls/bind/steer/start', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ controllerIndex: 0, timeoutSeconds: 15 })
})
  .then(response => response.json())
  .then(data => console.log(data));

// Sprawdź status wykrywania (polling)
const checkStatus = setInterval(() => {
  fetch('http://localhost:8080/api/controls/bind/steer')
    .then(response => response.json())
    .then(data => {
      console.log(data);
      if (data.data.isCompleted) {
        clearInterval(checkStatus);
        // Użyj wykrytej osi do bindowania
        if (data.data.detectedAxis !== null) {
          fetch('http://localhost:8080/api/controls/bind', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json'
            },
            body: JSON.stringify({
              action: 'STEER',
              controllerIndex: data.data.detectedControllerIndex,
              axleIndex: data.data.detectedAxis
            })
          });
        }
      }
    });
}, 500);

// Dodaj pierwszy bind dla GEARUP
fetch('http://localhost:8080/api/controls/bind', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    action: 'GEARUP',
    controllerIndex: 0,
    buttonIndex: 5
  })
})
  .then(response => response.json())
  .then(data => console.log(data));

// Pobierz wszystkie bindy
fetch('http://localhost:8080/api/controls/bindings')
  .then(response => response.json())
  .then(data => console.log(data));

// Usuń konkretny bind
fetch('http://localhost:8080/api/controls/bind', {
  method: 'DELETE',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    bindingId: 'GEARUP_1'
  })
})
  .then(response => response.json())
  .then(data => console.log(data));
```

### cURL

```bash
# Pobierz listę samochodów
curl http://localhost:8080/api/setup/cars

# Ustaw nick
curl -X POST http://localhost:8080/api/setup/nick \
  -H "Content-Type: application/json" \
  -d '{"nick":"Player1"}'

# Wybierz samochód
curl -X POST http://localhost:8080/api/setup/car \
  -H "Content-Type: application/json" \
  -d '{"carId":"ks_porsche_991_turbo_s"}'

# Wybierz tor
curl -X POST http://localhost:8080/api/setup/track \
  -H "Content-Type: application/json" \
  -d '{"trackId":"ks_nordschleife"}'

# Ustaw typ skrzyni
curl -X POST http://localhost:8080/api/setup/gearbox \
  -H "Content-Type: application/json" \
  -d '{"shifterType":"sequential"}'

# Uruchom grę
curl -X POST http://localhost:8080/api/setup/launch \
  -H "Content-Type: application/json"

# Rozpocznij wykrywanie osi kierownicy
curl -X POST http://localhost:8080/api/controls/bind/steer/start \
  -H "Content-Type: application/json" \
  -d '{"controllerIndex":0,"timeoutSeconds":15}'

# Sprawdź status wykrywania
curl http://localhost:8080/api/controls/bind/steer

# Zbinduj kierownicę (oś 1)
curl -X POST http://localhost:8080/api/controls/bind \
  -H "Content-Type: application/json" \
  -d '{"action":"STEER","controllerIndex":0,"axleIndex":1}'
```

---

## 9. Uwagi i najlepsze praktyki

- **API działa na porcie 8080 domyślnie**
- **CORS jest włączony** dla wszystkich źródeł (dla frontendu)
- **Wszystkie odpowiedzi są w formacie JSON**
- **W przypadku błędu**, pole `success` będzie `false`, a szczegóły błędu w polu `errorMessage`
- **Wszystkie zmiany są automatycznie zapisywane** i przywracane przy starcie
- **Osi są indeksowane od 1** w API, ale zapisywane jako 0-indexed w pliku
- **Wykrywanie inputów** wymaga polling (sprawdzanie statusu co 500ms)
- **Wszystkie operacje są logowane** w konsoli aplikacji

---

## 10. Dokumentacja dodatkowa

- **API_BINDINGS_GUIDE.md** - szczegółowy przewodnik po bindowaniu (wielokrotne bindy, biegi H-shiftera)
- **API_ENDPOINTS.md** - lista wszystkich endpointów
- **HOW_TO_RUN_API.md** - instrukcja uruchomienia API
- **SWAGGER_INFO.md** - informacje o Swagger UI
