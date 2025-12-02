# Uruchamianie gry i wczytywanie na tor

## Przegląd

Dokument opisuje pełny proces uruchamiania Assetto Corsa przez Content Manager oraz jak gra wczytuje samochód i tor na podstawie plików konfiguracyjnych.

---

## 1. Przepływ uruchamiania gry

### 1.1. Pełny proces od kliknięcia "Play" do wczytania na tor

```
1. Użytkownik klika "Play" w QuickDrive
   ↓
2. QuickDrive.ViewModel.Go()
   - Tworzy Game.BasicProperties z:
     * CarId = SelectedCar.Id (np. "ferrari_f40")
     * CarSkinId = SelectedCar.SelectedSkin?.Id (np. "racing_green")
     * TrackId = SelectedTrack.Id (np. "spa")
     * TrackConfigurationId = SelectedTrack.LayoutId (np. "spa_gp")
   ↓
3. QuickDriveModeViewModel.Drive(Game.BasicProperties)
   - Tworzy Game.StartProperties z wszystkimi właściwościami
   ↓
4. GameWrapper.StartAsync(StartProperties)
   - Przygotowuje dodatkowe helpery (track details, weather, etc.)
   ↓
5. Game.StartAsync(starter, properties)
   ↓
6. properties.Set() - Zapis plików konfiguracyjnych
   ↓
7. Zapis do race.ini:
   {Documents}\Assetto Corsa\cfg\race.ini
   
   [RACE]
   MODEL = ferrari_f40
   SKIN = racing_green
   TRACK = spa
   CONFIG_TRACK = spa_gp
   
   [CAR_0]
   MODEL = -
   SKIN = racing_green
   SETUP = 
   DRIVER_NAME = "Player Name"
   ↓
8. Zapis do assists.ini:
   {Documents}\Assetto Corsa\cfg\assists.ini
   
   [ASSISTS]
   ABS = 1
   TRACTION_CONTROL = 1
   ...
   ↓
9. Uruchomienie procesu gry
   - OfficialStarter: AssettoCorsa.exe → acs.exe
   - SteamStarter: acs.exe (bezpośrednio)
   ↓
10. Gra (acs.exe) uruchamia się i czyta pliki konfiguracyjne
```

### 1.2. Kluczowe klasy i metody

#### QuickDrive
- **Lokalizacja:** `AcManager\Pages\Drive\QuickDrive.xaml.cs`
- **Rola:** Główny interfejs użytkownika do wyboru samochodu, toru i warunków
- **Kluczowe metody:**
  - `Go()` - uruchamia grę po kliknięciu "Play"
  - `RunAsync()` - uruchamia grę bez pokazywania UI
  - `Show()` - pokazuje UI QuickDrive

#### GameWrapper
- **Lokalizacja:** `AcManager.Tools\SemiGui\GameWrapper.cs`
- **Rola:** Wrapper łączący UI z niskopoziomowym uruchamianiem gry
- **Kluczowe metody:**
  - `StartAsync()` - główna metoda uruchamiająca grę
  - `CreateStarter()` - tworzy starter dla procesu gry

#### Game.StartProperties
- **Lokalizacja:** `AcTools\Processes\Game.cs`
- **Rola:** Zawiera wszystkie właściwości potrzebne do uruchomienia gry
- **Właściwości:**
  - `BasicProperties` - samochód, tor, skórka
  - `AssistsProperties` - asystenci
  - `ConditionProperties` - warunki pogodowe, temperatura
  - `TrackProperties` - właściwości toru (dynamic track)
  - `ModeProperties` - właściwości trybu (Practice, Race, etc.)

#### Game.BasicProperties
- **Lokalizacja:** `AcTools\Processes\Game.Properties.cs`
- **Rola:** Zawiera podstawowe informacje o samochodzie i torze
- **Właściwości:**
  - `CarId` - ID samochodu
  - `CarSkinId` - ID skórki
  - `TrackId` - ID toru
  - `TrackConfigurationId` - ID konfiguracji toru (layout)

---

## 2. Jak Content Manager przygotowuje pliki konfiguracyjne

### 2.1. Zapis do race.ini

**Metoda:** `BasicProperties.Set()` w `AcTools\Processes\Game.Properties.cs`

