using System;
using System.Linq;
using IQPowerContentManager.Api.Controllers;
using IQPowerContentManager.Api.Models;
using IQPowerContentManager;
using AcTools.Utils;

namespace IQPowerContentManager.Api
{
    /// <summary>
    /// Pomocnicze metody do konwersji między stanem aplikacji a obiektami Controls
    /// </summary>
    public static class StateHelper
    {
        /// <summary>
        /// Zapisuje aktualny stan Controls do ApplicationState oraz do pliku controls.ini
        /// </summary>
        public static void SaveCurrentState()
        {
            try
            {
                // Pobierz stan Controls
                var controlsState = GetControlsState();

                // Utwórz ApplicationState
                var state = new ApplicationState
                {
                    Controls = controlsState
                };

                // Zapisz do pliku JSON (dla przywrócenia stanu przy starcie)
                ApplicationStateManager.SaveState(state);

                // Zapisz Controls do controls.ini (dla gry)
                SaveControlsToFile();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd zapisywania stanu: {ex.Message}");
            }
        }

        /// <summary>
        /// Zapisuje Controls do pliku controls.ini (publiczna metoda do użycia przed uruchomieniem gry)
        /// </summary>
        public static void SaveControlsToIniFile()
        {
            SaveControlsToFile();
        }

