# Lista plików i ścieżek używanych przez Content Manager

## Przegląd

Content Manager używa trzech głównych lokalizacji:
1. **Katalog Assetto Corsa** (`{AC_ROOT}`) - zawartość gry
2. **Katalog Dokumentów** (`{Documents}\Assetto Corsa`) - dane użytkownika gry
3. **Katalog danych Content Managera** (`{AppData}\Local\AcTools Content Manager`) - dane aplikacji

---

## 1. Katalog Assetto Corsa ({AC_ROOT})

**Domyślna lokalizacja:**
- Steam: `C:\Program Files (x86)\Steam\steamapps\common\assettocorsa\`
- Inne: zależy od instalacji

### 1.1 Zawartość gry

#### Samochody
- **Katalog:** `{AC_ROOT}\content\cars\`
- **Przykład:** `C:\...\assettocorsa\content\cars\ferrari_f40\`
- **Zawartość:**
  - `data.acd` - skompresowane dane samochodu
  - `logo.png` - logo samochodu
  - `ui\ui_car.json` - dane UI samochodu
  - `skins\` - skórki samochodu

#### Tory
- **Katalog:** `{AC_ROOT}\content\tracks\`
- **Przykład:** `C:\...\assettocorsa\content\tracks\spa\`
- **Zawartość:**
  - `data.acd` - skompresowane dane toru
  - `ui\ui_track.json` - dane UI toru
  - `surfaces.ini` - powierzchnie toru

#### Showroomy
- **Katalog:** `{AC_ROOT}\content\showroom\`
- **Zawartość:** Showroomy do wyświetlania samochodów

#### Pogoda
- **Katalog:** `{AC_ROOT}\content\weather\`
- **Zawartość:** Pliki pogodowe

#### Fonty
- **Katalog:** `{AC_ROOT}\content\fonts\`
- **Zawartość:** Fonty używane w grze

#### Modele kierowców
- **Katalog:** `{AC_ROOT}\content\driver\`
- **Zawartość:** Modele 3D kierowców

#### Kariera Kunos
- **Katalog:** `{AC_ROOT}\content\career\`
- **Zawartość:** Wydarzenia kariery

#### Aplikacje Python
- **Katalog:** `{AC_ROOT}\apps\python\`
- **Zawartość:** Aplikacje Python

#### Aplikacje Lua
- **Katalog:** `{AC_ROOT}\apps\lua\`
- **Zawartość:** Aplikacje Lua

#### Filtry PP
- **Katalog:** `{AC_ROOT}\system\cfg\ppfilters\`
- **Zawartość:** Filtry post-processingu

#### Konfiguracja systemowa
- **Katalog:** `{AC_ROOT}\system\cfg\`
- **Zawartość:** Pliki konfiguracyjne systemu

### 1.2 Pliki wykonywalne

- **Launcher:** `{AC_ROOT}\AssettoCorsa.exe`
- **Gra (64-bit):** `{AC_ROOT}\acs.exe`
- **Gra (32-bit):** `{AC_ROOT}\acs_x86.exe`
- **Showroom:** `{AC_ROOT}\acShowroom.exe`

### 1.3 Inne pliki

- **Logo AC:** `{AC_ROOT}\content\gui\logo_ac_app.png`
- **Ikony GUI:** `{AC_ROOT}\content\gui\icons\`
- **SFX:** `{AC_ROOT}\content\sfx\`
- **GUIDs SFX:** `{AC_ROOT}\content\sfx\GUIDs.txt`

---

## 2. Katalog Dokumentów ({Documents}\Assetto Corsa)

**Lokalizacja:**
- `C:\Users\{USERNAME}\Documents\Assetto Corsa\`

### 2.1 Konfiguracja (cfg)

**Katalog:** `{Documents}\Assetto Corsa\cfg\`

#### Pliki konfiguracyjne sesji
- **`race.ini`** - główny plik konfiguracyjny sesji
  - Zawiera: samochód, tor, warunki, tryb sesji
  - **Zapisywany przez:** `Game.BasicProperties.Set()`
  - **Czytany przez:** Assetto Corsa przy starcie

- **`assists.ini`** - ustawienia asystentów
  - Zawiera: ABS, TC, auto-blip, etc.
  - **Zapisywany przez:** `Game.AssistsProperties.Set()`
  - **Czytany przez:** Assetto Corsa przy starcie

#### Inne pliki konfiguracyjne
- **`video.ini`** - ustawienia wideo
- **`controls.ini`** - ustawienia sterowania
- **`python.ini`** - konfiguracja aplikacji Python
- **`showroom_start.ini`** - konfiguracja showroomu
- **`launcher.ini`** - konfiguracja launcher'a

### 2.2 Replay'e

**Katalog:** `{Documents}\Assetto Corsa\replay\`
- **Zawartość:** Pliki replay (.rpy)
- **Format:** `.rpy`

### 2.3 Setupy samochodów

**Katalog:** `{Documents}\Assetto Corsa\setups\`
- **Struktura:**
  - `{Documents}\Assetto Corsa\setups\{car_id}\{setup_name}.ini`
- **Przykład:**
  - `{Documents}\Assetto Corsa\setups\ferrari_f40\spa_qualifying.ini`

### 2.4 Zrzuty ekranu

**Katalog:** `{Documents}\Assetto Corsa\screens\`
- **Zawartość:** Zrzuty ekranu z gry

### 2.5 Wyniki wyścigów

**Katalog:** `{Documents}\Assetto Corsa\out\`
- **Pliki:**
  - **`race_out.json`** - wyniki wyścigu
    - **Czytany przez:** `Game.GetResult()`
    - **Format:** JSON

### 2.6 Logi

**Katalog:** `{Documents}\Assetto Corsa\logs\`
- **Pliki:**
  - **`log.txt`** - główny log gry
  - Inne logi: `{logFileName}.txt`

### 2.7 Dane launcher'a

**Katalog:** `{Documents}\Assetto Corsa\launcherdata\filestore\`

#### Pliki
- **`career.ini`** - postęp w karierze Kunos
- **`champs.ini`** - postęp w mistrzostwach użytkownika
- **`cmhelper.ini`** - pomocnik Content Managera (backdoor)

### 2.8 Mistrzostwa użytkownika

**Katalog:** `{Documents}\Assetto Corsa\champs\` lub `champs_cm\`
- **Zawartość:** Mistrzostwa stworzone przez użytkownika

### 2.9 Edytor materiałów

**Katalog:** `{Documents}\Assetto Corsa\Editor\Materials library\`
- **Zawartość:** Biblioteka materiałów do edycji

---

## 3. Katalog danych Content Managera

**Lokalizacja:**
- `C:\Users\{USERNAME}\AppData\Local\AcTools Content Manager\`
- **Lub:** `{EXE_DIR}\AcManager.exe Data\` (jeśli EXE zawiera "local" w nazwie)

### 3.1 Pliki danych aplikacji

#### Główne pliki danych
- **`Values.data`** - ustawienia aplikacji
  - **Zapis:** `ValuesStorage.Set()`
  - **Odczyt:** `ValuesStorage.Get()`
  - **Szyfrowanie:** Tak (opcjonalne)
  - **Kompresja:** Tak (opcjonalne)

- **`Cache.data`** - cache aplikacji
  - **Zapis:** `CacheStorage`
  - **Zawartość:** Cache'owane dane

- **`Authentication.data`** - dane uwierzytelniania
  - **Zapis:** `AuthenticationStorage`
  - **Zawartość:** Tokeny, klucze API, etc.
  - **Szyfrowanie:** Tak

#### Katalogi danych
- **`Data\`** - dane systemowe
- **`Data (User)\`** - dane użytkownika

### 3.2 Logi aplikacji

**Katalog:** `{AppData}\AcTools Content Manager\Logs\`
- **Pliki:**
  - `{id}.log` - logi aplikacji
  - **Format:** Tekstowy

### 3.3 Tymczasowe pliki

**Katalog:** `{AppData}\AcTools Content Manager\Temp\`
- **Podkatalogi:**
  - `Storages Backups\` - kopie zapasowe storage'ów
  - Inne tymczasowe pliki

### 3.4 Presety i szablony

**Katalog:** `{AppData}\AcTools Content Manager\Presets\`
- **Zawartość:** Presety użytkownika

### 3.5 Temy i tła

**Katalog:** `{AppData}\AcTools Content Manager\Themes\`
- **Podkatalogi:**
  - `Backgrounds\` - tła aplikacji
  - Inne temy

### 3.6 Lokalizacje

**Katalog:** `{AppData}\AcTools Content Manager\Locales\{ID}\`
- **Zawartość:** Pliki lokalizacji (języki)

### 3.7 Postęp sesji

**Katalog:** `{AppData}\AcTools Content Manager\Progress\Sessions\`
- **Zawartość:** Wyniki sesji wyścigowych

### 3.8 Pliki konfiguracyjne

- **`Arguments.txt`** - argumenty uruchomieniowe aplikacji
  - **Lokalizacja:** `{AppData}\AcTools Content Manager\Arguments.txt`
  - **Format:** Tekstowy, jeden argument na linię

- **`Trying to run.flag`** - flaga próby uruchomienia
  - **Lokalizacja:** `{AppData}\AcTools Content Manager\Trying to run.flag`

---

## 4. Szczegółowa lista plików

### 4.1 Pliki konfiguracyjne sesji (zapisywane przed uruchomieniem gry)

| Plik | Lokalizacja | Opis | Kiedy zapisywany |
|------|-------------|------|------------------|
| `race.ini` | `{Documents}\Assetto Corsa\cfg\` | Konfiguracja sesji (samochód, tor, warunki) | Przed uruchomieniem gry |
| `assists.ini` | `{Documents}\Assetto Corsa\cfg\` | Ustawienia asystentów | Przed uruchomieniem gry |
| `showroom_start.ini` | `{Documents}\Assetto Corsa\cfg\` | Konfiguracja showroomu | Przed uruchomieniem showroomu |

### 4.2 Pliki wyników (odczytywane po zakończeniu gry)

| Plik | Lokalizacja | Opis | Kiedy odczytywany |
|------|-------------|------|-------------------|
| `race_out.json` | `{Documents}\Assetto Corsa\out\` | Wyniki wyścigu | Po zakończeniu gry |
| `log.txt` | `{Documents}\Assetto Corsa\logs\` | Log gry | Po zakończeniu gry |

### 4.3 Pliki danych Content Managera

| Plik | Lokalizacja | Opis | Format |
|------|-------------|------|--------|
| `Values.data` | `{AppData}\AcTools Content Manager\` | Ustawienia aplikacji | Binarny (szyfrowany, kompresowany) |
| `Cache.data` | `{AppData}\AcTools Content Manager\` | Cache aplikacji | Binarny (kompresowany) |
| `Authentication.data` | `{AppData}\AcTools Content Manager\` | Dane uwierzytelniania | Binarny (szyfrowany) |

### 4.4 Pliki konfiguracyjne Assetto Corsa (tylko odczyt)

| Plik | Lokalizacja | Opis |
|------|-------------|------|
| `video.ini` | `{Documents}\Assetto Corsa\cfg\` | Ustawienia wideo |
| `controls.ini` | `{Documents}\Assetto Corsa\cfg\` | Ustawienia sterowania |
| `python.ini` | `{Documents}\Assetto Corsa\cfg\` | Konfiguracja Python |
| `launcher.ini` | `{Documents}\Assetto Corsa\cfg\` | Konfiguracja launcher'a |

### 4.5 Pliki danych gry (tylko odczyt)

| Plik | Lokalizacja | Opis |
|------|-------------|------|
| `career.ini` | `{Documents}\Assetto Corsa\launcherdata\filestore\` | Postęp w karierze |
| `champs.ini` | `{Documents}\Assetto Corsa\launcherdata\filestore\` | Postęp w mistrzostwach |
| `cmhelper.ini` | `{Documents}\Assetto Corsa\launcherdata\filestore\` | Pomocnik CM |

---

## 5. Struktura katalogów - pełna ścieżka

### 5.1 Assetto Corsa Root

```
{AC_ROOT}/
├── AssettoCorsa.exe
├── acs.exe
├── acs_x86.exe
├── acShowroom.exe
├── content/
│   ├── cars/
│   │   └── {car_id}/
│   │       ├── data.acd
│   │       ├── logo.png
│   │       ├── ui/
│   │       │   ├── ui_car.json
│   │       │   ├── badge.png
│   │       │   └── upgrade.png
│   │       └── skins/
│   │           └── {skin_id}/
│   ├── tracks/
│   │   └── {track_id}/
│   ├── showroom/
│   ├── weather/
│   ├── fonts/
│   ├── driver/
│   └── career/
├── apps/
│   ├── python/
│   └── lua/
├── system/
│   └── cfg/
│       └── ppfilters/
└── content/
    └── gui/
        ├── logo_ac_app.png
        └── icons/
