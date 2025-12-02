# Wykrywanie osi kontrolerów (kierownica, pedały, handbrake)

## Przegląd

System wykrywania osi w Content Managerze opiera się na DirectInput API (SlimDX), które skanuje podłączone kontrolery, enumeruje ich osie i odczytuje wartości w czasie rzeczywistym. Proces składa się z kilku etapów: skanowanie urządzeń, tworzenie reprezentacji osi, odczyt wartości i mapowanie do funkcji.

---

## Architektura systemu

### Główne komponenty:

1. **DirectInputScanner** - skanuje i enumeruje urządzenia DirectInput
2. **DirectInputDevice** - reprezentuje pojedyncze urządzenie (kontroler)
3. **DirectInputAxle** - reprezentuje pojedynczą oś kontrolera
4. **DisplayInputParams** - ładuje definicje nazw osi z plików JSON
5. **ControlsSettings** - zarządza bindami i odczytem wartości

---

## 1. Skanowanie urządzeń - DirectInputScanner

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/DirectInputScanner.cs`

### 1.1 Inicjalizacja

Klasa `DirectInputScanner` jest statyczna i zarządza globalnym skanowaniem urządzeń w tle.

```csharp
public static class DirectInputScanner {
    private static SlimDX.DirectInput.DirectInput _directInput;
    private static IList<Joystick> _staticData;
    private static string _staticDataFootprint;
    private static bool _threadStarted, _isActive;
    private static TimeSpan _scanTime;
}
```

### 1.2 Proces skanowania

#### Krok 1: Uruchomienie wątku skanowania

```csharp
private static void StartScanning() {
    if (_threadStarted) return;
    _threadStarted = true;
    _isActive = true;
    new Thread(Scan) {
        Name = "CM Devices Scan",
        IsBackground = true,
        Priority = ThreadPriority.BelowNormal
    }.Start();
}
```

**Wywołanie:** Metoda `Watch()` lub `GetAsync()` uruchamia skanowanie, jeśli jeszcze nie zostało uruchomione.

#### Krok 2: Główna pętla skanowania

```csharp
private static void Scan() {
    _directInput = new SlimDX.DirectInput.DirectInput();
    
    try {
        while (_isActive) {
            var getDevices = Stopwatch.StartNew();
            
            // Pobranie listy urządzeń
            var devices = _directInput?.GetDevices(
                DeviceClass.GameController, 
                DeviceEnumerationFlags.AttachedOnly
            );
            
            // Utworzenie "footprint" - unikalnego identyfikatora listy urządzeń
            footprint = devices?.Select(x => x.InstanceGuid)
                .JoinToString(';');
            
            // Sprawdzenie czy lista się zmieniła
            updated = _staticDataFootprint != footprint;
            
            if (updated) {
                // Tworzenie obiektów Joystick dla nowych urządzeń
                list = devices?.Select(x => {
                    var existing = _staticData?.FirstOrDefault(y =>
                        y.Information.InstanceGuid == x.InstanceGuid);
                    if (existing != null) {
                        return existing;  // Użyj istniejącego
                    }
                    
                    // Utwórz nowy Joystick
                    var result = new Joystick(_directInput, x.InstanceGuid);
                    if (result.Capabilities == null) {
                        throw new Exception("Never happens");
                    }
                    return result;
                }).ToArray();
                
                // Aktualizacja danych
                _staticData?.ApartFrom(list).DisposeEverything();
                _staticDataFootprint = footprint;
                _staticData = list;
                
                // Powiadomienie obserwatorów
                lock (Instances) {
                    for (var i = Instances.Count - 1; i >= 0; i--) {
                        Instances[i].RaiseUpdate(list);
                    }
                }
            }
            
            // Czekaj przed następnym skanowaniem
            Thread.Sleep(OptionMinRescanPeriod + getDevices.Elapsed);
        }
    } finally {
        DisposeHelper.Dispose(ref _directInput);
    }
}
```

**Kluczowe elementy:**

- **DeviceClass.GameController** - skanuje tylko kontrolery gier (koła, gamepady)
- **DeviceEnumerationFlags.AttachedOnly** - tylko podłączone urządzenia
- **InstanceGuid** - unikalny identyfikator każdego urządzenia
- **Footprint** - hash listy urządzeń do wykrywania zmian
- **Thread.Sleep** - minimalny okres między skanowaniami (domyślnie 3 sekundy)

### 1.3 Watcher - obserwator zmian

```csharp
public class Watcher : NotifyPropertyChanged, IDisposable {
    private IList<Joystick> _instanceData;
    