```csharp
public override void Set(IniFile file) {
    var section = file["RACE"];
    
    // Ustawienie samochodu
    section.SetId("MODEL", CarId ?? "");           // "ferrari_f40"
    section.SetId("MODEL_CONFIG", "");              // Konfiguracja samochodu
    section.SetId("SKIN", CarSkinId ?? "");         // "racing_green"
    
    // Ustawienie toru
    section.SetId("TRACK", TrackId ?? "");          // "spa"
    section.SetId("CONFIG_TRACK", TrackConfigurationId ?? "");  // "spa_gp"
    
    // Sekcja CAR_0 (samochód gracza)
    file["CAR_0"] = new IniFileSection(null) {
        ["SETUP"] = CarSetupId?.ToLowerInvariant() ?? "",
        ["SKIN"] = CarSkinId?.ToLowerInvariant(),
        ["MODEL"] = "-",  // "-" oznacza użycie samochodu z sekcji RACE
        ["MODEL_CONFIG"] = "",
        ["BALLAST"] = Ballast,
        ["RESTRICTOR"] = Restrictor,
        ["DRIVER_NAME"] = DriverName,
        ["NATION_CODE"] = DriverNationCode ?? GetNationCode(DriverNationality),
        ["NATIONALITY"] = DriverNationality
    };
    
    // Jeśli podano plik setupu
    if (!string.IsNullOrWhiteSpace(CarSetupFilename)) {
        file["CAR_0"].Set("_EXT_SETUP_FILENAME", CarSetupFilename);
    }
}

// Zapis do pliku
var raceIniPath = AcPaths.GetRaceIniFilename();
// {Documents}\Assetto Corsa\cfg\race.ini
file.Save(raceIniPath);
```

### 2.2. Zapis do assists.ini

**Metoda:** `AssistsProperties.Set()` w `AcTools\Processes\Game.Properties.cs`

```csharp
public override IDisposable Set() {
    var iniFile = ToIniFile();
    var assistsIniPath = AcPaths.GetAssistsIniFilename();
    // {Documents}\Assetto Corsa\cfg\assists.ini
    iniFile.Save(assistsIniPath);
    return null;
}
```

### 2.3. Struktura plików konfiguracyjnych

#### race.ini

**Lokalizacja:** `{Documents}\Assetto Corsa\cfg\race.ini`

```ini
[HEADER]
VERSION = 2

[RACE]
MODEL = ferrari_f40              # ID samochodu
SKIN = racing_green              # ID skórki
TRACK = spa                      # ID toru
CONFIG_TRACK = spa_gp            # ID konfiguracji toru (layout)
CARS = 1                         # Liczba samochodów
AI_LEVEL = 98                    # Poziom AI
FIXED_SETUP = 0                  # Czy używać stałego setupu
PENALTIES = 0                    # Czy włączyć kary

[CAR_0]
MODEL = -                        # "-" oznacza użycie z sekcji RACE
SKIN = racing_green
SETUP =                          # ID setupu (opcjonalne)
BALLAST = 0                      # Balast (kg)
RESTRICTOR = 0                   # Ogranicznik mocy (%)
DRIVER_NAME = "Player Name"      # Imię kierowcy
NATIONALITY = "POL"              # Narodowość
NATION_CODE = "POL"              # Kod narodowości

[SESSION_0]
NAME = "Practice"                # Nazwa sesji
TYPE = 1                         # 1 = Practice, 2 = Qualify, 3 = Race
DURATION_MINUTES = 0             # Czas trwania (0 = nieskończoność)
SPAWN_SET = "PIT"                # Miejsce startu (PIT, START, PIT_NEW)

[LIGHTING]
SUN_ANGLE = -48.0                # Kąt słońca
TIME_MULT = 1.0                  # Mnożnik czasu
CLOUD_SPEED = 0.2                # Prędkość chmur

[TEMPERATURE]
AMBIENT = 26                     # Temperatura otoczenia (°C)
ROAD = 32                        # Temperatura nawierzchni (°C)

[WEATHER]
NAME = "4_mid_clear"             # Nazwa pogody
```

#### assists.ini

**Lokalizacja:** `{Documents}\Assetto Corsa\cfg\assists.ini`