        /// <summary>
        /// Zapisuje Controls do pliku controls.ini w C:\Asseto-Manager-IQ
        /// </summary>
        private static void SaveControlsToFile()
        {
            try
            {
                var controllerType = typeof(ControlsController);
                var controlsField = controllerType.GetField("_controls",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

                if (controlsField == null) return;

                var controls = controlsField.GetValue(null) as Controls;
                if (controls == null) return;

                // Zapisuj do C:\Users\{current-user}\Documents\asseto-manager
                var documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var managerDir = System.IO.Path.Combine(documentsDir, "asseto-manager");
                var outputPath = System.IO.Path.Combine(managerDir, "controls.ini");

                if (!System.IO.Directory.Exists(managerDir))
                {
                    System.IO.Directory.CreateDirectory(managerDir);
                }

                controls.SaveToFile(outputPath);
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [STATE] Zapisano controls.ini do: {outputPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd zapisywania controls.ini: {ex.Message}");
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [STATE] Błąd zapisywania controls.ini: {ex.Message}");
            }
        }

        /// <summary>
        /// Wczytuje stan z ApplicationState i przywraca do Controls
        /// </summary>
        public static void LoadSavedState()
        {
            try
            {
                var state = ApplicationStateManager.LoadState();

                // Przywróć Controls jeśli są dane
                if (state.Controls != null)
                {
                    RestoreControlsState(state.Controls);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd wczytywania stanu: {ex.Message}");
            }
        }

        /// <summary>
        /// Pobiera aktualny stan Controls
        /// </summary>
        private static ControlsState GetControlsState()
        {
            // Używamy refleksji, aby uzyskać dostęp do prywatnego pola _controls
            var controllerType = typeof(ControlsController);
            var controlsField = controllerType.GetField("_controls",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            if (controlsField == null)
            {
                return new ControlsState();
            }

            var controls = controlsField.GetValue(null) as Controls;
            if (controls == null)
            {
                return new ControlsState();
            }

            // Konwertuj urządzenia z Controls na DeviceInfo
            var devices = controls.Devices?.Select(d => new IQPowerContentManager.Api.Models.DeviceInfo
            {
                Name = d.Name,
                Guid = d.InstanceGuid,
                ProductGuid = d.ProductGuid
            }).ToList() ?? new System.Collections.Generic.List<IQPowerContentManager.Api.Models.DeviceInfo>();

            return new ControlsState
            {
                Devices = devices,
                Steer = new SteerBinding
                {
                    ControllerIndex = controls.SteerAxleEntry.ControllerIndex,
                    AxleIndex = controls.SteerAxleEntry.AxleIndex,
                    DegreesOfRotation = controls.SteerAxleEntry.DegreesOfRotation,
                    Scale = controls.SteerAxleEntry.Scale
                },
                Throttle = new PedalBinding
                {
                    ControllerIndex = controls.ThrottleAxleEntry.ControllerIndex,
                    AxleIndex = controls.ThrottleAxleEntry.AxleIndex,
                    RangeFrom = controls.ThrottleAxleEntry.RangeFrom,
                    RangeTo = controls.ThrottleAxleEntry.RangeTo
                },
                Brakes = new PedalBinding
                {
                    ControllerIndex = controls.BrakesAxleEntry.ControllerIndex,
                    AxleIndex = controls.BrakesAxleEntry.AxleIndex,
                    RangeFrom = controls.BrakesAxleEntry.RangeFrom,
                    RangeTo = controls.BrakesAxleEntry.RangeTo
                },
                Clutch = new PedalBinding
                {
                    ControllerIndex = controls.ClutchAxleEntry.ControllerIndex,
                    AxleIndex = controls.ClutchAxleEntry.AxleIndex,
                    RangeFrom = controls.ClutchAxleEntry.RangeFrom,
                    RangeTo = controls.ClutchAxleEntry.RangeTo
                },
                Handbrake = new Binding
                {
                    ControllerIndex = controls.HandbrakeButtonEntry.ControllerIndex,
                    ButtonIndex = controls.HandbrakeButtonEntry.ButtonIndex >= 0 ? controls.HandbrakeButtonEntry.ButtonIndex : (int?)null,
                    AxleIndex = controls.HandbrakeAxleEntry.AxleIndex >= 0 ? controls.HandbrakeAxleEntry.AxleIndex : (int?)null
                },
                // GEARUP - pierwszy bind (może być też drugi przez PaddleUp)
                GearUp = new Binding
                {
                    ControllerIndex = controls.GearUpButtonEntry.ControllerIndex,
                    ButtonIndex = controls.GearUpButtonEntry.ButtonIndex >= 0 ? controls.GearUpButtonEntry.ButtonIndex : (int?)null
                },
                // GEARDN - pierwszy bind (może być też drugi przez PaddleDn)
                GearDown = new Binding
                {
                    ControllerIndex = controls.GearDnButtonEntry.ControllerIndex,
                    ButtonIndex = controls.GearDnButtonEntry.ButtonIndex >= 0 ? controls.GearDnButtonEntry.ButtonIndex : (int?)null
                },
                Camera = new Binding
                {
                    ControllerIndex = controls.CameraButtonEntry.ControllerIndex,
                    ButtonIndex = controls.CameraButtonEntry.ButtonIndex >= 0 ? controls.CameraButtonEntry.ButtonIndex : (int?)null
                },
                HShifter = new HShifterBinding
                {
                    Active = controls.ShifterActive == 1,
                    ControllerIndex = controls.ShifterJoy,
                    Gears = new System.Collections.Generic.Dictionary<string, int>(controls.ShifterGears)
                }
            };
        }

        /// <summary>
        /// Przywraca stan Controls z ControlsState
        /// </summary>
        private static void RestoreControlsState(ControlsState state)
        {
            var controllerType = typeof(ControlsController);
            var controlsField = controllerType.GetField("_controls",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            if (controlsField == null) return;

            var controls = controlsField.GetValue(null) as Controls;
            if (controls == null) return;

            // Przywróć listę urządzeń
            if (state.Devices != null && state.Devices.Count > 0)
            {
                controls.Devices = state.Devices.Select(d => new Controls.ControllerDevice
                {
                    Name = d.Name ?? "",
                    InstanceGuid = d.Guid ?? "",
                    ProductGuid = d.ProductGuid ?? ""
                }).ToList();
            }

            // Przywróć bindy
            if (state.Steer != null)
            {
                controls.SteerAxleEntry.ControllerIndex = state.Steer.ControllerIndex;
                if (state.Steer.AxleIndex.HasValue)
                {
                    // Konwertuj indeks osi od użytkownika (1, 2, 3...) na wewnętrzny (0, 1, 2...)
                    controls.SteerAxleEntry.AxleIndex = state.Steer.AxleIndex.Value - 1;
                }
                controls.SteerAxleEntry.DegreesOfRotation = state.Steer.DegreesOfRotation;
                controls.SteerAxleEntry.Scale = state.Steer.Scale;
            }

            if (state.Throttle != null)
            {
                controls.ThrottleAxleEntry.ControllerIndex = state.Throttle.ControllerIndex;
                if (state.Throttle.AxleIndex.HasValue)
                {
                    // Konwertuj indeks osi od użytkownika (1, 2, 3...) na wewnętrzny (0, 1, 2...)
                    controls.ThrottleAxleEntry.AxleIndex = state.Throttle.AxleIndex.Value - 1;
                }
                controls.ThrottleAxleEntry.RangeFrom = state.Throttle.RangeFrom;
                controls.ThrottleAxleEntry.RangeTo = state.Throttle.RangeTo;
            }

            if (state.Brakes != null)
            {
                controls.BrakesAxleEntry.ControllerIndex = state.Brakes.ControllerIndex;
                if (state.Brakes.AxleIndex.HasValue)
                {
                    // Konwertuj indeks osi od użytkownika (1, 2, 3...) na wewnętrzny (0, 1, 2...)
                    controls.BrakesAxleEntry.AxleIndex = state.Brakes.AxleIndex.Value - 1;
                }
                controls.BrakesAxleEntry.RangeFrom = state.Brakes.RangeFrom;
                controls.BrakesAxleEntry.RangeTo = state.Brakes.RangeTo;
            }

            if (state.Clutch != null)
            {
                controls.ClutchAxleEntry.ControllerIndex = state.Clutch.ControllerIndex;
                if (state.Clutch.AxleIndex.HasValue)
                {
                    // Konwertuj indeks osi od użytkownika (1, 2, 3...) na wewnętrzny (0, 1, 2...)
                    controls.ClutchAxleEntry.AxleIndex = state.Clutch.AxleIndex.Value - 1;
                }
                controls.ClutchAxleEntry.RangeFrom = state.Clutch.RangeFrom;
                controls.ClutchAxleEntry.RangeTo = state.Clutch.RangeTo;
            }

            if (state.Handbrake != null)
            {
                if (state.Handbrake.ButtonIndex.HasValue)
                {
                    controls.HandbrakeButtonEntry.ControllerIndex = state.Handbrake.ControllerIndex;
                    controls.HandbrakeButtonEntry.ButtonIndex = state.Handbrake.ButtonIndex.Value;
                }
                if (state.Handbrake.AxleIndex.HasValue)
                {
                    controls.HandbrakeAxleEntry.ControllerIndex = state.Handbrake.ControllerIndex;
                    // Konwertuj indeks osi od użytkownika (1, 2, 3...) na wewnętrzny (0, 1, 2...)
                    controls.HandbrakeAxleEntry.AxleIndex = state.Handbrake.AxleIndex.Value - 1;
                }
            }

            // GEARUP - przywróć pierwszy bind (drugi jest przez PaddleUp i nie jest zapisywany w ControlsState)
            if (state.GearUp != null && state.GearUp.ButtonIndex.HasValue)
            {
                controls.GearUpButtonEntry.ControllerIndex = state.GearUp.ControllerIndex;
                controls.GearUpButtonEntry.ButtonIndex = state.GearUp.ButtonIndex.Value;
            }

            // GEARDN - przywróć pierwszy bind (drugi jest przez PaddleDn i nie jest zapisywany w ControlsState)
            if (state.GearDown != null && state.GearDown.ButtonIndex.HasValue)
            {
                controls.GearDnButtonEntry.ControllerIndex = state.GearDown.ControllerIndex;
                controls.GearDnButtonEntry.ButtonIndex = state.GearDown.ButtonIndex.Value;
            }

            if (state.Camera != null && state.Camera.ButtonIndex.HasValue)
            {
                controls.CameraButtonEntry.ControllerIndex = state.Camera.ControllerIndex;
                controls.CameraButtonEntry.ButtonIndex = state.Camera.ButtonIndex.Value;
            }

            if (state.HShifter != null)
            {
                controls.ShifterActive = state.HShifter.Active ? 1 : 0;
                controls.ShifterJoy = state.HShifter.ControllerIndex;
                if (state.HShifter.Gears != null)
                {
                    foreach (var gear in state.HShifter.Gears)
                    {
                        if (controls.ShifterGears.ContainsKey(gear.Key))
                        {
                            controls.ShifterGears[gear.Key] = gear.Value;
                        }
                    }
                }
            }
        }
    }
}

