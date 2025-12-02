# Ładowanie Custom Shaders Patch (CSP) do gry

## Przegląd

Custom Shaders Patch (CSP) to modyfikacja Assetto Corsa, która dodaje zaawansowane efekty graficzne, fizykę i funkcje. Content Manager automatycznie zarządza instalacją i ładowaniem CSP.

---

## 1. Struktura instalacji CSP

### 1.1. Lokalizacja plików CSP

CSP jest instalowany w katalogu głównym Assetto Corsa:

```
{AC_ROOT}\
├── dwrite.dll              # [WYMAGANY] Główny plik DLL CSP (ładowany przez grę)
└── extension\              # [WYMAGANY] Folder z plikami CSP
    ├── config\             # Pliki konfiguracyjne CSP
    │   ├── general.ini     # Główny plik konfiguracyjny
    │   ├── data_manifest.ini  # Manifest z wersją CSP
    │   ├── weather_fx.ini  # Konfiguracja Weather FX
    │   ├── rain_fx.ini     # Konfiguracja Rain FX
    │   └── ... (inne pliki .ini)
    ├── lua\                # Skrypty Lua
    ├── shaders\            # Shadery
    ├── textures\           # Tekstury
    └── ... (inne pliki CSP)
```

**Przykładowa ścieżka:**
```
C:\Program Files (x86)\Steam\steamapps\common\assettocorsa\
├── dwrite.dll
└── extension\
    └── config\
        └── general.ini
```

### 1.2. Główne pliki CSP

#### dwrite.dll
- **Lokalizacja:** `{AC_ROOT}\dwrite.dll`
- **Rola:** Główny plik DLL, który jest ładowany przez grę przy starcie
- **Jak działa:** Windows automatycznie ładuje `dwrite.dll` gdy gra (`acs.exe`) uruchamia się
- **Weryfikacja:** Jeśli plik istnieje, CSP jest dostępny (ale może być wyłączony w konfiguracji)

#### extension/config/general.ini
- **Lokalizacja:** `{AC_ROOT}\extension\config\general.ini`
- **Rola:** Główny plik konfiguracyjny CSP
- **Kluczowa sekcja:**
  ```ini
  [BASIC]
  ENABLED = 1    # 1 = włączony, 0 = wyłączony
  ```
- **Weryfikacja:** Jeśli `ENABLED = 1`, CSP jest aktywny

#### extension/config/data_manifest.ini
- **Lokalizacja:** `{AC_ROOT}\extension\config\data_manifest.ini`
- **Rola:** Manifest z informacjami o wersji CSP
- **Zawartość:**
  ```ini
  [VERSION]
  SHADERS_PATCH = 0.2.3
  SHADERS_PATCH_BUILD = 2500
  ```

---

## 2. Jak gra wykrywa i ładuje CSP

### 2.1. Proces ładowania

```
1. Gra (acs.exe) uruchamia się
   ↓
2. Windows próbuje załadować biblioteki DLL
   ↓
3. Windows znajduje dwrite.dll w katalogu AC_ROOT
   ↓
4. dwrite.dll jest ładowany do procesu acs.exe
   ↓
5. dwrite.dll sprawdza czy istnieje folder extension/
   ↓
6. Jeśli istnieje, sprawdza extension/config/general.ini
   ↓
7. Czyta wartość [BASIC] ENABLED
   ↓
8. Jeśli ENABLED = 1:
   - CSP jest aktywny
   - Ładuje wszystkie moduły CSP
   - Wczytuje konfigurację z extension/config/
   ↓
9. Jeśli ENABLED = 0 lub brak pliku:
   - CSP jest wyłączony
   - Gra działa normalnie bez CSP
```

### 2.2. Weryfikacja czy CSP jest zainstalowany

**Kod w SettingsShadersPatch.xaml.cs (linia 30-32):**

```csharp
public static bool IsCustomShadersPatchInstalled() {
    return Directory.Exists(Path.Combine(
        AcRootDirectory.Instance.RequireValue, 
        "extension", "config"
    ));
}
```

**Sprawdzenie czy CSP jest aktywny:**