```

### 5.2 Documents\Assetto Corsa

```
{Documents}\Assetto Corsa/
├── cfg/
│   ├── race.ini              # ⚠️ ZAPISYWANY przez CM
│   ├── assists.ini           # ⚠️ ZAPISYWANY przez CM
│   ├── video.ini
│   ├── controls.ini
│   ├── python.ini
│   ├── showroom_start.ini
│   └── launcher.ini
├── replay/
│   └── *.rpy
├── setups/
│   └── {car_id}/
│       └── *.ini
├── screens/
│   └── *.png, *.jpg
├── out/
│   └── race_out.json        # ⚠️ ODCZYTYWANY przez CM
├── logs/
│   └── log.txt
├── launcherdata/
│   └── filestore/
│       ├── career.ini
│       ├── champs.ini
│       └── cmhelper.ini
├── champs/                   # lub champs_cm/
│   └── {championship}/
└── Editor/
    └── Materials library/
```

### 5.3 AppData\Local\AcTools Content Manager

```
{AppData}\Local\AcTools Content Manager/
├── Values.data               # ⚠️ GŁÓWNY PLIK USTAWIEŃ
├── Cache.data
├── Authentication.data
├── Arguments.txt
├── Trying to run.flag
├── Data/
│   └── (dane systemowe)
├── Data (User)/
│   └── (dane użytkownika)
├── Logs/
│   └── {id}.log
├── Temp/
│   └── Storages Backups/
├── Presets/
├── Themes/
│   └── Backgrounds/
├── Locales/
│   └── {locale_id}/
└── Progress/
    └── Sessions/
