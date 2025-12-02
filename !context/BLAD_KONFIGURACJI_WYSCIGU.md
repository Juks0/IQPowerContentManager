# Błąd: "Can't launch the race: invalid race configuration"

## Opis problemu

Po utworzeniu nowego projektu używając "context", gra Assetto Corsa z Custom Shaders Patch (CSP) nie może uruchomić wyścigu i wyświetla błąd:

```
ERROR | Can't launch the race: invalid race configuration.
```

## Analiza logów

### 1. Błąd w custom_shaders_patch.log

**Linia 131-132:**
```
Sim.loadTrack(): /, player_pit_position=0
Loading configs for , config: 
```

**Problem:** Tor jest pusty (`/`), a konfiguracja toru też jest pusta.

**Linia 154:**
```
ERROR | Can't launch the race: invalid race configuration.
```

**Linia 159:**
```
ERROR | Crash! Trying to generate nice dump: 00000083C85FC350
```

### 2. Błędy w log.txt

**Linia 131:**
```
Sim.loadTrack(): /, player_pit_position=0
```

**Linia 547-550:**
```
ERROR: INIReader: C:\Users\kacpe\Documents/Assetto Corsa/cfg/race.ini > KEY_NOT_FOUND: [SESSION_0] NAME
ERROR: INIReader: C:\Users\kacpe\Documents/Assetto Corsa/cfg/race.ini > KEY_NOT_FOUND: [SESSION_0] TYPE
ERROR: INIReader: C:\Users\kacpe\Documents/Assetto Corsa/cfg/race.ini > KEY_NOT_FOUND: [SESSION_0] SPAWN_SET
ERROR: INIReader: C:\Users\kacpe\Documents/Assetto Corsa/cfg/race.ini > KEY_NOT_FOUND: [RACE] TRACK
```

**Linia 553:**
```
[CSP SAYS] Can't launch the race: invalid race configuration.
```

### 3. Sprzeczność w logach

**Ważne:** W logu (linie 464-546) widzimy, że plik `race.ini` **ZAWIRA** wymagane wartości:

```ini
[RACE]
TRACK = ks_nordschleife
MODEL = cky_porsche992_gt3rs_2023
...

[SESSION_0]
NAME = "Practice"
TYPE = 1
SPAWN_SET = "PIT"
...
```

Ale później (linie 547-550) gra zgłasza, że te klucze **NIE ISTNIEJĄ**.

## Przyczyna problemu

### Możliwe przyczyny:

1. **Plik race.ini jest czytany dwa razy:**
   - Pierwszy raz: poprawnie (wartości są widoczne w logu)
   - Drugi raz: niepoprawnie (wartości są puste)

2. **Problem z formatowaniem pliku:**
   - Plik może mieć nieprawidłowe kodowanie (np. UTF-8 z BOM zamiast ASCII)
   - Plik może mieć nieprawidłowe znaki końca linii (np. tylko LF zamiast CRLF)
   - Plik może mieć nieprawidłowe spacje lub tabulatory

3. **Problem z parsowaniem INI:**
   - Parser INI może nie rozpoznawać niektórych sekcji lub kluczy
   - Plik może być uszkodzony lub niekompletny

4. **Problem z wartościami:**
   - Wartości mogą być puste (`TRACK = ` zamiast `TRACK = ks_nordschleife`)
   - Wartości mogą zawierać nieprawidłowe znaki

5. **Problem z kontekstem projektu:**
   - Jeśli używasz "context" do generowania konfiguracji, może on tworzyć niepoprawny format pliku
   - Może brakować wymaganych sekcji lub kluczy

## Rozwiązanie

### Krok 1: Sprawdź plik race.ini

Otwórz plik:
```
C:\Users\kacpe\Documents\Assetto Corsa\cfg\race.ini
```

**Sprawdź czy zawiera:**

