# Przewodnik po Bindowaniu - IQPower Content Manager API

## Wprowadzenie

API obsługuje zaawansowane bindowanie kontrolerów z możliwością przypisania wielu bindów do niektórych akcji oraz indywidualnego zarządzania każdym bindem.

## Podstawowe bindowanie

### Pojedyncze bindy

Większość akcji może mieć jeden bind:

```json
POST /api/controls/bind
{
  "action": "STEER",
  "controllerIndex": 0,
  "axleIndex": 0
}
```

**Dostępne akcje z pojedynczym bindem:**
- `STEER` - kierownica (oś)
- `THROTTLE` - gaz (oś)
- `BRAKES` - hamulce (oś)
- `CLUTCH` - sprzęgło (oś)
- `HANDBRAKE` - hamulec ręczny (przycisk lub oś)
- `CAMERA` - zmiana kamery (przycisk) ✅ **OBSŁUGIWANE**

**Przykład bindowania kamery:**
```json
POST /api/controls/bind
{
  "action": "CAMERA",
  "controllerIndex": 0,
  "buttonIndex": 8
}
```

## Wielokrotne bindy

### GEARUP i GEARDN - 2 bindy każdy

Możesz przypisać **2 bindy** dla każdej z tych akcji:
- `GEARUP` - bieg w górę
- `GEARDN` - bieg w dół

API automatycznie wybiera wolny slot (pierwszy lub drugi).

**Przykład - dodanie pierwszego binda dla GEARUP:**
```json
POST /api/controls/bind
{
  "action": "GEARUP",
  "controllerIndex": 0,
  "buttonIndex": 5
}
```

**Przykład - dodanie drugiego binda dla GEARUP:**
```json
POST /api/controls/bind
{
  "action": "GEARUP",
  "controllerIndex": 0,
  "buttonIndex": 6
}
```

**Uwaga:** Jeśli oba sloty są zajęte, API zwróci błąd. Musisz najpierw usunąć jeden z bindów.

## Bindowanie biegów H-shiftera

Możesz zbindować każdy bieg osobno używając następujących akcji:
- `GEAR_1` - bieg 1
- `GEAR_2` - bieg 2
- `GEAR_3` - bieg 3
- `GEAR_4` - bieg 4
- `GEAR_5` - bieg 5
- `GEAR_6` - bieg 6
- `GEAR_7` - bieg 7
- `GEAR_R` - bieg wsteczny (R)

**Przykład - bindowanie biegu 1:**
```json
POST /api/controls/bind
{
  "action": "GEAR_1",
  "controllerIndex": 0,
  "buttonIndex": 10
}
```

**Przykład - bindowanie biegu wstecznego:**
```json
POST /api/controls/bind
{
  "action": "GEAR_R",
  "controllerIndex": 0,
  "buttonIndex": 17
}
```

## Wyświetlanie wszystkich bindów

### GET /api/controls/bindings

Pobiera listę wszystkich akcji z ich aktualnymi bindami. Wszystkie bindy są wyświetlane jeden pod drugim w tablicy `bindings`.

**Przykładowa odpowiedź:**
```json
{
  "success": true,
  "data": {
    "actions": [
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
          },
          {
            "id": "GEAR_R",
            "controllerIndex": 0,
            "controllerName": "Logitech G29",
            "inputType": "button",
            "inputIndex": 17,
            "displayName": "Button 17"
          }
        ]
      }
    ]
  }
}
```

## Usuwanie bindów

### Usunięcie wszystkich bindów dla akcji

```http
DELETE /api/controls/bind/GEARUP
```

Usuwa wszystkie bindy dla akcji GEARUP (zarówno pierwszy jak i drugi).

### Usunięcie konkretnego binda

```json
DELETE /api/controls/bind
{
  "bindingId": "GEARUP_1"
}
```

Usuwa tylko konkretny bind na podstawie jego ID.

**Format ID bindów:**
- `GEARUP_1` - pierwszy bind dla GEARUP
- `GEARUP_2` - drugi bind dla GEARUP
- `GEARDN_1` - pierwszy bind dla GEARDN
- `GEARDN_2` - drugi bind dla GEARDN
- `GEAR_1` do `GEAR_7` - biegi 1-7
- `GEAR_R` - bieg wsteczny
- `STEER_1`, `THROTTLE_1`, `BRAKES_1`, etc. - standardowe bindy

## Przykładowy workflow

### 1. Sprawdź dostępne kontrolery
```http
GET /api/controls/devices
```

### 2. Zbinduj podstawowe kontrolki
```json
POST /api/controls/bind
{
  "action": "STEER",
  "controllerIndex": 0,
  "axleIndex": 0
}

POST /api/controls/bind
{
  "action": "THROTTLE",
  "controllerIndex": 0,
  "axleIndex": 1
}

POST /api/controls/bind
{
  "action": "BRAKES",
  "controllerIndex": 0,
  "axleIndex": 2
}
```

### 3. Zbinduj 2 przyciski dla GEARUP
```json
POST /api/controls/bind
{
  "action": "GEARUP",
  "controllerIndex": 0,
  "buttonIndex": 5
}

POST /api/controls/bind
{
  "action": "GEARUP",
  "controllerIndex": 0,
  "buttonIndex": 6
}
```

### 4. Zbinduj biegi H-shiftera
```json
POST /api/controls/bind
{
  "action": "GEAR_1",
  "controllerIndex": 0,
  "buttonIndex": 10
}

POST /api/controls/bind
{
  "action": "GEAR_2",
  "controllerIndex": 0,
  "buttonIndex": 11
}

// ... i tak dalej dla pozostałych biegów
```

### 5. Sprawdź wszystkie bindy
```http
GET /api/controls/bindings
```

### 6. Usuń niepotrzebny bind
```json
DELETE /api/controls/bind
{
  "bindingId": "GEARUP_2"
}
```

### 7. Zapisz ustawienia
```http
POST /api/controls/save
```

## Automatyczne zapisywanie

API automatycznie zapisuje stan po każdej zmianie bindów do pliku:
- **Lokalizacja**: `%AppData%\IQPowerContentManager\application_state.json`
- **Przywracanie**: Stan jest automatycznie wczytywany przy starcie API

Nie musisz ręcznie zapisywać - wszystko dzieje się automatycznie!

## Uwagi

1. **GEARUP/GEARDN**: Maksymalnie 2 bindy każdy. Jeśli chcesz dodać trzeci, musisz najpierw usunąć jeden z istniejących.

2. **Biegi H-shiftera**: Możesz zbindować dowolne kombinacje biegów. Nie musisz bindować wszystkich.

3. **ID bindów**: Każdy bind ma unikalny ID, który jest używany do jego identyfikacji i usuwania.

4. **Wyświetlanie**: Wszystkie bindy dla danej akcji są wyświetlane jeden pod drugim w odpowiedzi `/api/controls/bindings`.

5. **Kompatybilność**: Stare endpointy (`/api/controls/bind/h-shifter`, `/api/controls/bind/sequential`) nadal działają, ale zalecane jest używanie nowego systemu z `/api/controls/bind`.

