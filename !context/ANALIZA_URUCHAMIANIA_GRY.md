# Analiza uruchamiania gry i ustawiania samochodu/toru

## Przegląd ogólny

Projekt AcManager to launcher/manager dla Assetto Corsa. Główny mechanizm uruchamiania gry znajduje się w systemie **QuickDrive**, który obsługuje wybór samochodu, toru, warunków i trybu gry.

---

## 1. Uruchamianie gry po kliknięciu "Play"

### 1.1 Główny przepływ

**Plik:** `AcManager\Pages\Drive\QuickDrive.xaml.cs`

1. **Kliknięcie przycisku "Play"** wywołuje metodę `Go()` w `ViewModel` (linia 1027):
   ```csharp
   internal async Task Go() {
       var selectedCar = SelectedCar;
       var selectedMode = SelectedModeViewModel;
       // ... walidacja ...
       await selectedMode.Drive(new Game.BasicProperties { ... });
   }
   ```

2. **Metoda `Drive()`** jest abstrakcyjna w `QuickDriveModeViewModel` i implementowana przez konkretne tryby (Practice, Race, Hotlap, etc.)

3. **Przykład implementacji** (`QuickDrive_Practice.xaml.cs`, linia 52-72):
   ```csharp
   public override async Task Drive(Game.BasicProperties basicProperties, ...) {
       await StartAsync(new Game.StartProperties {
           BasicProperties = basicProperties,
           AssistsProperties = assistsProperties,
           ConditionProperties = conditionProperties,
           TrackProperties = trackProperties,
           ModeProperties = new Game.PracticeProperties { ... }
       });
   }
   ```

4. **`StartAsync()`** w `QuickDriveModeViewModel` (linia 69-72):
   ```csharp
   protected Task StartAsync(Game.StartProperties properties) {
       return GameWrapper.StartAsync(properties);
   }
   ```

### 1.2 GameWrapper - główny wrapper

**Plik:** `AcManager.Tools\SemiGui\GameWrapper.cs`

`GameWrapper.StartAsync()` (linia 66-68) przygotowuje właściwości i wywołuje:

1. **Przygotowanie właściwości:**
   - `StartAsync_AdjustProperties()` - dostosowanie temperatury dla RSR
   - `StartAsync_Prepare()` - przygotowanie dodatkowych helperów (track details, weather, etc.)
   - `StartAsync_PrepareRace()` - przygotowanie asystentów, nazwy kierowcy

2. **Uruchomienie gry:**
   ```csharp
   result = await Game.StartAsync(CreateStarter(properties), properties, new ProgressHandler(ui), cancellationToken);
   ```

### 1.3 Game.StartAsync - niskopoziomowe uruchomienie

**Plik:** `AcTools\Processes\Game.cs`

Metoda `StartAsync()` (linia 108-180) wykonuje:

1. **Przygotowanie plików konfiguracyjnych:**
   - `properties.Set()` - ustawia wszystkie właściwości w plikach konfiguracyjnych:
     - **`race.ini`** - główny plik konfiguracyjny sesji (samochód, tor, warunki)
     - **`assists.ini`** - plik z ustawieniami asystentów (ABS, TC, etc.)
   - `ClearUpIniFile()` - czyści poprzednie ustawienia w `race.ini`
   - `SetDefaultProperies()` - ustawia domyślne wartości

2. **Uruchomienie procesu gry:**
   ```csharp
   await starter.RunAsync(cancellation);
   var process = await starter.WaitUntilGameAsync(cancellation);
   await properties.SetGame(process);  // Ustawia właściwości w uruchomionej grze
   await starter.WaitGameAsync(cancellation);  // Czeka na zakończenie gry
   ```

### 1.4 Jakim plikiem uruchamiana jest gra?

**Pliki starterów:**
- `AcManager.Tools\Starters\OfficialStarter.cs` - uruchamia przez oryginalny launcher
- `AcManager.Tools\Starters\SteamStarter.cs` - uruchamia bezpośrednio przez Steam
- `AcTools\Processes\TrickyStarter.cs` - zastępuje AssettoCorsa.exe własnym starterem

**Proces uruchamiania:**