    internal void RaiseUpdate(IList<Joystick> newData) {
        _instanceData = newData;
        
        // Powiadomienie wszystkich oczekujących Tasków
        for (var i = _waitingFor.Count - 1; i >= 0; i--) {
            _waitingFor[i].TrySetResult(newData);
        }
        _waitingFor.Clear();
        
        // Powiadomienie przez event
        ActionExtension.InvokeInMainThreadAsync(() => {
            HasData = _instanceData != null;
            Update?.Invoke(this, EventArgs.Empty);
        });
    }
}
```

**Użycie w ControlsSettings:**

```csharp
DevicesScan = DirectInputScanner.Watch();
DevicesScan.Update += OnDevicesUpdate;

private void OnDevicesUpdate(object sender, EventArgs eventArgs) {
    var watcher = (DirectInputScanner.Watcher)sender;
    RescanDevices(watcher.Get());
}
```

---

## 2. Tworzenie reprezentacji urządzenia - DirectInputDevice

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/DirectInputDevice.cs`

### 2.1 Konstruktor - inicjalizacja urządzenia

```csharp
private DirectInputDevice([NotNull] Joystick device, int index) {
    Device = device.Information;
    
    // Identyfikatory
    InstanceId = GuidToString(device.Information.InstanceGuid);
    ProductId = GuidToString(device.Information.ProductGuid);
    Index = index;
    IsController = DirectInputDeviceUtils.IsController(device.Information.InstanceName);
    OriginalIniIds = new List<int>();
    
    _joystick = device;
    
    // Konfiguracja cooperative level
    var window = Application.Current?.MainWindow;
    if (window != null) {
        _joystick.SetCooperativeLevel(
            new WindowInteropHelper(window).Handle, 
            CooperativeLevel.Background | CooperativeLevel.Nonexclusive
        );
        _joystick.Properties.AxisMode = DeviceAxisMode.Absolute;
        
        if (!Acquire(_joystick)) {
            // Próba ponowna
            _joystick.SetCooperativeLevel(...);
            Acquire(_joystick);
        }
    }
    
    // ⭐ KLUCZOWE: Tworzenie tablicy osi
    Axis = Enumerable.Range(0, 8)
        .Select(x => new DirectInputAxle(this, x))
        .ToArray();
    
    // Tworzenie przycisków
    Buttons = Enumerable.Range(0, _joystick.Capabilities.ButtonCount)
        .Select(x => new DirectInputButton(this, x))
        .ToArray();
    
    // Tworzenie POV (hat switches)
    Povs = Enumerable.Range(0, _joystick.Capabilities.PovCount)
        .SelectMany(x => Enumerable.Range(0, 4)
            .Select(y => new { Id = x, Direction = (DirectInputPovDirection)y }))
        .Select(x => new DirectInputPov(this, x.Id, x.Direction))
        .ToArray();
    
    // Odświeżenie nazw i widoczności
    RefreshDescription();
}
```

**Kluczowe informacje:**

- **Zawsze tworzy 8 osi** (indeksy 0-7), nawet jeśli urządzenie ma mniej
- **CooperativeLevel.Background** - może odczytywać dane w tle
- **CooperativeLevel.Nonexclusive** - inne aplikacje też mogą używać urządzenia
- **DeviceAxisMode.Absolute** - wartości bezwzględne (0-65535), nie względne