```ini
[ASSISTS]
IDEAL_LINE = 0                   # Linia idealna (0 = wyłączona)
AUTO_BLIP = 0                    # Automatyczne blipowanie
STABILITY_CONTROL = 0            # Kontrola stabilności
AUTO_BRAKE = 0                   # Automatyczne hamowanie
AUTO_SHIFTER = 0                 # Automatyczna skrzynia biegów
ABS = 1                          # ABS (0 = wyłączony, 1 = factory, 2 = on)
TRACTION_CONTROL = 1             # Kontrola trakcji (0 = wyłączony, 1 = factory, 2 = on)
AUTO_CLUTCH = 0                  # Automatyczne sprzęgło
VISUALDAMAGE = 0                 # Wizualne uszkodzenia
DAMAGE = 100                     # Poziom uszkodzeń (0-100)
FUEL_RATE = 100                  # Zużycie paliwa (0-100)
TYRE_WEAR = 100                  # Zużycie opon (0-100)
TYRE_BLANKETS = 0                # Koce grzewcze opon
SLIPSTREAM = 100                 # Efekt slipstreamu (0-100)
```

---

## 3. Jak gra wczytuje samochód i tor

### 3.1. Krok 1: Odczyt plików konfiguracyjnych

Po uruchomieniu `acs.exe`, gra automatycznie czyta pliki z katalogu `{Documents}\Assetto Corsa\cfg\`:

```csharp
// Gra czyta bezpośrednio z systemu plików (nie przez API CM)
var raceIniPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    "Assetto Corsa", "cfg", "race.ini"
);

// Odczyt sekcji [RACE]
var carId = raceIni["RACE"]["MODEL"];        // np. "ferrari_f40"
var carSkinId = raceIni["RACE"]["SKIN"];     // np. "racing_green"
var trackId = raceIni["RACE"]["TRACK"];      // np. "spa"
var layoutId = raceIni["RACE"]["CONFIG_TRACK"]; // np. "spa_gp"
```

### 3.2. Krok 2: Budowanie ścieżek do contentu

Gra buduje ścieżki do folderów samochodu i toru:

```csharp
// Ścieżka do samochodu
var carPath = Path.Combine(acRoot, "content", "cars", carId);
// Przykład: C:\...\assettocorsa\content\cars\ferrari_f40

// Ścieżka do toru
var trackPath = Path.Combine(acRoot, "content", "tracks", trackId);
// Przykład: C:\...\assettocorsa\content\tracks\spa

// Jeśli jest layout, ścieżka może być:
var trackLayoutPath = Path.Combine(trackPath, layoutId);
// Przykład: C:\...\assettocorsa\content\tracks\spa\spa_gp
```

### 3.3. Krok 3: Wczytywanie danych samochodu

Gra wczytuje dane samochodu z folderu:

```
1. Sprawdza czy istnieje data.acd:
   - TAK → Wczytuje z archiwum ACD
   - NIE → Sprawdza czy istnieje data/
     - TAK → Wczytuje z katalogu data/
     - NIE → Błąd

2. Wczytuje pliki z data.acd lub data/:
   - car.ini → parametry samochodu (moc, masa, wymiary)
   - engine.ini → parametry silnika
   - drivetrain.ini → skrzynia biegów
   - suspensions.ini → zawieszenie
   - brakes.ini → hamulce
   - tyres.ini → opony
   - aerodynamics.ini → aerodynamika
   - electronics.ini → systemy elektroniczne
   - lods.ini → poziomy szczegółowości
   - digital_instruments.ini → wyświetlacze
   - driver3d.ini → model kierowcy
   - ai.ini → parametry AI

3. Wczytuje model 3D:
   - Sprawdza lods.ini → LOD_0.FILE (np. "ferrari_f40.kn5")
   - LUB największy plik .kn5 w katalogu samochodu
   - Wczytuje plik .kn5 (model 3D)

4. Wczytuje skórkę:
   - Ścieżka: {carPath}\skins\{carSkinId}\
   - Wczytuje livery.png, preview.jpg, ui_skin.json
   - Nakłada tekstury na model 3D

5. Wczytuje dźwięki:
   - Ścieżka: {carPath}\sfx\
   - Wczytuje pliki .bank (FMOD)