1. **OfficialStarter** (domyślny):
   - Uruchamia **`AssettoCorsa.exe`** (launcher)
   - Launcher następnie uruchamia **`acs.exe`** (64-bit) lub **`acs_x86.exe`** (32-bit)
   - Lokalizacja: `{AC_ROOT}\AssettoCorsa.exe` → `{AC_ROOT}\acs.exe`

2. **SteamStarter**:
   - Uruchamia bezpośrednio **`acs.exe`** lub **`acs_x86.exe`**
   - Pomija launcher AssettoCorsa.exe
   - Lokalizacja: `{AC_ROOT}\acs.exe`

3. **TrickyStarter**:
   - Zastępuje `AssettoCorsa.exe` własnym starterem
   - Starter uruchamia **`acs.exe`** z parametrem `--first-stage`
   - Po zakończeniu przywraca oryginalny `AssettoCorsa.exe`

**Kod uruchamiania** (`OfficialStarter.cs`, linia 74-77):
```csharp
LauncherProcess = Process.Start(new ProcessStartInfo {
    FileName = LauncherFilename,  // AssettoCorsa.exe
    WorkingDirectory = AcRootDirectory.Instance.RequireValue
});
```

**Kod SteamStarter** (`SteamStarter.cs`, linia 204-207):
```csharp
GameProcess = Process.Start(new ProcessStartInfo {
    FileName = AcsFilename,  // acs.exe lub acs_x86.exe
    WorkingDirectory = _acRoot
});
```

---

## 2. Ustawianie aktualnego samochodu i toru

### 2.1 Ustawianie w kontekście aplikacji (AcContext)

**Plik:** `AcManager.Tools\Helpers\AcContext.cs`

Gdy użytkownik wybiera samochód lub tor w QuickDrive:

1. **Samochód** (`QuickDrive.xaml.cs`, linia 498):
   ```csharp
   public CarObject SelectedCar {
       set {
           // ...
           AcContext.Instance.CurrentCar = value;  // Ustawia aktualny samochód
       }
   }
   ```

2. **Tor** (`QuickDrive.xaml.cs`, linia 596):
   ```csharp
   public TrackObjectBase SelectedTrack {
       set {
           // ...
           AcContext.Instance.CurrentTrack = value;  // Ustawia aktualny tor
       }
   }
   ```

`AcContext` to singleton przechowujący aktualnie wybrany samochód i tor w aplikacji.

### 2.2 Ustawianie w plikach konfiguracyjnych (dla gry)

**Pliki konfiguracyjne:**
- **`race.ini`** - główny plik konfiguracyjny sesji
- **`assists.ini`** - plik z ustawieniami asystentów

**Lokalizacja plików:**
- `race.ini`: `{Documents}\Assetto Corsa\cfg\race.ini`
- `assists.ini`: `{Documents}\Assetto Corsa\cfg\assists.ini`

**Plik:** `AcTools\Processes\Game.Properties.cs`

Gdy gra jest uruchamiana, właściwości są zapisywane do plików konfiguracyjnych:

#### BasicProperties.Set() (linia 93-122):

```csharp
public override void Set(IniFile file) {
    var section = file["RACE"];
    
    // Ustawienie samochodu
    section.SetId("MODEL", CarId ?? "");           // ID samochodu
    section.SetId("MODEL_CONFIG", "");              // Konfiguracja samochodu
    section.SetId("SKIN", CarSkinId ?? "");         // Skórka samochodu
    
    // Ustawienie toru
    section.SetId("TRACK", TrackId ?? "");          // ID toru
    section.SetId("CONFIG_TRACK", TrackConfigurationId ?? "");  // Konfiguracja toru (layout)
    
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
```

#### AssistsProperties.Set() (linia 743-746):

Asystenci są zapisywane do osobnego pliku `assists.ini`:

```csharp
public override IDisposable Set() {
    ToIniFile().Save(AcPaths.GetAssistsIniFilename());  // Zapisuje do assists.ini
    return null;
}
```

