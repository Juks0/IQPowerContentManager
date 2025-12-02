# Klasa DirectInputDevice - Dokumentacja

## Przegląd

Klasa `DirectInputDevice` jest centralnym komponentem systemu obsługi kontrolerów w Content Managerze. Reprezentuje pojedyncze urządzenie DirectInput (koło kierownicze, gamepad, joystick) i zarządza wszystkimi jego elementami wejściowymi: osiami, przyciskami i POV (hat switches).

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/DirectInputDevice.cs`

---

## Hierarchia klas i interfejsów

```
Displayable (FirstFloor.ModernUI.Presentation)
    ↓
DirectInputDevice
    ├── IDirectInputDevice (interfejs)
    └── IDisposable
```

### Zależności klasowe:

```
DirectInputDevice
    ├── używa → DirectInputAxle
    ├── używa → DirectInputButton
    ├── używa → DirectInputPov
    ├── używa → DisplayInputParams
    ├── używa → DirectInputDeviceUtils
    ├── implementuje → IDirectInputDevice
    └── opakowuje → SlimDX.DirectInput.Joystick
```

---

## Definicja klasy

```csharp
public sealed class DirectInputDevice : Displayable, IDirectInputDevice, IDisposable
```

**Kluczowe właściwości:**

- `sealed` - klasa nie może być dziedziczona
- Dziedziczy z `Displayable` - zapewnia `INotifyPropertyChanged` i podstawowe właściwości
- Implementuje `IDirectInputDevice` - interfejs definiujący kontrakt dla urządzeń DirectInput
- Implementuje `IDisposable` - do zwalniania zasobów

---

## Właściwości klasy

### 1. Identyfikatory urządzenia

```csharp
[NotNull]
public DeviceInstance Device { get; }  // Informacje o urządzeniu z DirectInput

public string InstanceId { get; }      // Unikalny GUID instancji (np. "{C24F046D-...}")
public string ProductId { get; }       // GUID produktu (np. "{C24F046D-...}")
public int Index { get; set; }         // Indeks w liście urządzeń (0, 1, 2, ...)
public IList<int> OriginalIniIds { get; }  // Lista indeksów z pliku controls.ini
```

**Przykład:**
```csharp
InstanceId = "{C24F046D-0000-0000-0000-504944564944}"
ProductId = "{C24F046D-0000-0000-0000-504944564944}"
Index = 0  // Pierwszy kontroler
```

### 2. Typ urządzenia

```csharp
public bool IsVirtual => false;        // Zawsze false dla prawdziwych urządzeń
public bool IsController { get; }      // true dla gamepadów, false dla kół
```

**Określanie typu:**
```csharp
IsController = DirectInputDeviceUtils.IsController(device.Information.InstanceName);
// Sprawdza czy nazwa pasuje do wzorca: "Controller (...)"
```

### 3. Elementy wejściowe

```csharp
public DirectInputAxle[] Axis { get; }           // Tablica 8 osi (indeksy 0-7)
public DirectInputButton[] Buttons { get; }     // Tablica wszystkich przycisków
public DirectInputPov[] Povs { get; }           // Tablica POV (hat switches)

