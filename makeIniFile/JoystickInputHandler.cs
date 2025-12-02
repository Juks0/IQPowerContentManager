using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using SlimDX;
using SlimDX.DirectInput;

namespace IQPowerContentManager
{
#nullable enable
    public class JoystickInputHandler
    {
        private DirectInput? _directInput;
        private Joystick? _joystick;
        private JoystickState? _lastState;
        private bool _isInitialized = false;
        private bool _unplugged = false;
        private bool _error = false;

        // Struktury do przechowywania stanu inputu
        public class AxisInfo : INotifyPropertyChanged
        {
            private double _value;
            private double _roundedValue;

            public int Id { get; set; }
            public string Name { get; set; } = "";

            /// <summary>
            /// Aktualna wartość osi (0.0-1.0) - aktualizowana w każdej klatce
            /// </summary>
            public double Value
            {
                get => _value;
                set
                {
                    // Ignoruj bardzo małe zmiany (mniejsze niż 0.01% - 0.0001)
                    // Ale zawsze aktualizuj jeśli zmiana jest większa
                    if (Math.Abs(_value - value) < 0.0001) return;

                    _value = value;
                    OnValueChanged();
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Zaokrąglona wartość osi - aktualizowana tylko gdy zmiana > 1% (0.01)
            /// Zgodnie z Content Managerem: zmniejsza liczbę eventów PropertyChanged
            /// </summary>
            public double RoundedValue
            {
                get => _roundedValue;
                private set
                {
                    if (Math.Abs(_roundedValue - value) < 0.0001) return;
                    _roundedValue = value;
                    OnPropertyChanged();
                }
            }

            /// <summary>
            /// Różnica między Value a RoundedValue
            /// </summary>
            public double Delta { get; private set; }

            /// <summary>
            /// Wywoływane gdy Value się zmienia - aktualizuje RoundedValue jeśli zmiana > 1%
            /// Zgodnie z Content Managerem: DirectInputAxle.OnValueChanged()
            /// </summary>
            private void OnValueChanged()
            {
                var value = Value;

                // Aktualizuj tylko jeśli zmiana > 0.01 (1%) - zgodnie z Content Managerem
                // Używamy Math.Abs() zamiast .Abs() extension method
                if (Math.Abs(value - RoundedValue) < 0.01) return;

                // Oblicz Delta przed aktualizacją RoundedValue
                Delta = value - RoundedValue;
                RoundedValue = value;

                // ⭐ To wywołuje PropertyChanged dla RoundedValue (przez setter)
                // Używane przez WheelAxleEntry do wykrywania ruchu osi podczas przypisywania bindów
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? ""));
            }
        }

        public class ButtonInfo
        {
            public int Id { get; set; }
            public bool Value { get; set; }
        }

        public class PovInfo
        {
            public int Id { get; set; }
            public int Direction { get; set; } // -1 jeśli nieaktywny, 0-36000 dla kierunku
            public bool Value { get; set; }
        }

        public List<AxisInfo> Axes { get; private set; } = new List<AxisInfo>();
        public List<ButtonInfo> Buttons { get; private set; } = new List<ButtonInfo>();
        public List<PovInfo> Povs { get; private set; } = new List<PovInfo>();

        // Śledzenie poprzednich wartości osi do wykrywania zmian
        private double[] _previousAxisValues = new double[8];

        public string DeviceName { get; private set; } = "";
        public Guid DeviceGuid { get; private set; }
        public bool IsConnected => _isInitialized && !_unplugged && !_error;
        public bool Unplugged => _unplugged;
        public bool Error => _error;