**Struktura assists.ini:**
```ini
[ASSISTS]
IDEAL_LINE = 0
AUTO_BLIP = 0
STABILITY_CONTROL = 0
AUTO_BRAKE = 0
AUTO_SHIFTER = 0
ABS = 1
TRACTION_CONTROL = 1
AUTO_CLUTCH = 0
VISUALDAMAGE = 0
DAMAGE = 100
FUEL_RATE = 100
TYRE_WEAR = 100
TYRE_BLANKETS = 0
SLIPSTREAM = 100
```

### 2.3 Przepływ danych przy uruchamianiu

```
QuickDrive.ViewModel.Go()
    ↓
QuickDriveModeViewModel.Drive(Game.BasicProperties)
    ↓
Game.BasicProperties zawiera:
    - CarId: ID samochodu
    - CarSkinId: ID skórki
    - TrackId: ID toru
    - TrackConfigurationId: ID konfiguracji toru (layout)
    ↓
Game.StartProperties.Set()
    ↓
BasicProperties.Set(iniFile) → Zapis do race.ini
AssistsProperties.Set() → Zapis do assists.ini
ConditionProperties.Set(iniFile) → Zapis do race.ini
TrackProperties.Set(iniFile) → Zapis do race.ini
ModeProperties.Set(iniFile) → Zapis do race.ini
    ↓
Zapis do race.ini:
    [RACE]
    MODEL = car_id
    SKIN = skin_id
    TRACK = track_id
    CONFIG_TRACK = layout_id
    ↓
Zapis do assists.ini:
    [ASSISTS]
    ABS = 1
    TRACTION_CONTROL = 1
    ...
    ↓
Uruchomienie AssettoCorsa.exe → acs.exe
    ↓
Gra (acs.exe) automatycznie czyta race.ini i assists.ini przy starcie
    ↓
Gra ładuje odpowiedni samochód i tor na podstawie race.ini
```

---

## 3. Kluczowe klasy i metody

### 3.1 QuickDrive
- **Lokalizacja:** `AcManager\Pages\Drive\QuickDrive.xaml.cs`
- **Rola:** Główny interfejs użytkownika do wyboru samochodu, toru i warunków
- **Kluczowe metody:**
  - `RunAsync()` - uruchamia grę bez pokazywania UI
  - `Show()` - pokazuje UI QuickDrive
  - `Activate()` - pokazuje UI lub uruchamia bezpośrednio

### 3.2 GameWrapper
- **Lokalizacja:** `AcManager.Tools\SemiGui\GameWrapper.cs`
- **Rola:** Wrapper łączący UI z niskopoziomowym uruchamianiem gry
- **Kluczowe metody:**
  - `StartAsync()` - główna metoda uruchamiająca grę
  - `CreateStarter()` - tworzy starter dla procesu gry

### 3.3 Game.StartProperties
- **Lokalizacja:** `AcTools\Processes\Game.cs`
- **Rola:** Zawiera wszystkie właściwości potrzebne do uruchomienia gry
- **Właściwości:**
  - `BasicProperties` - samochód, tor, skórka
  - `AssistsProperties` - asystenci
  - `ConditionProperties` - warunki pogodowe, temperatura
  - `TrackProperties` - właściwości toru (dynamic track)
  - `ModeProperties` - właściwości trybu (Practice, Race, etc.)

### 3.4 Game.BasicProperties
- **Lokalizacja:** `AcTools\Processes\Game.Properties.cs`
- **Rola:** Zawiera podstawowe informacje o samochodzie i torze
- **Właściwości:**
  - `CarId` - ID samochodu
  - `CarSkinId` - ID skórki
  - `TrackId` - ID toru
  - `TrackConfigurationId` - ID konfiguracji toru (layout)

### 3.5 AcContext
- **Lokalizacja:** `AcManager.Tools\Helpers\AcContext.cs`
- **Rola:** Singleton przechowujący aktualnie wybrany samochód i tor w aplikacji
- **Właściwości:**
  - `CurrentCar` - aktualny samochód
  - `CurrentTrack` - aktualny tor

---

## 4. Przykładowy przepływ przy kliknięciu "Play"

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

## 5. Pliki konfiguracyjne - struktura

### 5.1 race.ini

