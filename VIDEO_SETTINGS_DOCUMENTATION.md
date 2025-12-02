# Ustawienia Graficzne - Dokumentacja API

## Przegląd

API umożliwia konfigurację ustawień graficznych dla Assetto Corsa, w tym trybu wyświetlania, rozdzielczości ekranu i częstotliwości odświeżania. Wszystkie ustawienia są zapisywane w pliku `video.ini` w folderze `C:\Users\{current-user}\Documents\asseto-manager`.

---

## Endpointy API

### 1. Tryb wyświetlania (Display Mode)

#### POST /api/video/display-mode
Ustawia tryb wyświetlania gry.

**Request:**
```json
{
  "mode": "SINGLE_SCREEN"
}
```

**Dostępne opcje:**
- `"SINGLE_SCREEN"` - Single Screen (pojedynczy ekran)
- `"TRIPLE_SCREEN"` - Triple Screen (potrójny ekran)
- `"OPENVR"` - OpenVR
- `"STEAMVR"` - SteamVR

**Response (sukces):**
```json
{
  "success": true,
  "message": "Tryb wyświetlania ustawiony: Single Screen",
  "data": "Tryb wyświetlania ustawiony: Single Screen",
  "errorMessage": null
}
```

**Response (błąd):**
```json
{
  "success": false,
  "message": null,
  "data": null,
  "errorMessage": "Nieprawidłowy tryb wyświetlania. Dozwolone: SINGLE_SCREEN, TRIPLE_SCREEN, OPENVR, STEAMVR"
}
```

**Uwaga:** W pliku `video.ini` tryby są zapisywane jako:
- `SINGLE_SCREEN` → `CAMERA.MODE=DEFAULT`
- `TRIPLE_SCREEN` → `CAMERA.MODE=TRIPLE`
- `OPENVR` → `CAMERA.MODE=OPENVR`
- `STEAMVR` → `CAMERA.MODE=OPENVR` (SteamVR używa OPENVR w Assetto Corsa)

---

#### GET /api/video/display-mode
Pobiera aktualny tryb wyświetlania.

**Response:**
```json
{
  "success": true,
  "data": {
    "mode": "SINGLE_SCREEN",
    "displayName": "Single Screen"
  },
  "errorMessage": null
}
```

---

### 2. Rozdzielczość ekranu i Refresh Rate

#### POST /api/video/resolution
Ustawia rozdzielczość ekranu i częstotliwość odświeżania.

**Request:**
```json
{
  "width": 1920,
  "height": 1080,
  "refreshRate": 60,
  "index": 0
}
```

**Parametry:**
- `width` (wymagane) - Szerokość ekranu w pikselach (musi być > 0)
- `height` (wymagane) - Wysokość ekranu w pikselach (musi być > 0)
- `refreshRate` (wymagane) - Częstotliwość odświeżania w Hz (musi być > 0)
- `index` (opcjonalne) - Indeks rozdzielczości (domyślnie 0)

**Response (sukces):**
```json
{
  "success": true,
  "message": "Rozdzielczość ustawiona: 1920x1080@60Hz",
  "data": "Rozdzielczość ustawiona: 1920x1080@60Hz",
  "errorMessage": null
}
```

**Response (błąd):**
```json
{
  "success": false,
  "message": null,
  "data": null,
  "errorMessage": "Szerokość i wysokość muszą być większe od 0"
}
```

**Przykłady:**

1. **Full HD 60Hz:**
```json
{
  "width": 1920,
  "height": 1080,
  "refreshRate": 60
}
```

2. **4K 120Hz:**
```json
{
  "width": 3840,
  "height": 2160,
  "refreshRate": 120
}
```

3. **1440p 144Hz:**
```json
{
  "width": 2560,
  "height": 1440,
  "refreshRate": 144
}
```

---

#### GET /api/video/resolution
Pobiera aktualną rozdzielczość ekranu.