```

---

## 6. Kody źródłowe - lokalizacje

### 6.1 AcPaths.cs - ścieżki Assetto Corsa

**Plik:** `AcTools\Utils\AcPaths.cs`

**Główne metody:**
- `GetDocumentsDirectory()` → `{Documents}\Assetto Corsa`
- `GetDocumentsCfgDirectory()` → `{Documents}\Assetto Corsa\cfg`
- `GetReplaysDirectory()` → `{Documents}\Assetto Corsa\replay`
- `GetCarSetupsDirectory()` → `{Documents}\Assetto Corsa\setups`
- `GetRaceIniFilename()` → `{Documents}\Assetto Corsa\cfg\race.ini`
- `GetAssistsIniFilename()` → `{Documents}\Assetto Corsa\cfg\assists.ini`
- `GetResultJsonFilename()` → `{Documents}\Assetto Corsa\out\race_out.json`
- `GetCarsDirectory(acRoot)` → `{AC_ROOT}\content\cars`
- `GetTracksDirectory(acRoot)` → `{AC_ROOT}\content\tracks`

### 6.2 FilesStorage.cs - ścieżki Content Managera

**Plik:** `AcManager.Tools\Helpers\FilesStorage.cs`

**Główne metody:**
- `FilesStorage.Instance` → `{AppData}\Local\AcTools Content Manager`
- `GetFilename("Values.data")` → `{AppData}\...\Values.data`
- `GetFilename("Cache.data")` → `{AppData}\...\Cache.data`
- `GetFilename("Authentication.data")` → `{AppData}\...\Authentication.data`

### 6.3 EntryPoint.cs - inicjalizacja ścieżek

**Plik:** `AcManager\EntryPoint.cs`

**Kod:**
```csharp
ApplicationDataDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "AcTools Content Manager"
);
```

### 6.4 ValuesStorage.cs - zapis ustawień

**Plik:** `FirstFloor.ModernUI\Helpers\ValuesStorage.cs`

**Użycie:**
```csharp
// Zapis
ValuesStorage.Set("key", value);

