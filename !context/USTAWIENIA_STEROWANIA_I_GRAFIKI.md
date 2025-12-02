# Ustawianie bindów i ustawień graficznych w grze

## Przegląd

Content Manager zapisuje ustawienia sterowania (bindy) i grafiki bezpośrednio do plików konfiguracyjnych Assetto Corsa. Gra czyta te pliki przy starcie i automatycznie stosuje ustawienia.

---

## 1. Pliki konfiguracyjne

### 1.1 Sterowanie (Controls)

**Plik:** `{Documents}\Assetto Corsa\cfg\controls.ini`

**Lokalizacja w kodzie:**
- `AcTools\Utils\AcPaths.cs` → `GetCfgControlsFilename()`
- `AcManager.Tools\Helpers\AcSettings\ControlsSettings.cs`

**Zawartość:**
- Bindy klawiatury
- Bindy koła kierowniczego
- Bindy kontrolera (gamepad)
- Ustawienia Force Feedback
- Ustawienia H-shifter'a
- Ustawienia zaawansowane

### 1.2 Grafika (Video)

**Plik:** `{Documents}\Assetto Corsa\cfg\video.ini`

**Lokalizacja w kodzie:**
- `AcTools\Utils\AcPaths.cs` → `GetCfgVideoFilename()`
- `AcManager.Tools\Helpers\AcSettings\VideoSettings.cs`

**Zawartość:**
- Rozdzielczość
- Tryb pełnoekranowy
- VSync
- Anti-aliasing
- Anisotropic filtering
- Shadow map size
- Post-processing
- Efekty (motion blur, smoke, etc.)

---

## 2. Jak działa zapisywanie

### 2.1 Architektura

**Klasa bazowa:** `IniSettings` (`AcManager.Tools\Helpers\AcSettings\IniSettings.cs`)

**Klasy dziedziczące:**
- `ControlsSettings` - sterowanie
- `VideoSettings` - grafika

**Proces:**
```
Użytkownik zmienia ustawienie w UI
    ↓
ViewModel zmienia właściwość
    ↓
OnPropertyChanged() → Save()
    ↓
SetToIni() - ustawia wartości w IniFile
    ↓
Ini.Save(Filename) - zapisuje do pliku
    ↓
Gra czyta plik przy starcie
```

### 2.2 IniSettings - klasa bazowa

**Plik:** `AcManager.Tools\Helpers\AcSettings\IniSettings.cs`

**Konstruktor:**
```csharp
protected IniSettings(string name, bool reload = true, bool systemConfig = false) {
    var directory = systemConfig 
        ? AcPaths.GetSystemCfgDirectory(AcRootDirectory.Instance.RequireValue)
        : AcPaths.GetDocumentsCfgDirectory();
    
    Filename = Path.Combine(directory, name + ".ini");
    // name = "controls" → controls.ini
    // name = "video" → video.ini
    
    if (reload) {
        Reload();  // Ładuje plik przy starcie
    }
    
    // FileSystemWatcher - obserwuje zmiany w pliku
    var watcher = GetWatcher(directory);
    watcher.Changed += OnChanged;  // Automatyczne przeładowanie przy zmianie
}
```

**Metoda Save():**
```csharp
protected virtual async void Save() {
    if (IsSaving || IsLoading || Ini == null) return;
    
    IsSaving = true;
    await Task.Delay(500);  // Opóźnienie, aby uniknąć zbyt częstego zapisu
    
    try {
        SetToIni();  // Ustawia wartości w IniFile
        IgnoreChangesForAWhile();
        Ini.Save(Filename);  // Zapisuje do pliku
    } catch (Exception e) {
        // Obsługa błędów
    } finally {
        IsSaving = false;
    }
}
```

**Metoda SaveImmediately():**
```csharp
public void SaveImmediately() {
    try {
        SetToIni();
        IgnoreChangesForAWhile();
        Ini.Save(Filename);  // Zapis natychmiastowy (bez opóźnienia)
    } catch (Exception e) {
        // Obsługa błędów
    }
}
```

---

## 3. ControlsSettings - sterowanie

### 3.1 Inicjalizacja

**Plik:** `AcManager.Tools\Helpers\AcSettings\ControlsSettings.cs`

```csharp
internal ControlsSettings() : base("controls", false) {
    // base("controls") → tworzy ścieżkę do controls.ini
    SetCanSave(false);  // Początkowo wyłączone zapisywanie
    // ...
}
```