Plik `race.ini` jest zapisywany w katalogu `{Documents}\Assetto Corsa\cfg\` i zawiera:

```ini
[RACE]
MODEL = car_id              # ID samochodu
SKIN = skin_id              # ID skórki
TRACK = track_id             # ID toru
CONFIG_TRACK = layout_id     # ID konfiguracji toru (layout)

[CAR_0]
MODEL = -                    # "-" oznacza użycie z sekcji RACE
SKIN = skin_id
SETUP = setup_id
BALLAST = 0
RESTRICTOR = 0
DRIVER_NAME = "Player Name"
NATIONALITY = "POL"
NATION_CODE = "POL"

[SESSION_0]
NAME = "Practice"
TYPE = 1                     # 1 = Practice
DURATION_MINUTES = 0
SPAWN_SET = "PIT"

[LIGHTING]
SUN_ANGLE = -48.0
TIME_MULT = 1.0
CLOUD_SPEED = 0.2

[TEMPERATURE]
AMBIENT = 26
ROAD = 32

[WEATHER]
NAME = "4_mid_clear"
```

### 5.2 assists.ini

Plik `assists.ini` jest zapisywany w katalogu `{Documents}\Assetto Corsa\cfg\` i zawiera ustawienia asystentów:

```ini
[ASSISTS]
IDEAL_LINE = 0              # Linia idealna (0 = wyłączona)
AUTO_BLIP = 0               # Automatyczne blipowanie
STABILITY_CONTROL = 0        # Kontrola stabilności
AUTO_BRAKE = 0              # Automatyczne hamowanie
AUTO_SHIFTER = 0            # Automatyczna skrzynia biegów
ABS = 1                     # ABS (0 = wyłączony, 1 = factory, 2 = on)
TRACTION_CONTROL = 1        # Kontrola trakcji (0 = wyłączony, 1 = factory, 2 = on)
AUTO_CLUTCH = 0             # Automatyczne sprzęgło
VISUALDAMAGE = 0            # Wizualne uszkodzenia
DAMAGE = 100                # Poziom uszkodzeń (0-100)
FUEL_RATE = 100             # Zużycie paliwa (0-100)
TYRE_WEAR = 100             # Zużycie opon (0-100)
TYRE_BLANKETS = 0           # Koce grzewcze opon
SLIPSTREAM = 100            # Efekt slipstreamu (0-100)
```

### 5.3 Jak gra czyta te pliki?

**Assetto Corsa (`acs.exe`) automatycznie czyta pliki konfiguracyjne przy starcie:**

1. **Przy uruchomieniu** `acs.exe` sprawdza katalog `{Documents}\Assetto Corsa\cfg\`
2. **Czyta `race.ini`** i ładuje:
   - Samochód z sekcji `[RACE] MODEL`
   - Tor z sekcji `[RACE] TRACK` i `CONFIG_TRACK`
   - Warunki z sekcji `[LIGHTING]`, `[TEMPERATURE]`, `[WEATHER]`
   - Tryb sesji z sekcji `[SESSION_0]`
3. **Czyta `assists.ini`** i ustawia asystentów
4. **Ładuje zawartość** na podstawie odczytanych wartości

**Uwaga:** Gra nie czyta plików przez API Content Managera - czyta je bezpośrednio z systemu plików. Content Manager tylko zapisuje pliki przed uruchomieniem gry.

---

## 6. Gdzie znajdują się samochody do wyboru?

### 6.1 Lokalizacja fizyczna samochodów

**Katalog samochodów:**
- **Lokalizacja:** `{AC_ROOT}\content\cars\`
- **Przykład:** `C:\Program Files (x86)\Steam\steamapps\common\assettocorsa\content\cars\`

**Struktura katalogu samochodu:**
```
{AC_ROOT}\content\cars\
  ├── abarth500\              # ID samochodu
  │   ├── data.acd            # Skompresowane dane samochodu
  │   ├── logo.png            # Logo samochodu
  │   ├── ui\                 # Pliki UI
  │   │   ├── ui_car.json     # Główne dane samochodu (nazwa, specyfikacje)
  │   │   ├── badge.png       # Odznaka marki
  │   │   └── upgrade.png     # Ikona upgrade
  │   ├── skins\              # Skórki samochodu
  │   │   ├── 00_default\
  │   │   └── racing_green\
  │   └── ...
  ├── ferrari_f40\
  ├── lotus_elise_sc\
  └── ...