```ini
[HEADER]
VERSION = 2

[RACE]
TRACK = ks_nordschleife          # MUSI być wypełnione (nie puste!)
MODEL = cky_porsche992_gt3rs_2023 # MUSI być wypełnione (nie puste!)
CONFIG_TRACK =                   # Może być puste
CARS = 1
AI_LEVEL = 98
FIXED_SETUP = 0
PENALTIES = 0

[SESSION_0]
NAME = "Practice"                # MUSI być wypełnione (nie puste!)
TYPE = 1                         # MUSI być wypełnione (nie puste!)
DURATION_MINUTES = 0
SPAWN_SET = "PIT"                # MUSI być wypełnione (nie puste!)

[CAR_0]
MODEL = -                        # "-" oznacza użycie z sekcji RACE
SKIN = Racing_Green_Stripe
DRIVER_NAME = "gfd"
NATIONALITY = "POL"
NATION_CODE = "POL"
AI_LEVEL = 96
BALLAST = 0
RESTRICTOR = 0

[LIGHTING]
SUN_ANGLE = -48.0
TIME_MULT = 1.0
CLOUD_SPEED = 0.2

[WEATHER]
NAME = "4_mid_clear"

[TEMPERATURE]
AMBIENT = 26
ROAD = 32
```

### Krok 2: Sprawdź formatowanie

**Upewnij się, że:**
- Każda sekcja zaczyna się od `[NAZWA_SEKCJI]` w osobnej linii
- Każdy klucz jest w formacie `KLUCZ = wartość`
- Nie ma pustych linii między sekcjami (lub są tylko pojedyncze puste linie)
- Używasz znaków końca linii Windows (CRLF: `\r\n`)

### Krok 3: Sprawdź kodowanie

**Otwórz plik w Notatniku++ lub innym edytorze:**
- Sprawdź kodowanie: powinno być **ANSI** lub **UTF-8 bez BOM**
- Jeśli jest UTF-8 z BOM, zapisz jako UTF-8 bez BOM lub ANSI

### Krok 4: Sprawdź wartości

**Upewnij się, że:**
- `[RACE] TRACK` **NIE jest puste** - musi zawierać ID toru (np. `ks_nordschleife`)
- `[RACE] MODEL` **NIE jest puste** - musi zawierać ID samochodu
- `[SESSION_0] NAME` **NIE jest puste** - musi zawierać nazwę sesji
- `[SESSION_0] TYPE` **NIE jest puste** - musi zawierać typ sesji (1 = Practice, 2 = Qualifying, 3 = Race)
- `[SESSION_0] SPAWN_SET` **NIE jest puste** - musi zawierać miejsce startu (np. `"PIT"`)

### Krok 5: Sprawdź czy tor istnieje

**Upewnij się, że tor istnieje:**
```
{AC_ROOT}\content\tracks\ks_nordschleife\
```

Jeśli tor nie istnieje, zmień `TRACK` na istniejący tor.

### Krok 6: Sprawdź czy samochód istnieje

**Upewnij się, że samochód istnieje:**
```
{AC_ROOT}\content\cars\cky_porsche992_gt3rs_2023\
```

Jeśli samochód nie istnieje, zmień `MODEL` na istniejący samochód.

### Krok 7: Użyj Content Manager do naprawy

**Jeśli używasz Content Manager:**

1. Otwórz Content Manager
2. Przejdź do Quick Drive lub Race
3. Wybierz samochód i tor
4. Uruchom wyścig - Content Manager automatycznie wygeneruje poprawny plik `race.ini`

### Krok 8: Ręczna naprawa

**Jeśli problem nadal występuje:**

1. **Zapisz kopię zapasową** obecnego pliku `race.ini`
2. **Usuń** plik `race.ini`
3. **Uruchom grę** przez Content Manager - automatycznie utworzy nowy plik
4. **LUB** skopiuj poprawny plik `race.ini` z innego projektu

## Przykład poprawnego pliku race.ini