// Filtrowane listy (tylko widoczne w UI)
public IReadOnlyList<DirectInputAxle> VisibleAxis { get; set; }
public IReadOnlyList<DirectInputButton> VisibleButtons { get; set; }
public IReadOnlyList<DirectInputPov> VisiblePovs { get; set; }
```

### 4. Wewnętrzne pole

```csharp
private readonly Joystick _joystick;  // Obiekt SlimDX DirectInput
```

---

## Metody publiczne

### 1. Tworzenie instancji

```csharp
[CanBeNull]
public static DirectInputDevice Create(Joystick device, int iniId)
```

**Opis:** Statyczna metoda fabryki do tworzenia instancji urządzenia.

**Parametry:**
- `device` - obiekt `Joystick` z SlimDX DirectInput
- `iniId` - indeks urządzenia (używany w controls.ini)

**Zwraca:** `DirectInputDevice` lub `null` w przypadku błędu

**Obsługa błędów:**
```csharp
try {
    return new DirectInputDevice(device, iniId);
} catch (DirectInputNotFoundException e) {
    Logging.Warning(e);
    return null;
} catch (DirectInputException e) {
    Logging.Error(e);
    return null;
}
```

### 2. Pobieranie elementów wejściowych

```csharp
public DirectInputAxle GetAxle(int id)
public DirectInputButton GetButton(int id)
public DirectInputPov GetPov(int id, DirectInputPovDirection direction)
```

**Opis:** Pobierają elementy wejściowe po indeksie.

**Przykład:**
```csharp
var steeringAxis = device.GetAxle(0);      // Oś kierownicy
var throttleAxis = device.GetAxle(1);      // Oś gazu
var button1 = device.GetButton(0);         // Pierwszy przycisk
var povUp = device.GetPov(0, DirectInputPovDirection.Up);  // POV w górę
```

**Zwraca:** Element wejściowy lub `null` jeśli `id < 0` lub nie istnieje

### 3. Porównywanie urządzeń

```csharp
public bool Same(IDirectInputDevice other)
public bool Same(DeviceInstance other)
```

**Opis:** Sprawdzają czy to to samo urządzenie.

**Logika:**
```csharp
public bool Same(IDirectInputDevice other) {
    return other != null && (
        this.IsSameAs(other) ||  // Porównanie przez InstanceId/ProductId
        IsController && other.InstanceId == @"0"  // Fallback dla kontrolerów
    );
}
```

### 4. Odświeżanie opisu

```csharp
public void RefreshDescription()
```

**Opis:** Ładuje nazwy osi, przycisków i POV z plików JSON i aktualizuje widoczność.

**Proces:**
1. Próba załadowania definicji z pliku `{ProductGuid}.json`
2. Jeśli nie znaleziono i to kontroler, używa domyślnych dla Xbox
3. Przypisuje nazwy do wszystkich elementów
4. Filtruje widoczne elementy

### 5. Odczyt wartości

```csharp
public void OnTick()
```

**Opis:** Odczytuje aktualne wartości wszystkich elementów wejściowych.

**Wywoływane:** Cyklicznie przez `ControlsSettings.UpdateInputs()` (~60 FPS)

**Proces:**
1. Sprawdza dostępność urządzenia
2. Pobiera stan z DirectInput
3. Aktualizuje wartości osi (0.0-1.0)
4. Aktualizuje stany przycisków (true/false)
5. Aktualizuje stany POV

### 6. Narzędzia

```csharp
public void RunControlPanel()  // Otwiera panel sterowania urządzenia
public override string ToString()  // Reprezentacja tekstowa
public void Dispose()  // Zwolnienie zasobów
```

---

## Konstruktor - szczegółowa analiza

```csharp
private DirectInputDevice([NotNull] Joystick device, int index)
```

**Uwaga:** Konstruktor jest prywatny - użyj `Create()` do tworzenia instancji.

### Krok 1: Inicjalizacja właściwości

```csharp
Device = device.Information;
InstanceId = GuidToString(device.Information.InstanceGuid);
ProductId = GuidToString(device.Information.ProductGuid);
Index = index;
IsController = DirectInputDeviceUtils.IsController(device.Information.InstanceName);
OriginalIniIds = new List<int>();
_joystick = device;
```

### Krok 2: Konfiguracja cooperative level

```csharp
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
```

**CooperativeLevel:**
- `Background` - może odczytywać dane w tle (gdy okno nie jest aktywne)
- `Nonexclusive` - inne aplikacje też mogą używać urządzenia

**DeviceAxisMode:**
- `Absolute` - wartości bezwzględne (0-65535), nie względne

### Krok 3: Tworzenie osi

```csharp
Axis = Enumerable.Range(0, 8)
    .Select(x => new DirectInputAxle(this, x))
    .ToArray();
```

**Zawsze tworzy 8 osi** (indeksy 0-7), nawet jeśli urządzenie ma mniej:
- 0 = X (kierownica)
- 1 = Y (gaz)
- 2 = Z (hamulce)
- 3 = RotationX (sprzęgło)
- 4 = RotationY
- 5 = RotationZ
- 6 = Slider[0] (handbrake)
- 7 = Slider[1]

### Krok 4: Tworzenie przycisków

```csharp
Buttons = Enumerable.Range(0, _joystick.Capabilities.ButtonCount)
    .Select(x => new DirectInputButton(this, x))
    .ToArray();