        /// <summary>
        /// Inicjalizuje DirectInput i znajduje pierwsze dostępne urządzenie joystick
        /// </summary>
        public bool Initialize()
        {
            try
            {
                _directInput = new DirectInput();
                var devices = _directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);

                if (devices.Count == 0)
                {
                    Console.WriteLine("Nie znaleziono żadnego kontrolera.");
                    return false;
                }

                // Użyj pierwszego dostępnego urządzenia
                var deviceInstance = devices[0];
                DeviceName = deviceInstance.InstanceName;
                DeviceGuid = deviceInstance.InstanceGuid;

                _joystick = new Joystick(_directInput, deviceInstance.InstanceGuid);

                // Spróbuj ustawić cooperative level - niektóre kontrolery mogą tego wymagać
                try
                {
                    IntPtr consoleHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                    if (consoleHandle != IntPtr.Zero)
                    {
                        _joystick.SetCooperativeLevel(consoleHandle, CooperativeLevel.Background);
                    }
                }
                catch
                {
                    // Jeśli SetCooperativeLevel nie działa, spróbuj kontynuować bez niego
                }

                // Ustaw tryb osi na Absolute (0-65535) zgodnie z Content Managerem
                _joystick.Properties.AxisMode = DeviceAxisMode.Absolute;

                _joystick.Acquire();

                // Inicjalizuj osie - ustaw zakres dla każdej osi (0-65535 dla Absolute mode)
                // Zgodnie z Content Managerem: wartości są w zakresie 0-65535 (unsigned 16-bit)
                try
                {
                    foreach (var objectInfo in _joystick.GetObjects())
                    {
                        if ((objectInfo.ObjectType & ObjectDeviceType.Axis) != 0)
                        {
                            try
                            {
                                var properties = _joystick.GetObjectPropertiesById((int)objectInfo.ObjectType);
                                // Ustaw zakres na 0-65535 (unsigned 16-bit) dla Absolute mode
                                // W SlimDX SetRange przyjmuje signed int, więc używamy -32768 do +32767
                                // ale w Absolute mode wartości są interpretowane jako 0-65535
                                properties.SetRange(-32768, 32767);
                            }
                            catch
                            {
                                // Ignoruj błędy ustawiania zakresu dla pojedynczych osi
                            }
                        }
                    }
                }
                catch
                {
                    // Ignoruj błędy podczas inicjalizacji osi
                }

                // Sprawdź liczbę osi, przycisków i POV
                var capabilities = _joystick.Capabilities;

                // Inicjalizuj osie (0-7: X, Y, Z, RX, RY, RZ, Slider0, Slider1)
                for (int i = 0; i < 8; i++)
                {
                    var axis = new AxisInfo { Id = i, Name = GetAxisName(i) };
                    // Inicjalizuj wartość - dla kierownicy (oś 0) ustaw na 0.5 (środek)
                    // Dla innych osi ustaw na 0.0
                    if (i == 0)
                    {
                        // Dla kierownicy (oś 0) - środek to 0.5
                        axis.Value = 0.5; // To wywoła OnValueChanged() i ustawi RoundedValue na 0.5
                    }
                    else
                    {
                        axis.Value = 0.0; // To wywoła OnValueChanged() i ustawi RoundedValue na 0.0
                    }
                    Axes.Add(axis);
                    _previousAxisValues[i] = 0; // Inicjalizuj poprzednie wartości
                }

                // Inicjalizuj przyciski
                for (int i = 0; i < capabilities.ButtonCount; i++)
                {
                    Buttons.Add(new ButtonInfo { Id = i, Value = false });
                }

                // Inicjalizuj POV (Point of View - hat switch)
                for (int i = 0; i < capabilities.PovCount; i++)
                {
                    Povs.Add(new PovInfo { Id = i, Direction = -1, Value = false });
                }

                _isInitialized = true;
                _unplugged = false;
                _error = false;

                Console.WriteLine($"Znaleziono kontroler: {DeviceName}");
                Console.WriteLine($"Osie: {Axes.Count}, Przyciski: {Buttons.Count}, POV: {Povs.Count}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd inicjalizacji kontrolera: {ex.Message}");
                _error = true;
                return false;
            }
        }