// Odczyt
var value = ValuesStorage.Get<string>("key", defaultValue);
```

---

## 7. Przepływ danych - przykłady

### 7.1 Uruchomienie gry

```
1. QuickDrive.ViewModel.Go()
   ↓
2. Game.StartProperties.Set()
   ↓
3. Zapis do: {Documents}\Assetto Corsa\cfg\race.ini
   Zapis do: {Documents}\Assetto Corsa\cfg\assists.ini
   ↓
4. Uruchomienie acs.exe
   ↓
5. Gra czyta race.ini i assists.ini
   ↓
6. Po zakończeniu: Zapis do {Documents}\Assetto Corsa\out\race_out.json
   ↓
7. CM czyta race_out.json przez Game.GetResult()
```

### 7.2 Zapisywanie ustawień

```
1. Użytkownik zmienia ustawienie w UI
   ↓
2. ViewModel zmienia właściwość
   ↓
3. ValuesStorage.Set("key", value)
   ↓
4. Zapis do: {AppData}\Local\AcTools Content Manager\Values.data
```

### 7.3 Ładowanie samochodów

```
1. CarsManager.Initialize()
   ↓
2. Skanowanie: {AC_ROOT}\content\cars\
   ↓
3. Dla każdego katalogu:
   - Odczyt: {AC_ROOT}\content\cars\{car_id}\ui\ui_car.json
   - Tworzenie: CarObject
   ↓