**Lokalizacja pliku:**
- `{Documents}\Assetto Corsa\cfg\controls.ini`

### 3.2 Zapisywanie ustawień (SetToIni)

**Metoda:** `SetToIni()` (linia 1668-1750)

**Co jest zapisywane:**

#### Sekcja [HEADER]
```csharp
Ini["HEADER"].Set("INPUT_METHOD", InputMethod);
// INPUT_METHOD = 0 (keyboard), 1 (wheel), 2 (controller)
```

#### Sekcja [CONTROLLERS]
```csharp
Ini["CONTROLLERS"] = Devices
    .Select(x => (IDirectInputDevice)x)
    .Aggregate(new IniFileSection(null), (s, d, i) => {
        s.Set("CON" + d.Index, d.DisplayName);  // Nazwa kontrolera
        s.Set("PGUID" + d.Index, d.ProductId);  // GUID produktu
        return s;
    });
```

#### Sekcja [STEER]
```csharp
Ini["STEER"].Set("DEBOUNCING_MS", DebouncingInterval);
Ini["STEER"].Set("SCALE", WheelSteerScale);
Ini["STEER"].Set("FF_GAIN", WheelFfbGain);
Ini["STEER"].Set("FILTER_FF", WheelFfbFilter);
```

#### Sekcja [KEYBOARD]
```csharp
Ini["KEYBOARD"].Set("STEERING_SPEED", KeyboardSteeringSpeed);
Ini["KEYBOARD"].Set("STEERING_OPPOSITE_DIRECTION_SPEED", KeyboardOppositeLockSpeed);
Ini["KEYBOARD"].Set("STEER_RESET_SPEED", KeyboardReturnRate);
Ini["KEYBOARD"].Set("MOUSE_STEER", KeyboardMouseSteering);
```

#### Sekcja [X360] (kontroler)
```csharp
Ini["X360"].Set("STEER_THUMB", ControllerSteeringStick?.Id);
Ini["X360"].Set("STEER_GAMMA", ControllerSteeringGamma);
Ini["X360"].Set("STEER_FILTER", ControllerSteeringFilter);
Ini["X360"].Set("SPEED_SENSITIVITY", ControllerSpeedSensitivity);
Ini["X360"].Set("RUMBLE_INTENSITY", ControllerRumbleIntensity);
```

#### Bindy przycisków
```csharp
foreach (var entry in Entries) {
    entry.Save(Ini);  // Zapisuje bindy dla każdego przycisku
}
```

**Przykład struktury controls.ini:**
```ini
[HEADER]
INPUT_METHOD = 1

[CONTROLLERS]
CON0 = Logitech G29
PGUID0 = {C24F046D-0000-0000-0000-504944564944}

[STEER]
JOY = 0
SCALE = 1.0
DEBOUNCING_MS = 0
FF_GAIN = 1.0
FILTER_FF = 0.0

[THROTTLE]
JOY = 0
AXLE = 0

[BRAKES]
JOY = 0
AXLE = 1

[CLUTCH]
JOY = 0
AXLE = 2

[KEYBOARD]
STEERING_SPEED = 0.5
STEERING_OPPOSITE_DIRECTION_SPEED = 0.5
STEER_RESET_SPEED = 0.5

[FF_TWEAKS]
MIN_FF = 0.0
CENTER_BOOST_GAIN = 0.0
CENTER_BOOST_RANGE = 0.0

[FF_ENHANCEMENT]
CURBS = 0.0
ROAD = 0.0
SLIPS = 0.0
ABS = 0.0
```

### 3.3 Ładowanie ustawień (LoadFromIni)

**Metoda:** `LoadFromIni()` (linia 1516-1637)

**Proces:**
1. Odczytuje plik `controls.ini`
2. Parsuje sekcje i wartości
3. Ustawia właściwości w `ControlsSettings`
4. Aktualizuje UI przez DataBinding

**Przykład:**
```csharp
protected override void LoadFromIni() {
    var section = Ini["STEER"];
    WheelSteerScale = section.GetDouble("SCALE", 1.0);
    WheelFfbGain = section.GetDouble("FF_GAIN", 1.0).ToPercentage();
    
    section = Ini["KEYBOARD"];
    KeyboardSteeringSpeed = section.GetDouble("STEERING_SPEED", 0.5);
    // ...
}
```