        /// <summary>
        /// Inicjalizuje konkretne urządzenie po GUID
        /// </summary>
        public bool Initialize(Guid deviceGuid)
        {
            try
            {
                // Wyczyść poprzednie listy
                Axes.Clear();
                Buttons.Clear();
                Povs.Clear();
                // Wyczyść poprzednie wartości osi
                for (int i = 0; i < _previousAxisValues.Length; i++)
                {
                    _previousAxisValues[i] = 0;
                }

                _directInput = new DirectInput();

                try
                {
                    _joystick = new Joystick(_directInput, deviceGuid);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Błąd tworzenia obiektu Joystick: {ex.Message}", ex);
                }

                // Spróbuj ustawić cooperative level - niektóre kontrolery mogą tego wymagać
                // Dla aplikacji konsolowej używamy handle konsoli lub pomijamy
                try
                {
                    // Spróbuj najpierw z handle konsoli
                    IntPtr consoleHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                    if (consoleHandle == IntPtr.Zero)
                    {
                        // Jeśli nie ma handle okna, spróbuj bez cooperative level
                        // Niektóre kontrolery działają bez tego
                    }
                    else
                    {
                        _joystick.SetCooperativeLevel(consoleHandle, CooperativeLevel.Background);
                    }
                }
                catch
                {
                    // Jeśli SetCooperativeLevel nie działa, spróbuj kontynuować bez niego
                    // Niektóre kontrolery działają bez tego ustawienia
                }

                // Ustaw tryb osi na Absolute (0-65535) zgodnie z Content Managerem
                _joystick.Properties.AxisMode = DeviceAxisMode.Absolute;

                var acquireResult = _joystick.Acquire();
                if (acquireResult.IsFailure)
                {
                    throw new Exception($"Nie udało się przejąć kontrolera: {acquireResult}");
                }

                DeviceName = _joystick.Information.InstanceName;
                DeviceGuid = deviceGuid;

                // Inicjalizuj osie - ustaw zakres dla każdej osi (0-65535 dla Absolute mode)
                // Zgodnie z Content Managerem: wartości są w zakresie 0-65535 (unsigned 16-bit)
                try
                {
                    var objects = _joystick.GetObjects();
                    foreach (var objectInfo in objects)
                    {
                        if ((objectInfo.ObjectType & ObjectDeviceType.Axis) != 0)
                        {
                            try
                            {
                                var properties = _joystick.GetObjectPropertiesById((int)objectInfo.ObjectType);
                                // Ustaw zakres na 0-65535 (unsigned 16-bit) dla Absolute mode
                                // W SlimDX SetRange przyjmuje signed int, więc używamy -32768 do +32767
                                // ale w Absolute mode wartości są interpretowane jako 0-65535
                                properties.SetRange(-32768, 32767);
                            }
                            catch
                            {
                                // Ignoruj błędy ustawiania zakresu dla pojedynczych osi
                            }
                        }
                    }
                }
                catch
                {
                    // Ignoruj błędy podczas inicjalizacji osi
                }

                var capabilities = _joystick.Capabilities;

                // Inicjalizuj osie (0-7: X, Y, Z, RX, RY, RZ, Slider0, Slider1)
                for (int i = 0; i < 8; i++)
                {
                    var axis = new AxisInfo { Id = i, Name = GetAxisName(i) };
                    // Inicjalizuj wartość - dla kierownicy (oś 0) ustaw na 0.5 (środek)
                    // Dla innych osi ustaw na 0.0
                    if (i == 0)
                    {
                        // Dla kierownicy (oś 0) - środek to 0.5
                        axis.Value = 0.5; // To wywoła OnValueChanged() i ustawi RoundedValue na 0.5
                    }
                    else
                    {
                        axis.Value = 0.0; // To wywoła OnValueChanged() i ustawi RoundedValue na 0.0
                    }
                    Axes.Add(axis);
                    _previousAxisValues[i] = 0; // Inicjalizuj poprzednie wartości
                }

                // Inicjalizuj przyciski
                for (int i = 0; i < capabilities.ButtonCount; i++)
                {
                    Buttons.Add(new ButtonInfo { Id = i, Value = false });
                }

                // Inicjalizuj POV
                for (int i = 0; i < capabilities.PovCount; i++)
                {
                    Povs.Add(new PovInfo { Id = i, Direction = -1, Value = false });
                }

                _isInitialized = true;
                _unplugged = false;
                _error = false;

                return true;
            }
            catch (Exception ex)
            {
                // Wyczyść stan w przypadku błędu
                if (_joystick != null && !_joystick.Disposed)
                {
                    try
                    {
                        _joystick.Unacquire();
                        _joystick.Dispose();
                    }
                    catch { }
                }
                if (_directInput != null)
                {
                    try
                    {
                        _directInput.Dispose();
                    }
                    catch { }
                }
                _joystick = null;
                _directInput = null;
                _error = true;
                // Przekaż szczegóły błędu przez wyjątek
                throw new Exception($"Błąd inicjalizacji kontrolera '{DeviceName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Aktualizuje stan inputu z kontrolera (wywołuj w pętli)
        /// Zgodnie z dokumentacją Content Managera:
        /// - Jedno wywołanie GetCurrentState() — pobiera pełny stan urządzenia
        /// - Jedna pętla dla osi — aktualizuje wszystkie 8 osi
        /// - Jedna pętla dla przycisków — aktualizuje wszystkie przyciski
        /// - Jedna pętla dla POV — aktualizuje wszystkie POV
        /// </summary>
        public void OnTick()
        {
            try
            {
                if (!_isInitialized || _joystick == null || _joystick.Disposed)
                {
                    return;
                }

                var acquireResult = _joystick.Acquire();
                var pollResult = _joystick.Poll();

                if (acquireResult.IsFailure || pollResult.IsFailure)
                {
                    // Jeśli nie udało się przejąć kontrolera, oznacz jako odłączony
                    if (acquireResult.IsFailure)
                    {
                        _unplugged = true;
                    }
                    return;
                }

                // 2. Jedno wywołanie GetCurrentState() — pobiera pełny stan urządzenia
                var state = _joystick.GetCurrentState();

                // 3. Jedna pętla dla osi — aktualizuje wszystkie 8 osi
                for (var i = 0; i < Axes.Count; i++)
                {
                    var axis = Axes[i];

                    // DEBUG: Pokaż surowe wartości z DirectInput przed normalizacją
                    int rawValue = 0;
                    switch (axis.Id)
                    {
                        case 0: rawValue = state.X; break;
                        case 1: rawValue = state.Y; break;
                        case 2: rawValue = state.Z; break;
                        case 3: rawValue = state.RotationX; break;
                        case 4: rawValue = state.RotationY; break;
                        case 5: rawValue = state.RotationZ; break;
                        case 6: rawValue = state.GetSliders().Length > 0 ? state.GetSliders()[0] : 0; break;
                        case 7: rawValue = state.GetSliders().Length > 1 ? state.GetSliders()[1] : 0; break;
                    }

                    var newValue = GetAxisValue(axis.Id, state);

                    // DEBUG: Pokaż zmiany wartości dla ważnych osi:
                    // - Oś 0 (X): kierownica lub gaz z pedałów
                    // - Oś 1 (Y): hamulce z pedałów
                    // - Oś 2 (Z): sprzęgło z pedałów
                    // - Oś 6, 7: hamulec ręczny
                    bool shouldDebug = false;
                    string axisDescription = "";

                    if (axis.Id == 0)
                    {
                        // Oś 0 może być kierownicą (R16 Base) lub gazem (pedały)
                        if (DeviceName.Contains("R16") || DeviceName.Contains("Base") || DeviceName.Contains("Wheel"))
                        {
                            axisDescription = "Kierownica";
                            shouldDebug = true;
                        }
                        else if (DeviceName.Contains("pedal") || DeviceName.Contains("Pedal") || DeviceName.Contains("CRP"))
                        {
                            axisDescription = "Gaz (Throttle)";
                            shouldDebug = true;
                        }
                        else
                        {
                            axisDescription = "X";
                            shouldDebug = true; // Dla bezpieczeństwa pokazuj wszystkie osie 0
                        }
                    }
                    else if (axis.Id == 1)
                    {
                        // Oś 1 to hamulce w pedałach
                        if (DeviceName.Contains("pedal") || DeviceName.Contains("Pedal") || DeviceName.Contains("CRP"))
                        {
                            axisDescription = "Hamulce (Brakes)";
                            shouldDebug = true;
                        }
                        else
                        {
                            axisDescription = "Y";
                            shouldDebug = true; // Dla bezpieczeństwa pokazuj wszystkie osie 1
                        }
                    }
                    else if (axis.Id == 2)
                    {
                        // Oś 2 to sprzęgło w pedałach
                        if (DeviceName.Contains("pedal") || DeviceName.Contains("Pedal") || DeviceName.Contains("CRP"))
                        {
                            axisDescription = "Sprzęgło (Clutch)";
                            shouldDebug = true;
                        }
                        else
                        {
                            axisDescription = "Z";
                            shouldDebug = true; // Dla bezpieczeństwa pokazuj wszystkie osie 2
                        }
                    }
                    else if (axis.Id == 6 || axis.Id == 7)
                    {
                        // Oś 6 lub 7 to hamulec ręczny
                        if (DeviceName.Contains("Handbrake") || DeviceName.Contains("handbrake") || DeviceName.Contains("HBP"))
                        {
                            axisDescription = axis.Id == 6 ? "Hamulec ręczny (Slider0)" : "Hamulec ręczny (Slider1)";
                            shouldDebug = true;
                        }
                        else
                        {
                            axisDescription = axis.Id == 6 ? "Slider0" : "Slider1";
                            shouldDebug = true; // Dla bezpieczeństwa pokazuj wszystkie osie 6 i 7
                        }
                    }

                    // Pokaż debugowanie jeśli wartość się zmieniła o więcej niż 0.001 (0.1%)
                    if (shouldDebug && Math.Abs(newValue - axis.Value) > 0.001)
                    {
                        string axisName = GetAxisName(axis.Id);
                        Console.WriteLine($"[DEBUG OnTick] [{DeviceName}] Axis {axis.Id} ({axisName}) - {axisDescription}: Raw={rawValue}, Old={axis.Value:F3}, New={newValue:F3}, Change={newValue - axis.Value:F3}");
                    }

                    // Zapisz poprzednią wartość PRZED aktualizacją
                    // Jeśli to pierwszy odczyt (_previousAxisValues[i] == 0 i axis.Value == 0),
                    // ustaw poprzednią wartość na aktualną, aby uniknąć fałszywych wykryć
                    if (_previousAxisValues[i] == 0 && axis.Value == 0 && newValue != 0)
                    {
                        _previousAxisValues[i] = newValue; // Inicjalizuj poprzednią wartością aktualną
                    }
                    else
                    {
                        _previousAxisValues[i] = axis.Value; // Zapisz poprzednią wartość
                    }

                    // Aktualizuj wartość osi (zgodnie z dokumentacją)
                    axis.Value = newValue;
                }

                // 4. Jedna pętla dla przycisków — aktualizuje wszystkie przyciski
                var buttons = state.GetButtons();
                for (var i = 0; i < Buttons.Count; i++)
                {
                    var button = Buttons[i];
                    // Zgodnie z dokumentacją: b.Value = b.Id < buttons.Length && buttons[b.Id]
                    button.Value = button.Id < buttons.Length && buttons[button.Id];
                }

                // 5. Jedna pętla dla POV — aktualizuje wszystkie POV
                var povs = state.GetPointOfViewControllers();
                for (var i = 0; i < Povs.Count; i++)
                {
                    var pov = Povs[i];
                    var povValue = pov.Id < povs.Length ? povs[pov.Id] : -1;
                    pov.Direction = povValue;
                    // POV jest aktywny jeśli wartość != -1
                    pov.Value = povValue != -1;
                }

                _lastState = state;
            }
            catch (DirectInputException e) when (e.Message.Contains(@"DIERR_UNPLUGGED"))
            {
                _unplugged = true;
            }
            catch (DirectInputException e)
            {
                if (!_error)
                {
                    Console.WriteLine($"Błąd odczytu kontrolera: {e.Message}");
                    _error = true;
                }
            }
        }

        /// <summary>
        /// Pobiera wartość osi (0.0 - 1.0) zgodnie z Content Managerem
        /// Zgodnie z dokumentacją: w trybie Absolute wartości są w zakresie 0-65535 (unsigned 16-bit)
        /// Ale SlimDX może zwracać signed int (-32768 do +32767), więc konwertujemy do unsigned
        /// </summary>
        private double GetAxisValue(int id, JoystickState state)
        {
            // Funkcja pomocnicza do konwersji signed int na unsigned (0-65535) i normalizacji do 0.0-1.0
            double NormalizeAxis(int rawValue)
            {
                // DEBUG: Pokaż surową wartość z DirectInput
                // Console.WriteLine($"[DEBUG GetAxisValue] Raw value: {rawValue}");

                // Jeśli wartość jest ujemna (signed), konwertuj na unsigned
                uint unsignedValue = rawValue < 0 ? (uint)(rawValue + 65536) : (uint)rawValue;
                // Normalizuj do zakresu 0.0-1.0
                double normalized = unsignedValue / 65535d;

                // DEBUG: Pokaż znormalizowaną wartość
                // Console.WriteLine($"[DEBUG GetAxisValue] Unsigned: {unsignedValue}, Normalized: {normalized:F3}");

                return normalized;
            }

            int rawValue = 0;
            switch (id)
            {
                case 0: // X - Kierownica (środek = 0.5)
                    rawValue = state.X;
                    break;
                case 1: // Y - Gaz/Throttle (0.0 = min, 1.0 = max)
                    rawValue = state.Y;
                    break;
                case 2: // Z - Hamulec/Brake (0.0 = min, 1.0 = max)
                    rawValue = state.Z;
                    break;
                case 3: // RotationX - Sprzęgło/Clutch (0.0 = min, 1.0 = max)
                    rawValue = state.RotationX;
                    break;
                case 4: // RotationY
                    rawValue = state.RotationY;
                    break;
                case 5: // RotationZ
                    rawValue = state.RotationZ;
                    break;
                case 6: // Slider0 - Hamulec ręczny lub dodatkowa oś
                    if (state.GetSliders().Length > 0)
                    {
                        rawValue = state.GetSliders()[0];
                    }
                    else
                    {
                        return 0d;
                    }
                    break;
                case 7: // Slider1
                    if (state.GetSliders().Length > 1)
                    {
                        rawValue = state.GetSliders()[1];
                    }
                    else
                    {
                        return 0d;
                    }
                    break;
                default:
                    return 0;
            }

            return NormalizeAxis(rawValue);
        }

        /// <summary>
        /// Zwraca nazwę osi
        /// </summary>
        public string GetAxisName(int id)
        {
            switch (id)
            {
                case 0: return "X";
                case 1: return "Y";
                case 2: return "Z";
                case 3: return "RX";
                case 4: return "RY";
                case 5: return "RZ";
                case 6: return "Slider0";
                case 7: return "Slider1";
                default: return $"Axis{id}";
            }
        }

        /// <summary>
        /// Zwraca listę wszystkich dostępnych urządzeń
        /// </summary>
        public static List<DeviceInfo> GetAvailableDevices()
        {
            var devices = new List<DeviceInfo>();
            try
            {
                var directInput = new DirectInput();
                var deviceList = directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);

                foreach (var deviceInstance in deviceList)
                {
                    devices.Add(new DeviceInfo
                    {
                        Name = deviceInstance.InstanceName,
                        Guid = deviceInstance.InstanceGuid,
                        ProductGuid = deviceInstance.ProductGuid
                    });
                }

                directInput.Dispose();
            }
            catch (BadImageFormatException ex)
            {
                Console.WriteLine($"Błąd architektury: SlimDX wymaga x86. Szczegóły: {ex.Message}");
                Console.WriteLine("Uruchom projekt w trybie x86 lub zainstaluj odpowiednią wersję SlimDX.");
            }
            catch (System.IO.FileNotFoundException ex)
            {
                Console.WriteLine($"Nie znaleziono biblioteki SlimDX: {ex.Message}");
                Console.WriteLine("Upewnij się, że plik SlimDX.dll znajduje się w katalogu libs lub zainstalowany jest pakiet NuGet.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd pobierania listy urządzeń: {ex.Message}");
            }

            return devices;
        }

        /// <summary>
        /// Formatuje GUID do formatu używanego w controls.ini (bez nawiasów klamrowych)
        /// </summary>
        public static string FormatGuid(Guid guid)
        {
            return guid.ToString("D").ToUpper();
        }

        /// <summary>
        /// Zwraca informacje o aktualnie aktywnych inputach (do debugowania)
        /// </summary>
        public string GetActiveInputsInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine($"Kontroler: {DeviceName}");
            info.AppendLine("Aktywne osie:");
            foreach (var axis in Axes)
            {
                if (Math.Abs(axis.Value - 0.5) > 0.01) // Więcej niż 1% od środka
                {
                    info.AppendLine($"  {axis.Name} (ID: {axis.Id}): {axis.Value:F3}");
                }
            }
            info.AppendLine("Naciśnięte przyciski:");
            foreach (var button in Buttons)
            {
                if (button.Value)
                {
                    info.AppendLine($"  Button {button.Id}");
                }
            }
            info.AppendLine("Aktywne POV:");
            foreach (var pov in Povs)
            {
                if (pov.Value)
                {
                    info.AppendLine($"  POV {pov.Id}: {pov.Direction}°");
                }
            }
            return info.ToString();
        }

        /// <summary>
        /// Zwraca pierwszy aktywny input (do automatycznego mapowania)
        /// </summary>
        public InputBinding GetFirstActiveInput()
        {
            // NAJPIERW sprawdź przyciski - mają priorytet, bo są natychmiastowe
            for (int i = 0; i < Buttons.Count; i++)
            {
                if (Buttons[i].Value)
                {
                    // Dla przycisków: Id to indeks przycisku, Value to ID przycisku (do wyświetlania)
                    return new InputBinding { Type = InputType.Button, Id = i, Value = i };
                }
            }

            // Potem sprawdź POV
            for (int i = 0; i < Povs.Count; i++)
            {
                if (Povs[i].Value)
                {
                    return new InputBinding { Type = InputType.Pov, Id = i, Value = Povs[i].Direction / 100.0 };
                }
            }

            // Na końcu sprawdź osie - dla każdej osi sprawdzamy czy jest aktywna
            // Zgodnie z Content Managerem: używamy RoundedValue (aktualizuje się tylko gdy zmiana > 1%)
            // Ale też sprawdzamy aktualną wartość Value aby wykryć ruch
            for (int i = 0; i < Axes.Count; i++)
            {
                var axis = Axes[i];
                var value = axis.Value;
                var roundedValue = axis.RoundedValue;
                var previousValue = _previousAxisValues[i];
                var change = Math.Abs(value - previousValue);

                // Dla osi 0 (X) - może być kierownica (R16 base) lub gaz (pedały)
                // W R16 base: oś X to kierownica (środek = 0.5)
                // W pedałach: oś X to gaz (0.0 = zwolniony, 1.0 = wciśnięty)
                if (i == 0)
                {
                    // Sprawdź czy to kierownica (odchylenie od środka > 1%) lub zmiana wartości > 1%
                    // Dla pedałów: wykryj jeśli wartość > 0.5% (0.005) lub zmiana > 0.5%
                    bool isSteering = DeviceName.Contains("R16") || DeviceName.Contains("Base") || DeviceName.Contains("Wheel");
                    bool isPedal = DeviceName.Contains("pedal") || DeviceName.Contains("Pedal") || DeviceName.Contains("CRP");

                    if (isSteering)
                    {
                        // Kierownica: odchylenie od środka > 1% lub zmiana > 1%
                        if (Math.Abs(value - 0.5) > 0.01 || change > 0.01)
                        {
                            return new InputBinding { Type = InputType.Axis, Id = i, Value = value };
                        }
                    }
                    else if (isPedal)
                    {
                        // Gaz z pedałów: wartość > 0.5% lub zmiana > 0.5% (bardziej czułe dla pedałów)
                        if (value > 0.005 || change > 0.005)
                        {
                            return new InputBinding { Type = InputType.Axis, Id = i, Value = value };
                        }
                    }
                    else
                    {
                        // Dla innych urządzeń: sprawdź oba warunki
                        if (Math.Abs(value - 0.5) > 0.01 || change > 0.01 || value > 0.005)
                        {
                            return new InputBinding { Type = InputType.Axis, Id = i, Value = value };
                        }
                    }
                }
                // Dla osi 1 (Y) - hamulce w pedałach
                // W pedałach: oś Y to hamulce (0.0 = zwolniony, 1.0 = wciśnięty)
                else if (i == 1)
                {
                    bool isPedal = DeviceName.Contains("pedal") || DeviceName.Contains("Pedal") || DeviceName.Contains("CRP");

                    if (isPedal)
                    {
                        // Hamulce z pedałów: wartość > 0.5% lub zmiana > 0.5% (bardziej czułe dla pedałów)
                        if (value > 0.005 || change > 0.005)
                        {
                            return new InputBinding { Type = InputType.Axis, Id = i, Value = value };
                        }
                    }
                    else
                    {
                        // Dla innych urządzeń: standardowa detekcja
                        if (change > 0.01 || value > 0.01)
                        {
                            return new InputBinding { Type = InputType.Axis, Id = i, Value = value };
                        }
                    }
                }
                // Dla osi 2 (Z) - sprzęgło w pedałach
                // W pedałach: oś Z to sprzęgło (0.0 = zwolniony, 1.0 = wciśnięty)
                else if (i == 2)
                {
                    bool isPedal = DeviceName.Contains("pedal") || DeviceName.Contains("Pedal") || DeviceName.Contains("CRP");

                    if (isPedal)
                    {
                        // Sprzęgło z pedałów: wartość > 0.5% lub zmiana > 0.5% (bardziej czułe dla pedałów)
                        if (value > 0.005 || change > 0.005)
                        {
                            return new InputBinding { Type = InputType.Axis, Id = i, Value = value };
                        }
                    }
                    else
                    {
                        // Dla innych urządzeń: standardowa detekcja
                        if (change > 0.01 || value > 0.01)
                        {
                            return new InputBinding { Type = InputType.Axis, Id = i, Value = value };
                        }
                    }
                }
                // Dla innych osi (3-7) - dodatkowe osie
                else if (i > 2)
                {
                    // Wykryj zmianę wartości > 1%
                    if (change > 0.01)
                    {
                        return new InputBinding { Type = InputType.Axis, Id = i, Value = value };
                    }
                }
            }

            return null!;
        }

        /// <summary>
        /// Wykrywa kombinację przycisków dla skrzyni biegów H (manualnej)
        /// Zwraca numer biegu: -1 = neutral, 1-6 = biegi, 7 = wsteczny
        /// Obsługuje różne konfiguracje: pojedyncze przyciski lub kombinacje osi X/Y
        /// </summary>
        public int GetHShifterGear()
        {
            if (!_isInitialized)
                return -1;

            // Oś X: -1.0 (lewo) do 1.0 (prawo) - poziome położenie dźwigni
            // Oś Y: -1.0 (góra) do 1.0 (dół) - pionowe położenie dźwigni
            if (Axes.Count >= 2)
            {
                double x = Axes[0].Value; // Może być oś X lub inna
                double y = Axes[1].Value; // Może być oś Y lub inna

                // Sprawdź czy to może być skrzynia H (wartości są w zakresie 0.0-1.0)
                // Dla skrzyni H typowe wartości to kombinacje osi
                // Można też sprawdzić czy są dodatkowe osie (np. Slider0, Slider1)
                for (int i = 6; i < Axes.Count && i < 8; i++)
                {
                    double axisX = Axes[i].Value;
                    double axisY = (i + 1 < Axes.Count) ? Axes[i + 1].Value : 0;

                    // Mapowanie dla skrzyni H na podstawie pozycji osi
                    // To wymaga kalibracji, ale ogólna logika:
                    if (axisX < 0.2 && axisY < 0.2) return 1; // Bieg 1 (lewy górny)
                    if (axisX > 0.8 && axisY < 0.2) return 2; // Bieg 2 (prawy górny)
                    if (axisX < 0.2 && axisY > 0.5 && axisY < 0.8) return 3; // Bieg 3 (lewy środkowy)
                    if (axisX > 0.8 && axisY > 0.5 && axisY < 0.8) return 4; // Bieg 4 (prawy środkowy)
                    if (axisX < 0.2 && axisY > 0.8) return 5; // Bieg 5 (lewy dolny)
                    if (axisX > 0.8 && axisY > 0.8) return 6; // Bieg 6 (prawy dolny)
                    if (axisX > 0.4 && axisX < 0.6 && axisY > 0.8) return 7; // Wsteczny (środek dolny)
                }
            }

            // Metoda 2: Sprawdź przyciski (dla skrzyń H które używają przycisków)
            if (Buttons.Count >= 7)
            {
                // Sprawdź wsteczny (zwykle ostatni przycisk lub przycisk 6)
                if (Buttons.Count > 6 && Buttons[6].Value)
                    return 7; // Wsteczny

                // Sprawdź biegi 1-6
                for (int i = 0; i < 6 && i < Buttons.Count; i++)
                {
                    if (Buttons[i].Value)
                        return i + 1;
                }
            }

            return -1; // Neutral
        }

        /// <summary>
        /// Sprawdza czy skrzynia sekwencyjna jest aktywna (przyciski gear up/down)
        /// Zwraca: -1 = neutral, 0 = gear up naciśnięty, 1 = gear down naciśnięty
        /// </summary>
        public int GetSequentialShifterState(int gearUpButtonId, int gearDownButtonId)
        {
            if (!_isInitialized)
                return -1;

            if (gearUpButtonId >= 0 && gearUpButtonId < Buttons.Count && Buttons[gearUpButtonId].Value)
                return 0; // Gear up

            if (gearDownButtonId >= 0 && gearDownButtonId < Buttons.Count && Buttons[gearDownButtonId].Value)
                return 1; // Gear down

            return -1; // Neutral
        }

        /// <summary>
        /// Sprawdza czy skrzynia sekwencyjna jest aktywna (przyciski gear up/down)
        /// </summary>
        public bool IsSequentialShifterActive(int gearUpButtonId, int gearDownButtonId)
        {
            return GetSequentialShifterState(gearUpButtonId, gearDownButtonId) != -1;
        }

        /// <summary>
        /// Pobiera wartość kierownicy (0.0 = skręt w lewo, 0.5 = środek, 1.0 = skręt w prawo)
        /// </summary>
        public double GetSteeringValue(int axisId = 0)
        {
            if (axisId >= 0 && axisId < Axes.Count)
                return Axes[axisId].Value;
            return 0.5; // Środek
        }

        /// <summary>
        /// Pobiera wartość pedału gazu (0.0 = zwolniony, 1.0 = wciśnięty)
        /// </summary>
        public double GetThrottleValue(int axisId = 1)
        {
            if (axisId >= 0 && axisId < Axes.Count)
                return Axes[axisId].Value;
            return 0.0;
        }

        /// <summary>
        /// Pobiera wartość pedału hamulca (0.0 = zwolniony, 1.0 = wciśnięty)
        /// </summary>
        public double GetBrakeValue(int axisId = 2)
        {
            if (axisId >= 0 && axisId < Axes.Count)
                return Axes[axisId].Value;
            return 0.0;
        }

        /// <summary>
        /// Pobiera wartość pedału sprzęgła (0.0 = zwolniony, 1.0 = wciśnięty)
        /// </summary>
        public double GetClutchValue(int axisId = 3)
        {
            if (axisId >= 0 && axisId < Axes.Count)
                return Axes[axisId].Value;
            return 0.0;
        }

        /// <summary>
        /// Sprawdza czy hamulec ręczny jest wciśnięty (przycisk lub oś)
        /// </summary>
        public bool IsHandbrakePressed(int buttonId = -1, int axisId = -1)
        {
            // Sprawdź przycisk
            if (buttonId >= 0 && buttonId < Buttons.Count && Buttons[buttonId].Value)
                return true;

            // Sprawdź oś (jeśli hamulec ręczny jest na osi)
            if (axisId >= 0 && axisId < Axes.Count && Axes[axisId].Value > 0.5)
                return true;

            return false;
        }

        /// <summary>
        /// Zwalnia zasoby
        /// </summary>
        public void Dispose()
        {
            if (_joystick != null && !_joystick.Disposed)
            {
                _joystick.Unacquire();
                _joystick.Dispose();
            }
            if (_directInput != null)
            {
                _directInput.Dispose();
            }
            _isInitialized = false;
        }
    }

    // Klasy pomocnicze
    public class DeviceInfo
    {
        public string Name { get; set; } = "";
        public Guid Guid { get; set; }
        public Guid ProductGuid { get; set; }
    }

    public enum InputType
    {
        Axis,
        Button,
        Pov
    }

    public class InputBinding
    {
        public InputType Type { get; set; }
        public int Id { get; set; }
        public double Value { get; set; }
    }
}