```

### 3.4. Krok 4: Wczytywanie danych toru

Gra wczytuje dane toru z folderu:

```
1. Sprawdza czy istnieje data.acd:
   - TAK → Wczytuje z archiwum ACD
   - NIE → Sprawdza czy istnieje data/
     - TAK → Wczytuje z katalogu data/
     - NIE → Błąd

2. Wczytuje pliki z data.acd lub data/:
   - track.ini → parametry toru (długość, szerokość)
   - surfaces.ini → powierzchnie (asfalt, trawa, itp.)
   - ai.ini → parametry AI dla toru
   - ai_hints.ini → wskazówki dla AI
   - drs_zones.ini → strefy DRS
   - cameras.ini → kamery
   - lights.ini → oświetlenie

3. Wczytuje modele 3D:
   - Jeśli jest layout: models_{layoutId}.ini
   - Jeśli nie ma layoutu: models.ini
   - Wczytuje pliki .kn5 (modele 3D toru)

4. Wczytuje skórkę toru (jeśli jest):
   - Ścieżka: {trackPath}\skins\{skinId}\
   - Wczytuje tekstury skórki
```

### 3.5. Krok 5: Inicjalizacja sesji

Po wczytaniu wszystkich danych, gra inicjalizuje sesję:

```
1. Tworzy obiekt samochodu z wczytanymi parametrami
2. Tworzy obiekt toru z wczytanymi parametrami
3. Ustawia warunki pogodowe (z race.ini [WEATHER], [LIGHTING], [TEMPERATURE])
4. Ustawia asystentów (z assists.ini)
5. Ustawia tryb sesji (z race.ini [SESSION_0])
6. Umieszcza gracza na torze (SPAWN_SET z race.ini)
7. Rozpoczyna sesję
```

---

## 4. Szczegółowy przepływ wczytywania

**Diagram przepływu:**

```
acs.exe uruchomiony
    ↓
Odczyt race.ini z {Documents}\Assetto Corsa\cfg\
    ↓
Pobranie wartości:
  - MODEL = "ferrari_f40"
  - SKIN = "racing_green"
  - TRACK = "spa"
  - CONFIG_TRACK = "spa_gp"
    ↓
Budowanie ścieżek:
  - carPath = {AC_ROOT}\content\cars\ferrari_f40
  - trackPath = {AC_ROOT}\content\tracks\spa
    ↓
WCZYTYWANIE SAMOCHODU:
  ├─ Sprawdzenie: data.acd lub data/
  ├─ Wczytanie plików INI (car.ini, engine.ini, itp.)
  ├─ Wczytanie modelu 3D (.kn5)
  ├─ Wczytanie skórki: skins\racing_green\
  └─ Wczytanie dźwięków: sfx\*.bank
    ↓
WCZYTYWANIE TORU:
  ├─ Sprawdzenie: data.acd lub data/
  ├─ Wczytanie plików INI (track.ini, surfaces.ini, itp.)
  ├─ Wczytanie modeli 3D (.kn5) z models_spa_gp.ini
  └─ Wczytanie skórki (jeśli jest)
    ↓
Inicjalizacja sesji:
  ├─ Ustawienie warunków pogodowych
  ├─ Ustawienie asystentów
  ├─ Ustawienie trybu sesji
  └─ Umieszczenie gracza na torze
    ↓