```ini
[HEADER]
VERSION = 2

[RACE]
TRACK = ks_nordschleife
CONFIG_TRACK = 
MODEL = cky_porsche992_gt3rs_2023
MODEL_CONFIG = 
CARS = 1
AI_LEVEL = 98
FIXED_SETUP = 0
PENALTIES = 0

[REMOTE]
ACTIVE = 0
SERVER_IP = 
SERVER_PORT = 
NAME = 
TEAM = 
GUID = 
REQUESTED_CAR = 
PASSWORD = 

[CAR_0]
MODEL = -
MODEL_CONFIG = 
SKIN = Racing_Green_Stripe
DRIVER_NAME = "gfd"
NATIONALITY = "POL"
NATION_CODE = "POL"
AI_LEVEL = 96
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
TYPE = 1
DURATION_MINUTES = 0
SPAWN_SET = "PIT"

[LAP_INVALIDATOR]
ALLOWED_TYRES_OUT = -1

[AUTOSPAWN]
ACTIVE = 0
```

## Diagnostyka

### Sprawdź logi

**1. Sprawdź log CSP:**
```
{Documents}\Assetto Corsa\logs\custom_shaders_patch.log
```

Szukaj linii:
- `Sim.loadTrack(): /` - jeśli tor jest pusty, to problem
- `Can't launch the race: invalid race configuration` - główny błąd

**2. Sprawdź log gry:**
```
{Documents}\Assetto Corsa\logs\log.txt
```

Szukaj linii:
- `KEY_NOT_FOUND: [RACE] TRACK` - brak toru
- `KEY_NOT_FOUND: [SESSION_0] NAME` - brak nazwy sesji
- `[CSP SAYS] Can't launch the race: invalid race configuration` - błąd CSP

### Sprawdź czy plik jest zapisywany poprawnie

**Jeśli używasz kodu do zapisu race.ini:**

```csharp
// Upewnij się, że używasz poprawnej metody zapisu
var raceIniPath = AcPaths.GetRaceIniFilename();
iniFile.Save(raceIniPath);

// Sprawdź czy plik został zapisany
if (File.Exists(raceIniPath)) {
    var content = File.ReadAllText(raceIniPath);
    // Sprawdź czy zawiera wymagane sekcje
    if (!content.Contains("[RACE]") || !content.Contains("TRACK =")) {
        // Problem z zapisem
    }
}
```

## Zapobieganie problemowi

### 1. Zawsze używaj Content Manager

Content Manager automatycznie generuje poprawny plik `race.ini` z wszystkimi wymaganymi sekcjami i wartościami.

### 2. Sprawdź wartości przed zapisem

**Jeśli tworzysz plik programowo:**

```csharp
// Sprawdź czy wszystkie wymagane wartości są ustawione
if (string.IsNullOrWhiteSpace(trackId)) {
    throw new Exception("TRACK nie może być pusty!");
}

if (string.IsNullOrWhiteSpace(carId)) {
    throw new Exception("MODEL nie może być pusty!");
}

// Dopiero potem zapisz
iniFile["RACE"].Set("TRACK", trackId);
iniFile["RACE"].Set("MODEL", carId);
iniFile.Save(raceIniPath);
```

### 3. Użyj domyślnej konfiguracji

**Zawsze ustaw domyślne wartości:**

```csharp
// Użyj Game.DefaultRaceConfig jako bazy
var config = Game.DefaultRaceConfig.Clone();
config["RACE"].Set("TRACK", trackId);
config["RACE"].Set("MODEL", carId);
// ... ustaw pozostałe wartości
config.Save(raceIniPath);
```

## Podsumowanie

**Problem:** Plik `race.ini` jest niepoprawnie sformatowany lub zawiera puste wartości dla wymaganych kluczy.

**Rozwiązanie:**
1. Sprawdź plik `race.ini` - upewnij się, że wszystkie wymagane wartości są wypełnione
2. Sprawdź formatowanie - użyj poprawnego kodowania i znaków końca linii
3. Użyj Content Manager do automatycznej generacji poprawnego pliku
4. Sprawdź czy tor i samochód istnieją w katalogu gry

**Najczęstsze przyczyny:**
- Puste wartości dla `[RACE] TRACK` lub `[RACE] MODEL`
- Brak sekcji `[SESSION_0]` lub puste wartości w niej
- Nieprawidłowe kodowanie pliku
- Problem z parsowaniem INI przez grę lub CSP