**Response:**
```json
{
  "success": true,
  "data": {
    "width": 1920,
    "height": 1080,
    "refreshRate": 60,
    "index": 0
  },
  "errorMessage": null
}
```

---

### 3. Zapis i wczytanie ustawień

#### POST /api/video/save
Zapisuje ustawienia wideo do pliku `video.ini` w folderze `C:\Users\{current-user}\Documents\asseto-manager`.

**Response:**
```json
{
  "success": true,
  "message": "Ustawienia wideo zapisane do: C:\\Users\\{user}\\Documents\\asseto-manager\\video.ini",
  "data": "Ustawienia wideo zapisane do: C:\\Users\\{user}\\Documents\\asseto-manager\\video.ini",
  "errorMessage": null
}
```

**Uwaga:** Folder `asseto-manager` jest tworzony automatycznie, jeśli nie istnieje.

---

#### POST /api/video/load
Wczytuje ustawienia wideo z pliku. Najpierw próbuje wczytać z `C:\Users\{current-user}\Documents\asseto-manager\video.ini`, jeśli plik nie istnieje, próbuje wczytać z domyślnego miejsca (`%Documents%\Assetto Corsa\cfg\video.ini`).

**Response (sukces):**
```json
{
  "success": true,
  "message": "Ustawienia wideo wczytane z: C:\\Users\\{user}\\Documents\\asseto-manager\\video.ini",
  "data": "Ustawienia wideo wczytane z: C:\\Users\\{user}\\Documents\\asseto-manager\\video.ini",
  "errorMessage": null
}
```

**Response (błąd - plik nie istnieje):**
```json
{
  "success": false,
  "message": null,
  "data": null,
  "errorMessage": "Plik nie istnieje: C:\\Users\\{user}\\Documents\\asseto-manager\\video.ini"
}
```

---

#### GET /api/video/state
Pobiera pełny stan ustawień wideo.

**Response:**
```json
{
  "success": true,
  "data": {
    "width": 1920,
    "height": 1080,
    "refreshRate": 60,
    "displayMode": "SINGLE_SCREEN",
    "displayModeName": "Single Screen",
    "index": 0
  },
  "errorMessage": null
}
```

---

## Model danych

### SetDisplayModeRequest
```typescript
interface SetDisplayModeRequest {
  mode: string; // SINGLE_SCREEN, TRIPLE_SCREEN, OPENVR, STEAMVR
}
```

### DisplayModeInfo
```typescript
interface DisplayModeInfo {
  mode: string; // SINGLE_SCREEN, TRIPLE_SCREEN, OPENVR, STEAMVR
  displayName: string; // "Single Screen", "Triple Screen", "OpenVR", "SteamVR"
}
```

### SetResolutionRequest
```typescript
interface SetResolutionRequest {
  width: number;      // Szerokość w pikselach (> 0)
  height: number;     // Wysokość w pikselach (> 0)
  refreshRate: number; // Częstotliwość odświeżania w Hz (> 0)
  index?: number;     // Indeks rozdzielczości (opcjonalne, domyślnie 0)
}
```

### ResolutionInfo
```typescript
interface ResolutionInfo {
  width: number;
  height: number;
  refreshRate: number;
  index: number;
}
```

### VideoState
```typescript
interface VideoState {
  width: number;
  height: number;
  refreshRate: number;
  displayMode: string;      // SINGLE_SCREEN, TRIPLE_SCREEN, OPENVR, STEAMVR
  displayModeName: string;  // "Single Screen", "Triple Screen", "OpenVR", "SteamVR"
  index: number;
}
```

---

## Przykłady użycia

### JavaScript (Fetch API)

#### Ustawienie trybu wyświetlania
```javascript
fetch('http://localhost:8080/api/video/display-mode', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    mode: 'TRIPLE_SCREEN'
  })
})
  .then(response => response.json())
  .then(data => {
    console.log('Tryb wyświetlania ustawiony:', data);
  });
```