### 2.2 Mapowanie osi DirectInput do indeksów

DirectInput definiuje standardowe osie, które są mapowane do indeksów 0-7:

| Indeks | DirectInput | Opis | Typowe użycie |
|--------|-------------|------|---------------|
| **0** | `state.X` | Oś X | **Kierownica** |
| **1** | `state.Y` | Oś Y | **Gaz** |
| **2** | `state.Z` | Oś Z | **Hamulce** |
| **3** | `state.RotationX` | Obrót X | **Sprzęgło** |
| **4** | `state.RotationY` | Obrót Y | Dodatkowe |
| **5** | `state.RotationZ` | Obrót Z | Dodatkowe |
| **6** | `state.GetSliders()[0]` | Slider 1 | **Handbrake** |
| **7** | `state.GetSliders()[1]` | Slider 2 | Dodatkowe |

**Uwaga:** To są standardowe mapowania. Rzeczywiste mapowanie zależy od producenta kontrolera.

---

## 3. Odczyt wartości osi - OnTick()

**Lokalizacja:** `DirectInputDevice.OnTick()` (linia 171)

### 3.1 Proces odczytu

```csharp
public void OnTick() {
    try {
        // Sprawdzenie czy urządzenie jest dostępne
        if (_joystick.Disposed || 
            _joystick.Acquire().IsFailure || 
            _joystick.Poll().IsFailure || 
            Result.Last.IsFailure) {
            return;
        }
        
        // Pobranie aktualnego stanu
        var state = _joystick.GetCurrentState();
        
        // ⭐ Odczyt wartości wszystkich osi
        for (var i = 0; i < Axis.Length; i++) {
            var a = Axis[i];
            a.Value = GetAxisValue(a.Id, state);
        }
        
        // Odczyt przycisków
        var buttons = state.GetButtons();
        for (var i = 0; i < Buttons.Length; i++) {
            var b = Buttons[i];
            b.Value = b.Id < buttons.Length && buttons[b.Id];
        }
        
        // Odczyt POV
        var povs = state.GetPointOfViewControllers();
        for (var i = 0; i < Povs.Length; i++) {
            var b = Povs[i];
            b.Value = b.Direction.IsInRange(
                b.Id < povs.Length ? povs[b.Id] : -1
            );
        }
    } catch (DirectInputException e) when (e.Message.Contains(@"DIERR_UNPLUGGED")) {
        Unplugged = true;
    } catch (DirectInputException e) {
        if (!Error) {
            Logging.Warning("Exception: " + e);
            Error = true;
        }
    }
}
```

**Wywołanie:** Metoda `OnTick()` jest wywoływana dla każdego urządzenia w `ControlsSettings.UpdateInputs()`:

```csharp
private void OnTick(object sender, EventArgs e) {
    if (Application.Current?.MainWindow?.IsVisible == true) {
        UpdateInputs();
    }
}

private void UpdateInputs() {
    if (Used == 0) return;
    
    foreach (var device in Devices) {
        device.OnTick();  // ⭐ Odczyt wartości dla każdego urządzenia
    }
}
```

**Częstotliwość:** `OnTick()` jest wywoływana przy każdym renderowaniu UI (przez `CompositionTargetEx.Rendering`), co daje około 60 FPS.

### 3.2 Mapowanie wartości - GetAxisValue()

