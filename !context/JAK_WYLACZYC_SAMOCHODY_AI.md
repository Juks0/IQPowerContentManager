# Jak wyłączyć samochody AI/traffic i używać tylko swojego samochodu

## Problem

Podczas uruchamiania gry widzisz błędy o brakujących kluczach w plikach INI samochodów traffic:

```
ERROR: INIReader: content/cars/traffic_toyota_camry/data/lights.ini > KEY_NOT_FOUND: [BRAKE_0] OFF_COLOR
ERROR: INIReader: content/cars/traffic_volvo_v70jp/data/lights.ini > KEY_NOT_FOUND: [BRAKE_0] OFF_COLOR
ERROR: INIReader: content/cars/traffic_aegis_izuzu_npr_box/data/driver3d.ini > KEY_NOT_FOUND: [SHIFT_ANIMATION] INVERT_SHIFTING_HANDS
...
```

**Przyczyna:** Gra próbuje załadować samochody AI/traffic, ponieważ w pliku `race.ini` jest ustawione `CARS = 7` (lub więcej), co oznacza, że gra ma załadować 7 samochodów (1 gracz + 6 AI).

## Rozwiązanie: Ustaw CARS = 1

Aby używać **tylko swojego samochodu** bez samochodów AI/traffic, musisz ustawić `CARS = 1` w pliku `race.ini`.

### Rozwiązanie 1: Przez Content Manager (Zalecane)

1. **Otwórz Content Manager**
2. **Przejdź do:** Drive → Quick Drive (lub Race)
3. **Wybierz swój samochód** (np. `cky_porsche992_gt3rs_2023`)
4. **Wybierz tor**
5. **W sekcji "Opponents" lub "AI Cars":**
   - Ustaw liczbę przeciwników na **0**
   - LUB wyłącz opcję "Add AI cars"
6. **Uruchom wyścig** - Content Manager automatycznie ustawi `CARS = 1`

### Rozwiązanie 2: Ręczna edycja race.ini

**Plik:** `{Documents}\Assetto Corsa\cfg\race.ini`

**Znajdź sekcję `[RACE]` i zmień:**

```ini
[RACE]
MODEL = cky_porsche992_gt3rs_2023
TRACK = ks_nordschleife
CONFIG_TRACK = 
CARS = 1                    # ZMIEŃ NA 1 (tylko gracz, bez AI)
AI_LEVEL = 98              # Możesz usunąć lub zostawić
FIXED_SETUP = 0
PENALTIES = 0
```

**Ważne:** 
- `CARS = 1` oznacza **tylko samochód gracza** (CAR_0)
- `CARS = 2` oznacza **1 gracz + 1 AI**
- `CARS = 7` oznacza **1 gracz + 6 AI** (dlatego widzisz błędy z samochodami traffic)

### Rozwiązanie 3: Programowo (jeśli tworzysz konfigurację w kodzie)

**Jeśli używasz Content Manager API:**

```csharp
await GameWrapper.StartAsync(new Game.StartProperties {
    BasicProperties = new Game.BasicProperties {
        CarId = "cky_porsche992_gt3rs_2023",
        TrackId = "ks_nordschleife"
    },
    ModeProperties = new Game.PracticeProperties {
        // Practice mode automatycznie ustawia CARS = 1
    }
});
```

**Lub jeśli używasz Race mode:**

```csharp
await GameWrapper.StartAsync(new Game.StartProperties {
    BasicProperties = new Game.BasicProperties {
        CarId = "cky_porsche992_gt3rs_2023",
        TrackId = "ks_nordschleife"
    },
    ModeProperties = new Game.RaceProperties {
        BotCars = new Game.AiCar[0],  // Pusta lista = brak AI
        // LUB
        BotCars = null,  // null = brak AI
    }
});
```

**Jeśli tworzysz plik race.ini ręcznie:**

```csharp
var raceIni = new IniFile();
raceIni["RACE"].Set("MODEL", "cky_porsche992_gt3rs_2023");
raceIni["RACE"].Set("TRACK", "ks_nordschleife");
raceIni["RACE"].Set("CARS", 1);  // TYLKO GRACZ
raceIni["RACE"].Set("AI_LEVEL", 98);  // Opcjonalne

raceIni["CAR_0"] = new IniFileSection(null) {
    ["MODEL"] = "-",  // Używa samochodu z sekcji RACE
    ["SKIN"] = "Racing_Green_Stripe"
};

raceIni["SESSION_0"] = new IniFileSection(null) {
    ["NAME"] = "Practice",
    ["TYPE"] = 1,  // 1 = Practice
    ["DURATION_MINUTES"] = 0,
    ["SPAWN_SET"] = "PIT"
};

// NIE DODAWAJ sekcji CAR_1, CAR_2, itp. - to są samochody AI

raceIni.Save(AcPaths.GetRaceIniFilename());
```

## Pełny przykład poprawnego race.ini (tylko gracz)

