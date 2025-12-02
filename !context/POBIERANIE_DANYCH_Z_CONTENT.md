# Pobieranie danych z folderu content

## Przegląd

Content Manager pobiera dane z folderu `content` Assetto Corsa w kilku warstwach. Poniżej opisano główne mechanizmy i klasy odpowiedzialne za dostęp do danych.

---

## 1. Struktura folderu content

Folder `content` znajduje się w katalogu głównym Assetto Corsa (`{AC_ROOT}\content\`) i zawiera:

- **`cars/`** - samochody
- **`tracks/`** - tory wyścigowe
- **`showroom/`** - showroomy
- **`weather/`** - pliki pogodowe
- **`fonts/`** - fonty
- **`driver/`** - modele kierowców
- **`career/`** - kariera Kunos

---

## 2. Główne klasy i mechanizmy

### 2.1. AcPaths.cs - Ścieżki do folderów content

**Lokalizacja:** `AcTools\Utils\AcPaths.cs`

Klasa statyczna zawierająca metody pomocnicze do budowania ścieżek do folderów content:

```csharp
// Pobranie katalogu samochodów
public static string GetCarsDirectory(string acRoot) {
    return Path.Combine(acRoot, "content", "cars");
}

// Pobranie katalogu torów
public static string GetTracksDirectory(string acRoot) {
    return Path.Combine(acRoot, "content", "tracks");
}

// Pobranie katalogu konkretnego samochodu
public static string GetCarDirectory(string acRoot, string carName) {
    return Path.Combine(GetCarsDirectory(acRoot), carName);
}

// Inne katalogi content:
- GetShowroomsDirectory(acRoot) → content/showroom
- GetWeatherDirectory(acRoot) → content/weather
- GetFontsDirectory(acRoot) → content/fonts
- GetDriverModelsDirectory(acRoot) → content/driver
```

**Weryfikacja katalogu AC:**
```csharp
public static bool IsAcRoot(string directory) {
    return Directory.Exists(Path.Combine(directory, "content", "cars")) 
        && Directory.Exists(Path.Combine(directory, "apps"))
        && (File.Exists(Path.Combine(directory, "acs.exe")) 
            || File.Exists(Path.Combine(directory, "acs_pro.exe")));
}
```

---

### 2.2. AcRootDirectory.cs - Zarządzanie katalogiem głównym AC

**Lokalizacja:** `AcManager.Tools\Managers\AcRootDirectory.cs`

Klasa singleton zarządzająca katalogiem głównym Assetto Corsa i tworząca obiekty `AcDirectories` dla każdego typu contentu:

```csharp
public class AcRootDirectory {
    public static AcRootDirectory Instance { get; private set; }
    
    // Właściwości z katalogami dla różnych typów contentu
    public AcDirectories CarsDirectories { get; private set; }
    public AcDirectories TracksDirectories { get; private set; }
    public AcDirectories ShowroomsDirectories { get; private set; }
    public AcDirectories WeatherDirectories { get; private set; }
    // ... itd.
    
    private void UpdateDirectories() {
        // Tworzenie obiektów AcDirectories dla każdego typu
        CarsDirectories = Value == null ? null 
            : new AcDirectories(AcPaths.GetCarsDirectory(Value));
        TracksDirectories = Value == null ? null 
            : new AcDirectories(AcPaths.GetTracksDirectory(Value));
        // ... itd.
    }
}
```

**Weryfikacja katalogu:**
```csharp
public static bool CheckDirectory(string directory, bool verboseMode) {
    // Sprawdza:
    // 1. Czy katalog istnieje
    // 2. Czy istnieje folder apps/
    // 3. Czy istnieje folder content/
    // 4. Czy istnieje folder content/cars/
    // 5. Czy istnieje acs.exe lub acs_pro.exe
}
```

---

### 2.3. DataWrapper.cs - Odczyt danych z samochodów/torów

**Lokalizacja:** `AcTools\DataFile\DataWrapper.cs`

Klasa odpowiedzialna za odczyt danych z folderów samochodów i torów. Obsługuje zarówno spakowane pliki `.acd` jak i rozpakowane katalogi `data/`:

```csharp
public class DataWrapper : DataWrapperBase {
    public static readonly string PackedFileName = "data.acd";
    public static readonly string UnpackedDirectoryName = "data";
    
    private Acd _acd; // Archiwum ACD
    
    private DataWrapper(string carDirectory) {
        ParentDirectory = carDirectory;
        
        var dataAcd = Path.Combine(carDirectory, PackedFileName);
        if (File.Exists(dataAcd)) {
            // Samochód/tor jest spakowany
            _acd = Acd.FromFile(dataAcd);
            SetIsPacked(true);
        } else {
            // Samochód/tor jest rozpakowany
            var dataDirectory = Path.Combine(carDirectory, UnpackedDirectoryName);
            if (Directory.Exists(dataDirectory)) {
                _acd = Acd.FromDirectory(dataDirectory);
            } else {
                SetIsEmpty(true);
            }
        }
    }
    
    // Odczyt danych z archiwum/katalogu
    public override string GetData(string name) {
        return _acd?.GetEntry(name)?.ToString();
    }
    
    // Sprawdzenie czy plik istnieje
    public override bool Contains(string name) {
        return !IsEmpty && _acd?.GetEntry(name) != null;
    }
    
    // Tworzenie wrappera z katalogu samochodu
    public static DataWrapper FromCarDirectory(string carDirectory) {
        return new DataWrapper(carDirectory);
    }
    
    // Tworzenie wrappera z AC root i ID samochodu
    public static DataWrapper FromCarDirectory(string acRoot, string carId) {
        return FromCarDirectory(AcPaths.GetCarDirectory(acRoot, carId));
    }
}
```

**Przykład użycia:**
```csharp
var carDir = AcPaths.GetCarDirectory(acRoot, "ferrari_f40");
var data = DataWrapper.FromCarDirectory(carDir);
var carIni = data.GetIniFile("car.ini");
var power = carIni["ENGINE"].GetDouble("POWER", 0);
```

---

### 2.4. AcManagerNew<T> - System zarządzania obiektami contentu

**Lokalizacja:** `AcManager.Tools\AcManagersNew\AcManagerNew.cs`

Bazowa klasa dla menedżerów różnych typów contentu (samochody, tory, showroomy, itp.):

```csharp
public abstract class AcManagerNew<T> : FileAcManager<T> 
    where T : AcCommonObject {
    
    // Właściwość z katalogami do skanowania
    public abstract IAcDirectories Directories { get; }
    
    // Skanowanie katalogów
    protected override IEnumerable<AcPlaceholderNew> ScanOverride() {
        var directories = Directories;
        if (directories == null) return new AcPlaceholderNew[0];
        
        // Pobranie wszystkich podkatalogów z folderu content
        return directories.GetContentDirectories().Select(dir => {
            var id = directories.GetId(dir); // ID z nazwy katalogu
            return Filter(id, dir) 
                ? CreateAcPlaceholder(id, directories.CheckIfEnabled(dir)) 
                : null;
        }).NonNull();
    }
    
    // Filtrowanie obiektów (może być nadpisane)
    protected virtual bool Filter(string id, string filename) {
        return true;
    }
}
```

**Przykłady menedżerów:**
- `CarsManager` - zarządza samochodami
- `TracksManager` - zarządza torami
- `ShowroomsManager` - zarządza showroomami
- `WeatherManager` - zarządza pogodą

---

### 2.5. CarsManager.cs - Przykład menedżera

**Lokalizacja:** `AcManager.Tools\Managers\CarsManager.cs`

```csharp
public class CarsManager : AcManagerNew<CarObject> {
    public static CarsManager Instance { get; private set; }
    
    public override IAcDirectories Directories 
        => AcRootDirectory.Instance.CarsDirectories;
    
    // Filtrowanie - pomija tymczasowe katalogi i samochody Kunos bez ui_car.json
    protected override bool Filter(string id, string filename) {
        if (id.StartsWith(@"__cm_tmp_")) {
            return false; // Pomijaj tymczasowe
        }
        
        if (id.StartsWith(@"ks_")) {
            var uiCarJson = Path.Combine(filename, @"ui", @"ui_car.json");
            if (!File.Exists(uiCarJson)) return false; // Kunos bez UI
        }
        
        return base.Filter(id, filename);
    }
}
```

---

### 2.6. FilesStorage.cs - Pobieranie plików z Data (Content Manager)

**Lokalizacja:** `AcManager.Tools\Helpers\FilesStorage.cs`

Klasa do pobierania plików z folderów `Data` i `Data (User)` Content Managera (nie z folderu content AC, ale z danych aplikacji):

```csharp
public class FilesStorage : AbstractFilesStorage {
    public static readonly string DataDirName = "Data";
    public static readonly string DataUserDirName = "Data (User)";
    
    // Pobranie pliku z priorytetem dla plików użytkownika
    public ContentEntry GetContentFile(params string[] name) {
        var nameJoined = Path.Combine(name);
        var contentFile = Combine(DataDirName, nameJoined);
        var contentUserFile = Combine(DataUserDirName, nameJoined);
        
        // Pliki użytkownika mają priorytet
        var isOverrided = File.Exists(contentUserFile);
        return new ContentEntry(
            isOverrided ? contentUserFile : contentFile, 
            isOverrided, 
            false
        );
    }
    
    // Wczytanie pliku tekstowego
    public string LoadContentFile(string dir, string name = null) {
        var entry = GetContentFile(dir, name);
        if (!entry.Exists) return null;
        return FileUtils.ReadAllText(entry.Filename);
    }
    
    // Wczytanie pliku JSON
    public JObject LoadJsonContentFile(string dir, string name = null) {
        var entry = GetContentFile(dir, name);
        if (!entry.Exists) return null;
        return JObject.Parse(FileUtils.ReadAllText(entry.Filename));
    }
    
    // Pobranie wszystkich plików z filtrem
    public IEnumerable<ContentEntry> GetContentFilesFiltered(
        string searchPattern, 
        params string[] name
    ) {
        var nameJoined = Path.Combine(name);
        var contentDir = EnsureDirectory(DataDirName, nameJoined);
        var contentUserDir = EnsureDirectory(DataUserDirName, nameJoined);
        
        // Pliki użytkownika mają priorytet
        var contentUserFiles = Directory.GetFiles(contentUserDir, searchPattern)
            .Select(x => new ContentEntry(x, true, false)).ToList();
        var temp = contentUserFiles.Select(x => x.Name);
        
        return Directory.GetFiles(contentDir, searchPattern)
            .Select(x => new ContentEntry(x, false, false))
            .Where(x => !temp.Contains(x.Name))
            .Concat(contentUserFiles)
            .OrderBy(x => x.Name);
    }
}
```

---

## 3. Przepływ pobierania danych

### 3.1. Inicjalizacja

```
1. EntryPoint.cs
   ↓
2. AcRootDirectory.Initialize(ścieżka)
   ↓
3. AcRootDirectory.UpdateDirectories()
   ↓
4. Tworzenie AcDirectories dla każdego typu:
   - CarsDirectories = new AcDirectories(AcPaths.GetCarsDirectory(Value))
   - TracksDirectories = new AcDirectories(AcPaths.GetTracksDirectory(Value))
   - ... itd.
```

### 3.2. Skanowanie contentu

```
1. CarsManager.Initialize()
   ↓
2. CarsManager.Scan() lub EnsureLoadedAsync()
   ↓
3. AcManagerNew.ActualScan()
   ↓
4. FileAcManager.ScanOverride()
   ↓
5. Directories.GetContentDirectories()
   → Zwraca wszystkie podkatalogi z content/cars/
   ↓
6. Dla każdego katalogu:
   - Pobranie ID (nazwa katalogu)
   - Sprawdzenie Filter(id, filename)
   - Utworzenie AcPlaceholderNew
   ↓
7. Utworzenie CarObject dla każdego placeholdera
   ↓
8. CarObject.Load() - wczytanie danych z plików
```

### 3.3. Odczyt danych z samochodu/toru

```
1. CarObject.Load()
   ↓
2. DataWrapper.FromCarDirectory(carDirectory)
   ↓
3. Sprawdzenie czy istnieje data.acd:
   - TAK → Acd.FromFile(data.acd)
   - NIE → Sprawdzenie czy istnieje data/
     - TAK → Acd.FromDirectory(data/)
     - NIE → IsEmpty = true
   ↓
4. Odczyt plików przez DataWrapper.GetData(nazwa_pliku)
   ↓
5. Parsowanie plików INI/JSON:
   - data.GetIniFile("car.ini")
   - data.GetIniFile("lods.ini")
   - data.GetIniFile("tyres.ini")
   - ... itd.
```

---

## 4. Przykłady użycia

### 4.1. Pobranie listy samochodów

```csharp
// Inicjalizacja (zwykle w EntryPoint)
AcRootDirectory.Initialize(@"C:\Program Files\Steam\steamapps\common\assettocorsa");
CarsManager.Initialize();

// Pobranie wszystkich samochodów
var cars = CarsManager.Instance.Loaded;
foreach (var car in cars) {
    Console.WriteLine($"{car.Id}: {car.Name}");
}

// Pobranie konkretnego samochodu
var f40 = CarsManager.Instance.GetById("ferrari_f40");
if (f40 != null) {
    Console.WriteLine($"Moc: {f40.SpecsPower} HP");
}
```

### 4.2. Odczyt danych z samochodu

```csharp
var acRoot = AcRootDirectory.Instance.RequireValue;
var carDir = AcPaths.GetCarDirectory(acRoot, "ferrari_f40");

// Utworzenie wrappera danych
var data = DataWrapper.FromCarDirectory(carDir);

// Odczyt pliku INI
var carIni = data.GetIniFile("car.ini");
var power = carIni["ENGINE"].GetDouble("POWER", 0);
var torque = carIni["ENGINE"].GetDouble("TORQUE", 0);

// Odczyt pliku LODs
var lodsIni = data.GetIniFile("lods.ini");
var lod0File = lodsIni["LOD_0"].GetNonEmpty("FILE");

// Sprawdzenie czy plik istnieje
if (data.Contains("tyres.ini")) {
    var tyresIni = data.GetIniFile("tyres.ini");
    // ...
}
```

### 4.3. Pobranie listy torów

```csharp
TracksManager.Initialize();
var tracks = TracksManager.Instance.Loaded;

foreach (var track in tracks) {
    Console.WriteLine($"{track.Id}: {track.Name}");
    foreach (var layout in track.Layouts) {
        Console.WriteLine($"  Layout: {layout.Id}");
    }
}
```

### 4.4. Skanowanie katalogu content

```csharp
var acRoot = AcRootDirectory.Instance.RequireValue;

// Pobranie wszystkich katalogów samochodów
var carsDir = AcPaths.GetCarsDirectory(acRoot);
var carDirs = Directory.GetDirectories(carsDir);

foreach (var carDir in carDirs) {
    var carId = Path.GetFileName(carDir);
    Console.WriteLine($"Samochód: {carId}");
    
    // Sprawdzenie czy ma data.acd
    var dataAcd = Path.Combine(carDir, "data.acd");
    if (File.Exists(dataAcd)) {
        Console.WriteLine("  Spakowany (data.acd)");
    } else {
        var dataDir = Path.Combine(carDir, "data");
        if (Directory.Exists(dataDir)) {
            Console.WriteLine("  Rozpakowany (data/)");
        }
    }
}
```

---

## 5. Obsługa plików spakowanych vs rozpakowanych

### Spakowane (data.acd)
- Pliki są skompresowane w archiwum ACD
- Szybsze wczytywanie dla małych plików
- Wymaga dekompresji do odczytu

### Rozpakowane (data/)
- Pliki są w zwykłych katalogach
- Łatwiejszy dostęp bezpośredni
- Większe zużycie miejsca na dysku

**DataWrapper** automatycznie wykrywa format i używa odpowiedniego mechanizmu odczytu.

---

## 6. Monitoring zmian w folderze content

Menedżery używają `IDirectoryListener` do monitorowania zmian:

```csharp
// W AcManagerNew<T>
public override void ActualScan() {
    base.ActualScan();
    
    if (_subscribed || !IsScanned || Directories == null) return;
    _subscribed = true;
    Directories.Subscribe(this); // Subskrypcja zmian
}
```

Gdy plik/katalog zostanie dodany/usunięty/zmieniony w folderze content, menedżer automatycznie odświeża listę obiektów.

---

## 7. Szczegółowa struktura plików w folderach content

### 7.1. Struktura folderu samochodu (np. abarth500)

**Przykładowa ścieżka:**
```
C:\Program Files (x86)\Steam\steamapps\common\assettocorsa\content\cars\abarth500\
```

**Pełna struktura katalogu:**

```
abarth500\
├── data.acd                    # [OPCJONALNE] Spakowane dane samochodu (alternatywa dla data/)
│                               # Jeśli istnieje, data/ jest ignorowane
│
├── data\                       # [OPCJONALNE] Rozpakowane dane samochodu (alternatywa dla data.acd)
│   ├── car.ini                 # Główne parametry samochodu (moc, masa, wymiary)
│   ├── engine.ini               # Parametry silnika
│   ├── drivetrain.ini           # Skrzynia biegów, dyferencjał
│   ├── suspensions.ini          # Zawieszenie
│   ├── brakes.ini               # Hamulce
│   ├── tyres.ini                # Opony
│   ├── aerodynamics.ini         # Aerodynamika
│   ├── electronics.ini          # Systemy elektroniczne (ABS, TC, itp.)
│   ├── lods.ini                 # Poziomy szczegółowości (LOD_0, LOD_HR, itp.)
│   ├── digital_instruments.ini  # Konfiguracja wyświetlaczy cyfrowych
│   ├── driver3d.ini             # Model kierowcy 3D
│   ├── ai.ini                   # Parametry AI
│   └── ... (inne pliki INI)
│
├── logo.png                     # [WYMAGANE] Logo samochodu (wyświetlane w UI)
│
├── ui\                          # [WYMAGANE] Pliki interfejsu użytkownika
│   ├── ui_car.json              # [WYMAGANE] Główne dane samochodu (nazwa, marka, rok, specyfikacje)
│   ├── badge.png                # [WYMAGANE] Odznaka marki (dla samochodów Kunos)
│   ├── upgrade.png              # [OPCJONALNE] Ikona upgrade (dla samochodów z parent)
│   ├── cm_textures.json         # [OPCJONALNE] Konfiguracja tekstur CM
│   └── cm_textures.lua          # [OPCJONALNE] Skrypt Lua dla tekstur CM
│
├── skins\                       # [OPCJONALNE] Skórki samochodu
│   ├── 00_default\              # Domyślna skórka
│   │   ├── ui_skin.json         # Dane skórki (nazwa, autor)
│   │   ├── preview.jpg           # Podgląd skórki
│   │   ├── livery.png           # Malowanie samochodu
│   │   └── ... (inne pliki skórki)
│   └── racing_green\            # Przykładowa dodatkowa skórka
│       └── ...
│
├── *.kn5                        # [WYMAGANE] Modele 3D samochodu (główny plik)
│                               # Nazwa z lods.ini lub największy plik .kn5
│
└── sfx\                         # [OPCJONALNE] Dźwięki samochodu
    └── *.bank                   # Pliki dźwiękowe FMOD
```

**Pliki używane przez Content Manager:**

#### Z głównego katalogu:
- `logo.png` - wyświetlany w liście samochodów
- `ui/ui_car.json` - **WYMAGANY** - dane samochodu (nazwa, marka, rok, specyfikacje)
- `ui/badge.png` - odznaka marki (sprawdzane przez `CheckBrandBadge()`)
- `ui/upgrade.png` - ikona upgrade (sprawdzane przez `CheckUpgradeIcon()` dla samochodów z parent)
- `*.kn5` - główny model 3D (odczytywany przez `AcPaths.GetMainCarFilename()`)

#### Z data.acd lub data/:
- `car.ini` - parametry samochodu (moc, masa, wymiary)
- `engine.ini` - parametry silnika
- `drivetrain.ini` - skrzynia biegów, dyferencjał
- `suspensions.ini` - zawieszenie
- `brakes.ini` - hamulce
- `tyres.ini` - opony
- `aerodynamics.ini` - aerodynamika
- `electronics.ini` - systemy elektroniczne
- `lods.ini` - **WAŻNY** - definiuje pliki LOD (poziomy szczegółowości)
- `digital_instruments.ini` - konfiguracja wyświetlaczy
- `driver3d.ini` - model kierowcy 3D
- `ai.ini` - parametry AI

#### Z skins/:
- Każda skórka w osobnym podkatalogu
- `ui_skin.json` - dane skórki
- `preview.jpg` - podgląd
- `livery.png` - malowanie

**Przykład odczytu plików:**
```csharp
var carDir = @"C:\...\content\cars\abarth500";
var data = DataWrapper.FromCarDirectory(carDir);

// Odczyt z data.acd lub data/
var carIni = data.GetIniFile("car.ini");
var power = carIni["ENGINE"].GetDouble("POWER", 0);
var weight = carIni["BASIC"].GetDouble("TOTALMASS", 0);

var lodsIni = data.GetIniFile("lods.ini");
var lod0File = lodsIni["LOD_0"].GetNonEmpty("FILE"); // np. "abarth500.kn5"

// Odczyt z głównego katalogu
var logoPath = Path.Combine(carDir, "logo.png");
var uiCarJson = Path.Combine(carDir, "ui", "ui_car.json");
var json = JObject.Parse(File.ReadAllText(uiCarJson));
var name = json["name"]?.ToString();
```

---

### 7.2. Struktura folderu toru (np. spa)

**Przykładowa ścieżka:**
```
C:\Program Files (x86)\Steam\steamapps\common\assettocorsa\content\tracks\spa\
```

**Pełna struktura katalogu:**

```
spa\
├── data.acd                    # [OPCJONALNE] Spakowane dane toru (alternatywa dla data/)
│
├── data\                       # [OPCJONALNE] Rozpakowane dane toru
│   ├── track.ini               # Główne parametry toru
│   ├── surfaces.ini            # Powierzchnie toru (asfalt, trawa, itp.)
│   ├── ai.ini                  # Parametry AI dla toru
│   ├── ai_hints.ini            # Wskazówki dla AI
│   ├── drs_zones.ini           # Strefy DRS
│   ├── cameras.ini             # Kamery
│   ├── lights.ini              # Oświetlenie
│   └── ... (inne pliki INI)
│
├── models.ini                  # [OPCJONALNE] Modele 3D (dla toru bez layoutów)
│                               # Dla torów z layoutami: models_{layout_id}.ini
│
├── ui\                         # [WYMAGANE] Pliki interfejsu użytkownika
│   ├── ui_track.json           # [WYMAGANE] Główne dane toru (nazwa, długość, itp.)
│   ├── preview.png             # [OPCJONALNE] Podgląd toru
│   ├── outline.png             # [OPCJONALNE] Zarys toru
│   └── {layout_id}\            # [OPCJONALNE] Layouty toru (np. spa_gp, spa_24h)
│       ├── ui_track.json       # Dane layoutu
│       ├── preview.png
│       └── outline.png
│
├── skins\                      # [OPCJONALNE] Skórki toru
│   ├── default\                # Domyślna skórka
│   └── cm_skins\               # Skórki Content Managera
│       └── {skin_id}\
│           └── ui_track_skin.json
│
├── *.kn5                       # [WYMAGANE] Modele 3D toru
│
└── surfaces.ini                 # [OPCJONALNE] Powierzchnie (alternatywa dla data/surfaces.ini)
```

**Pliki używane przez Content Manager:**

#### Z głównego katalogu:
- `ui/ui_track.json` - **WYMAGANY** - dane toru (nazwa, długość, layouty)
- `ui/preview.png` - podgląd toru
- `ui/outline.png` - zarys toru
- `models.ini` - modele 3D (dla toru bez layoutów)
- `models_{layout_id}.ini` - modele 3D dla konkretnego layoutu
- `surfaces.ini` - powierzchnie (jeśli nie ma w data/)

#### Z data.acd lub data/:
- `track.ini` - parametry toru
- `surfaces.ini` - powierzchnie toru
- `ai.ini` - parametry AI
- `ai_hints.ini` - wskazówki dla AI
- `drs_zones.ini` - strefy DRS
- `cameras.ini` - kamery
- `lights.ini` - oświetlenie
- `traffic.json` - [OPCJONALNE] Konfiguracja ruchu (dla Traffic Planner)

#### Z skins/:
- `default/` - domyślna skórka
- `cm_skins/{skin_id}/ui_track_skin.json` - skórki Content Managera

**Przykład odczytu plików:**
```csharp
var trackDir = @"C:\...\content\tracks\spa";
var data = DataWrapper.FromCarDirectory(trackDir);

// Odczyt z data.acd lub data/
var trackIni = data.GetIniFile("track.ini");
var length = trackIni["LENGTH"].GetDouble("LENGTH", 0);

var surfacesIni = data.GetIniFile("surfaces.ini");
// lub z głównego katalogu:
var surfacesPath = Path.Combine(trackDir, "surfaces.ini");

// Odczyt z głównego katalogu
var uiTrackJson = Path.Combine(trackDir, "ui", "ui_track.json");
var json = JObject.Parse(File.ReadAllText(uiTrackJson));
var name = json["name"]?.ToString();

// Layouty
var uiDir = Path.Combine(trackDir, "ui");
var layouts = Directory.GetDirectories(uiDir)
    .Where(x => File.Exists(Path.Combine(x, "ui_track.json")));
```

---

### 7.3. Lista wszystkich plików używanych przez Content Manager

#### Dla samochodów (CarObject):

**Z głównego katalogu:**
- `logo.png` - logo samochodu
- `ui/ui_car.json` - **WYMAGANY** - dane samochodu
- `ui/badge.png` - odznaka marki
- `ui/upgrade.png` - ikona upgrade (dla samochodów z parent)
- `ui/cm_textures.json` - konfiguracja tekstur CM
- `ui/cm_textures.lua` - skrypt Lua dla tekstur CM
- `*.kn5` - modele 3D (główny plik)

**Z data.acd lub data/:**
- `car.ini` - parametry samochodu
- `engine.ini` - silnik
- `drivetrain.ini` - skrzynia biegów
- `suspensions.ini` - zawieszenie
- `brakes.ini` - hamulce
- `tyres.ini` - opony
- `aerodynamics.ini` - aerodynamika
- `electronics.ini` - systemy elektroniczne
- `lods.ini` - **WAŻNY** - poziomy szczegółowości
- `digital_instruments.ini` - wyświetlacze cyfrowe
- `driver3d.ini` - model kierowcy
- `ai.ini` - parametry AI

**Z skins/{skin_id}/:**
- `ui_skin.json` - dane skórki
- `preview.jpg` - podgląd
- `livery.png` - malowanie
- `skin.ini` - [OPCJONALNE] Konfiguracja skórki
- `ext_config.ini` - [OPCJONALNE] Konfiguracja rozszerzona

#### Dla torów (TrackObject):

**Z głównego katalogu:**
- `ui/ui_track.json` - **WYMAGANY** - dane toru
- `ui/preview.png` - podgląd
- `ui/outline.png` - zarys
- `ui/{layout_id}/ui_track.json` - dane layoutu
- `models.ini` - modele 3D (dla toru bez layoutów)
- `models_{layout_id}.ini` - modele 3D dla layoutu
- `surfaces.ini` - powierzchnie (alternatywa dla data/)

**Z data.acd lub data/:**
- `track.ini` - parametry toru
- `surfaces.ini` - powierzchnie
- `ai.ini` - parametry AI
- `ai_hints.ini` - wskazówki AI
- `drs_zones.ini` - strefy DRS
- `cameras.ini` - kamery
- `lights.ini` - oświetlenie
- `traffic.json` - [OPCJONALNE] Traffic Planner

**Z skins/{skin_id}/:**
- `ui_track_skin.json` - dane skórki toru

---

## 8. Podsumowanie

1. **AcPaths** - buduje ścieżki do folderów content
2. **AcRootDirectory** - zarządza katalogiem głównym AC i tworzy AcDirectories
3. **AcDirectories** - reprezentuje katalogi do skanowania (np. content/cars/)
4. **AcManagerNew<T>** - bazowa klasa do zarządzania obiektami contentu
5. **DataWrapper** - odczyt danych z plików .acd lub katalogów data/
6. **FilesStorage** - pobieranie plików z danych Content Managera (nie z content AC)

**Struktura plików:**
- **Samochody:** `{AC_ROOT}\content\cars\{car_id}\` zawiera `logo.png`, `ui/ui_car.json`, `data.acd` lub `data/`, `skins/`, `*.kn5`
- **Tory:** `{AC_ROOT}\content\tracks\{track_id}\` zawiera `ui/ui_track.json`, `data.acd` lub `data/`, `models.ini`, `skins/`, `*.kn5`

Cały system jest zaprojektowany tak, aby automatycznie wykrywać i wczytywać content z folderu `content` Assetto Corsa, obsługując zarówno pliki spakowane jak i rozpakowane.

