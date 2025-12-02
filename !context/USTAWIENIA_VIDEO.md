# Ustawienia wideo - TripleScreen, Single, VR i rozdzielczość

## Przegląd

Content Manager zapisuje ustawienia wideo (tryb wyświetlania i rozdzielczość) bezpośrednio do pliku konfiguracyjnego Assetto Corsa. Gra czyta ten plik przy starcie i automatycznie stosuje ustawienia.

---

## 1. Plik konfiguracyjny

**Plik:** `{Documents}\Assetto Corsa\cfg\video.ini`

**Lokalizacja w kodzie:**
- `AcTools\Utils\AcPaths.cs` → `GetCfgVideoFilename()`
- `AcManager.Tools\Helpers\AcSettings\VideoSettings.cs`

**Przykład ścieżki:** `C:\Users\Jan\Documents\Assetto Corsa\cfg\video.ini`

---

## 2. Tryb wyświetlania (TripleScreen, Single, VR)

### 2.1 Format w pliku

Zapisuje się w sekcji `[CAMERA]` jako `MODE`:

```ini
[CAMERA]
MODE = DEFAULT    ; Single screen
MODE = TRIPLE     ; Triple screen
MODE = OCULUS     ; Oculus Rift VR
MODE = OPENVR     ; OpenVR (SteamVR)
```

### 2.2 Dostępne wartości

| Wartość | Opis | Wyświetlana nazwa |
|---------|------|-------------------|
| `DEFAULT` | Single screen (pojedynczy ekran) | Single screen |
| `TRIPLE` | Triple screen (trzy ekrany) | Triple screen |
| `OCULUS` | Oculus Rift VR | Oculus Rift |
| `OPENVR` | OpenVR (SteamVR) | OpenVR |

### 2.3 Przykłady

**Single screen:**
```ini
[CAMERA]
MODE = DEFAULT
```

**Triple screen:**
```ini
[CAMERA]
MODE = TRIPLE
```

**Oculus Rift VR:**
```ini
[CAMERA]
MODE = OCULUS
```

**SteamVR:**
```ini
[CAMERA]
MODE = OPENVR
```

### 2.4 Lokalizacja w kodzie

**Definicja wartości:**
```csharp
// AcManager.Tools/Helpers/AcSettings/VideoSettings.cs, linia 112-118
private static IEnumerable<SettingEntry> GetCameraModes() {
    return new[] {
        new SettingEntry("DEFAULT", ToolsStrings.AcSettings_CameraMode_SingleScreen),
        new SettingEntry("TRIPLE", ToolsStrings.AcSettings_CameraMode_TripleScreen),
        new SettingEntry("OCULUS", ToolsStrings.AcSettings_CameraMode_OculusRift),
        new SettingEntry("OPENVR", ToolsStrings.AcSettings_CameraMode_OpenVr),
    }.Concat(PatchHelper.CustomVideoModes);
}
```

**Zapis:**
```csharp
// AcManager.Tools/Helpers/AcSettings/VideoSettings.cs, linia 785
ini["CAMERA"].Set("MODE", CameraMode);
```

**Odczyt:**
```csharp
// AcManager.Tools/Helpers/AcSettings/VideoSettings.cs, linia 726
CameraMode = Ini["CAMERA"].GetEntry("MODE", CameraModes);
```

---

## 3. Rozdzielczość

### 3.1 Format w pliku

Rozdzielczość zapisuje się w sekcji `[VIDEO]`:

```ini
[VIDEO]
WIDTH = 1920
HEIGHT = 1080
REFRESH = 60
INDEX = 0

[REFRESH]
VALUE = 60
```

### 3.2 Opis wartości

- **WIDTH** = szerokość ekranu w pikselach (int)
- **HEIGHT** = wysokość ekranu w pikselach (int)
- **REFRESH** = częstotliwość odświeżania w Hz (int)
- **INDEX** = indeks rozdzielczości na liście dostępnych rozdzielczości (int, 0 = pierwsza)
- **VALUE** (w sekcji `[REFRESH]`) = częstotliwość odświeżania w Hz (int)

### 3.3 Przykłady

**Full HD 60Hz:**
```ini
[VIDEO]
WIDTH = 1920
HEIGHT = 1080
REFRESH = 60
INDEX = 0

[REFRESH]
VALUE = 60
```

**4K 144Hz:**
```ini
[VIDEO]
WIDTH = 3840
HEIGHT = 2160
REFRESH = 144
INDEX = 5

[REFRESH]
VALUE = 144
```

**Triple screen (5760x1080):**
```ini
[VIDEO]
WIDTH = 5760
HEIGHT = 1080
REFRESH = 60
INDEX = 0

[REFRESH]
VALUE = 60
```

### 3.4 Lokalizacja w kodzie

**Zapis:**
```csharp
// AcManager.Tools/Helpers/AcSettings/VideoSettings.cs, linia 763-769
var section = ini["VIDEO"];
if (Resolution != null) {
    section.Set("WIDTH", Resolution.Width);
    section.Set("HEIGHT", Resolution.Height);
    section.Set("REFRESH", Resolution.Framerate);
    ini["REFRESH"].Set("VALUE", Resolution.Framerate);
    section.Set("INDEX", Resolution.Index);
}
```