```csharp
double GetAxisValue(int id, JoystickState state) {
    switch (id) {
        case 0:
            return state.X / 65535d;           // 0.0 - 1.0
        case 1:
            return state.Y / 65535d;           // 0.0 - 1.0
        case 2:
            return state.Z / 65535d;           // 0.0 - 1.0
        case 3:
            return state.RotationX / 65535d;   // 0.0 - 1.0
        case 4:
            return state.RotationY / 65535d;   // 0.0 - 1.0
        case 5:
            return state.RotationZ / 65535d;   // 0.0 - 1.0
        case 6:
            return state.GetSliders().Length > 0 
                ? state.GetSliders()[0] / 65535d 
                : 0d;
        case 7:
            return state.GetSliders().Length > 1 
                ? state.GetSliders()[1] / 65535d 
                : 0d;
        default:
            return 0;
    }
}
```

**Konwersja wartości:**

- **DirectInput:** 0 - 65535 (16-bit unsigned integer)
- **Content Manager:** 0.0 - 1.0 (double, znormalizowane)
- **Dzielenie przez 65535d** - konwersja do zakresu 0.0-1.0

**Przykład:**
- `state.X = 32767` → `Value = 0.5` (środek)
- `state.X = 0` → `Value = 0.0` (minimum)
- `state.X = 65535` → `Value = 1.0` (maksimum)

---

## 4. Reprezentacja osi - DirectInputAxle

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/DirectInputAxle.cs`

### 4.1 Definicja klasy

```csharp
public sealed class DirectInputAxle : InputProviderBase<double>, IDirectInputProvider {
    public IDirectInputDevice Device { get; }  // Urządzenie do którego należy
    public int Id { get; }                     // Indeks osi (0-7)
    public string DefaultName { get; }         // Domyślna nazwa (np. "Axis 1")
    
    // Właściwości wartości
    public double Value { get; set; }          // Aktualna wartość (0.0-1.0)
    public double RoundedValue { get; set; }   // Zaokrąglona wartość (zmiana > 0.01)
    public double Delta { get; set; }          // Różnica między Value a RoundedValue
    
    // Właściwości wyświetlania
    public string DisplayName { get; set; }    // Pełna nazwa (np. "Steering Wheel")
    public string ShortName { get; set; }      // Skrócona nazwa (np. "SW")
    public bool IsVisible { get; set; }        // Czy pokazywać w UI
}
```

### 4.2 Konstruktor

```csharp
public DirectInputAxle(IDirectInputDevice device, int id) : base(id) {
    Device = device;
    DefaultName = string.Format(ToolsStrings.Input_Axle, (id + 1).ToInvariantString());
    // Domyślnie: "Axis 1", "Axis 2", itd.
    
    SetDisplayParams(null, true);  // Ustaw domyślne parametry
}
```

### 4.3 Aktualizacja zaokrąglonej wartości

```csharp
protected override void OnValueChanged() {
    var value = Value;
    
    // Aktualizuj tylko jeśli zmiana > 0.01 (1%)
    if ((value - RoundedValue).Abs() < 0.01) return;
    
    Delta = value - RoundedValue;
    RoundedValue = value;
    
    // ⭐ To wywołuje PropertyChanged dla RoundedValue
    // Używane przez WheelAxleEntry do wykrywania ruchu osi
}
```

**Dlaczego RoundedValue?**

- Zmniejsza liczbę eventów `PropertyChanged`
- Filtruje małe fluktuacje (szum)
- Używane przez `WheelAxleEntry` do wykrywania ruchu osi podczas przypisywania bindów

---

## 5. Przypisywanie nazw osiom - RefreshDescription()

**Lokalizacja:** `DirectInputDevice.RefreshDescription()` (linia 130)

### 5.1 Proces przypisywania nazw

```csharp
public void RefreshDescription() {
    // Próba załadowania definicji z pliku JSON
    if (!DisplayInputParams.Get(
        Device.ProductGuid.ToString(), 
        out var displayName, 
        out var axisP, 
        out var buttonsP, 
        out var povsP
    ) && IsController) {
        // Jeśli nie znaleziono, użyj domyślnych dla kontrolera Xbox
        DisplayInputParams.Get(
            DirectInputDeviceUtils.GetXboxControllerGuid(), 
            out _, 
            out axisP, 
            out buttonsP, 
            out povsP
        );
    }
    
    // Ustaw nazwę urządzenia
    DisplayName = displayName ?? FixDisplayName(Device.InstanceName);
    
    // ⭐ Przypisz nazwy do osi
    Proc(Axis, axisP);
    Proc(Buttons, buttonsP);
    Proc(Povs, povsP);
    
    void Proc(IEnumerable<IInputProvider> items, DisplayInputParams p) {
        foreach (var t in items) {
            t.SetDisplayParams(
                p?.Name(t.Id),      // Nazwa dla tego indeksu
                p?.Test(t.Id) ?? true  // Czy pokazywać w UI
            );
        }
    }
    
    // Filtrowanie widocznych elementów
    VisibleAxis = Axis.Where(x => x.IsVisible).ToList();
    VisibleButtons = Buttons.Where(x => x.IsVisible).ToList();
    VisiblePovs = Povs.Where(x => x.IsVisible).ToList();
}
```

### 5.2 Pliki definicji - DisplayInputParams

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/DisplayInputParams.cs`