### 3.4 Kiedy jest zapisywane?

**Automatycznie:**
- Gdy użytkownik zmienia ustawienie w UI
- `OnPropertyChanged()` wywołuje `Save()`
- Opóźnienie 500ms (debounce) - zapisuje po zakończeniu zmian

**Ręcznie:**
- `SaveImmediately()` - zapis natychmiastowy
- `SavePreset()` - zapis jako preset

---

## 4. VideoSettings - grafika

### 4.1 Inicjalizacja

**Plik:** `AcManager.Tools\Helpers\AcSettings\VideoSettings.cs`

```csharp
internal VideoSettings() : base(@"video") {
    // base("video") → tworzy ścieżkę do video.ini
    CustomResolution.PropertyChanged += (sender, args) => Save();
    // Automatyczny zapis przy zmianie rozdzielczości
}
```

**Lokalizacja pliku:**
- `{Documents}\Assetto Corsa\cfg\video.ini`

### 4.2 Zapisywanie ustawień (SetToIni)

**Metoda:** `SetToIni()` (linia 761-831)

**Co jest zapisywane:**

#### Sekcja [VIDEO]
```csharp
var section = ini["VIDEO"];
section.Set("WIDTH", Resolution.Width);
section.Set("HEIGHT", Resolution.Height);
section.Set("REFRESH", Resolution.Framerate);
section.Set("FULLSCREEN", Fullscreen);
section.Set("VSYNC", VerticalSynchronization);
section.Set("AASAMPLES", AntiAliasingLevel);
section.Set("ANISOTROPIC", AnisotropicLevel);
section.Set("SHADOW_MAP_SIZE", ShadowMapSize);
section.Set("FPS_CAP_MS", FramerateLimitEnabled ? 1e3 / FramerateLimit : 0d);
```

#### Sekcja [CAMERA]
```csharp
ini["CAMERA"].Set("MODE", CameraMode);
// MODE = DEFAULT, TRIPLE, OCULUS, OPENVR
```

#### Sekcja [ASSETTOCORSA]
```csharp
section = ini["ASSETTOCORSA"];
section.Set("HIDE_ARMS", HideArms);
section.Set("HIDE_STEER", HideSteeringWheel);
section.Set("LOCK_STEER", LockSteeringWheel);
section.Set("WORLD_DETAIL", WorldDetails);
```

#### Sekcja [EFFECTS]
```csharp
ini["EFFECTS"].Set("MOTION_BLUR", MotionBlur);
ini["EFFECTS"].Set("RENDER_SMOKE_IN_MIRROR", SmokeInMirrors);
ini["EFFECTS"].Set("SMOKE", SmokeLevel);
```

#### Sekcja [POST_PROCESS]
```csharp
section = ini["POST_PROCESS"];
section.Set("ENABLED", PostProcessing);
section.Set("QUALITY", PostProcessingQuality);
section.Set("FILTER", PostProcessingFilter);
section.Set("GLARE", GlareQuality);
section.Set("DOF", DepthOfFieldQuality);
section.Set("RAYS_OF_GOD", Sunrays);
section.Set("HEAT_SHIMMER", HeatShimmering);
section.Set("FXAA", Fxaa);
```

#### Sekcja [MIRROR]
```csharp
ini["MIRROR"].Set("HQ", MirrorsHighQuality);
ini["MIRROR"].Set("SIZE", MirrorsResolution);
```

**Przykład struktury video.ini:**
```ini
[VIDEO]
WIDTH = 1920
HEIGHT = 1080
REFRESH = 60
FULLSCREEN = 1
VSYNC = 1
AASAMPLES = 4
ANISOTROPIC = 16
SHADOW_MAP_SIZE = 1024
FPS_CAP_MS = 0

[CAMERA]
MODE = DEFAULT

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

### 4.3 Ładowanie ustawień (LoadFromIni)

**Metoda:** `LoadFromIni()` (linia 692-760)

**Proces:**
1. Odczytuje plik `video.ini`
2. Parsuje sekcje i wartości
3. Ustawia właściwości w `VideoSettings`
4. Aktualizuje UI przez DataBinding

---

## 5. Przepływ danych - przykład

### 5.1 Zmiana bindy klawiatury

```
1. Użytkownik klika przycisk w UI (AcSettingsControls)
   ↓