**Odczyt:**
```csharp
// AcManager.Tools/Helpers/AcSettings/VideoSettings.cs, linia 698-712
CustomResolution.Width = section.GetInt("WIDTH", 0);
CustomResolution.Height = section.GetInt("HEIGHT", 0);
CustomResolution.Framerate = Ini["REFRESH"].GetInt("VALUE", 0);

var resolution = Resolutions.GetByIdOrDefault(section.GetInt("INDEX", 0)) ??
        Resolutions.FirstOrDefault(x => x.Same(CustomResolution)) ?? CustomResolution;
Resolution = resolution;
```

---

## 4. Kompletny przykład pliku video.ini

```ini
[VIDEO]
WIDTH = 1920
HEIGHT = 1080
REFRESH = 60
INDEX = 0
FULLSCREEN = 1
VSYNC = 1
AASAMPLES = 4
ANISOTROPIC = 16
SHADOW_MAP_SIZE = 2048
FPS_CAP_MS = 0

[REFRESH]
VALUE = 60

[CAMERA]
MODE = TRIPLE

[ASSETTOCORSA]
HIDE_ARMS = 0
HIDE_STEER = 0
LOCK_STEER = 0
WORLD_DETAIL = 3

[EFFECTS]
MOTION_BLUR = 0
RENDER_SMOKE_IN_MIRROR = 1
SMOKE = 3

[POST_PROCESS]
ENABLED = 1
QUALITY = 2
FILTER = default
GLARE = 2
DOF = 2
RAYS_OF_GOD = 1
HEAT_SHIMMER = 1
FXAA = 1

[MIRROR]
HQ = 1
SIZE = 512
```

---

## 5. Jak działa zapisywanie

### 5.1 Architektura

**Klasa:** `VideoSettings` (`AcManager.Tools\Helpers\AcSettings\VideoSettings.cs`)

**Dziedziczy z:** `IniPresetableSettings` → `IniSettings`

**Proces:**
```
Użytkownik zmienia ustawienie w UI
    ↓
ViewModel zmienia właściwość (np. CameraMode = "TRIPLE")
    ↓
OnPropertyChanged() → Save()
    ↓
SetToIni() - ustawia wartości w IniFile
    ↓
Ini.Save(Filename) - zapisuje do pliku video.ini
    ↓
Gra czyta plik przy starcie
```

### 5.2 Metody

**Zapis:**
- `SetToIni(IniFile ini)` - ustawia wartości w obiekcie IniFile
- `Save()` - zapisuje do pliku (z opóźnieniem 500ms - debounce)
- `SaveImmediately()` - zapis natychmiastowy

**Odczyt:**
- `LoadFromIni()` - ładuje wartości z pliku
- Automatycznie wywoływane przy inicjalizacji

### 5.3 Automatyczny zapis

- **Opóźnienie:** 500ms (debounce) - zapisuje po zakończeniu zmian
- **FileSystemWatcher:** automatyczne przeładowanie przy zmianie pliku zewnętrznie
- **Ochrona przed pętlą:** ignoruje zmiany przez 3 sekundy po zapisie

---

## 6. Dostęp w kodzie

### 6.1 AcSettingsHolder

**Plik:** `AcManager.Tools\Helpers\AcSettings\AcSettingsHolder.cs`

**Singleton z dostępem do ustawień:**
```csharp
// Pobranie instancji
var video = AcSettingsHolder.Video;

// Ustawienie trybu wyświetlania
video.CameraMode = video.CameraModes.FirstOrDefault(x => x.Value == "TRIPLE");

// Ustawienie rozdzielczości
video.Resolution = video.Resolutions.FirstOrDefault(x => x.Width == 1920 && x.Height == 1080);

// Automatyczny zapis przy zmianie
```

### 6.2 Właściwości VideoSettings

**Tryb wyświetlania:**
```csharp
public SettingEntry CameraMode { get; set; }
public BetterObservableCollection<SettingEntry> CameraModes { get; }
```

**Rozdzielczość:**
```csharp
public ResolutionEntry Resolution { get; set; }
public ResolutionEntry[] Resolutions { get; }
public ResolutionEntry CustomResolution { get; }
```

---

## 7. Ważne uwagi

### 7.1 Kolejność zapisu

- Zapisuje tylko `video.ini`
- Nie zapisuje presetów (w przeciwieństwie do ControlsSettings)

### 7.2 Backup

⚠️ **Plik jest nadpisywany przez Content Manager:**
- `{Documents}\Assetto Corsa\cfg\video.ini`

✅ **Warto backupować:**
- Przed większymi zmianami
- Przed aktualizacją CM

### 7.3 Synchronizacja

**Content Manager:**
- Zapisuje ustawienia do pliku
- Obserwuje zmiany w pliku (FileSystemWatcher)
- Automatycznie przeładowuje przy zmianie zewnętrznej