**Ścieżka plików:** `{Content}/controllers/{GUID}.json`

**Format pliku JSON:**

```json
{
  "name": "Logitech G29 Racing Wheel",
  "axis": [
    "Steering Wheel",
    "Throttle",
    "Brakes",
    "Clutch",
    "Axis 5",
    "Axis 6",
    "Handbrake",
    "Axis 8"
  ],
  "buttons": [
    "Button 1",
    "Button 2",
    ...
  ],
  "pov": [
    "POV Up",
    "POV Down",
    ...
  ]
}
```

**Alternatywny format (obiekt):**

```json
{
  "name": "Logitech G29",
  "axes": {
    "0": "Steering Wheel",
    "1": "Throttle",
    "2": "Brakes",
    "3": "Clutch",
    "6": "Handbrake"
  },
  "buttons": {
    "0": "X",
    "1": "Circle",
    ...
  }
}
```

**Ładowanie definicji:**

```csharp
public static bool Get(
    [NotNull] string guid, 
    out string displayName, 
    out DisplayInputParams axes, 
    out DisplayInputParams buttons, 
    out DisplayInputParams povs
) {
    var file = FilesStorage.Instance.GetContentFile(
        ContentCategory.Controllers, 
        $"{guid}.json"
    );
    
    if (file.Exists) {
        try {
            var jData = JsonExtension.Parse(File.ReadAllText(file.Filename));
            displayName = ContentUtils.Translate(jData.GetStringValueOnly("name"));
            axes = new DisplayInputParams(
                jData["axis"] ?? jData["axes"] ?? jData["axles"]
            );
            buttons = new DisplayInputParams(jData["buttons"]);
            povs = new DisplayInputParams(
                jData["pov"] ?? jData["povs"] ?? jData["pointOfViews"]
            );
            return true;
        } catch (Exception e) {
            Logging.Warning(e);
        }
    }
    
    displayName = null;
    axes = null;
    buttons = null;
    povs = null;
    return false;
}
```

### 5.3 Ustawianie nazw w DirectInputAxle

```csharp
protected override void SetDisplayName(string displayName) {
    if (displayName?.Length > 2) {
        var index = displayName.IndexOf(';');
        if (index != -1) {
            // Format: "SW;Steering Wheel"
            ShortName = displayName.Substring(0, index);      // "SW"
            DisplayName = displayName.Substring(index + 1).ToTitle();  // "Steering Wheel"
        } else {
            // Format: "Steering Wheel"
            var abbreviation = displayName
                .Where((x, i) => i == 0 || char.IsWhiteSpace(displayName[i - 1]))
                .Take(3)
                .JoinToString();
            ShortName = abbreviation.ToUpper();  // "SW"
            DisplayName = displayName.ToTitle();  // "Steering Wheel"
        }
    } else {
        // Domyślna nazwa
        ShortName = displayName?.ToTitle() ?? (Id + 1).As<string>();
        DisplayName = string.Format(ToolsStrings.Input_Axle, ShortName);
    }
}
```