2. ViewModel zmienia właściwość
   KeyboardButtonEntry.Key = newKey
   ↓
3. OnPropertyChanged() wywołuje Save()
   ↓
4. ControlsSettings.Save()
   ↓
5. ControlsSettings.SetToIni()
   - Ustawia wartości w IniFile
   - entry.Save(Ini) dla każdego przycisku
   ↓
6. Ini.Save(Filename)
   - Zapisuje do: {Documents}\Assetto Corsa\cfg\controls.ini
   ↓
7. Gra (acs.exe) czyta controls.ini przy starcie
   ↓
8. Nowy bind jest aktywny w grze
```

### 5.2 Zmiana rozdzielczości

```
1. Użytkownik zmienia rozdzielczość w UI (AcSettingsVideo)
   ↓
2. ViewModel zmienia właściwość
   VideoSettings.Resolution = newResolution
   ↓
3. OnPropertyChanged() wywołuje Save()
   ↓
4. VideoSettings.Save()
   ↓
5. VideoSettings.SetToIni()
   - Ustawia WIDTH, HEIGHT, REFRESH w sekcji [VIDEO]
   ↓
6. Ini.Save(Filename)
   - Zapisuje do: {Documents}\Assetto Corsa\cfg\video.ini
   ↓
7. Gra (acs.exe) czyta video.ini przy starcie
   ↓
8. Nowa rozdzielczość jest zastosowana
```

---

## 6. FileSystemWatcher - automatyczne przeładowanie

### 6.1 Obserwowanie zmian

**IniSettings** używa `FileSystemWatcher` do obserwowania zmian w plikach:

```csharp
var watcher = GetWatcher(directory);
watcher.Changed += OnChanged;  // Plik został zmieniony
watcher.Created += OnChanged;  // Plik został utworzony
watcher.Deleted += OnChanged;  // Plik został usunięty
watcher.Renamed += OnRenamed;  // Plik został przemianowany
```

**Efekt:**
- Jeśli plik `controls.ini` lub `video.ini` zostanie zmieniony zewnętrznie (np. przez grę)
- Content Manager automatycznie przeładuje ustawienia
- UI zostanie zaktualizowane

### 6.2 Ochrona przed pętlą

**Mechanizm:**
- `IgnoreChangesForAWhile()` - ignoruje zmiany przez 3 sekundy po zapisie
- Zapobiega pętli: zapis → FileSystemWatcher → przeładowanie → zapis

---

## 7. Presety

### 7.1 Presety sterowania

**Katalog:** `{Documents}\Assetto Corsa\cfg\controllers\`

**Zapisywanie:**
```csharp
public void SavePreset(string filename) {
    CurrentPresetFilename = filename;
    CurrentPresetChanged = false;
    SaveImmediately();
    Ini.Save(filename);  // Zapisuje jako preset
}
```

**Struktura:**
- `{Documents}\Assetto Corsa\cfg\controllers\{preset_name}.ini`

### 7.2 Presety grafiki

**Zapisywanie:**
- Przez `VideoSettings` (dziedziczy po `IniPresetableSettings`)
- Presety są zapisywane w katalogu presetów

---

## 8. Przykłady kodu

### 8.1 Zmiana bindy programatycznie

```csharp
// Pobranie ustawień
var controls = AcSettingsHolder.Controls;

// Zmiana bindy dla gazu
var throttleEntry = controls.WheelAxleEntries.FirstOrDefault(x => x.Id == "THROTTLE");
if (throttleEntry != null) {
    // Ustawienie nowego inputu
    throttleEntry.Input = newInput;
    // Automatyczny zapis przez OnPropertyChanged()
}
```

### 8.2 Zmiana rozdzielczości programatycznie

```csharp
// Pobranie ustawień
var video = AcSettingsHolder.Video;