4. Przechowywanie w pamięci (CarsManager.Instance)
```

---

## 8. Ważne uwagi

### 8.1 Pliki zapisywane przez Content Manager

⚠️ **Te pliki są nadpisywane przez Content Manager:**
- `{Documents}\Assetto Corsa\cfg\race.ini`
- `{Documents}\Assetto Corsa\cfg\assists.ini`
- `{AppData}\Local\AcTools Content Manager\Values.data`
- `{AppData}\Local\AcTools Content Manager\Cache.data`
- `{AppData}\Local\AcTools Content Manager\Authentication.data`

### 8.2 Pliki tylko do odczytu

✅ **Te pliki są tylko odczytywane:**
- Wszystkie pliki w `{AC_ROOT}\content\`
- `{Documents}\Assetto Corsa\cfg\video.ini`
- `{Documents}\Assetto Corsa\cfg\controls.ini`
- `{Documents}\Assetto Corsa\launcherdata\filestore\career.ini`

### 8.3 Pliki generowane przez grę

📝 **Te pliki są tworzone przez Assetto Corsa:**
- `{Documents}\Assetto Corsa\out\race_out.json`
- `{Documents}\Assetto Corsa\logs\log.txt`
- `{Documents}\Assetto Corsa\replay\*.rpy`
- `{Documents}\Assetto Corsa\screens\*.png`

---

## 9. Zmienne środowiskowe i ścieżki

### 9.1 Zmienne używane

- `Environment.SpecialFolder.MyDocuments` → `C:\Users\{USERNAME}\Documents`
- `Environment.SpecialFolder.LocalApplicationData` → `C:\Users\{USERNAME}\AppData\Local`

### 9.2 Przykładowe pełne ścieżki

**Windows 10/11, użytkownik "Jan":**

```
AC_ROOT: C:\Program Files (x86)\Steam\steamapps\common\assettocorsa
Documents: C:\Users\Jan\Documents\Assetto Corsa
AppData: C:\Users\Jan\AppData\Local\AcTools Content Manager