**Assetto Corsa:**
- Czyta plik przy starcie
- Nie czyta zmian w trakcie działania (wymaga restartu)

### 7.4 Wymagania

**Rozdzielczość:**
- Wymaga restartu gry, aby zostać zastosowana
- W trybie pełnoekranowym musi być zgodna z dostępnymi rozdzielczościami systemowymi

**Tryb wyświetlania:**
- Wymaga restartu gry
- VR wymaga odpowiednich sterowników (Oculus/SteamVR)

---

## 8. Przykłady użycia

### 8.1 Zmiana trybu na Triple Screen

```csharp
var video = AcSettingsHolder.Video;
var tripleMode = video.CameraModes.FirstOrDefault(x => x.Value == "TRIPLE");
if (tripleMode != null) {
    video.CameraMode = tripleMode;
    // Automatyczny zapis
}
```

### 8.2 Zmiana rozdzielczości

```csharp
var video = AcSettingsHolder.Video;
var resolution = video.Resolutions.FirstOrDefault(x => 
    x.Width == 1920 && x.Height == 1080 && x.Framerate == 60);
if (resolution != null) {
    video.Resolution = resolution;
    // Automatyczny zapis
}
```

### 8.3 Ustawienie niestandardowej rozdzielczości

```csharp
var video = AcSettingsHolder.Video;
video.CustomResolution.Width = 2560;
video.CustomResolution.Height = 1440;
video.CustomResolution.Framerate = 144;
video.Resolution = video.CustomResolution;
// Automatyczny zapis
```

---

## 9. Podsumowanie

### 9.1 Plik używany

| Plik | Lokalizacja | Opis |
|------|-------------|------|
| `video.ini` | `{Documents}\Assetto Corsa\cfg\` | Ustawienia wideo |

### 9.2 Sekcje wideo.ini

| Sekcja | Klucz | Opis |
|--------|-------|------|
| `[VIDEO]` | `WIDTH`, `HEIGHT`, `REFRESH`, `INDEX` | Rozdzielczość |
| `[REFRESH]` | `VALUE` | Częstotliwość odświeżania |
| `[CAMERA]` | `MODE` | Tryb wyświetlania (DEFAULT/TRIPLE/OCULUS/OPENVR) |

### 9.3 Proces zapisu

1. **Użytkownik zmienia ustawienie** w UI
2. **ViewModel** aktualizuje właściwość
3. **OnPropertyChanged()** wywołuje `Save()`
4. **SetToIni()** ustawia wartości w `IniFile`
5. **Ini.Save()** zapisuje do pliku
6. **Gra czyta plik** przy starcie

### 9.4 Proces odczytu

1. **VideoSettings** konstruktor ładuje plik
2. **LoadFromIni()** parsuje wartości
3. **Właściwości** są ustawiane w klasie
4. **UI** jest aktualizowane przez DataBinding

---

## 10. Ważne klasy

### 10.1 VideoSettings

**Plik:** `AcManager.Tools\Helpers\AcSettings\VideoSettings.cs`

**Główne właściwości:**
- `CameraMode` - aktualny tryb wyświetlania
- `CameraModes` - lista dostępnych trybów
- `Resolution` - aktualna rozdzielczość
- `Resolutions` - lista dostępnych rozdzielczości
- `CustomResolution` - niestandardowa rozdzielczość

### 10.2 ResolutionEntry

**Plik:** `AcManager.Tools\Helpers\AcSettings\VideoSettings.cs` (linia 24-100)

**Właściwości:**
- `Width` - szerokość
- `Height` - wysokość
- `Framerate` - częstotliwość odświeżania
- `Index` - indeks na liście

### 10.3 Strony UI

**Grafika:**
- `AcManager\Pages\AcSettings\AcSettingsVideo.xaml`
- `AcManager\Pages\AcSettings\AcSettingsVideo.xaml.cs`

---

## Podsumowanie końcowe

Content Manager zapisuje ustawienia wideo **bezpośrednio do pliku konfiguracyjnego Assetto Corsa**:

1. **video.ini** - wszystkie ustawienia wideo (tryb wyświetlania, rozdzielczość, efekty)

**Proces:**
- Zmiana w UI → ViewModel → Save() → SetToIni() → Zapis do pliku
- Gra czyta plik przy starcie i stosuje ustawienia

**Automatyzacja:**
- Automatyczny zapis przy zmianie (z opóźnieniem 500ms)
- Automatyczne przeładowanie przy zmianie pliku (FileSystemWatcher)
- Ochrona przed pętlą zapisu/odczytu

**Lokalizacja:**
- `{Documents}\Assetto Corsa\cfg\video.ini`

**Tryby wyświetlania:**
- `DEFAULT` = Single screen
- `TRIPLE` = Triple screen
- `OCULUS` = Oculus Rift VR
- `OPENVR` = OpenVR (SteamVR)

**Rozdzielczość:**
- `WIDTH` = szerokość (int)
- `HEIGHT` = wysokość (int)
- `REFRESH` = częstotliwość odświeżania (int)
- `INDEX` = indeks rozdzielczości (int)