// Zmiana rozdzielczości
video.Resolution = new ResolutionEntry {
    Width = 2560,
    Height = 1440,
    Framerate = 144
};
// Automatyczny zapis przez OnPropertyChanged()
```

### 8.3 Zapis natychmiastowy

```csharp
// Zapis bez opóźnienia
AcSettingsHolder.Controls.SaveImmediately();
AcSettingsHolder.Video.SaveImmediately();
```

---

## 9. Struktura plików - pełne ścieżki

### 9.1 controls.ini

**Lokalizacja:**
- `C:\Users\{USERNAME}\Documents\Assetto Corsa\cfg\controls.ini`

**Główne sekcje:**
- `[HEADER]` - metoda wejścia
- `[CONTROLLERS]` - lista kontrolerów
- `[STEER]` - kierownica
- `[THROTTLE]` - gaz
- `[BRAKES]` - hamulce
- `[CLUTCH]` - sprzęgło
- `[KEYBOARD]` - ustawienia klawiatury
- `[X360]` - ustawienia kontrolera
- `[FF_TWEAKS]` - Force Feedback
- `[FF_ENHANCEMENT]` - ulepszenia FFB
- `[__EXTRA_CM]` - dodatkowe ustawienia CM

### 9.2 video.ini

**Lokalizacja:**
- `C:\Users\{USERNAME}\Documents\Assetto Corsa\cfg\video.ini`

**Główne sekcje:**
- `[VIDEO]` - podstawowe ustawienia wideo
- `[CAMERA]` - tryb kamery
- `[ASSETTOCORSA]` - ustawienia gry
- `[EFFECTS]` - efekty
- `[POST_PROCESS]` - post-processing
- `[MIRROR]` - lustra
- `[REFRESH]` - częstotliwość odświeżania
- `[TRIPLE_BUFFER]` - potrójne buforowanie

### 9.3 Presety kontrolerów

**Katalog:**
- `C:\Users\{USERNAME}\Documents\Assetto Corsa\cfg\controllers\`

**Pliki:**
- `{GUID}.ini` - preset dla konkretnego kontrolera (GUID)
- `{preset_name}.ini` - preset użytkownika

---

## 10. Kiedy gra czyta te pliki?

### 10.1 Przy starcie gry

**Assetto Corsa (`acs.exe`):**
1. Uruchamia się
2. Sprawdza katalog `{Documents}\Assetto Corsa\cfg\`
3. Czyta `controls.ini` i ustawia bindy
4. Czyta `video.ini` i ustawia grafikę
5. Stosuje ustawienia

### 10.2 W trakcie gry

**Niektóre ustawienia** mogą być zmieniane w trakcie gry:
- Rozdzielczość (wymaga restartu)
- VSync (może być zmieniane)
- Post-processing (może być zmieniane)

**Bindy** wymagają restartu gry, aby zostały zastosowane.

---

## 11. Ważne uwagi

### 11.1 Kolejność zapisu

**ControlsSettings:**
- Zapisuje `controls.ini` główny
- Zapisuje presety kontrolerów (`{GUID}.ini`)

**VideoSettings:**
- Zapisuje tylko `video.ini`

### 11.2 Backup

⚠️ **Te pliki są nadpisywane przez Content Manager:**
- `{Documents}\Assetto Corsa\cfg\controls.ini`
- `{Documents}\Assetto Corsa\cfg\video.ini`

✅ **Warto backupować:**
- Presety kontrolerów: `{Documents}\Assetto Corsa\cfg\controllers\`

### 11.3 Synchronizacja

**Content Manager:**
- Zapisuje ustawienia do plików
- Obserwuje zmiany w plikach (FileSystemWatcher)
- Automatycznie przeładowuje przy zmianie

**Gra:**
- Czyta pliki tylko przy starcie
- Nie zapisuje zmian w trakcie gry (dla większości ustawień)

---

## 12. Przykład kompletnego przepływu

### 12.1 Zmiana bindy gazu na klawiaturze

```
1. Użytkownik otwiera: AcSettingsControls → Keyboard
   ↓
2. Kliknie przycisk "Throttle" i naciśnie nowy klawisz (np. W)
   ↓
3. KeyboardButtonEntry.Key = Key.W
   ↓
4. OnPropertyChanged() → Save()
   ↓
5. ControlsSettings.SetToIni()
   Ini["KEYBOARD"].Set("GAS", "W")
   ↓
6. Ini.Save("C:\Users\Jan\Documents\Assetto Corsa\cfg\controls.ini")
   ↓
7. Plik zapisany na dysku
   ↓
8. Użytkownik uruchamia grę
   ↓
9. Gra czyta controls.ini
   ↓
10. Bind W = gaz jest aktywny
```

### 12.2 Zmiana rozdzielczości na 4K

```
1. Użytkownik otwiera: AcSettingsVideo
   ↓
