# API Changelog - IQPower Content Manager

## Zmiany wprowadzone w API

### 1. Usunięcie automatycznego bindowania (Autobind)

**Data:** Aktualna wersja

**Zmiany:**
- Usunięto automatyczne bindowanie po wykryciu inputu
- API teraz tylko wykrywa input (oś/przycisk) i zwraca informacje
- Frontend musi ręcznie wywołać `POST /api/controls/bind` z wykrytymi wartościami

**Przed:**
- API automatycznie bindowało po wykryciu inputu

**Po:**
- API wykrywa input i zwraca status
- Frontend wywołuje `POST /api/controls/bind` z wykrytymi wartościami

**Endpointy wykrywania:**
- `POST /api/controls/bind/{action}/start` - rozpoczyna wykrywanie
- `GET /api/controls/bind/{action}` - sprawdza status wykrywania

---

### 2. Indeksowanie osi

**Data:** Aktualna wersja

**Zmiany:**
- Osi są indeksowane od 1 w API (dla użytkownika)
- W pliku `controls.ini` osi są zapisywane jako 0-indexed (0, 1, 2, ...)
- API automatycznie konwertuje między formatami

**Przykład:**
- Użytkownik wybiera "oś 1" w frontendzie
- API otrzymuje `axleIndex: 1`
- API konwertuje na `axleIndex: 0` wewnętrznie
- W pliku controls.ini zapisuje się jako `AXLE=0`
- W odpowiedziach API zwraca `axleIndex: 1`

**Wpływ:**
- Wszystkie odpowiedzi API używają 1-indexed dla osi
- Wszystkie logi w konsoli używają 1-indexed dla osi
- Plik controls.ini używa 0-indexed (zgodnie z formatem Assetto Corsa)

---

### 3. Logowanie w konsoli

**Data:** Aktualna wersja

**Zmiany:**
- Dodano szczegółowe logowanie wszystkich operacji w konsoli
- Wszystkie operacje bindowania są logowane z timestampem
- Logi zawierają informacje o wykrytych inputach, błędach, sukcesach

**Format logów:**
```
[2024-01-15 10:30:00.123] [BINDING] [STEER] Rozpoczęto wykrywanie na urządzeniu: Logitech G29 (index: 0)
[2024-01-15 10:30:05.456] [BINDING] [STEER] ✓ Wykryto oś 1 na urządzeniu Logitech G29 (index: 0)
```

**Kategorie logów:**
- `[BINDING]` - operacje bindowania
- `[SETUP]` - operacje konfiguracji (nick, car, track, gearbox)
- `[API]` - operacje API (rejestracja kontrolerów, start/stop)

---

### 4. Setup Controller - Nowe endpointy

**Data:** Aktualna wersja

**Dodano nowy kontroler:** `SetupController`

**Endpointy:**

#### Nick (Nazwa gracza)
- `POST /api/setup/nick` - ustawia nick
- `GET /api/setup/nick` - pobiera aktualny nick

#### Samochody (Cars)
- `GET /api/setup/cars` - pobiera listę dostępnych samochodów z czytelnymi nazwami
- `POST /api/setup/car` - wybiera samochód
- `GET /api/setup/car` - pobiera aktualnie wybrany samochód

**Dostępne samochody:**
- Porsche 911 Turbo S (`ks_porsche_991_turbo_s`)
- Porsche 992 GT3 RS (`cky_porsche992_gt3rs_2023`)
- Nissan GT-R NISMO (`ks_nissan_gtr`)

#### Tory (Tracks)
- `GET /api/setup/tracks` - pobiera listę dostępnych torów
- `POST /api/setup/track` - wybiera tor
- `GET /api/setup/track` - pobiera aktualnie wybrany tor

#### Skrzynia biegów (Gearbox)
- `GET /api/setup/gearbox-types` - pobiera listę dostępnych typów skrzyni
- `POST /api/setup/gearbox` - ustawia typ skrzyni
- `GET /api/setup/gearbox` - pobiera aktualny typ skrzyni

**Dostępne typy skrzyni:**
- `automatic` - Automatyczna skrzynia biegów
- `sequential` - Sekwencyjna skrzynia biegów
- `h-pattern` - Manualna skrzynia biegów H-Pattern

#### Podsumowanie i uruchomienie
- `GET /api/setup/summary` - pobiera podsumowanie konfiguracji
- `POST /api/setup/launch` - uruchamia grę z aktualną konfiguracją

**Uwagi:**
- Wszystkie wybory są zapisywane w `application_state.json`
- Endpoint `/api/setup/launch` wymaga ustawienia wszystkich pól (nick, shifterType, trackId, carId)
- Automatycznie znajduje ścieżkę do Assetto Corsa (sprawdza typowe lokalizacje Steam)