**Przykłady:**

- `"SW;Steering Wheel"` → ShortName: "SW", DisplayName: "Steering Wheel"
- `"Throttle"` → ShortName: "THR", DisplayName: "Throttle"
- `null` → ShortName: "1", DisplayName: "Axis 1"

---

## 6. Przypisywanie osi do funkcji - WheelAxleEntry

**Lokalizacja:** `AcManager.Tools/Helpers/AcSettingsControls/WheelAxleEntry.cs`

### 6.1 Ładowanie z controls.ini

```csharp
public override void Load(IniFile ini, IReadOnlyList<IDirectInputDevice> devices) {
    var section = ini[Id];  // np. ini["THROTTLE"]
    
    // Pobranie indeksu kontrolera
    var deviceId = section.GetInt("JOY", -1);
    
    // Znalezienie urządzenia
    var device = devices.FirstOrDefault(x => 
        x.OriginalIniIds.Contains(deviceId)
    );
    
    // ⭐ Przypisanie osi do binda
    Input = device?.GetAxle(section.GetInt("AXLE", -1));
    
    // Dla pedałów - zakres MIN/MAX
    if (RangeMode) {
        var from = (int)(section.GetDouble("MIN", -1d) * 50 + 50).Clamp(0, 100);
        var to = (int)(section.GetDouble("MAX", 1d) * 50 + 50).Clamp(0, 100);
        RangeFrom = from;
        RangeTo = to;
    }
    
    // Dla kierownicy - LOCK, SCALE, itd.
    else {
        DegreesOfRotation = section.GetInt("LOCK", 900);
        Scale = section.GetDouble("SCALE", 1d).ToIntPercentage();
    }
}
```

**Przykład dla gazu:**

```ini
[THROTTLE]
JOY = 0      # Pierwszy kontroler
AXLE = 1     # Oś Y (indeks 1)
```

**Kod:**

```csharp
var throttleEntry = WheelAxleEntries.First(x => x.Id == "THROTTLE");
// throttleEntry.Input = devices[0].GetAxle(1)
// throttleEntry.Input = devices[0].Axis[1]
// throttleEntry.Input.Value = aktualna wartość osi Y (0.0-1.0)
```

### 6.2 Wykrywanie ruchu osi podczas przypisywania

```csharp
// W ControlsSettings.cs
private void DeviceAxleEventHandler(object sender, PropertyChangedEventArgs e) {
    if (e.PropertyName == nameof(DirectInputAxle.RoundedValue) && 
        sender is DirectInputAxle axle) {
        
        // ⭐ Jeśli użytkownik porusza osią, przypisz ją do oczekującego binda
        AssignInput(axle).Ignore();
    }
}
```

**Proces:**

1. Użytkownik klika "Przypisz" dla binda (np. "Gaz")
2. `entry.IsWaiting = true`
3. Użytkownik porusza osią (np. naciska pedał gazu)
4. `DirectInputAxle.RoundedValue` się zmienia
5. `DeviceAxleEventHandler` wywołuje `AssignInput(axle)`
6. `AssignInput()` sprawdza czy jest oczekujący bind
7. Jeśli tak, przypisuje oś: `waiting.Input = axle`

---

## 7. Przepływ danych - kompletny przykład

### 7.1 Scenariusz: Użytkownik porusza kierownicą