2. Wybiera rozdzielczość: 3840x2160@60Hz
   ↓
3. VideoSettings.Resolution = new ResolutionEntry { Width=3840, Height=2160, Framerate=60 }
   ↓
4. OnPropertyChanged() → Save()
   ↓
5. VideoSettings.SetToIni()
   Ini["VIDEO"].Set("WIDTH", 3840)
   Ini["VIDEO"].Set("HEIGHT", 2160)
   Ini["VIDEO"].Set("REFRESH", 60)
   ↓
6. Ini.Save("C:\Users\Jan\Documents\Assetto Corsa\cfg\video.ini")
   ↓
7. Plik zapisany na dysku
   ↓
8. Użytkownik uruchamia grę
   ↓
9. Gra czyta video.ini
   ↓
10. Gra uruchamia się w rozdzielczości 4K
```

---

## 13. Kody źródłowe - lokalizacje

### 13.1 ControlsSettings

**Plik:** `AcManager.Tools\Helpers\AcSettings\ControlsSettings.cs`

**Kluczowe metody:**
- `LoadFromIni()` (linia 1516) - ładowanie z pliku
- `SetToIni()` (linia 1668) - zapis do pliku
- `Save()` (linia 753) - zapis z opóźnieniem
- `SaveImmediately()` (dziedziczone z IniSettings) - zapis natychmiastowy
- `SavePreset()` (linia 1752) - zapis jako preset

### 13.2 VideoSettings

**Plik:** `AcManager.Tools\Helpers\AcSettings\VideoSettings.cs`

**Kluczowe metody:**
- `LoadFromIni()` (linia 692) - ładowanie z pliku
- `SetToIni()` (linia 761) - zapis do pliku
- `Save()` (dziedziczone z IniSettings) - zapis z opóźnieniem

### 13.3 IniSettings (bazowa)

**Plik:** `AcManager.Tools\Helpers\AcSettings\IniSettings.cs`

**Kluczowe metody:**
- Konstruktor (linia 37) - inicjalizacja i ścieżka pliku
- `Save()` (linia 148) - zapis z opóźnieniem 500ms
- `SaveImmediately()` (linia 174) - zapis natychmiastowy
- `Reload()` (linia 82) - przeładowanie z pliku
- `LoadFromIni()` (abstrakcyjna) - implementowana w klasach dziedziczących

### 13.4 AcPaths

**Plik:** `AcTools\Utils\AcPaths.cs`

**Metody:**
- `GetCfgControlsFilename()` (linia 58) → `{Documents}\Assetto Corsa\cfg\controls.ini`
- `GetCfgVideoFilename()` (linia 48) → `{Documents}\Assetto Corsa\cfg\video.ini`
- `GetDocumentsCfgDirectory()` (linia 27) → `{Documents}\Assetto Corsa\cfg`

---

## 14. Przykładowe struktury plików

### 14.1 controls.ini - pełny przykład

```ini
[HEADER]
INPUT_METHOD = 1

[CONTROLLERS]
CON0 = Logitech G29
PGUID0 = {C24F046D-0000-0000-0000-504944564944}
__IGUID0 = {C24F046D-0000-0000-0000-504944564944}

[STEER]
JOY = 0
AXLE = 0
SCALE = 1.0
DEBOUNCING_MS = 0
FF_GAIN = 1.0
FILTER_FF = 0.0

[THROTTLE]
JOY = 0
AXLE = 1

[BRAKES]
JOY = 0
AXLE = 2

[CLUTCH]
JOY = 0
AXLE = 3

[KEYBOARD]
STEERING_SPEED = 0.5
STEERING_OPPOSITE_DIRECTION_SPEED = 0.5
STEER_RESET_SPEED = 0.5
MOUSE_STEER = 0
MOUSE_ACCELERATOR_BRAKE = 0
MOUSE_SPEED = 0.5

[FF_TWEAKS]
MIN_FF = 0.0
CENTER_BOOST_GAIN = 0.0
CENTER_BOOST_RANGE = 0.0

[FF_ENHANCEMENT]
CURBS = 0.0
ROAD = 0.0
SLIPS = 0.0
ABS = 0.0

[FF_ENHANCEMENT_2]
UNDERSTEER = 0

[FF_SKIP_STEPS]
VALUE = 0

[ADVANCED]
COMBINE_WITH_KEYBOARD_CONTROL = 0