```

**Kod lokalizacji** (`AcTools\Utils\AcPaths.cs`, linia 67-68):
```csharp
public static string GetCarsDirectory([NotNull] string acRoot) {
    return Path.Combine(acRoot, "content", "cars");
}
```

### 6.2 Jak są skanowane i ładowane?

**CarsManager** (`AcManager.Tools\Managers\CarsManager.cs`):

1. **Skanowanie katalogów:**
   - `CarsManager` dziedziczy po `AcManagerNew<CarObject>`
   - Używa `AcRootDirectory.Instance.CarsDirectories` do skanowania
   - Skanuje katalog `{AC_ROOT}\content\cars\`
   - Dla każdego podkatalogu tworzy `CarObject`

2. **Filtrowanie:**
   ```csharp
   protected override bool Filter(string id, string filename) {
       // Pomija tymczasowe samochody
       if (id.StartsWith(@"__cm_tmp_")) return false;
       
       // Dla samochodów Kunos (ks_*) wymaga ui_car.json
       if (id.StartsWith(@"ks_")) {
           var uiCarJson = Path.Combine(filename, @"ui", @"ui_car.json");
           if (!File.Exists(uiCarJson)) return false;
       }
       
       return base.Filter(id, filename);
   }
   ```

3. **Tworzenie obiektów:**
   ```csharp
   protected override CarObject CreateAcObject(string id, bool enabled) {
       return new CarObject(this, id, enabled);
   }
   ```

### 6.3 Jak są wyświetlane w UI?

**Lista samochodów** (`AcManager\Pages\Lists\CarsListPage.xaml.cs`):

1. **Strona listy:**
   - `CarsListPage` - główna strona z listą wszystkich samochodów
   - Używa `CarsManager.Instance` jako źródła danych
   - Wyświetla samochody w formie listy/kafelków

2. **Dostęp do samochodów:**
   ```csharp
   // Pobranie samochodu po ID
   var car = CarsManager.Instance.GetById("ferrari_f40");
   
   // Pobranie wszystkich samochodów
   var allCars = CarsManager.Instance.WrappersList;
   
   // Pobranie domyślnego samochodu
   var defaultCar = CarsManager.Instance.GetDefault(); // abarth500
   ```

3. **W QuickDrive:**
   - `QuickDrive` używa `CarsManager.Instance` do wyświetlania dostępnych samochodów
   - Użytkownik wybiera samochód z listy
   - Wybrany samochód jest ustawiany w `SelectedCar`

### 6.4 Struktura danych samochodu

**CarObject** (`AcManager.Tools\Objects\CarObject.cs`):

- **ID:** Nazwa katalogu (np. `ferrari_f40`)
- **Location:** Pełna ścieżka do katalogu samochodu
- **Name:** Nazwa wyświetlana (z `ui_car.json`)
- **Brand:** Marka samochodu
- **Year:** Rok produkcji
- **SkinsManager:** Manager skórek dla tego samochodu
- **Enabled:** Czy samochód jest włączony

**Plik `ui_car.json`:**
```json
{
  "name": "Ferrari F40",
  "brand": "Ferrari",
  "year": 1987,
  "specs": {
    "bhp": 478,
    "weight": 1100
  }
}
```

---

## Podsumowanie

1. **Uruchamianie gry:** 
   - QuickDrive → GameWrapper → Game.StartAsync → Starter (Official/Steam/Tricky) → `AssettoCorsa.exe` → `acs.exe`

2. **Pliki konfiguracyjne:**
   - **`race.ini`** - główny plik z ustawieniami sesji (samochód, tor, warunki)
   - **`assists.ini`** - plik z ustawieniami asystentów
   - Lokalizacja: `{Documents}\Assetto Corsa\cfg\`

3. **Lokalizacja samochodów:**
   - Fizyczna lokalizacja: `{AC_ROOT}\content\cars\{car_id}\`
   - Skanowanie: `CarsManager` automatycznie skanuje katalog przy starcie
   - Wyświetlanie: `CarsListPage` i `QuickDrive` używają `CarsManager.Instance`

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