Sesja rozpoczęta - gracz na torze
```

---

## 5. Startery - jak uruchamiana jest gra

### 5.1. OfficialStarter (domyślny)

**Lokalizacja:** `AcManager.Tools\Starters\OfficialStarter.cs`

- Uruchamia **`AssettoCorsa.exe`** (launcher)
- Launcher następnie uruchamia **`acs.exe`** (64-bit) lub **`acs_x86.exe`** (32-bit)
- Lokalizacja: `{AC_ROOT}\AssettoCorsa.exe` → `{AC_ROOT}\acs.exe`

**Kod:**
```csharp
LauncherProcess = Process.Start(new ProcessStartInfo {
    FileName = LauncherFilename,  // AssettoCorsa.exe
    WorkingDirectory = AcRootDirectory.Instance.RequireValue
});
```

### 5.2. SteamStarter

**Lokalizacja:** `AcManager.Tools\Starters\SteamStarter.cs`

- Uruchamia bezpośrednio **`acs.exe`** lub **`acs_x86.exe`**
- Pomija launcher AssettoCorsa.exe

**Kod:**
```csharp
GameProcess = Process.Start(new ProcessStartInfo {
    FileName = AcsFilename,  // acs.exe lub acs_x86.exe
    WorkingDirectory = _acRoot
});
```

### 5.3. TrickyStarter

**Lokalizacja:** `AcTools\Processes\TrickyStarter.cs`

- Zastępuje `AssettoCorsa.exe` własnym starterem
- Starter uruchamia **`acs.exe`** z parametrem `--first-stage`
- Po zakończeniu przywraca oryginalny `AssettoCorsa.exe`

---

## 6. Ważne uwagi

1. **Gra czyta pliki bezpośrednio z systemu plików** - nie używa API Content Managera
2. **Content Manager tylko zapisuje pliki** przed uruchomieniem gry
3. **Gra automatycznie wykrywa format** - czy plik jest spakowany (data.acd) czy rozpakowany (data/)
4. **Ścieżki są względne do AC_ROOT** - gra buduje pełne ścieżki na podstawie katalogu głównego AC
5. **Layouty torów** - jeśli `CONFIG_TRACK` jest ustawiony, gra szuka plików w podkatalogu layoutu
6. **Skórki** - gra wczytuje skórki z folderu `skins/{skin_id}/` w katalogu samochodu/toru

---

## 7. Obsługa błędów

Jeśli gra nie może wczytać samochodu/toru:

1. **Brak katalogu samochodu/toru:**
   - Gra wyświetla błąd: "Car/Track not found"
   - Sesja nie może się rozpocząć

2. **Brak pliku data.acd lub data/:**
   - Gra wyświetla błąd: "Data not found"
   - Samochód/tor jest niekompletny

3. **Brak modelu 3D (.kn5):**
   - Gra wyświetla błąd: "Model not found"
   - Nie można wyświetlić samochodu/toru

4. **Brak skórki:**
   - Gra używa domyślnej skórki (00_default)
   - Jeśli brak domyślnej, używa białej tekstury

---

## 8. Przykładowy przepływ przy kliknięciu "Play"

1. **Użytkownik klika "Play"** w QuickDrive
2. **`ViewModel.Go()`** jest wywoływane
3. **Tworzone jest `Game.BasicProperties`** z:
   - `CarId = SelectedCar.Id`
   - `CarSkinId = SelectedCar.SelectedSkin?.Id`
   - `TrackId = SelectedTrack.Id`
   - `TrackConfigurationId = SelectedTrack.LayoutId`
4. **Wywoływane jest `selectedMode.Drive()`** (np. Practice)
5. **Tworzone jest `Game.StartProperties`** z wszystkimi właściwościami
6. **Wywoływane jest `GameWrapper.StartAsync()`**
7. **`Game.StartAsync()`** przygotowuje pliki konfiguracyjne:
   - Czyści poprzednie ustawienia w `race.ini`
   - Zapisuje nowe ustawienia do `race.ini` (samochód, tor, warunki)
   - Zapisuje ustawienia asystentów do `assists.ini`
8. **Uruchamiany jest proces gry:**
   - **OfficialStarter**: `AssettoCorsa.exe` → launcher uruchamia `acs.exe`
   - **SteamStarter**: bezpośrednio `acs.exe` lub `acs_x86.exe`
9. **Gra (`acs.exe`) automatycznie czyta pliki konfiguracyjne przy starcie:**
   - Czyta `race.ini` z katalogu `{Documents}\Assetto Corsa\cfg\`
   - Czyta `assists.ini` z katalogu `{Documents}\Assetto Corsa\cfg\`
   - Ładuje samochód i tor na podstawie `race.ini`
10. **Gracz wchodzi do gry** z wybranym samochodem i torem

---

## 9. Pomijanie menu - bezpośrednie wejście do wyścigu

### 9.1. Jak działa AUTOSPAWN

Content Manager automatycznie ustawia sekcję `[AUTOSPAWN]` w pliku `race.ini` przed uruchomieniem gry. To powoduje, że gra pomija menu główne i od razu wchodzi do sesji wyścigowej.

**Kod w OfficialStarter.cs (linia 65-70):**

```csharp
new IniFile(AcPaths.GetRaceIniFilename()) {
    ["AUTOSPAWN"] = {
        ["ACTIVE"] = true,
        ["__CM_SERVICE"] = IniFile.Nothing
    }
}.Save();
```

### 9.2. Struktura sekcji AUTOSPAWN w race.ini

**Przed uruchomieniem gry:**
```ini
[AUTOSPAWN]
ACTIVE = 1
```

**Po zakończeniu gry (CleanUp):**
```ini
[AUTOSPAWN]
ACTIVE = 
```

Sekcja jest czyszczona po zakończeniu gry, aby następne uruchomienie (bez Content Managera) pokazywało normalne menu.

### 9.3. Jak to działa

1. **Content Manager zapisuje `AUTOSPAWN.ACTIVE = true`** do `race.ini` przed uruchomieniem gry
2. **Gra (`acs.exe`) czyta `race.ini`** przy starcie
3. **Jeśli `AUTOSPAWN.ACTIVE = true`**, gra:
   - Pomija menu główne
   - Od razu ładuje sesję wyścigową na podstawie ustawień z `race.ini`
   - Umieszcza gracza na torze zgodnie z `SPAWN_SET` z sekcji `[SESSION_0]`

### 9.4. Ręczne ustawienie AUTOSPAWN

Jeśli chcesz ręcznie ustawić AUTOSPAWN (np. do testów), możesz dodać do `race.ini`:

```ini
[AUTOSPAWN]
ACTIVE = 1
```

**Lokalizacja pliku:**
- `{Documents}\Assetto Corsa\cfg\race.ini`

**Uwaga:** Content Manager automatycznie czyści tę sekcję po zakończeniu gry, więc ręczne ustawienie zostanie usunięte.

### 9.5. Dlaczego może nie działać?

1. **Stary launcher Assetto Corsa** - starsze wersje mogą nie obsługiwać AUTOSPAWN
2. **Błędny plik race.ini** - jeśli plik jest uszkodzony, gra może nie odczytać AUTOSPAWN
3. **Użycie SteamStarter** - SteamStarter uruchamia bezpośrednio `acs.exe`, co powinno działać, ale może wymagać dodatkowych ustawień

### 9.6. Sprawdzenie czy AUTOSPAWN działa

1. Uruchom grę przez Content Manager
2. Sprawdź plik `{Documents}\Assetto Corsa\cfg\race.ini` - powinien zawierać:
   ```ini
   [AUTOSPAWN]
   ACTIVE = 1
   ```
3. Jeśli gra nadal pokazuje menu, sprawdź:
   - Czy używasz najnowszej wersji Assetto Corsa
   - Czy używasz OfficialStarter (nie TrickyStarter)
   - Czy plik `race.ini` jest poprawnie zapisany

---

## 10. Podsumowanie

1. **Uruchamianie gry:** 
   - QuickDrive → GameWrapper → Game.StartAsync → Starter (Official/Steam/Tricky) → `AssettoCorsa.exe` → `acs.exe`

2. **Pliki konfiguracyjne:**
   - **`race.ini`** - główny plik z ustawieniami sesji (samochód, tor, warunki)
   - **`assists.ini`** - plik z ustawieniami asystentów
   - Lokalizacja: `{Documents}\Assetto Corsa\cfg\`

3. **Lokalizacja samochodów i torów:**
   - Fizyczna lokalizacja: `{AC_ROOT}\content\cars\{car_id}\` i `{AC_ROOT}\content\tracks\{track_id}\`
   - Skanowanie: `CarsManager` i `TracksManager` automatycznie skanują katalogi przy starcie

4. **Ustawianie samochodu/toru w aplikacji:** 
   - `AcContext.Instance.CurrentCar/Track` (singleton w aplikacji)

5. **Ustawianie samochodu/toru w grze:** 
   - Zapis do `race.ini` przez `BasicProperties.Set()`
   - Zapis do `assists.ini` przez `AssistsProperties.Set()`
   - Gra (`acs.exe`) automatycznie czyta te pliki przy starcie

6. **Jak gra czyta pliki:**
   - Assetto Corsa (`acs.exe`) czyta pliki bezpośrednio z systemu plików przy uruchomieniu
   - Content Manager zapisuje pliki przed uruchomieniem gry
   - Gra ładuje samochód i tor na podstawie wartości z `race.ini`