---

### 5. HANDBRAKE zawsze jako oś

**Data:** Aktualna wersja

**Zmiany:**
- HANDBRAKE jest teraz zawsze bindowany jako oś (axis)
- Usunięto możliwość bindowania HANDBRAKE jako przycisk
- Wykrywanie HANDBRAKE używa `DetectAxisInBackground` (tak jak STEER, THROTTLE, BRAKES, CLUTCH)

**Przed:**
- HANDBRAKE mógł być bindowany jako przycisk lub oś
- API pytało użytkownika o wybór

**Po:**
- HANDBRAKE zawsze wymaga `AxleIndex`
- Jeśli podano `ButtonIndex` dla HANDBRAKE, API zwraca błąd
- W odpowiedziach API HANDBRAKE zwraca tylko `AxleIndex` (bez `ButtonIndex`)

**Endpointy:**
- `POST /api/controls/bind/handbrake/start` - wykrywa oś
- `GET /api/controls/bind/handbrake` - sprawdza status wykrywania

---

### 6. Osobne endpointy dla GEARUP i PADDLEUP

**Data:** Aktualna wersja

**Zmiany:**
- GEARUP i PADDLEUP są teraz osobnymi akcjami w API
- Każda akcja zapisuje się do osobnej sekcji w pliku controls.ini

**Przed:**
- API automatycznie wybierało między GEARUP a PADDLEUP w zależności od dostępności
- Oba bindy były wyświetlane jako jedna akcja "GEARUP" z dwoma bindami

**Po:**
- `POST /api/controls/bind` z `Action: "GEARUP"` → zapisuje się jako `[GEARUP]` w pliku
- `POST /api/controls/bind` z `Action: "PADDLEUP"` → zapisuje się jako `[PADDLEUP]` w pliku
- `POST /api/controls/bind` z `Action: "GEARDN"` → zapisuje się jako `[GEARDN]` w pliku
- `POST /api/controls/bind` z `Action: "PADDLEDN"` → zapisuje się jako `[PADDLEDN]` w pliku

**Nowe endpointy wykrywania:**
- `POST /api/controls/bind/gearup/start` i `GET /api/controls/bind/gearup`
- `POST /api/controls/bind/paddleup/start` i `GET /api/controls/bind/paddleup`
- `POST /api/controls/bind/geardn/start` i `GET /api/controls/bind/geardn`
- `POST /api/controls/bind/paddledn/start` i `GET /api/controls/bind/paddledn`

**Format w pliku controls.ini:**

```
[GEARUP]
JOY=0
BUTTON=5
KEY=0x57 ; W
__CM_ALT_BUTTON=-1

[PADDLEUP]
JOY=0
BUTTON=6
```

**GetBindings:**
- GEARUP i PADDLEUP są wyświetlane jako osobne akcje
- Każda akcja ma swój własny bind (nie ma już "GEARUP_1" i "GEARUP_2")

**GetState:**
- Dodano pola `PaddleUp` i `PaddleDown` do `ControlsState`

---

### 7. Naprawienie zapisu GEARDN do pliku

**Data:** Aktualna wersja

**Problem:**
- Sekcja `[GEARDN]` w pliku controls.ini była zakodowana na sztywno z wartościami `JOY=3`, `BUTTON=12`, itd.

**Rozwiązanie:**
- Sekcja `[GEARDN]` teraz używa wartości z obiektów `GearDnButtonEntry` i `PaddleDnButtonEntry`
- Format zapisu jest teraz spójny z `[GEARUP]`

**Format w pliku controls.ini:**

```
[GEARDN]
JOY=0
BUTTON=3
KEY=0x53 ; S
__CM_ALT_BUTTON=-1
__CM_ALT_JOY=-1
```

---

### 8. Usunięte modele

**Data:** Aktualna wersja

**Usunięto:**
- `AutoBindRequest` - zastąpiony przez `BindingDetectionRequest`
- `AutoBindResponse` - usunięty (nie jest już potrzebny)

**Nowe modele:**
- `BindingDetectionRequest` - request do rozpoczęcia wykrywania inputu
- `BindingDetectionStatus` - status wykrywania inputu

---

## Podsumowanie zmian w endpointach

### Nowe endpointy

#### Setup
- `GET /api/setup/cars` - lista samochodów
- `GET /api/setup/tracks` - lista torów
- `GET /api/setup/gearbox-types` - lista typów skrzyni
- `POST /api/setup/launch` - uruchomienie gry