```ini
[HEADER]
VERSION = 2

[RACE]
MODEL = cky_porsche992_gt3rs_2023
CONFIG_TRACK = 
TRACK = ks_nordschleife
CARS = 1                    # TYLKO GRACZ - NIE ZMIENIAJ NA WIĘCEJ!
AI_LEVEL = 98              # Opcjonalne (nie używane gdy CARS = 1)
FIXED_SETUP = 0
PENALTIES = 0

[REMOTE]
ACTIVE = 0

[CAR_0]
MODEL = -                   # "-" oznacza użycie z sekcji RACE
MODEL_CONFIG = 
SKIN = Racing_Green_Stripe
DRIVER_NAME = "Your Name"
NATIONALITY = "POL"
NATION_CODE = "POL"
BALLAST = 0
RESTRICTOR = 0
SETUP = 

[GHOST_CAR]
RECORDING = 1
PLAYING = 1
SECONDS_ADVANTAGE = 0
LOAD = 1
FILE = 

[REPLAY]
ACTIVE = 0
FILENAME = 

[LIGHTING]
SUN_ANGLE = -48.0
TIME_MULT = 1.0
CLOUD_SPEED = 0.2

[TEMPERATURE]
AMBIENT = 26
ROAD = 32

[WEATHER]
NAME = "4_mid_clear"

[DYNAMIC_TRACK]
SESSION_START = 100
RANDOMNESS = 0
LAP_GAIN = 1
SESSION_TRANSFER = 50

[GROOVE]
VIRTUAL_LAPS = 10
STARTING_LAPS = 0
MAX_LAPS = 30

[SESSION_0]
NAME = "Practice"
TYPE = 1                    # 1 = Practice, 2 = Qualify, 3 = Race
DURATION_MINUTES = 0
SPAWN_SET = "PIT"

[LAP_INVALIDATOR]
ALLOWED_TYRES_OUT = -1
```

**Uwaga:** W tym przykładzie **NIE MA** sekcji `[CAR_1]`, `[CAR_2]`, itp. - to są samochody AI, które powodują błędy.

## Dlaczego widzisz błędy z samochodami traffic?

Gdy `CARS > 1`, gra próbuje załadować samochody AI. Jeśli nie określisz konkretnych samochodów AI w sekcjach `[CAR_1]`, `[CAR_2]`, itp., gra **automatycznie wybiera losowe samochody** z dostępnych w grze, w tym samochody traffic (traffic_toyota_camry, traffic_volvo_v70jp, itp.).

Te samochody traffic często mają niekompletne pliki INI (brakujące opcjonalne klucze), co powoduje ostrzeżenia w logu.

## Jak dodać konkretne samochody AI (opcjonalne)

Jeśli chcesz mieć samochody AI, ale **nie traffic**, możesz określić konkretne samochody:

```ini
[RACE]
MODEL = cky_porsche992_gt3rs_2023
TRACK = ks_nordschleife
CARS = 4                    # 1 gracz + 3 AI

[CAR_0]
MODEL = -                   # Gracz (używa samochodu z RACE)

[CAR_1]
MODEL = ferrari_f40         # AI 1 - konkretny samochód
SKIN = racing_red
AI_LEVEL = 95

[CAR_2]
MODEL = lamborghini_huracan # AI 2 - konkretny samochód
SKIN = yellow
AI_LEVEL = 90

[CAR_3]
MODEL = mclaren_mp4_12c     # AI 3 - konkretny samochód
SKIN = orange
AI_LEVEL = 92
```

**Ważne:** Jeśli nie określisz `MODEL` dla `[CAR_1]`, `[CAR_2]`, itp., gra wybierze losowe samochody, w tym traffic.

## Sprawdzenie konfiguracji

### Przed uruchomieniem gry:

1. **Otwórz plik:** `{Documents}\Assetto Corsa\cfg\race.ini`
2. **Sprawdź wartość `CARS`:**
   - Jeśli `CARS = 1` → ✅ Tylko gracz, brak błędów z AI
   - Jeśli `CARS > 1` → ⚠️ Będą samochody AI, mogą być błędy
3. **Sprawdź sekcje `[CAR_X]`:**
   - Powinna być tylko `[CAR_0]` (gracz)
   - Jeśli są `[CAR_1]`, `[CAR_2]`, itp. → to są samochody AI

### Po uruchomieniu gry:

**Sprawdź log:** `{Documents}\Assetto Corsa\logs\log.txt`

**Jeśli widzisz:**
```
Creating car: traffic_toyota_camry []
Creating car: traffic_volvo_v70jp []
Creating car: traffic_aegis_izuzu_npr_box []
```

To oznacza, że gra ładuje samochody traffic → **zmień `CARS = 1`**.

**Jeśli widzisz tylko:**
```
Creating car: cky_porsche992_gt3rs_2023 []
```

To oznacza, że wszystko jest OK → **tylko twój samochód jest ładowany**.

## Podsumowanie

**Aby wyłączyć samochody AI/traffic:**

1. ✅ Ustaw `CARS = 1` w pliku `race.ini`
2. ✅ Upewnij się, że są tylko sekcje `[CAR_0]` (gracz)
3. ✅ Usuń lub nie dodawaj sekcji `[CAR_1]`, `[CAR_2]`, itp.
4. ✅ Uruchom wyścig przez Content Manager z opcją "0 opponents"

**Rezultat:**
- ✅ Tylko twój samochód będzie ładowany
- ✅ Brak błędów z samochodami traffic
- ✅ Szybsze ładowanie gry
- ✅ Mniej użycia pamięci

**Pamiętaj:** Te błędy o brakujących kluczach są **niekrytyczne** - gra działa normalnie nawet z nimi. Jednak jeśli chcesz mieć czyste logi i używać tylko swojego samochodu, ustaw `CARS = 1`.