```csharp
// W PatchHelper (zakomentowany kod, ale logika jest używana)
public static bool IsActive() {
    return GetActualConfigValue("general.ini", "BASIC", "ENABLED").As(false);
}
```

---

## 3. Instalacja CSP przez Content Manager

### 3.1. Automatyczna instalacja

Content Manager może automatycznie pobrać i zainstalować CSP:

**Kod w PatchUpdater.cs:**

```csharp
// Pobranie listy dostępnych wersji
var versions = await PatchVersionInfo.GetPatchManifestAsync(cancellationToken);

// Instalacja najnowszej zalecanej wersji
var latestRecommended = versions
    .Where(x => x.Tags?.Contains("recommended") == true)
    .MaxEntry(x => x.Build);

await PatchUpdater.Instance.InstallAsync(latestRecommended, cancellationToken);
```

### 3.2. Instalacja z pliku ZIP

Content Manager może zainstalować CSP z pliku ZIP:

**Kod w ShadersPatchEntry.cs:**

```csharp
// Rozpoznanie CSP w archiwum ZIP
if (directory.HasSubFile(PatchHelper.MainFileName)  // dwrite.dll
    && directory.HasSubDirectory("extension")) {
    
    // To jest CSP
    return new ShadersPatchEntry(...);
}
```

**Proces instalacji:**

1. **Rozpakowanie archiwum:**
   - Sprawdzenie czy zawiera `dwrite.dll` i folder `extension/`
   - Rozpakowanie do katalogu AC_ROOT

2. **Kopiowanie plików:**
   - `dwrite.dll` → `{AC_ROOT}\dwrite.dll`
   - `extension/` → `{AC_ROOT}\extension/`

3. **Tworzenie logu instalacji:**
   - `extension/installed.log` - log zainstalowanych plików

4. **Włączenie CSP:**
   - Automatyczne ustawienie `general.ini [BASIC] ENABLED = 1`

### 3.3. Ręczna instalacja

Jeśli masz plik ZIP z CSP:

1. **Rozpakuj archiwum:**
   - Skopiuj `dwrite.dll` do `{AC_ROOT}\`
   - Skopiuj folder `extension/` do `{AC_ROOT}\`

2. **Włącz CSP:**
   - Otwórz `{AC_ROOT}\extension\config\general.ini`
   - Ustaw `[BASIC] ENABLED = 1`

3. **Weryfikacja:**
   - Uruchom Content Manager
   - Sprawdź czy CSP jest wykryty w ustawieniach

---

## 4. Włączanie/wyłączanie CSP

### 4.1. Przez Content Manager

**Kod w App.xaml.cs (linia 455-475):**

```csharp
// Włączenie CSP
BbCodeBlock.AddLinkCommand(new Uri("cmd://csp/enable"), new SimpleLinkCommand(() => {
    using (var model = PatchSettingsModel.Create()) {
        var item = model.Configs?
            .FirstOrDefault(x => x.FileNameWithoutExtension == "general")?
            .SectionsOwn.GetByIdOrDefault("BASIC")?
            .GetByIdOrDefault("ENABLED");
        if (item != null) {
            item.Value = @"1";  // Włącz
        }
    }
}));

// Wyłączenie CSP
BbCodeBlock.AddLinkCommand(new Uri("cmd://csp/disable"), new SimpleLinkCommand(() => {
    using (var model = PatchSettingsModel.Create()) {
        var item = model.Configs?
            .FirstOrDefault(x => x.FileNameWithoutExtension == "general")?
            .SectionsOwn.GetByIdOrDefault("BASIC")?
            .GetByIdOrDefault("ENABLED");
        if (item != null) {
            item.Value = @"0";  // Wyłącz
        }
    }
}));
```

### 4.2. Ręczne włączenie/wyłączenie

**Edytuj plik:** `{AC_ROOT}\extension\config\general.ini`

**Aby włączyć CSP:**
```ini
[BASIC]
ENABLED = 1
```

**Aby wyłączyć CSP:**
```ini
[BASIC]
ENABLED = 0
```

**Uwaga:** Zmiany są natychmiastowe - następne uruchomienie gry użyje nowych ustawień.

---

## 5. Struktura plików konfiguracyjnych CSP

### 5.1. extension/config/general.ini

Główny plik konfiguracyjny CSP:

```ini
[BASIC]
ENABLED = 1                    # Czy CSP jest włączony (1 = tak, 0 = nie)