```

**Tworzy tyle przycisków ile ma urządzenie** (np. Logitech G29 ma 23 przyciski).

### Krok 5: Tworzenie POV

```csharp
Povs = Enumerable.Range(0, _joystick.Capabilities.PovCount)
    .SelectMany(x => Enumerable.Range(0, 4)
        .Select(y => new { Id = x, Direction = (DirectInputPovDirection)y }))
    .Select(x => new DirectInputPov(this, x.Id, x.Direction))
    .ToArray();
```

**Dla każdego POV tworzy 4 kierunki:**
- Up (0)
- Right (1)
- Down (2)
- Left (3)

**Przykład:** Jeśli urządzenie ma 1 POV, tworzy 4 obiekty: POV0-Up, POV0-Right, POV0-Down, POV0-Left

### Krok 6: Odświeżanie opisu

```csharp
RefreshDescription();
FilesStorage.Instance.Watcher(ContentCategory.Controllers).Update += OnDefinitionsUpdate;
```

**Subskrypcja:** Nasłuchuje zmian w plikach definicji kontrolerów.

---

## Metoda OnTick() - szczegółowa analiza

```csharp
public void OnTick() {
    try {
        // 1. Sprawdzenie dostępności
        if (_joystick.Disposed || 
            _joystick.Acquire().IsFailure || 
            _joystick.Poll().IsFailure || 
            Result.Last.IsFailure) {
            return;
        }
        
        // 2. Pobranie stanu
        var state = _joystick.GetCurrentState();
        
        // 3. Aktualizacja osi
        for (var i = 0; i < Axis.Length; i++) {
            var a = Axis[i];
            a.Value = GetAxisValue(a.Id, state);
        }
        
        // 4. Aktualizacja przycisków
        var buttons = state.GetButtons();
        for (var i = 0; i < Buttons.Length; i++) {
            var b = Buttons[i];
            b.Value = b.Id < buttons.Length && buttons[b.Id];
        }
        
        // 5. Aktualizacja POV
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

### Mapowanie wartości osi - GetAxisValue()

```csharp
double GetAxisValue(int id, JoystickState state) {
    switch (id) {
        case 0: return state.X / 65535d;                    // 0.0 - 1.0
        case 1: return state.Y / 65535d;                    // 0.0 - 1.0
        case 2: return state.Z / 65535d;                   // 0.0 - 1.0
        case 3: return state.RotationX / 65535d;            // 0.0 - 1.0
        case 4: return state.RotationY / 65535d;           // 0.0 - 1.0
        case 5: return state.RotationZ / 65535d;             // 0.0 - 1.0
        case 6: return state.GetSliders().Length > 0 
            ? state.GetSliders()[0] / 65535d : 0d;
        case 7: return state.GetSliders().Length > 1 
            ? state.GetSliders()[1] / 65535d : 0d;
        default: return 0;
    }
}
```

**Konwersja:**
- DirectInput: 0 - 65535 (16-bit unsigned integer)
- Content Manager: 0.0 - 1.0 (double, znormalizowane)

---

## Powiązane klasy

### 1. DirectInputAxle

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/DirectInputAxle.cs`

**Hierarchia:**
```
InputProviderBase<double>
    ↓
DirectInputAxle
    ├── IDirectInputProvider
    └── IInputProvider
```

**Reprezentuje:** Pojedynczą oś kontrolera (kierownica, gaz, hamulce, itd.)

**Właściwości:**
```csharp
public IDirectInputDevice Device { get; }  // Urządzenie do którego należy
public int Id { get; }                     // Indeks osi (0-7)
public double Value { get; set; }          // Aktualna wartość (0.0-1.0)
public double RoundedValue { get; set; }   // Zaokrąglona wartość (zmiana > 0.01)
public double Delta { get; set; }          // Różnica Value - RoundedValue
public string DisplayName { get; set; }    // Pełna nazwa (np. "Steering Wheel")
public string ShortName { get; set; }      // Skrócona nazwa (np. "SW")
public bool IsVisible { get; set; }       // Czy pokazywać w UI
```

**Użycie w DirectInputDevice:**
```csharp
// Tworzenie osi
Axis = Enumerable.Range(0, 8)
    .Select(x => new DirectInputAxle(this, x))
    .ToArray();

// Pobieranie osi
public DirectInputAxle GetAxle(int id) {
    return id < 0 ? null : Axis.GetByIdOrDefault(id);
}

// Aktualizacja wartości
a.Value = GetAxisValue(a.Id, state);
```

### 2. DirectInputButton

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/DirectInputButton.cs`

**Hierarchia:**
```
InputProviderBase<bool>
    ↓
DirectInputButton
    ├── IDirectInputProvider
    └── IInputProvider
```

**Reprezentuje:** Pojedynczy przycisk na kontrolerze

**Właściwości:**
```csharp
public IDirectInputDevice Device { get; }  // Urządzenie do którego należy
public int Id { get; }                     // Indeks przycisku (0, 1, 2, ...)
public bool Value { get; set; }            // Stan przycisku (true = naciśnięty)
public string DisplayName { get; set; }    // Pełna nazwa (np. "Button 1")
public string ShortName { get; set; }      // Skrócona nazwa (np. "B1")
public bool IsVisible { get; set; }        // Czy pokazywać w UI
```

**Użycie w DirectInputDevice:**
```csharp
// Tworzenie przycisków
Buttons = Enumerable.Range(0, _joystick.Capabilities.ButtonCount)
    .Select(x => new DirectInputButton(this, x))
    .ToArray();

// Pobieranie przycisku
public DirectInputButton GetButton(int id) {
    return id < 0 ? null : Buttons.GetByIdOrDefault(id);
}

// Aktualizacja stanu
b.Value = b.Id < buttons.Length && buttons[b.Id];
```

### 3. DirectInputPov

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/DirectInputPov.cs`

**Hierarchia:**
```
InputProviderBase<bool>
    ↓
DirectInputButton
    ↓
DirectInputPov
    ├── IDirectInputProvider
    └── IInputProvider
```

**Reprezentuje:** Pojedynczy kierunek POV (hat switch) - np. "POV Up", "POV Left"

**Właściwości:**
```csharp
public IDirectInputDevice Device { get; }  // Urządzenie do którego należy
public int Id { get; }                     // Indeks POV (0, 1, ...)
public DirectInputPovDirection Direction { get; }  // Kierunek (Up, Right, Down, Left)
public bool Value { get; set; }            // Stan (true = aktywny)
public string DisplayName { get; set; }    // Pełna nazwa (np. "POV 1 ↑")
public string ShortName { get; set; }      // Skrócona nazwa (np. "P1↑")
```

**Użycie w DirectInputDevice:**
```csharp
// Tworzenie POV (4 kierunki dla każdego POV)
Povs = Enumerable.Range(0, _joystick.Capabilities.PovCount)
    .SelectMany(x => Enumerable.Range(0, 4)
        .Select(y => new { Id = x, Direction = (DirectInputPovDirection)y }))
    .Select(x => new DirectInputPov(this, x.Id, x.Direction))
    .ToArray();

// Pobieranie POV
public DirectInputPov GetPov(int id, DirectInputPovDirection direction) {
    return id < 0 ? null : Povs.FirstOrDefault(x => 
        x.Id == id && x.Direction == direction
    );
}

// Aktualizacja stanu
b.Value = b.Direction.IsInRange(
    b.Id < povs.Length ? povs[b.Id] : -1
);
```

### 4. DirectInputPovDirection

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/DirectInputPovDirection.cs`

**Definicja:**
```csharp
public enum DirectInputPovDirection {
    Left = 0,
    Up = 1,
    Right = 2,
    Down = 3
}
```

**Rozszerzenie:**
```csharp
public static class DirectInputPovDirectionExtension {
    public static bool IsInRange(this DirectInputPovDirection direction, int value) {
        // value to kąt w setnych stopniach (0-36000)
        switch (direction) {
            case DirectInputPovDirection.Left:
                return value > 22500 && value <= 31500;  // 225° - 315°
            case DirectInputPovDirection.Up:
                return value >= 0 && value <= 4500 || value > 31500;  // 0° - 45° lub 315° - 360°
            case DirectInputPovDirection.Right:
                return value > 4500 && value <= 13500;  // 45° - 135°
            case DirectInputPovDirection.Down:
                return value > 13500 && value <= 22500;  // 135° - 225°
        }
    }
}
```

**Użycie:** Sprawdza czy POV jest w danym kierunku na podstawie kąta z DirectInput.

### 5. InputProviderBase<T>

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/InputProviderBase.cs`

**Hierarchia:**
```
Displayable
    ↓
InputProviderBase<T>
    ├── IInputProvider
    └── IDirectInputProvider (dla DirectInput*)
```

**Reprezentuje:** Bazową klasę dla wszystkich elementów wejściowych

**Właściwości:**
```csharp
public int Id { get; }                     // Indeks elementu
public T Value { get; set; }               // Wartość (double dla osi, bool dla przycisków)
public string DisplayName { get; set; }    // Pełna nazwa
public string ShortName { get; set; }      // Skrócona nazwa
public bool IsVisible { get; set; }        // Czy widoczny w UI
public string DefaultDisplayName { get; }  // Domyślna nazwa
public string DefaultShortName { get; }    // Domyślna skrócona nazwa
```

**Metody:**
```csharp
public void SetDisplayParams(string displayName, bool isVisible)  // Ustawia nazwę i widoczność
protected abstract void SetDisplayName(string displayName)        // Abstrakcyjna - implementowana w klasach pochodnych
protected virtual void OnValueChanged()                           // Wywoływane gdy Value się zmienia
```

**Użycie:**
- `DirectInputAxle` dziedziczy z `InputProviderBase<double>`
- `DirectInputButton` dziedziczy z `InputProviderBase<bool>`
- `DirectInputPov` dziedziczy z `DirectInputButton` (więc też `InputProviderBase<bool>`)

### 6. IDirectInputProvider

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/IDirectInputProvider.cs`

**Definicja:**
```csharp
public interface IDirectInputProvider {
    IDirectInputDevice Device { get; }
}
```

**Implementowane przez:**
- `DirectInputAxle`
- `DirectInputButton`
- `DirectInputPov`

**Cel:** Zapewnia dostęp do urządzenia dla każdego elementu wejściowego.

### 7. IInputProvider

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/IInputProvider.cs`

**Definicja:**
```csharp
public interface IInputProvider : IWithId<int> {
    string DisplayName { get; }
    string ShortName { get; }
    string DefaultShortName { get; }
    string DefaultDisplayName { get; }
    void SetDisplayParams([CanBeNull] string displayName, bool isVisible);
    bool IsVisible { get; }
}
```

**Implementowane przez:**
- `InputProviderBase<T>` (a więc wszystkie klasy pochodne)

**Cel:** Wspólny interfejs dla wszystkich elementów wejściowych (osie, przyciski, POV, klawiatura).

### 8. IDirectInputDevice

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/IDirectInputDevice.cs`

**Definicja:**
```csharp
public interface IDirectInputDevice {
    string InstanceId { get; }
    string ProductId { get; }
    bool IsVirtual { get; }
    bool IsController { get; }
    string DisplayName { get; }
    int Index { get; }
    IList<int> OriginalIniIds { get; }
    
    bool Same(IDirectInputDevice other);
    
    [CanBeNull]
    DirectInputAxle GetAxle(int id);
    
    [CanBeNull]
    DirectInputButton GetButton(int id);
    
    [CanBeNull]
    DirectInputPov GetPov(int id, DirectInputPovDirection direction);
}
```

**Implementowane przez:**
- `DirectInputDevice` (prawdziwe urządzenie)
- `PlaceholderInputDevice` (placeholder dla odłączonych urządzeń)

**Cel:** Abstrakcja pozwalająca na użycie zarówno prawdziwych urządzeń jak i placeholderów.

### 9. DisplayInputParams

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/DisplayInputParams.cs`

**Reprezentuje:** Definicje nazw osi, przycisków i POV z plików JSON

**Użycie w DirectInputDevice:**
```csharp
public void RefreshDescription() {
    // Próba załadowania definicji
    if (!DisplayInputParams.Get(
        Device.ProductGuid.ToString(), 
        out var displayName, 
        out var axisP, 
        out var buttonsP, 
        out var povsP
    ) && IsController) {
        // Fallback dla kontrolerów
        DisplayInputParams.Get(
            DirectInputDeviceUtils.GetXboxControllerGuid(), 
            out _, out axisP, out buttonsP, out povsP
        );
    }
    
    // Przypisanie nazw
    Proc(Axis, axisP);
    Proc(Buttons, buttonsP);
    Proc(Povs, povsP);
}
```

**Format pliku JSON:**
```json
{
  "name": "Logitech G29 Racing Wheel",
  "axis": ["Steering Wheel", "Throttle", "Brakes", "Clutch", ...],
  "buttons": ["Button 1", "Button 2", ...],
  "pov": ["POV Up", "POV Down", ...]
}
```

### 10. DirectInputDeviceUtils

**Lokalizacja:** `AcManager.Tools/Helpers/DirectInput/DirectInputDeviceUtils.cs`

**Metody:**
```csharp
public static bool IsController(string deviceName) {
    return Regex.IsMatch(deviceName, @"^Controller \((.+)\)$");
}

public static string GetXboxControllerGuid() {
    return @"028E045E-0000-0000-0000-504944564944";
}
```

**Użycie:**
- `IsController()` - sprawdza czy urządzenie to kontroler (gamepad)
- `GetXboxControllerGuid()` - zwraca GUID kontrolera Xbox (używany jako fallback)

---

## Diagram relacji klas

```
┌─────────────────────────────────────┐
│      DirectInputDevice              │
│  (główna klasa - reprezentuje       │
│   urządzenie DirectInput)           │
└──────────────┬──────────────────────┘
               │
               ├─── tworzy ───┐
               │              │
               ▼              ▼
    ┌──────────────────┐  ┌──────────────────┐
    │ DirectInputAxle  │  │ DirectInputButton│
    │  (8 osi)         │  │  (N przycisków)  │
    └──────────────────┘  └────────┬─────────┘
                                   │
                                   │ dziedziczy
                                   ▼
                          ┌──────────────────┐
                          │  DirectInputPov  │
                          │  (4 kierunki     │
                          │   dla każdego)   │
                          └──────────────────┘
                                   │
                                   │ wszystkie dziedziczą z
                                   ▼
                          ┌──────────────────┐
                          │InputProviderBase │
                          │     <T>          │
                          └──────────────────┘
                                   │
                                   │ implementuje
                                   ▼
                          ┌──────────────────┐
                          │ IInputProvider   │
                          │ IDirectInput     │
                          │   Provider      │
                          └──────────────────┘
```

---

## Przykłady użycia

### Przykład 1: Tworzenie urządzenia

```csharp
// W DirectInputScanner lub ControlsSettings
var joystick = new Joystick(directInput, deviceGuid);
var device = DirectInputDevice.Create(joystick, 0);

if (device != null) {
    Console.WriteLine($"Urządzenie: {device.DisplayName}");
    Console.WriteLine($"Osi: {device.Axis.Length}");
    Console.WriteLine($"Przycisków: {device.Buttons.Length}");
    Console.WriteLine($"POV: {device.Povs.Length}");
}
```

### Przykład 2: Pobieranie osi

```csharp
// Pobranie osi kierownicy (indeks 0)
var steeringAxis = device.GetAxle(0);
if (steeringAxis != null) {
    Console.WriteLine($"Kierownica: {steeringAxis.DisplayName}");
    Console.WriteLine($"Wartość: {steeringAxis.Value}");  // 0.0 - 1.0
}

// Pobranie osi gazu (indeks 1)
var throttleAxis = device.GetAxle(1);
```

### Przykład 3: Odczyt wartości

```csharp
// OnTick() jest wywoływane automatycznie przez ControlsSettings
// Ale można też wywołać ręcznie:
device.OnTick();

// Po wywołaniu OnTick(), wartości są zaktualizowane:
foreach (var axis in device.Axis) {
    Console.WriteLine($"{axis.DisplayName}: {axis.Value}");
}

foreach (var button in device.Buttons) {
    if (button.Value) {
        Console.WriteLine($"{button.DisplayName} jest naciśnięty");
    }
}
```

### Przykład 4: Filtrowanie widocznych elementów

```csharp
// Po RefreshDescription(), VisibleAxis zawiera tylko widoczne osie
foreach (var axis in device.VisibleAxis) {
    Console.WriteLine($"{axis.DisplayName} (widoczna)");
}

// Można też sprawdzić wszystkie osie:
foreach (var axis in device.Axis) {
    if (axis.IsVisible) {
        Console.WriteLine($"{axis.DisplayName}");
    }
}
```

### Przykład 5: Porównywanie urządzeń

```csharp
var device1 = DirectInputDevice.Create(joystick1, 0);
var device2 = DirectInputDevice.Create(joystick2, 1);

if (device1.Same(device2)) {
    Console.WriteLine("To to samo urządzenie");
} else {
    Console.WriteLine("Różne urządzenia");
}
```

---

## Obsługa błędów

### 1. Urządzenie odłączone

```csharp
catch (DirectInputException e) when (e.Message.Contains(@"DIERR_UNPLUGGED")) {
    Unplugged = true;
}
```

**Reakcja:** Ustawia flagę `Unplugged = true`, ale nie usuwa urządzenia z listy.

### 2. Błąd podczas tworzenia

```csharp
public static DirectInputDevice Create(Joystick device, int iniId) {
    try {
        return new DirectInputDevice(device, iniId);
    } catch (DirectInputNotFoundException e) {
        Logging.Warning(e);
        return null;  // Zwraca null zamiast rzucać wyjątek
    } catch (DirectInputException e) {
        Logging.Error(e);
        return null;
    }
}
```

**Reakcja:** Loguje błąd i zwraca `null` zamiast rzucać wyjątek.

### 3. Nie można nabyć urządzenia (Acquire)

```csharp
if (!Acquire(_joystick)) {
    // Próba ponowna
    _joystick.SetCooperativeLevel(...);
    Acquire(_joystick);
}
```

**Reakcja:** Próbuje ponownie z innym cooperative level.

---

## Wydajność i optymalizacje

### 1. Częstotliwość OnTick()

- Wywoływane ~60 razy na sekundę (przy każdym renderowaniu UI)
- Używa `RoundedValue` w `DirectInputAxle` do zmniejszenia liczby eventów
- Aktualizuje tylko gdy zmiana > 1%

### 2. Tworzenie osi

- Zawsze tworzy 8 osi (niezależnie od rzeczywistej liczby)
- Niektóre osie mogą nie istnieć w urządzeniu (zwracają 0.0)
- To upraszcza kod i zapewnia spójność

### 3. Caching definicji

- `DisplayInputParams` ładuje definicje z plików JSON
- Definicje są cache'owane przez `FilesStorage`
- `RefreshDescription()` jest wywoływane tylko gdy potrzebne

---

## Podsumowanie

Klasa `DirectInputDevice` jest centralnym komponentem systemu obsługi kontrolerów:

1. **Reprezentuje urządzenie** - opakowuje `Joystick` z SlimDX DirectInput
2. **Zarządza elementami** - tworzy i zarządza osiami, przyciskami i POV
3. **Odczytuje wartości** - cyklicznie aktualizuje stany wszystkich elementów
4. **Przypisuje nazwy** - ładuje czytelne nazwy z plików JSON
5. **Udostępnia API** - metody do pobierania elementów po indeksie
6. **Obsługuje błędy** - gracefully obsługuje odłączone urządzenia i błędy

**Kluczowe zależności:**
- `DirectInputAxle` - reprezentuje osie
- `DirectInputButton` - reprezentuje przyciski
- `DirectInputPov` - reprezentuje POV
- `InputProviderBase<T>` - bazowa klasa dla elementów wejściowych
- `DisplayInputParams` - definicje nazw
- `IDirectInputDevice` - interfejs urządzenia

---

**Dokumentacja utworzona na podstawie analizy kodu źródłowego Content Managera.**