```
1. Fizyczny ruch kierownicy
   ↓
2. DirectInput odbiera sygnał
   state.X = 32767 (środek)
   ↓
3. DirectInputDevice.OnTick()
   GetAxisValue(0, state) → 32767 / 65535 = 0.5
   Axis[0].Value = 0.5
   ↓
4. DirectInputAxle.OnValueChanged()
   RoundedValue = 0.5 (jeśli zmiana > 0.01)
   PropertyChanged("RoundedValue")
   ↓
5. ControlsSettings.DeviceAxleEventHandler()
   AssignInput(axle) - jeśli IsWaiting
   ↓
6. WheelAxleEntry.Input = axle
   PropertyChanged("Input")
   ↓
7. WheelAxleEntry.UpdateValue()
   Value = przetworzona wartość (z RangeMode, Gamma, itd.)
   ↓
8. UI aktualizuje się
   Pokazuje wartość i przypisany bind
```

### 7.2 Scenariusz: Skanowanie nowego urządzenia

```
1. Użytkownik podłącza Logitech G29
   ↓
2. DirectInputScanner.Scan() (w tle, co 3 sekundy)
   GetDevices(DeviceClass.GameController) → lista urządzeń
   ↓
3. Wykrycie nowego InstanceGuid
   Utworzenie Joystick dla nowego urządzenia
   ↓
4. DirectInputScanner.Watcher.RaiseUpdate()
   Event Update wywołany
   ↓
5. ControlsSettings.OnDevicesUpdate()
   RescanDevices(watcher.Get())
   ↓
6. DirectInputDevice.Create() dla każdego Joystick
   new DirectInputDevice(joystick, index)
   ↓
7. DirectInputDevice konstruktor
   Axis = 8 osi (0-7)
   Buttons = wszystkie przyciski
   RefreshDescription() - ładuje nazwy z JSON
   ↓
8. ControlsSettings.Devices.Add()
   Urządzenie dostępne w UI
   ↓
9. Użytkownik widzi "Logitech G29" w liście kontrolerów
   Może przypisać osie do bindów
```

---

## 8. Mapowanie standardowe vs rzeczywiste

### 8.1 Standardowe mapowanie DirectInput

| Indeks | DirectInput | Typowe użycie |
|--------|-------------|---------------|
| 0 | X | Kierownica |
| 1 | Y | Gaz |
| 2 | Z | Hamulce |
| 3 | RotationX | Sprzęgło |
| 6 | Slider[0] | Handbrake |

### 8.2 Rzeczywiste mapowanie - przykłady

#### Logitech G29:
- **Oś 0 (X):** Kierownica
- **Oś 1 (Y):** Gaz
- **Oś 2 (Z):** Hamulce
- **Oś 3 (RotationX):** Sprzęgło
- **Oś 6 (Slider[0]):** Handbrake (jeśli podłączony)

#### Thrustmaster T300:
- **Oś 0 (X):** Kierownica
- **Oś 1 (Y):** Gaz
- **Oś 2 (Z):** Hamulce
- **Oś 3 (RotationX):** Sprzęgło

#### Fanatec CSL Elite:
- **Oś 0 (X):** Kierownica
- **Oś 1 (Y):** Gaz
- **Oś 2 (Z):** Hamulce
- **Oś 3 (RotationX):** Sprzęgło
- **Oś 6 (Slider[0]):** Handbrake

**Uwaga:** Rzeczywiste mapowanie może się różnić w zależności od:
- Modelu kontrolera
- Konfiguracji sterowników
- Ustawień w panelu sterowania Windows

---

## 9. Obsługa błędów i edge cases

### 9.1 Urządzenie odłączone

```csharp
catch (DirectInputException e) when (e.Message.Contains(@"DIERR_UNPLUGGED")) {
    Unplugged = true;
}
```

**Reakcja:**
- `Unplugged = true` - flaga że urządzenie zostało odłączone
- Urządzenie pozostaje w liście jako "placeholder"
- Po ponownym podłączeniu, jeśli GUID się zgadza, zostaje przywrócone

### 9.2 Urządzenie nie może być nabyte (Acquire)

```csharp
if (!Acquire(_joystick)) {
    // Próba ponowna z innym cooperative level
    _joystick.SetCooperativeLevel(...);
    Acquire(_joystick);
}
```