race.ini: C:\Users\Jan\Documents\Assetto Corsa\cfg\race.ini
Values.data: C:\Users\Jan\AppData\Local\AcTools Content Manager\Values.data
```

---

## 10. Podsumowanie - szybka referencja

### 10.1 Główne katalogi

| Katalog | Ścieżka | Zawartość |
|---------|---------|-----------|
| AC Root | `{AC_ROOT}` | Zawartość gry |
| Documents | `{Documents}\Assetto Corsa` | Dane użytkownika gry |
| AppData | `{AppData}\Local\AcTools Content Manager` | Dane aplikacji CM |

### 10.2 Najważniejsze pliki

| Plik | Lokalizacja | Rola |
|------|-------------|------|
| `race.ini` | `{Documents}\Assetto Corsa\cfg\` | Konfiguracja sesji |
| `assists.ini` | `{Documents}\Assetto Corsa\cfg\` | Asystenci |
| `race_out.json` | `{Documents}\Assetto Corsa\out\` | Wyniki wyścigu |
| `Values.data` | `{AppData}\...\` | Ustawienia CM |
| `Cache.data` | `{AppData}\...\` | Cache CM |
| `Authentication.data` | `{AppData}\...\` | Uwierzytelnianie |

### 10.3 Kody źródłowe

| Plik | Opis |
|------|------|
| `AcTools\Utils\AcPaths.cs` | Ścieżki Assetto Corsa |
| `AcManager.Tools\Helpers\FilesStorage.cs` | Ścieżki Content Managera |
| `AcManager\EntryPoint.cs` | Inicjalizacja ścieżek |
| `FirstFloor.ModernUI\Helpers\ValuesStorage.cs` | Zapisywanie ustawień |

---

## 11. Przykłady użycia w kodzie

### 11.1 Odczyt pliku race.ini

```csharp
var raceIni = new IniFile(AcPaths.GetRaceIniFilename());
var carId = raceIni["RACE"].GetNonEmpty("MODEL");
```

### 11.2 Zapis do race.ini

```csharp
var raceIni = new IniFile(AcPaths.GetRaceIniFilename());
raceIni["RACE"].Set("MODEL", "ferrari_f40");
raceIni.Save();
```

### 11.3 Zapis ustawień

```csharp
ValuesStorage.Set("SelectedCar", "ferrari_f40");
var carId = ValuesStorage.Get<string>("SelectedCar");
```

### 11.4 Odczyt wyniku wyścigu

```csharp
var result = Game.GetResult(gameStartTime);
if (result != null) {
    // Wyniki dostępne w result
}
```

---

## 12. Backup i przywracanie

### 12.1 Co warto backupować

**Ustawienia Content Managera:**
- `{AppData}\Local\AcTools Content Manager\Values.data`
- `{AppData}\Local\AcTools Content Manager\Cache.data`

**Dane użytkownika:**
- `{Documents}\Assetto Corsa\setups\` - setupy samochodów
- `{Documents}\Assetto Corsa\replay\` - replay'e
- `{Documents}\Assetto Corsa\launcherdata\filestore\champs.ini` - mistrzostwa

**Zawartość:**
- `{AC_ROOT}\content\cars\` - samochody
- `{AC_ROOT}\content\tracks\` - tory

### 12.2 Czego nie trzeba backupować

- `{Documents}\Assetto Corsa\cfg\race.ini` - generowany automatycznie
- `{Documents}\Assetto Corsa\out\race_out.json` - wyniki wyścigów
- `{AppData}\Local\AcTools Content Manager\Temp\` - pliki tymczasowe

---

## Podsumowanie końcowe

Content Manager używa trzech głównych lokalizacji:

1. **{AC_ROOT}** - zawartość gry (tylko odczyt)
2. **{Documents}\Assetto Corsa** - dane użytkownika gry (odczyt/zapis)
3. **{AppData}\Local\AcTools Content Manager** - dane aplikacji (zapis)

Najważniejsze pliki:
- `race.ini` - konfiguracja sesji
- `assists.ini` - asystenci
- `Values.data` - ustawienia CM
- `race_out.json` - wyniki wyścigów

Wszystkie ścieżki są zarządzane przez klasy `AcPaths` i `FilesStorage`.