#### Controls - Wykrywanie
- `POST /api/controls/bind/paddleup/start` - wykrywanie PADDLEUP
- `GET /api/controls/bind/paddleup` - status PADDLEUP
- `POST /api/controls/bind/paddledn/start` - wykrywanie PADDLEDN
- `GET /api/controls/bind/paddledn` - status PADDLEDN

### Zmienione endpointy

#### Controls - Bindowanie
- `POST /api/controls/bind` - teraz obsługuje `Action: "PADDLEUP"` i `Action: "PADDLEDN"` jako osobne akcje
- `GET /api/controls/bindings` - wyświetla GEARUP, PADDLEUP, GEARDN, PADDLEDN jako osobne akcje
- `GET /api/controls/state` - zwraca `PaddleUp` i `PaddleDown` w `ControlsState`

### Usunięte endpointy

- Brak (wszystkie endpointy są zachowane, tylko zmieniona logika)

---

## Migracja dla frontendu

### 1. Wykrywanie inputów

**Przed:**
```javascript
// API automatycznie bindowało po wykryciu
POST /api/controls/bind/steer/start
GET /api/controls/bind/steer // zwracał już zbindowany bind
```

**Po:**
```javascript
// 1. Rozpocznij wykrywanie
POST /api/controls/bind/steer/start
{ "controllerIndex": 0, "timeoutSeconds": 15 }

// 2. Sprawdzaj status (polling co 500ms)
GET /api/controls/bind/steer
// Zwraca: { "detectedAxis": 1, "isCompleted": true, ... }

// 3. Zbinduj ręcznie
POST /api/controls/bind
{
  "action": "STEER",
  "controllerIndex": 0,
  "axleIndex": 1 // użyj wykrytej wartości
}
```

### 2. Indeksowanie osi

**Przed:**
- Osi były 0-indexed w API

**Po:**
- Osi są 1-indexed w API
- Frontend powinien wyświetlać "Oś 1", "Oś 2", itd.
- Przy wysyłaniu requestu użyj wartości 1-indexed (API automatycznie konwertuje)

### 3. GEARUP i PADDLEUP

**Przed:**
```javascript
POST /api/controls/bind
{ "action": "GEARUP", ... }
// API automatycznie wybierało między GEARUP a PADDLEUP
```

**Po:**
```javascript
// Wybierz konkretną akcję
POST /api/controls/bind
{ "action": "GEARUP", ... } // zapisuje się jako [GEARUP]

POST /api/controls/bind
{ "action": "PADDLEUP", ... } // zapisuje się jako [PADDLEUP]
```

### 4. HANDBRAKE

**Przed:**
- HANDBRAKE mógł być przyciskiem lub osią

**Po:**
- HANDBRAKE zawsze wymaga `AxleIndex`
- Nie używaj `ButtonIndex` dla HANDBRAKE

---

## Format pliku controls.ini

### Przed zmianami
```
[GEARDN]
JOY=3
BUTTON=12
__CM_ALT_BUTTON=13
KEY=-1
__CM_ALT_JOY=-1
```

### Po zmianach
```
[GEARUP]
JOY=0
BUTTON=5
KEY=0x57 ; W
__CM_ALT_BUTTON=-1

[PADDLEUP]
JOY=0
BUTTON=6

[GEARDN]
JOY=0
BUTTON=3
KEY=0x53 ; S
__CM_ALT_BUTTON=-1
__CM_ALT_JOY=-1

[PADDLEDN]
JOY=0
BUTTON=4
```

---

## Uwagi dla deweloperów

1. **Indeksowanie osi:** Pamiętaj, że osi są 1-indexed w API, ale 0-indexed w pliku
2. **Wykrywanie inputów:** Użyj polling (sprawdzaj status co 500ms) zamiast oczekiwać na automatyczne bindowanie
3. **GEARUP vs PADDLEUP:** Są to teraz osobne akcje - użytkownik musi wybrać, którą chce użyć
4. **HANDBRAKE:** Zawsze jako oś - nie próbuj bindować jako przycisk
5. **Logowanie:** Wszystkie operacje są logowane w konsoli - sprawdzaj logi w przypadku problemów

---

## Kompatybilność wsteczna

**Breaking Changes:**
- ❌ Usunięto automatyczne bindowanie - frontend musi ręcznie bindować
- ❌ GEARUP i PADDLEUP są teraz osobnymi akcjami
- ❌ HANDBRAKE nie może być już przyciskiem

**Zachowane:**
- ✅ Wszystkie istniejące endpointy działają (tylko zmieniona logika)
- ✅ Format pliku controls.ini jest kompatybilny z Assetto Corsa
- ✅ Struktura odpowiedzi API pozostaje taka sama (tylko dodane nowe pola)

---

## Data ostatniej aktualizacji

**Wersja:** Aktualna  
**Data:** 2024-01-15