[GRAPHICS]
...                            # Ustawienia graficzne

[PHYSICS]
...                            # Ustawienia fizyki

[FEATURES]
...                            # Funkcje CSP
```

### 5.2. extension/config/data_manifest.ini

Manifest z informacjami o wersji:

```ini
[VERSION]
SHADERS_PATCH = 0.2.3          # Wersja CSP
SHADERS_PATCH_BUILD = 2500     # Numer builda

[FEATURES]
CONDITIONS_24H = 1             # Obsługa 24h
SNOW = 1                        # Obsługa śniegu
...                             # Inne funkcje
```

### 5.3. Lokalizacja plików użytkownika

Pliki konfiguracyjne użytkownika (nadpisują domyślne):

**Lokalizacja:** `{Documents}\Assetto Corsa\cfg\extension\`

```
{Documents}\Assetto Corsa\cfg\extension\
├── general.ini                 # Nadpisuje extension/config/general.ini
├── weather_fx.ini             # Nadpisuje extension/config/weather_fx.ini
└── ... (inne pliki .ini)
```

**Priorytet:**
1. Pliki użytkownika: `{Documents}\Assetto Corsa\cfg\extension\*.ini`
2. Pliki domyślne: `{AC_ROOT}\extension\config\*.ini`

---

## 6. Weryfikacja czy CSP działa

### 6.1. W Content Manager

1. **Otwórz ustawienia CSP:**
   - Settings → Custom Shaders Patch
   - Lub użyj skrótu klawiszowego

2. **Sprawdź status:**
   - Jeśli CSP jest zainstalowany, zobaczysz listę konfiguracji
   - Jeśli nie, zobaczysz opcję instalacji

3. **Sprawdź wersję:**
   - W ustawieniach CSP zobaczysz zainstalowaną wersję

### 6.2. W grze

1. **Uruchom grę**
2. **Sprawdź menu:**
   - Jeśli CSP jest aktywny, zobaczysz dodatkowe opcje w menu
   - W ustawieniach grafiki będą dodatkowe opcje CSP

3. **Sprawdź logi:**
   - `{Documents}\Assetto Corsa\logs\custom_shaders_patch.log`
   - Jeśli plik istnieje i zawiera logi, CSP działa

### 6.3. Programowo

**Sprawdzenie czy CSP jest zainstalowany:**

```csharp
var extensionConfigDir = Path.Combine(
    AcRootDirectory.Instance.RequireValue, 
    "extension", "config"
);
bool isInstalled = Directory.Exists(extensionConfigDir);
```

**Sprawdzenie czy CSP jest aktywny:**

```csharp
var generalIniPath = Path.Combine(
    AcRootDirectory.Instance.RequireValue,
    "extension", "config", "general.ini"
);

if (File.Exists(generalIniPath)) {
    var ini = IniFile.Parse(File.ReadAllText(generalIniPath));
    bool isEnabled = ini["BASIC"].GetBool("ENABLED", false);
}
```

**Sprawdzenie wersji CSP:**

```csharp
var manifestPath = Path.Combine(
    AcRootDirectory.Instance.RequireValue,
    "extension", "config", "data_manifest.ini"
);

if (File.Exists(manifestPath)) {
    var manifest = IniFile.Parse(File.ReadAllText(manifestPath));
    var version = manifest["VERSION"].GetNonEmpty("SHADERS_PATCH");
    var build = manifest["VERSION"].GetNonEmpty("SHADERS_PATCH_BUILD");
    Console.WriteLine($"CSP Version: {version}, Build: {build}");
}
```

---

## 7. Aktualizacja CSP

### 7.1. Automatyczna aktualizacja

Content Manager może automatycznie sprawdzać i aktualizować CSP:

**Kod w PatchUpdater.cs:**

```csharp
// Sprawdzenie czy jest dostępna nowa wersja
await PatchUpdater.Instance.CheckAndUpdateIfNeeded();