#### Ustawienie rozdzielczości
```javascript
fetch('http://localhost:8080/api/video/resolution', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    width: 2560,
    height: 1440,
    refreshRate: 144
  })
})
  .then(response => response.json())
  .then(data => {
    console.log('Rozdzielczość ustawiona:', data);
  });
```

#### Pobranie stanu ustawień
```javascript
fetch('http://localhost:8080/api/video/state')
  .then(response => response.json())
  .then(data => {
    console.log('Aktualne ustawienia:', data.data);
    // data.data zawiera: width, height, refreshRate, displayMode, displayModeName, index
  });
```

#### Zapisanie ustawień
```javascript
fetch('http://localhost:8080/api/video/save', {
  method: 'POST'
})
  .then(response => response.json())
  .then(data => {
    console.log('Ustawienia zapisane:', data);
  });
```

### cURL

#### Ustawienie trybu wyświetlania
```bash
curl -X POST http://localhost:8080/api/video/display-mode \
  -H "Content-Type: application/json" \
  -d '{"mode":"SINGLE_SCREEN"}'
```

#### Ustawienie rozdzielczości
```bash
curl -X POST http://localhost:8080/api/video/resolution \
  -H "Content-Type: application/json" \
  -d '{
    "width": 1920,
    "height": 1080,
    "refreshRate": 60
  }'
```

#### Pobranie stanu
```bash
curl http://localhost:8080/api/video/state
```

#### Zapisanie ustawień
```bash
curl -X POST http://localhost:8080/api/video/save
```

---

## Przechowywanie konfiguracji

### Lokalizacja pliku
```
C:\Users\{current-user}\Documents\asseto-manager\video.ini
```

### Format w pliku video.ini

Plik `video.ini` zawiera następujące sekcje:

```ini
[VIDEO]
WIDTH=1920
HEIGHT=1080
REFRESH=60
INDEX=0
FULLSCREEN=1
VSYNC=0
AASAMPLES=4
ANISOTROPIC=16
SHADOW_MAP_SIZE=4096
FPS_CAP_MS=5
AAQUALITY=0
DISABLE_LEGACY_HDR=1

[REFRESH]
VALUE=60

[CAMERA]
MODE=DEFAULT
```

**Mapowanie trybów wyświetlania:**
- `SINGLE_SCREEN` → `CAMERA.MODE=DEFAULT`
- `TRIPLE_SCREEN` → `CAMERA.MODE=TRIPLE`
- `OPENVR` → `CAMERA.MODE=OPENVR`
- `STEAMVR` → `CAMERA.MODE=OPENVR`

---

## Kopiowanie przed uruchomieniem gry

Przed uruchomieniem gry, plik `video.ini` jest automatycznie kopiowany z folderu `asseto-manager` do właściwego miejsca dla gry:

```
%Documents%\Assetto Corsa\cfg\video.ini
```

Dzięki temu gra używa skonfigurowanych ustawień graficznych.

---

## Walidacja

### Przy ustawianiu trybu wyświetlania
- Tryb nie może być pusty
- Musi być jednym z dozwolonych: `SINGLE_SCREEN`, `TRIPLE_SCREEN`, `OPENVR`, `STEAMVR`

### Przy ustawianiu rozdzielczości
- `width` musi być > 0
- `height` musi być > 0
- `refreshRate` musi być > 0
- `index` jest opcjonalne (domyślnie 0)

---

## Przykładowe scenariusze użycia

### Scenariusz 1: Konfiguracja podstawowa
1. Użytkownik ustawia tryb wyświetlania: `POST /api/video/display-mode` z `mode: "SINGLE_SCREEN"`
2. Użytkownik ustawia rozdzielczość: `POST /api/video/resolution` z `width: 1920, height: 1080, refreshRate: 60`
3. Użytkownik zapisuje ustawienia: `POST /api/video/save`
4. Przed uruchomieniem gry, plik jest kopiowany do właściwego miejsca