**Przyczyny:**
- Inna aplikacja ma wyłączny dostęp
- Urządzenie jest używane przez system
- Błąd sterownika

### 9.3 Brak definicji w pliku JSON

```csharp
if (!DisplayInputParams.Get(...) && IsController) {
    // Użyj domyślnych dla kontrolera Xbox
    DisplayInputParams.Get(DirectInputDeviceUtils.GetXboxControllerGuid(), ...);
}
```

**Fallback:**
- Jeśli nie ma pliku `{GUID}.json`, używa domyślnych nazw
- Dla kontrolerów (gamepady), używa mapowania Xbox
- Dla kół, używa generycznych nazw "Axis 1", "Axis 2", itd.

### 9.4 Oś nie istnieje w urządzeniu

```csharp
double GetAxisValue(int id, JoystickState state) {
    switch (id) {
        case 6:
            return state.GetSliders().Length > 0 
                ? state.GetSliders()[0] / 65535d 
                : 0d;  // ⭐ Zwraca 0 jeśli slider nie istnieje
    }
}
```

**Zachowanie:**
- Jeśli oś nie istnieje, zwraca wartość domyślną (0.0)
- Nie powoduje błędu
- Oś jest nadal dostępna w UI, ale zawsze zwraca 0

---

## 10. Optymalizacje i wydajność

### 10.1 Częstotliwość odczytu

- **OnTick():** Wywoływane przy każdym renderowaniu UI (~60 FPS)
- **RoundedValue:** Aktualizowane tylko przy zmianie > 1%
- **PropertyChanged:** Wywoływane tylko dla RoundedValue, nie Value

**Efekt:** Zmniejsza liczbę eventów i aktualizacji UI.

### 10.2 Skanowanie urządzeń

- **Minimalny okres:** 3 sekundy między skanowaniami
- **Wątek w tle:** Nie blokuje UI
- **Caching:** Przechowuje listę urządzeń, aktualizuje tylko przy zmianie

**Efekt:** Minimalne obciążenie CPU.

### 10.3 Lazy loading

- **Definicje JSON:** Ładowane tylko gdy potrzebne
- **Joystick objects:** Tworzone tylko dla nowych urządzeń
- **Watcher:** Subskrypcja tylko gdy używane

**Efekt:** Szybsze uruchomienie aplikacji.

---

## 11. Podsumowanie - kluczowe punkty

### 11.1 Proces wykrywania osi

1. **Skanowanie:** `DirectInputScanner` skanuje urządzenia w tle
2. **Tworzenie:** `DirectInputDevice` tworzy 8 osi (indeksy 0-7)
3. **Mapowanie:** `GetAxisValue()` mapuje DirectInput do 0.0-1.0
4. **Odczyt:** `OnTick()` odczytuje wartości w czasie rzeczywistym
5. **Nazwy:** `RefreshDescription()` ładuje nazwy z plików JSON
6. **Przypisanie:** `WheelAxleEntry` przypisuje osie do bindów

### 11.2 Standardowe mapowanie

- **Oś 0 (X):** Kierownica
- **Oś 1 (Y):** Gaz
- **Oś 2 (Z):** Hamulce
- **Oś 3 (RotationX):** Sprzęgło
- **Oś 6 (Slider[0]):** Handbrake

### 11.3 Kluczowe klasy

| Klasa | Rola |
|-------|------|
| `DirectInputScanner` | Skanuje i enumeruje urządzenia |
| `DirectInputDevice` | Reprezentuje urządzenie, tworzy osie |
| `DirectInputAxle` | Reprezentuje pojedynczą oś |
| `DisplayInputParams` | Ładuje definicje nazw z JSON |
| `WheelAxleEntry` | Przypisuje osie do bindów |

---

**Dokumentacja utworzona na podstawie analizy kodu źródłowego Content Managera.**

