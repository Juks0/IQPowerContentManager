using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IQPowerContentManager
{
#nullable enable
    public class Controls
    {
        // [HEADER]
        public int InputMethod { get; set; } = 1;

        // [CONTROLLERS]
        public string Con0 { get; set; } = "";
        public string PGuid0 { get; set; } = "";

        // Lista wszystkich urządzeń (ustawiana przez API)
        public List<ControllerDevice> Devices { get; set; } = new List<ControllerDevice>();

        public class ControllerDevice
        {
            public string Name { get; set; } = "";
            public string InstanceGuid { get; set; } = ""; // __IGUID
            public string ProductGuid { get; set; } = ""; // PGUID
            public int OriginalIndex { get; set; } = -1; // Oryginalny indeks z API przed sortowaniem
        }

        // Bindowanie osi (kierownica, pedały)
        public WheelAxleEntry SteerAxleEntry { get; private set; }
        public WheelAxleEntry ThrottleAxleEntry { get; private set; }
        public WheelAxleEntry BrakesAxleEntry { get; private set; }
        public WheelAxleEntry ClutchAxleEntry { get; private set; }
        public WheelAxleEntry HandbrakeAxleEntry { get; private set; }

        // Bindowanie przycisków
        public WheelButtonEntry GearUpButtonEntry { get; private set; }
        public WheelButtonEntry GearDnButtonEntry { get; private set; }
        public WheelButtonEntry PaddleUpButtonEntry { get; private set; }
        public WheelButtonEntry PaddleDnButtonEntry { get; private set; }
        public WheelButtonEntry HandbrakeButtonEntry { get; private set; }
        public WheelButtonEntry CameraButtonEntry { get; private set; }

        // [SHIFTER] - H-pattern shifter
        public int ShifterActive { get; set; } = 0;
        public int ShifterJoy { get; set; } = -1;
        public Dictionary<string, int> ShifterGears { get; private set; } // GEAR_1, GEAR_2, ..., GEAR_7, GEAR_R

        // [RESET_RACE]
        public string ResetRaceKey { get; set; }

        // Lista wszystkich bindów
        public List<BaseEntry> AllEntries { get; private set; }

        // Właściwości kompatybilności wstecznej (dla starego kodu)
        public int SteerJoy => SteerAxleEntry.ControllerIndex;
        public int SteerAxle => SteerAxleEntry.AxleIndex;
        public double SteerScale => SteerAxleEntry.Scale / 100.0;
        public int SteerLock => SteerAxleEntry.DegreesOfRotation;

        public int ThrottleJoy => ThrottleAxleEntry.ControllerIndex;
        public int ThrottleAxle => ThrottleAxleEntry.AxleIndex;
        public double ThrottleMin => 0.02 * ThrottleAxleEntry.RangeFrom - 1.0;
        public double ThrottleMax => 0.02 * ThrottleAxleEntry.RangeTo - 1.0;

        public int BrakesJoy => BrakesAxleEntry.ControllerIndex;
        public int BrakesAxle => BrakesAxleEntry.AxleIndex;
        public double BrakesMin => 0.02 * BrakesAxleEntry.RangeFrom - 1.0;
        public double BrakesMax => 0.02 * BrakesAxleEntry.RangeTo - 1.0;

        public int ClutchJoy => ClutchAxleEntry.ControllerIndex;
        public int ClutchAxle => ClutchAxleEntry.AxleIndex;
        public double ClutchMin => 0.02 * ClutchAxleEntry.RangeFrom - 1.0;
        public double ClutchMax => 0.02 * ClutchAxleEntry.RangeTo - 1.0;

        public int GearUpJoy => GearUpButtonEntry.ControllerIndex;
        public int GearUpButton => GearUpButtonEntry.ButtonIndex;
        public string GearUpKey => GearUpButtonEntry.KeyCode;

        public int GearDnJoy => GearDnButtonEntry.ControllerIndex;
        public int GearDnButton => GearDnButtonEntry.ButtonIndex;
        public string GearDnKey => GearDnButtonEntry.KeyCode;

        public int HandbrakeJoy => HandbrakeButtonEntry.ControllerIndex;
        public int HandbrakeButton => HandbrakeButtonEntry.ButtonIndex;
        public string HandbrakeKey => HandbrakeButtonEntry.KeyCode;

        public int CameraJoy => CameraButtonEntry.ControllerIndex;
        public int CameraButton => CameraButtonEntry.ButtonIndex;
        public string CameraKey => CameraButtonEntry.KeyCode;

        public Controls()
        {
            // Tworzenie bindów osi
            SteerAxleEntry = new WheelAxleEntry("STEER", "Kierownica", rangeMode: false)
            {
                DegreesOfRotation = 900,
                Scale = 100,
                Filter = 0,
                SpeedSensitivity = 0
            };

            ThrottleAxleEntry = new WheelAxleEntry("THROTTLE", "Gaz", rangeMode: true)
            {
                RangeFrom = 0,
                RangeTo = 100
            };

            BrakesAxleEntry = new WheelAxleEntry("BRAKES", "Hamulce", rangeMode: true)
            {
                RangeFrom = 0,
                RangeTo = 100
            };

            ClutchAxleEntry = new WheelAxleEntry("CLUTCH", "Sprzęgło", rangeMode: true)
            {
                RangeFrom = 0,
                RangeTo = 100
            };

            HandbrakeAxleEntry = new WheelAxleEntry("HANDBRAKE", "Hamulec ręczny", rangeMode: true)
            {
                RangeFrom = 0,
                RangeTo = 100
            };

            // Tworzenie bindów przycisków
            GearUpButtonEntry = new WheelButtonEntry("GEARUP", "Bieg w górę")
            {
                KeyCode = "0x57"
            };

            GearDnButtonEntry = new WheelButtonEntry("GEARDN", "Bieg w dół")
            {
                KeyCode = "0x53"
            };

            PaddleUpButtonEntry = new WheelButtonEntry("PADDLEUP", "Łopatka w górę")
            {
                KeyCode = ""
            };

            PaddleDnButtonEntry = new WheelButtonEntry("PADDLEDN", "Łopatka w dół")
            {
                KeyCode = ""
            };

            HandbrakeButtonEntry = new WheelButtonEntry("HANDBRAKE", "Hamulec ręczny")
            {
                KeyCode = "0x48"
            };

            CameraButtonEntry = new WheelButtonEntry("ACTION_CHANGE_CAMERA", "Zmiana kamery")
            {
                KeyCode = "0x43"
            };

            // Inicjalizacja słownika biegów H-shiftera
            ShifterGears = new Dictionary<string, int>
            {
                { "GEAR_1", -1 },
                { "GEAR_2", -1 },
                { "GEAR_3", -1 },
                { "GEAR_4", -1 },
                { "GEAR_5", -1 },
                { "GEAR_6", -1 },
                { "GEAR_7", -1 },
                { "GEAR_R", -1 }
            };

            // Lista wszystkich bindów
            AllEntries = new List<BaseEntry>
            {
                SteerAxleEntry,
                ThrottleAxleEntry,
                BrakesAxleEntry,
                ClutchAxleEntry,
                HandbrakeAxleEntry,
                GearUpButtonEntry,
                GearDnButtonEntry,
                PaddleUpButtonEntry,
                PaddleDnButtonEntry,
                HandbrakeButtonEntry,
                CameraButtonEntry
            };
        }

        /// <summary>
        /// Znajduje Entry dla danej akcji
        /// </summary>
        public BaseEntry? GetEntry(string action)
        {
            return AllEntries.FirstOrDefault(e => e.Id.Equals(action, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Przypisuje bind osi do akcji
        /// </summary>
        public bool AssignAxisBinding(string action, int controllerIndex, int axleIndex)
        {
            var entry = GetEntry(action) as WheelAxleEntry;
            if (entry == null)
                return false;

            entry.ControllerIndex = controllerIndex;
            entry.AxleIndex = axleIndex;
            return true;
        }

        /// <summary>
        /// Przypisuje bind przycisku do akcji
        /// </summary>
        public bool AssignButtonBinding(string action, int controllerIndex, int buttonIndex)
        {
            var entry = GetEntry(action) as WheelButtonEntry;
            if (entry == null)
                return false;

            entry.ControllerIndex = controllerIndex;
            entry.ButtonIndex = buttonIndex;
            return true;
        }

        /// <summary>
        /// Usuwa bind dla określonej akcji
        /// </summary>
        public bool UnbindAction(string action)
        {
            var entry = GetEntry(action);
            if (entry == null)
                return false;

            entry.Clear();

            // Dla osi, ustaw również AxleIndex na -1
            if (entry is WheelAxleEntry axleEntry)
            {
                axleEntry.AxleIndex = -1;
            }

            // Dla przycisków, ButtonIndex jest już ustawiany w Clear()
            // Dla H-shiftera, obsłuż osobno
            if (action == "H_SHIFTER" || action == "SHIFTER")
            {
                ShifterActive = 0;
                ShifterJoy = -1;
                foreach (var gearKey in ShifterGears.Keys.ToList())
                {
                    ShifterGears[gearKey] = -1;
                }
            }

            return true;
        }

        /// <summary>
        /// Usuwa wszystkie bindy
        /// </summary>
        public void UnbindAll()
        {
            // Wyczyść wszystkie osie
            foreach (var entry in AllEntries)
            {
                entry.Clear();
                if (entry is WheelAxleEntry axleEntry)
                {
                    axleEntry.AxleIndex = -1;
                }
            }

            // Wyczyść H-shifter
            ShifterActive = 0;
            ShifterJoy = -1;
            foreach (var gearKey in ShifterGears.Keys.ToList())
            {
                ShifterGears[gearKey] = -1;
            }
        }

        // Metoda AutoMapFromJoystick została usunięta - użyj interaktywnego bindowania z Program.cs

        /// <summary>
        /// Aktualizuje informacje o kontrolerze z handlera
        /// </summary>
        public void UpdateControllerInfo(JoystickInputHandler handler)
        {
            if (handler != null && handler.IsConnected)
            {
                Con0 = handler.DeviceName;
                PGuid0 = JoystickInputHandler.FormatGuid(handler.DeviceGuid);
                InputMethod = 1; // Wheel
            }
        }

        /// <summary>
        /// Zapisuje ustawienia kontrolerów do pliku INI
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku controls.ini</param>
        public void SaveToFile(string filePath)
        {
            var sb = new StringBuilder();

            // [HEADER]
            sb.AppendLine("[HEADER]");
            string inputMethodStr = InputMethod == 0 ? "KEYBOARD" : (InputMethod == 1 ? "WHEEL" : "GAMEPAD");
            sb.AppendLine($"INPUT_METHOD={inputMethodStr}");
            sb.AppendLine();

            // [CONTROLLERS] - użyj zapisanych urządzeń (ustawionych przez API) lub pobierz automatycznie
            sb.AppendLine("[CONTROLLERS]");

            // Mapowanie: oryginalny indeks z API -> nowy indeks po sortowaniu
            var indexMapping = new Dictionary<int, int>();

            if (Devices != null && Devices.Count > 0)
            {
                // Użyj urządzeń ustawionych przez API
                for (int i = 0; i < Devices.Count; i++)
                {
                    Console.WriteLine($"Device {i}: {Devices[i].Name}");
                    var device = Devices[i];
                    sb.AppendLine($"CON{i}={device.Name}");
                    sb.AppendLine($"__IGUID{i}={EnsureGuidFormat(device.InstanceGuid)}");
                    sb.AppendLine($"PGUID{i}={EnsureGuidFormat(device.ProductGuid)}");

                    // Mapuj oryginalny indeks na nowy indeks po sortowaniu
                    if (device.OriginalIndex >= 0)
                    {
                        indexMapping[device.OriginalIndex] = i;
                    }
                }
            }
            else
            {
                // Fallback: pobierz automatycznie (dla kompatybilności wstecznej)
                var autoDevices = JoystickInputHandler.GetAvailableDevices();
                for (int i = 0; i < autoDevices.Count; i++)
                {
                    var device = autoDevices[i];
                    sb.AppendLine($"CON{i}={device.Name}");
                    sb.AppendLine($"__IGUID{i}={JoystickInputHandler.FormatGuid(device.Guid)}");
                    sb.AppendLine($"PGUID{i}={JoystickInputHandler.FormatGuid(device.ProductGuid)}");
                }
            }
            sb.AppendLine();

            // Funkcja pomocnicza do mapowania oryginalnego indeksu na nowy indeks po sortowaniu
            int MapControllerIndex(int originalIndex)
            {
                if (indexMapping.ContainsKey(originalIndex))
                    return indexMapping[originalIndex];
                return originalIndex; // Fallback: jeśli nie ma mapowania, użyj oryginalnego indeksu
            }

            // Funkcja pomocnicza do normalizacji wartości - jeśli jest całkowita (1.0), zwraca jako liczbę całkowitą (1)
            string NormalizeValue(double value)
            {
                if (value == Math.Truncate(value))
                    return ((int)value).ToString();
                return value.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            }

            // [STEER]
            sb.AppendLine("[STEER]");
            sb.AppendLine($"JOY={MapControllerIndex(SteerAxleEntry.ControllerIndex)}");
            // AxleIndex jest już wewnętrznie od 0, zapisujemy bez zmian (w pliku też od 0)
            int steerAxle = SteerAxleEntry.AxleIndex >= 0 ? SteerAxleEntry.AxleIndex : -1;
            sb.AppendLine($"AXLE={steerAxle}");
            sb.AppendLine($"SCALE={NormalizeValue(SteerAxleEntry.Scale / 100.0)}");
            sb.AppendLine($"LOCK={SteerAxleEntry.DegreesOfRotation}");
            sb.AppendLine($"DEBOUNCING_MS={SteerAxleEntry.DebouncingMs}");
            sb.AppendLine($"FF_GAIN={NormalizeValue(SteerAxleEntry.FfGain)}");
            sb.AppendLine($"FILTER_FF={NormalizeValue(SteerAxleEntry.FilterFf)}");
            sb.AppendLine($"STEER_FILTER={NormalizeValue(SteerAxleEntry.Filter / 100.0)}");
            sb.AppendLine($"SPEED_SENSITIVITY={NormalizeValue(SteerAxleEntry.SpeedSensitivity / 100.0)}");
            sb.AppendLine($"STEER_GAMMA=1");
            sb.AppendLine();

            // [THROTTLE]
            sb.AppendLine("[THROTTLE]");
            sb.AppendLine($"JOY={MapControllerIndex(ThrottleAxleEntry.ControllerIndex)}");
            // AxleIndex jest już wewnętrznie od 0, zapisujemy bez zmian (w pliku też od 0)
            int throttleAxle = ThrottleAxleEntry.AxleIndex >= 0 ? ThrottleAxleEntry.AxleIndex : -1;
            sb.AppendLine($"AXLE={throttleAxle}");
            double throttleMin = 0.02 * ThrottleAxleEntry.RangeFrom - 1.0;
            double throttleMax = 0.02 * ThrottleAxleEntry.RangeTo - 1.0;
            sb.AppendLine($"MIN={NormalizeValue(throttleMin)}");
            sb.AppendLine($"MAX={NormalizeValue(throttleMax)}");
            sb.AppendLine($"GAMMA=1");
            sb.AppendLine();

            // [BRAKES]
            sb.AppendLine("[BRAKES]");
            sb.AppendLine($"JOY={MapControllerIndex(BrakesAxleEntry.ControllerIndex)}");
            // AxleIndex jest już wewnętrznie od 0, zapisujemy bez zmian (w pliku też od 0)
            int brakesAxle = BrakesAxleEntry.AxleIndex >= 0 ? BrakesAxleEntry.AxleIndex : -1;
            sb.AppendLine($"AXLE={brakesAxle}");
            double brakesMin = 0.02 * BrakesAxleEntry.RangeFrom - 1.0;
            double brakesMax = 0.02 * BrakesAxleEntry.RangeTo - 1.0;
            sb.AppendLine($"MIN={NormalizeValue(brakesMin)}");
            sb.AppendLine($"MAX={NormalizeValue(brakesMax)}");
            sb.AppendLine($"GAMMA=1");
            sb.AppendLine();

            // [CLUTCH]
            sb.AppendLine("[CLUTCH]");
            sb.AppendLine($"JOY={MapControllerIndex(ClutchAxleEntry.ControllerIndex)}");
            // AxleIndex jest już wewnętrznie od 0, zapisujemy bez zmian (w pliku też od 0)
            int clutchAxle = ClutchAxleEntry.AxleIndex >= 0 ? ClutchAxleEntry.AxleIndex : -1;
            sb.AppendLine($"AXLE={clutchAxle}");
            double clutchMin = 0.02 * ClutchAxleEntry.RangeFrom - 1.0;
            double clutchMax = 0.02 * ClutchAxleEntry.RangeTo - 1.0;
            sb.AppendLine($"MIN={NormalizeValue(clutchMin)}");
            sb.AppendLine($"MAX={NormalizeValue(clutchMax)}");
            sb.AppendLine($"GAMMA=1");
            sb.AppendLine();

            // [HANDBRAKE]
            sb.AppendLine("[HANDBRAKE]");
            int handbrakeJoy = HandbrakeButtonEntry.ControllerIndex >= 0 ? MapControllerIndex(HandbrakeButtonEntry.ControllerIndex) : (HandbrakeAxleEntry.ControllerIndex >= 0 ? MapControllerIndex(HandbrakeAxleEntry.ControllerIndex) : -1);
            sb.AppendLine($"JOY={handbrakeJoy}");
            sb.AppendLine($"BUTTON={HandbrakeButtonEntry.ButtonIndex}");
            if (!string.IsNullOrEmpty(HandbrakeButtonEntry.KeyCode))
            {
                sb.AppendLine($"KEY={HandbrakeButtonEntry.KeyCode} ; {GetKeyName(HandbrakeButtonEntry.KeyCode)}");
            }
            // AxleIndex jest już wewnętrznie od 0, zapisujemy bez zmian (w pliku też od 0)
            int handbrakeAxle = HandbrakeAxleEntry.AxleIndex >= 0 ? HandbrakeAxleEntry.AxleIndex : -1;
            sb.AppendLine($"AXLE={handbrakeAxle}");
            sb.AppendLine($"GAMMA=1");
            double handbrakeMin = 0.02 * HandbrakeAxleEntry.RangeFrom - 1.0;
            double handbrakeMax = 0.02 * HandbrakeAxleEntry.RangeTo - 1.0;
            sb.AppendLine($"MIN={NormalizeValue(handbrakeMin)}");
            sb.AppendLine($"MAX={NormalizeValue(handbrakeMax)}");
            sb.AppendLine();

            // [GEARUP]
            sb.AppendLine("[GEARUP]");
            sb.AppendLine($"JOY={MapControllerIndex(GearUpButtonEntry.ControllerIndex)}");
            sb.AppendLine($"BUTTON={GearUpButtonEntry.ButtonIndex}");
            if (!string.IsNullOrEmpty(GearUpButtonEntry.KeyCode))
            {
                sb.AppendLine($"KEY={GearUpButtonEntry.KeyCode} ; {GetKeyName(GearUpButtonEntry.KeyCode)}");
            }
            int paddleUpAlt = PaddleUpButtonEntry.ButtonIndex >= 0 ? PaddleUpButtonEntry.ButtonIndex : -1;
            sb.AppendLine($"__CM_ALT_BUTTON={paddleUpAlt}");
            sb.AppendLine();

            // [GEARDN]
            sb.AppendLine("[GEARDN]");
            sb.AppendLine($"JOY={MapControllerIndex(GearDnButtonEntry.ControllerIndex)}");
            sb.AppendLine($"BUTTON={GearDnButtonEntry.ButtonIndex}");
            if (!string.IsNullOrEmpty(GearDnButtonEntry.KeyCode))
            {
                sb.AppendLine($"KEY={GearDnButtonEntry.KeyCode} ; {GetKeyName(GearDnButtonEntry.KeyCode)}");
            }
            int paddleDnAlt = PaddleDnButtonEntry.ButtonIndex >= 0 ? PaddleDnButtonEntry.ButtonIndex : -1;
            sb.AppendLine($"__CM_ALT_BUTTON={paddleDnAlt}");
            int paddleDnAltJoy = PaddleDnButtonEntry.ButtonIndex >= 0 ? MapControllerIndex(PaddleDnButtonEntry.ControllerIndex) : -1;
            sb.AppendLine($"__CM_ALT_JOY={paddleDnAltJoy}");
            sb.AppendLine();

            // [__EXT_GEAR_UP] - drugi bind skrzyni biegów (paddle up)
            sb.AppendLine("[__EXT_GEAR_UP]");
            sb.AppendLine($"JOY={MapControllerIndex(PaddleUpButtonEntry.ControllerIndex)}");
            sb.AppendLine("KEY=-1");
            sb.AppendLine("KEY_MODIFICATOR=");
            sb.AppendLine($"BUTTON={PaddleUpButtonEntry.ButtonIndex}");
            sb.AppendLine("BUTTON_MODIFICATOR=-1");
            sb.AppendLine();

            // [__EXT_GEAR_DOWN] - drugi bind skrzyni biegów (paddle down)
            sb.AppendLine("[__EXT_GEAR_DOWN]");
            sb.AppendLine($"JOY={MapControllerIndex(PaddleDnButtonEntry.ControllerIndex)}");
            sb.AppendLine("KEY=-1");
            sb.AppendLine("KEY_MODIFICATOR=");
            sb.AppendLine($"BUTTON={PaddleDnButtonEntry.ButtonIndex}");
            sb.AppendLine("BUTTON_MODIFICATOR=-1");
            sb.AppendLine();

            // [ACTION_CHANGE_CAMERA]
            sb.AppendLine("[ACTION_CHANGE_CAMERA]");
            sb.AppendLine($"JOY={MapControllerIndex(CameraButtonEntry.ControllerIndex)}");
            sb.AppendLine($"BUTTON={CameraButtonEntry.ButtonIndex}");
            if (!string.IsNullOrEmpty(CameraButtonEntry.KeyCode))
            {
                sb.AppendLine($"KEY={CameraButtonEntry.KeyCode} ; {GetKeyName(CameraButtonEntry.KeyCode)}");
            }
            sb.AppendLine();

            // [SHIFTER]
            sb.AppendLine("[SHIFTER]");
            sb.AppendLine($"ACTIVE={ShifterActive}");
            sb.AppendLine($"JOY={MapControllerIndex(ShifterJoy)}");
            foreach (var gear in ShifterGears)
            {
                sb.AppendLine($"{gear.Key}={gear.Value}");
            }
            sb.AppendLine();

            // [RESET_RACE]
            sb.AppendLine("[RESET_RACE]");
            sb.AppendLine($"KEY={ResetRaceKey} ; {GetKeyName(ResetRaceKey)}");
            sb.AppendLine($"JOY=-1");
            sb.AppendLine($"BUTTON=-1");
            sb.AppendLine($"BUTTON_MODIFICATOR=-1");
            sb.AppendLine();

            // [ADVANCED]
            sb.AppendLine("[ADVANCED]");
            sb.AppendLine($"COMBINE_WITH_KEYBOARD_CONTROL=0");
            sb.AppendLine();

            // Wszystkie pozostałe sekcje akcji (z wartościami -1 jeśli nie zbindowane)
            WriteActionSection(sb, "KERS", -1, -1, -1);
            WriteActionSection(sb, "DRS", -1, -1, -1);
            WriteActionSection(sb, "ACTION_HEADLIGHTS", -1, -1, -1);
            WriteActionSection(sb, "ACTION_HEADLIGHTS_FLASH", -1, -1, -1);
            WriteActionSection(sb, "ACTION_HORN", -1, -1, -1);
            WriteActionSection(sb, "BALANCEUP", -1, -1, -1);
            WriteActionSection(sb, "BALANCEDN", -1, -1, -1);
            WriteActionSection(sb, "TURBOUP", -1, -1, -1);
            WriteActionSection(sb, "TURBODN", -1, -1, -1);
            WriteActionSection(sb, "TCUP", -1, -1, -1);
            WriteActionSection(sb, "TCDN", -1, -1, -1);
            WriteActionSection(sb, "ABSUP", -1, -1, -1);
            WriteActionSection(sb, "ABSDN", -1, -1, -1);
            WriteActionSection(sb, "ENGINE_BRAKE_UP", -1, -1, -1);
            WriteActionSection(sb, "ENGINE_BRAKE_DN", -1, -1, -1);

            // [KEYBOARD]
            sb.AppendLine("[KEYBOARD]");
            sb.AppendLine($"GAS=-1");
            sb.AppendLine($"BRAKE=-1");
            sb.AppendLine($"RIGHT=-1");
            sb.AppendLine($"LEFT=-1");
            sb.AppendLine($"STEERING_SPEED=1.75");
            sb.AppendLine($"STEERING_OPPOSITE_DIRECTION_SPEED=2.5");
            sb.AppendLine($"STEER_RESET_SPEED=1.8");
            sb.AppendLine($"MOUSE_STEER=0");
            sb.AppendLine($"MOUSE_ACCELERATOR_BRAKE=0");
            sb.AppendLine($"MOUSE_SPEED=0.1");
            sb.AppendLine();

            // [GEAR_1] do [GEAR_7] i [GEAR_R] - osobne sekcje dla każdego biegu
            foreach (var gear in ShifterGears)
            {
                sb.AppendLine($"[{gear.Key}]");
                int gearJoy = gear.Value >= 0 ? MapControllerIndex(ShifterJoy) : -1;
                sb.AppendLine($"JOY={gearJoy}");
                sb.AppendLine();
            }

            // Zapis do pliku
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private void WriteActionSection(StringBuilder sb, string actionName, int joy, int key, int button)
        {
            sb.AppendLine($"[{actionName}]");
            sb.AppendLine($"JOY={joy}");
            sb.AppendLine($"KEY={key}");
            sb.AppendLine($"BUTTON={button}");
            sb.AppendLine();
        }

        private string EnsureGuidFormat(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return guid;
            // Usuń nawiasy klamrowe jeśli są
            if (guid.StartsWith("{") && guid.EndsWith("}"))
                return guid.Substring(1, guid.Length - 2);
            return guid;
        }

        private string GetKeyName(string keyCode)
        {
            if (string.IsNullOrEmpty(keyCode) || !keyCode.StartsWith("0x"))
                return "";

            try
            {
                int keyValue = Convert.ToInt32(keyCode, 16);
                // Mapowanie podstawowych klawiszy
                var keyMap = new Dictionary<int, string>
                {
                    { 0x57, "W" }, { 0x53, "S" }, { 0x41, "A" }, { 0x44, "D" },
                    { 0x48, "H" }, { 0x43, "C" }, { 0x52, "R" }, { 0x54, "T" },
                    { 0x51, "Q" }, { 0x45, "E" }, { 0x47, "G" }, { 0x49, "I" },
                    { 0x4A, "J" }, { 0x4C, "L" }, { 0x4E, "N" }, { 0x50, "P" },
                    { 0x20, "Space" }, { 0x31, "1" }, { 0x32, "2" }, { 0x33, "3" },
                    { 0x34, "4" }, { 0x35, "5" }, { 0x59, "Y" }
                };
                return keyMap.ContainsKey(keyValue) ? keyMap[keyValue] : "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Wczytuje ustawienia kontrolerów z pliku INI
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku controls.ini</param>
        /// <returns>true jeśli wczytano pomyślnie, false w przeciwnym razie</returns>
        public bool LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);
                var iniData = new Dictionary<string, Dictionary<string, string>>();
                string currentSection = null;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    // Pomiń puste linie i komentarze
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";") || trimmed.StartsWith("#"))
                        continue;

                    // Sekcja [NAZWA]
                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim();
                        if (!iniData.ContainsKey(currentSection))
                        {
                            iniData[currentSection] = new Dictionary<string, string>();
                        }
                        continue;
                    }

                    // Klucz = wartość
                    if (currentSection != null && trimmed.Contains("="))
                    {
                        var parts = trimmed.Split(new[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim();
                            var value = parts[1].Trim();
                            iniData[currentSection][key] = value;
                        }
                    }
                }

                // Wczytaj [HEADER]
                if (iniData.ContainsKey("HEADER"))
                {
                    var header = iniData["HEADER"];
                    if (header.ContainsKey("INPUT_METHOD") && int.TryParse(header["INPUT_METHOD"], out int inputMethod))
                        InputMethod = inputMethod;
                }

                // Wczytaj [CONTROLLERS]
                if (iniData.ContainsKey("CONTROLLERS"))
                {
                    var controllers = iniData["CONTROLLERS"];
                    if (controllers.ContainsKey("CON0"))
                        Con0 = controllers["CON0"];
                    if (controllers.ContainsKey("PGUID0"))
                        PGuid0 = controllers["PGUID0"];
                }

                // Wczytaj wszystkie bindy
                foreach (var entry in AllEntries)
                {
                    entry.Load(iniData);
                }

                // Wczytaj [__EXT_GEAR_UP] i [__EXT_GEAR_DOWN] (drugi bind skrzyni biegów)
                if (iniData.ContainsKey("__EXT_GEAR_UP"))
                {
                    var section = iniData["__EXT_GEAR_UP"];
                    if (section.ContainsKey("JOY") && int.TryParse(section["JOY"], out int joy))
                        PaddleUpButtonEntry.ControllerIndex = joy;
                    if (section.ContainsKey("BUTTON") && int.TryParse(section["BUTTON"], out int button))
                        PaddleUpButtonEntry.ButtonIndex = button;
                }

                if (iniData.ContainsKey("__EXT_GEAR_DOWN"))
                {
                    var section = iniData["__EXT_GEAR_DOWN"];
                    if (section.ContainsKey("JOY") && int.TryParse(section["JOY"], out int joy))
                        PaddleDnButtonEntry.ControllerIndex = joy;
                    if (section.ContainsKey("BUTTON") && int.TryParse(section["BUTTON"], out int button))
                        PaddleDnButtonEntry.ButtonIndex = button;
                }

                // Wczytaj [SHIFTER]
                if (iniData.ContainsKey("SHIFTER"))
                {
                    var shifter = iniData["SHIFTER"];
                    if (shifter.ContainsKey("ACTIVE") && int.TryParse(shifter["ACTIVE"], out int active))
                        ShifterActive = active;
                    if (shifter.ContainsKey("JOY") && int.TryParse(shifter["JOY"], out int joy))
                        ShifterJoy = joy;

                    // Wczytaj wszystkie biegi H-shiftera
                    foreach (var gearKey in ShifterGears.Keys.ToList())
                    {
                        if (shifter.ContainsKey(gearKey) && int.TryParse(shifter[gearKey], out int buttonIndex))
                        {
                            ShifterGears[gearKey] = buttonIndex;
                        }
                    }
                }

                // Wczytaj [RESET_RACE]
                if (iniData.ContainsKey("RESET_RACE"))
                {
                    var resetRace = iniData["RESET_RACE"];
                    if (resetRace.ContainsKey("KEY"))
                        ResetRaceKey = resetRace["KEY"];
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas wczytywania controls.ini: {ex.Message}");
                return false;
            }
        }
    }
}