[SHIFTER]
ACTIVE = 0
JOY = -1

[__EXTRA_CM]
AUTO_ADJUST_SCALE = 0
DELAY_SPECIFIC_SYSTEM_COMMANDS = 2
SHOW_SYSTEM_DELAYS = 1
SYSTEM_IGNORE_POV_IN_PITS = 1
HARDWARE_LOCK = 0

[__LAUNCHER_CM]
PRESET_NAME = 
PRESET_CHANGED = 0
```

### 14.2 video.ini - pełny przykład

```ini
[VIDEO]
WIDTH = 1920
HEIGHT = 1080
REFRESH = 60
FULLSCREEN = 1
VSYNC = 1
AASAMPLES = 4
ANISOTROPIC = 16
SHADOW_MAP_SIZE = 1024
FPS_CAP_MS = 0
INDEX = 0

[REFRESH]
VALUE = 60

[CAMERA]
MODE = DEFAULT

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

[SATURATION]
LEVEL = 1.0
```

---

## 15. Podsumowanie

### 15.1 Pliki używane

| Plik | Lokalizacja | Opis |
|------|-------------|------|
| `controls.ini` | `{Documents}\Assetto Corsa\cfg\` | Ustawienia sterowania |
| `video.ini` | `{Documents}\Assetto Corsa\cfg\` | Ustawienia grafiki |
| `{GUID}.ini` | `{Documents}\Assetto Corsa\cfg\controllers\` | Presety kontrolerów |

### 15.2 Proces zapisu

1. **Użytkownik zmienia ustawienie** w UI
2. **ViewModel** aktualizuje właściwość
3. **OnPropertyChanged()** wywołuje `Save()`
4. **SetToIni()** ustawia wartości w `IniFile`
5. **Ini.Save()** zapisuje do pliku
6. **Gra czyta plik** przy starcie

### 15.3 Proces odczytu

1. **IniSettings** konstruktor ładuje plik
2. **LoadFromIni()** parsuje wartości
3. **Właściwości** są ustawiane w klasie
4. **UI** jest aktualizowane przez DataBinding

### 15.4 Automatyczne przeładowanie

- **FileSystemWatcher** obserwuje zmiany w plikach
- Jeśli plik zostanie zmieniony zewnętrznie, CM automatycznie przeładuje
- Ochrona przed pętlą: ignoruje zmiany przez 3 sekundy po zapisie

---

## 16. Ważne klasy

### 16.1 AcSettingsHolder

**Plik:** `AcManager.Tools\Helpers\AcSettings\AcSettingsHolder.cs`

**Singleton z dostępem do ustawień:**
```csharp
// Sterowanie
var controls = AcSettingsHolder.Controls;
controls.WheelSteerScale = 1.5;
// Automatyczny zapis

// Grafika
var video = AcSettingsHolder.Video;
video.Resolution = newResolution;
// Automatyczny zapis
```

### 16.2 Strony UI

**Sterowanie:**
- `AcManager\Pages\AcSettings\AcSettingsControls.xaml`
- `AcManager\Pages\AcSettings\AcSettingsControls_Keyboard.xaml`
- `AcManager\Pages\AcSettings\AcSettingsControls_Wheel.xaml`

**Grafika:**
- `AcManager\Pages\AcSettings\AcSettingsVideo.xaml`

---

## Podsumowanie końcowe

Content Manager zapisuje ustawienia sterowania i grafiki **bezpośrednio do plików konfiguracyjnych Assetto Corsa**:

1. **controls.ini** - wszystkie bindy i ustawienia sterowania
2. **video.ini** - wszystkie ustawienia grafiki

**Proces:**
- Zmiana w UI → ViewModel → Save() → SetToIni() → Zapis do pliku
- Gra czyta pliki przy starcie i stosuje ustawienia

**Automatyzacja:**
- Automatyczny zapis przy zmianie (z opóźnieniem 500ms)
- Automatyczne przeładowanie przy zmianie pliku (FileSystemWatcher)
- Ochrona przed pętlą zapisu/odczytu

**Lokalizacje:**
- `{Documents}\Assetto Corsa\cfg\controls.ini`
- `{Documents}\Assetto Corsa\cfg\video.ini`
- `{Documents}\Assetto Corsa\cfg\controllers\{GUID}.ini` (presety)