### Scenariusz 2: Konfiguracja dla Triple Screen
1. Użytkownik ustawia tryb: `POST /api/video/display-mode` z `mode: "TRIPLE_SCREEN"`
2. Użytkownik ustawia rozdzielczość: `POST /api/video/resolution` z `width: 5760, height: 1080, refreshRate: 60` (3x 1920x1080)
3. Użytkownik zapisuje: `POST /api/video/save`

### Scenariusz 3: Konfiguracja dla VR
1. Użytkownik ustawia tryb: `POST /api/video/display-mode` z `mode: "STEAMVR"`
2. Użytkownik ustawia rozdzielczość: `POST /api/video/resolution` z odpowiednimi wartościami dla VR
3. Użytkownik zapisuje: `POST /api/video/save`

### Scenariusz 4: Wczytanie istniejących ustawień
1. Użytkownik wczytuje ustawienia: `POST /api/video/load`
2. Użytkownik sprawdza aktualne wartości: `GET /api/video/state`
3. Użytkownik modyfikuje ustawienia i zapisuje: `POST /api/video/save`

---

## Logowanie

Wszystkie operacje są logowane w konsoli z timestampem:

```
[2024-01-15 10:30:00.123] [VIDEO] Tryb wyświetlania ustawiony: SINGLE_SCREEN (CAMERA.MODE=DEFAULT)
[2024-01-15 10:30:05.456] [VIDEO] Rozdzielczość ustawiona: 1920x1080@60Hz
[2024-01-15 10:30:10.789] [VIDEO] Zapisano video.ini do: C:\Users\...\Documents\asseto-manager\video.ini
[2024-01-15 10:30:15.012] [VIDEO] video.ini skopiowany z ... do: ...\Assetto Corsa\cfg\video.ini
```

---

## Błędy i obsługa

### Błąd: Nieprawidłowy tryb wyświetlania
```json
{
  "success": false,
  "errorMessage": "Nieprawidłowy tryb wyświetlania. Dozwolone: SINGLE_SCREEN, TRIPLE_SCREEN, OPENVR, STEAMVR"
}
```
**Rozwiązanie:** Upewnij się, że używasz jednego z dozwolonych trybów.

### Błąd: Nieprawidłowa rozdzielczość
```json
{
  "success": false,
  "errorMessage": "Szerokość i wysokość muszą być większe od 0"
}
```
**Rozwiązanie:** Upewnij się, że `width`, `height` i `refreshRate` są większe od 0.

### Błąd: Plik nie istnieje
```json
{
  "success": false,
  "errorMessage": "Plik nie istnieje: C:\\Users\\...\\Documents\\asseto-manager\\video.ini"
}
```
**Rozwiązanie:** Najpierw zapisz ustawienia używając `POST /api/video/save`, lub upewnij się, że plik istnieje w domyślnym miejscu.

---

## Uwagi techniczne

1. **Mapowanie trybów:** SteamVR i OpenVR używają tego samego trybu w pliku `video.ini` (`CAMERA.MODE=OPENVR`), ale API rozróżnia je dla wygody użytkownika.

2. **Persystencja:** Konfiguracja jest zapisywana w `video.ini` w folderze `asseto-manager` i przywracana przy wczytywaniu.

3. **Kopiowanie przed uruchomieniem:** Przed uruchomieniem gry, plik `video.ini` jest automatycznie kopiowany do właściwego miejsca dla Assetto Corsa.

4. **Walidacja:** API waliduje wszystkie dane wejściowe przed zapisem.

5. **Logowanie:** Wszystkie operacje są logowane w konsoli z timestampem.

6. **Encoding:** Plik `video.ini` jest zapisywany z kodowaniem `Encoding.Default` (Windows-1252) dla zgodności z Assetto Corsa.

---

## Data ostatniej aktualizacji

**Wersja:** Aktualna  
**Data:** 2024-01-15  
**Ostatnia zmiana:** Dodano endpointy do konfiguracji ustawień graficznych (tryb wyświetlania, rozdzielczość, refresh rate)