// Instalacja konkretnej wersji
var versionInfo = PatchUpdater.Instance.Versions
    .FirstOrDefault(x => x.Build == buildNumber);
await PatchUpdater.Instance.InstallAsync(versionInfo, cancellationToken);
```

### 7.2. Ręczna aktualizacja

1. **Pobierz nową wersję CSP:**
   - Ze strony autora lub przez Content Manager

2. **Zainstaluj:**
   - Użyj Content Manager do instalacji z pliku ZIP
   - LUB ręcznie skopiuj pliki (zastąpi stare)

3. **Weryfikacja:**
   - Sprawdź wersję w ustawieniach CSP

---

## 8. Rozwiązywanie problemów

### 8.1. CSP nie jest wykrywany

**Problem:** Content Manager nie wykrywa CSP

**Rozwiązanie:**
1. Sprawdź czy istnieje `{AC_ROOT}\extension\config\`
2. Sprawdź czy istnieje `{AC_ROOT}\dwrite.dll`
3. Sprawdź czy pliki nie są uszkodzone
4. Zainstaluj CSP ponownie

### 8.2. CSP nie ładuje się w grze

**Problem:** CSP jest zainstalowany, ale nie działa w grze

**Rozwiązanie:**
1. Sprawdź `extension/config/general.ini`:
   ```ini
   [BASIC]
   ENABLED = 1
   ```
2. Sprawdź czy `dwrite.dll` nie jest zablokowany przez antywirus
3. Sprawdź logi gry: `{Documents}\Assetto Corsa\logs\custom_shaders_patch.log`
4. Sprawdź czy wersja CSP jest kompatybilna z wersją gry

### 8.3. Błędy podczas instalacji

**Problem:** Błąd podczas instalacji CSP

**Rozwiązanie:**
1. Sprawdź czy masz uprawnienia do zapisu w katalogu AC_ROOT
2. Sprawdź czy gra nie jest uruchomiona
3. Sprawdź czy `dwrite.dll` nie jest używany przez inny proces
4. Spróbuj zainstalować ręcznie

### 8.4. CSP powoduje crash gry

**Problem:** Gra się zawiesza lub crashuje po włączeniu CSP

**Rozwiązanie:**
1. Sprawdź czy wersja CSP jest kompatybilna
2. Wyłącz CSP (`ENABLED = 0`)
3. Sprawdź logi błędów
4. Zainstaluj starszą/nowszą wersję CSP

---

## 9. Ważne uwagi

1. **dwrite.dll jest ładowany automatycznie** - Windows ładuje ten plik DLL gdy gra się uruchamia
2. **ENABLED kontroluje aktywność** - nawet jeśli `dwrite.dll` istnieje, CSP nie działa jeśli `ENABLED = 0`
3. **Pliki użytkownika mają priorytet** - konfiguracja z `{Documents}\Assetto Corsa\cfg\extension\` nadpisuje domyślną
4. **CSP wymaga restartu gry** - zmiany w konfiguracji wymagają ponownego uruchomienia gry
5. **Wersja CSP jest ważna** - różne wersje mogą mieć różne funkcje i wymagania

---

## 10. Podsumowanie

1. **Instalacja CSP:**
   - `dwrite.dll` → `{AC_ROOT}\dwrite.dll`
   - `extension/` → `{AC_ROOT}\extension/`

2. **Włączenie CSP:**
   - `extension/config/general.ini` → `[BASIC] ENABLED = 1`

3. **Jak gra ładuje CSP:**
   - Windows automatycznie ładuje `dwrite.dll` przy starcie `acs.exe`
   - `dwrite.dll` sprawdza `extension/config/general.ini`
   - Jeśli `ENABLED = 1`, CSP jest aktywny

4. **Content Manager:**
   - Automatycznie wykrywa zainstalowany CSP
   - Może pobrać i zainstalować CSP
   - Umożliwia zarządzanie konfiguracją CSP
   - Automatycznie włącza CSP po instalacji

5. **Konfiguracja:**
   - Domyślna: `{AC_ROOT}\extension\config\*.ini`
   - Użytkownika: `{Documents}\Assetto Corsa\cfg\extension\*.ini` (ma priorytet)

