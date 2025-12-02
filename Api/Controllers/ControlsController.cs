using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http;
using IQPowerContentManager.Api.Models;
using IQPowerContentManager.Api;
using AcTools.Utils;

namespace IQPowerContentManager.Api.Controllers
{
    /// <summary>
    /// Kontroler do zarządzania kontrolerami i bindowaniem przycisków
    /// </summary>
    [RoutePrefix("api/controls")]
    public class ControlsController : ApiController
    {
        private static Controls _controls = new Controls();
        private static List<JoystickInputHandler> _activeHandlers = new List<JoystickInputHandler>();
        private static readonly object _handlersLock = new object();

        // Sesje nasłuchiwania na input
        private static Dictionary<string, BindingDetectionStatus> _activeDetections = new Dictionary<string, BindingDetectionStatus>();
        private static Dictionary<string, CancellationTokenSource> _cancellationTokens = new Dictionary<string, CancellationTokenSource>();
        private static readonly object _detectionsLock = new object();

        /// <summary>
        /// Pobiera listę ustawionych urządzeń (te które będą zapisane w controls.ini)
        /// </summary>
        /// <returns>Lista ustawionych urządzeń</returns>
        [HttpGet]
        [Route("devices")]
        public IHttpActionResult GetDevices()
        {
            try
            {
                var deviceInfos = new List<IQPowerContentManager.Api.Models.DeviceInfo>();

                // Zwróć urządzenia z _controls.Devices (ustawione przez użytkownika)
                if (_controls.Devices != null && _controls.Devices.Count > 0)
                {
                    for (int i = 0; i < _controls.Devices.Count; i++)
                    {
                        deviceInfos.Add(new IQPowerContentManager.Api.Models.DeviceInfo
                        {
                            Index = i,
                            Name = _controls.Devices[i].Name,
                            Guid = _controls.Devices[i].InstanceGuid,
                            ProductGuid = _controls.Devices[i].ProductGuid
                        });
                    }
                }

                return Ok(ApiResponse<List<IQPowerContentManager.Api.Models.DeviceInfo>>.Ok(deviceInfos));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<List<IQPowerContentManager.Api.Models.DeviceInfo>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Pobiera listę wszystkich fizycznie dostępnych kontrolerów w systemie (kierownice, pedały, itp.)
        /// </summary>
        /// <returns>Lista wszystkich dostępnych urządzeń wejściowych</returns>
        [HttpGet]
        [Route("devices/available")]
        public IHttpActionResult GetAvailableDevices()
        {
            try
            {
                var devices = JoystickInputHandler.GetAvailableDevices();
                var deviceInfos = new List<IQPowerContentManager.Api.Models.DeviceInfo>();

                for (int i = 0; i < devices.Count; i++)
                {
                    deviceInfos.Add(new IQPowerContentManager.Api.Models.DeviceInfo
                    {
                        Index = i,
                        Name = devices[i].Name,
                        Guid = JoystickInputHandler.FormatGuid(devices[i].Guid),
                        ProductGuid = JoystickInputHandler.FormatGuid(devices[i].ProductGuid)
                    });
                }

                return Ok(ApiResponse<List<IQPowerContentManager.Api.Models.DeviceInfo>>.Ok(deviceInfos));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<List<IQPowerContentManager.Api.Models.DeviceInfo>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Przypisuje bind do akcji (przycisk lub oś)
        /// </summary>
        /// <param name="request">Request zawierający akcję, indeks kontrolera i przycisku/osie</param>
        /// <returns>Potwierdzenie przypisania binda</returns>
        [HttpPost]
        [Route("bind")]
        public IHttpActionResult Bind([FromBody] BindRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Action))
                {
                    return Ok(ApiResponse.Error("Action nie może być pusty"));
                }

                bool success = false;

                // GEARUP - zawsze zapisuje się jako [GEARUP]
                if (request.Action == "GEARUP")
                {
                    if (!request.ButtonIndex.HasValue)
                    {
                        return Ok(ApiResponse.Error("Dla GEARUP musisz podać ButtonIndex"));
                    }
                    success = _controls.AssignButtonBinding("GEARUP", request.ControllerIndex, request.ButtonIndex.Value);
                }
                // PADDLEUP - zawsze zapisuje się jako [PADDLEUP]
                else if (request.Action == "PADDLEUP")
                {
                    if (!request.ButtonIndex.HasValue)
                    {
                        return Ok(ApiResponse.Error("Dla PADDLEUP musisz podać ButtonIndex"));
                    }
                    success = _controls.AssignButtonBinding("PADDLEUP", request.ControllerIndex, request.ButtonIndex.Value);
                }
                // GEARDN - zawsze zapisuje się jako [GEARDN]
                else if (request.Action == "GEARDN")
                {
                    if (!request.ButtonIndex.HasValue)
                    {
                        return Ok(ApiResponse.Error("Dla GEARDN musisz podać ButtonIndex"));
                    }
                    success = _controls.AssignButtonBinding("GEARDN", request.ControllerIndex, request.ButtonIndex.Value);
                }
                // PADDLEDN - zawsze zapisuje się jako [PADDLEDN]
                else if (request.Action == "PADDLEDN")
                {
                    if (!request.ButtonIndex.HasValue)
                    {
                        return Ok(ApiResponse.Error("Dla PADDLEDN musisz podać ButtonIndex"));
                    }
                    success = _controls.AssignButtonBinding("PADDLEDN", request.ControllerIndex, request.ButtonIndex.Value);
                }
                // Obsługa bindów dla biegów H-shiftera (GEAR_1 do GEAR_7 i GEAR_R)
                else if (request.Action.StartsWith("GEAR_"))
                {
                    if (!request.ButtonIndex.HasValue)
                    {
                        return Ok(ApiResponse.Error("Dla biegów musisz podać ButtonIndex"));
                    }

                    // Ustaw shifter jako aktywny
                    _controls.ShifterActive = 1;
                    _controls.ShifterJoy = request.ControllerIndex;

                    // Przypisz przycisk do odpowiedniego biegu
                    if (_controls.ShifterGears.ContainsKey(request.Action))
                    {
                        _controls.ShifterGears[request.Action] = request.ButtonIndex.Value;
                        success = true;
                    }
                    else
                    {
                        return Ok(ApiResponse.Error($"Nieprawidłowa nazwa biegu: {request.Action}. Użyj GEAR_1, GEAR_2, ..., GEAR_7, GEAR_R"));
                    }
                }
                else
                {
                    // Standardowe bindy
                    // Mapowanie akcji API na akcje w Controls
                    string actionToUse = request.Action;
                    if (request.Action == "CAMERA")
                    {
                        actionToUse = "ACTION_CHANGE_CAMERA";
                    }

                    // HANDBRAKE zawsze jako oś
                    if (request.Action == "HANDBRAKE")
                    {
                        if (request.AxleIndex.HasValue)
                        {
                            // Konwertuj indeks osi od użytkownika (1, 2, 3...) na wewnętrzny (0, 1, 2...)
                            int internalAxleIndex = request.AxleIndex.Value - 1;
                            success = _controls.AssignAxisBinding(actionToUse, request.ControllerIndex, internalAxleIndex);
                        }
                        else if (request.ButtonIndex.HasValue)
                        {
                            // Jeśli podano ButtonIndex dla HANDBRAKE, zignoruj go i wymuś użycie AxleIndex
                            return Ok(ApiResponse.Error("HANDBRAKE musi być bindowany jako oś (AxleIndex), nie jako przycisk"));
                        }
                        else
                        {
                            return Ok(ApiResponse.Error("Musisz podać AxleIndex dla HANDBRAKE"));
                        }
                    }
                    else if (request.ButtonIndex.HasValue)
                    {
                        success = _controls.AssignButtonBinding(actionToUse, request.ControllerIndex, request.ButtonIndex.Value);
                    }
                    else if (request.AxleIndex.HasValue)
                    {
                        // Konwertuj indeks osi od użytkownika (1, 2, 3...) na wewnętrzny (0, 1, 2...)
                        int internalAxleIndex = request.AxleIndex.Value - 1;
                        success = _controls.AssignAxisBinding(actionToUse, request.ControllerIndex, internalAxleIndex);
                    }
                    else
                    {
                        return Ok(ApiResponse.Error("Musisz podać ButtonIndex lub AxleIndex"));
                    }
                }

                if (success)
                {
                    StateHelper.SaveCurrentState();
                    return Ok(ApiResponse.Ok($"Przypisano {request.Action}"));
                }
                else
                {
                    return Ok(ApiResponse.Error($"Nie udało się przypisać {request.Action}"));
                }
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.Error(ex.Message));
            }
        }

        /// <summary>
        /// Przypisuje bindy dla skrzyni biegów H-pattern (7 biegów + wsteczny)
        /// </summary>
        /// <param name="request">Request zawierający indeks kontrolera i mapowanie biegów na przyciski</param>
        /// <returns>Potwierdzenie przypisania H-shiftera</returns>
        [HttpPost]
        [Route("bind/h-shifter")]
        public IHttpActionResult BindHShifter([FromBody] BindHShifterRequest request)
        {
            try
            {
                if (request?.Gears == null)
                {
                    return Ok(ApiResponse.Error("Gears nie może być null"));
                }

                _controls.ShifterActive = 1;
                _controls.ShifterJoy = request.ControllerIndex;

                foreach (var gear in request.Gears)
                {
                    if (_controls.ShifterGears.ContainsKey(gear.Key))
                    {
                        _controls.ShifterGears[gear.Key] = gear.Value;
                    }
                }

                StateHelper.SaveCurrentState();
                return Ok(ApiResponse.Ok("H-shifter zbindowany"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.Error(ex.Message));
            }
        }

        /// <summary>
        /// Przypisuje bindy dla skrzyni sekwencyjnej (bieg w górę i w dół)
        /// </summary>
        /// <param name="request">Request zawierający indeks kontrolera i przyciski biegów</param>
        /// <returns>Potwierdzenie przypisania skrzyni sekwencyjnej</returns>
        [HttpPost]
        [Route("bind/sequential")]
        public IHttpActionResult BindSequential([FromBody] BindSequentialRequest request)
        {
            try
            {
                if (request.GearUpButton.HasValue)
                {
                    _controls.AssignButtonBinding("GEARUP", request.ControllerIndex, request.GearUpButton.Value);
                }

                if (request.GearDownButton.HasValue)
                {
                    _controls.AssignButtonBinding("GEARDN", request.ControllerIndex, request.GearDownButton.Value);
                }

                StateHelper.SaveCurrentState();
                return Ok(ApiResponse.Ok("Skrzynia sekwencyjna zbindowana"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.Error(ex.Message));
            }
        }

        /// <summary>
        /// Ustawia listę urządzeń do zapisania w controls.ini
        /// </summary>
        /// <param name="request">Request zawierający listę urządzeń z nazwami i GUID</param>
        /// <returns>Potwierdzenie ustawienia urządzeń</returns>
        [HttpPost]
        [Route("devices")]
        public IHttpActionResult SetDevices([FromBody] SetDevicesRequest request)
        {
            try
            {
                if (request?.Devices == null)
                {
                    return Ok(ApiResponse.Error("Lista urządzeń nie może być pusta"));
                }

                // Konwertuj urządzenia z API na format Controls i posortuj alfabetycznie według nazwy
                // Zapisz oryginalny indeks przed sortowaniem, aby móc zmapować ControllerIndex w bindach
                _controls.Devices = request.Devices
                    .OrderBy(d => d.Name ?? "", StringComparer.OrdinalIgnoreCase)
                    .Select(d => new Controls.ControllerDevice
                    {
                        Name = d.Name ?? "",
                        InstanceGuid = d.Guid ?? "",
                        ProductGuid = d.ProductGuid ?? "",
                        OriginalIndex = d.Index
                    }).ToList();

                // Zapisz stan
                StateHelper.SaveCurrentState();

                return Ok(ApiResponse.Ok($"Ustawiono {_controls.Devices.Count} urządzeń"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.Error(ex.Message));
            }
        }

        /// <summary>
        /// Usuwa urządzenie z listy urządzeń na podstawie GUID
        /// </summary>
        /// <param name="guid">GUID urządzenia do usunięcia</param>
        /// <returns>Potwierdzenie usunięcia urządzenia</returns>
        [HttpDelete]
        [Route("devices/{guid}")]
        public IHttpActionResult DeleteDevice(string guid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(guid))
                {
                    return Ok(ApiResponse.Error("GUID nie może być pusty"));
                }

                if (_controls.Devices == null || _controls.Devices.Count == 0)
                {
                    return Ok(ApiResponse.Error("Lista urządzeń jest pusta"));
                }

                // Normalizuj GUID (usuń nawiasy klamrowe jeśli są)
                string normalizedGuid = guid.Trim().TrimStart('{').TrimEnd('}');

                // Znajdź i usuń urządzenie po InstanceGuid lub ProductGuid
                var deviceToRemove = _controls.Devices.FirstOrDefault(d =>
                {
                    string deviceInstanceGuid = d.InstanceGuid?.Trim().TrimStart('{').TrimEnd('}') ?? "";
                    string deviceProductGuid = d.ProductGuid?.Trim().TrimStart('{').TrimEnd('}') ?? "";
                    return deviceInstanceGuid.Equals(normalizedGuid, StringComparison.OrdinalIgnoreCase) ||
                           deviceProductGuid.Equals(normalizedGuid, StringComparison.OrdinalIgnoreCase);
                });

                if (deviceToRemove == null)
                {
                    return Ok(ApiResponse.Error($"Nie znaleziono urządzenia z GUID: {guid}"));
                }

                // Sprawdź czy urządzenie jest używane w bindach
                // Używamy OriginalIndex, ponieważ ControllerIndex w bindach odnosi się do oryginalnego indeksu z API
                int originalIndex = deviceToRemove.OriginalIndex;
                bool isUsed = false;
                string usedIn = "";

                if (originalIndex >= 0)
                {
                    // Sprawdź wszystkie bindy
                    if (_controls.SteerAxleEntry.ControllerIndex == originalIndex) { isUsed = true; usedIn = "STEER"; }
                    else if (_controls.ThrottleAxleEntry.ControllerIndex == originalIndex) { isUsed = true; usedIn = "THROTTLE"; }
                    else if (_controls.BrakesAxleEntry.ControllerIndex == originalIndex) { isUsed = true; usedIn = "BRAKES"; }
                    else if (_controls.ClutchAxleEntry.ControllerIndex == originalIndex) { isUsed = true; usedIn = "CLUTCH"; }
                    else if (_controls.HandbrakeButtonEntry.ControllerIndex == originalIndex || _controls.HandbrakeAxleEntry.ControllerIndex == originalIndex) { isUsed = true; usedIn = "HANDBRAKE"; }
                    else if (_controls.GearUpButtonEntry.ControllerIndex == originalIndex) { isUsed = true; usedIn = "GEARUP"; }
                    else if (_controls.PaddleUpButtonEntry.ControllerIndex == originalIndex) { isUsed = true; usedIn = "PADDLEUP"; }
                    else if (_controls.GearDnButtonEntry.ControllerIndex == originalIndex) { isUsed = true; usedIn = "GEARDN"; }
                    else if (_controls.PaddleDnButtonEntry.ControllerIndex == originalIndex) { isUsed = true; usedIn = "PADDLEDN"; }
                    else if (_controls.CameraButtonEntry.ControllerIndex == originalIndex) { isUsed = true; usedIn = "CAMERA"; }
                    else if (_controls.ShifterJoy == originalIndex) { isUsed = true; usedIn = "SHIFTER"; }
                }

                if (isUsed)
                {
                    return Ok(ApiResponse.Error($"Nie można usunąć urządzenia - jest używane w bindzie: {usedIn}. Najpierw usuń bind."));
                }

                // Usuń urządzenie
                _controls.Devices.Remove(deviceToRemove);

                // Zapisz stan
                StateHelper.SaveCurrentState();

                return Ok(ApiResponse.Ok($"Usunięto urządzenie: {deviceToRemove.Name}"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.Error(ex.Message));
            }
        }

        /// <summary>
        /// Usuwa wszystkie urządzenia z listy urządzeń
        /// </summary>
        /// <returns>Potwierdzenie usunięcia wszystkich urządzeń</returns>
        [HttpDelete]
        [Route("devices")]
        public IHttpActionResult DeleteAllDevices()
        {
            try
            {
                if (_controls.Devices == null || _controls.Devices.Count == 0)
                {
                    return Ok(ApiResponse.Ok("Lista urządzeń jest już pusta"));
                }

                // Sprawdź czy jakiekolwiek urządzenie jest używane w bindach
                var usedDevices = new List<string>();

                // Sprawdź wszystkie bindy i zbierz informacje o używanych urządzeniach
                var allControllerIndices = new HashSet<int>();

                if (_controls.SteerAxleEntry.ControllerIndex >= 0) allControllerIndices.Add(_controls.SteerAxleEntry.ControllerIndex);
                if (_controls.ThrottleAxleEntry.ControllerIndex >= 0) allControllerIndices.Add(_controls.ThrottleAxleEntry.ControllerIndex);
                if (_controls.BrakesAxleEntry.ControllerIndex >= 0) allControllerIndices.Add(_controls.BrakesAxleEntry.ControllerIndex);
                if (_controls.ClutchAxleEntry.ControllerIndex >= 0) allControllerIndices.Add(_controls.ClutchAxleEntry.ControllerIndex);
                if (_controls.HandbrakeButtonEntry.ControllerIndex >= 0) allControllerIndices.Add(_controls.HandbrakeButtonEntry.ControllerIndex);
                if (_controls.HandbrakeAxleEntry.ControllerIndex >= 0) allControllerIndices.Add(_controls.HandbrakeAxleEntry.ControllerIndex);
                if (_controls.GearUpButtonEntry.ControllerIndex >= 0) allControllerIndices.Add(_controls.GearUpButtonEntry.ControllerIndex);
                if (_controls.PaddleUpButtonEntry.ControllerIndex >= 0) allControllerIndices.Add(_controls.PaddleUpButtonEntry.ControllerIndex);
                if (_controls.GearDnButtonEntry.ControllerIndex >= 0) allControllerIndices.Add(_controls.GearDnButtonEntry.ControllerIndex);
                if (_controls.PaddleDnButtonEntry.ControllerIndex >= 0) allControllerIndices.Add(_controls.PaddleDnButtonEntry.ControllerIndex);
                if (_controls.CameraButtonEntry.ControllerIndex >= 0) allControllerIndices.Add(_controls.CameraButtonEntry.ControllerIndex);
                if (_controls.ShifterJoy >= 0) allControllerIndices.Add(_controls.ShifterJoy);

                // Sprawdź które urządzenia są używane
                foreach (var device in _controls.Devices)
                {
                    if (device.OriginalIndex >= 0 && allControllerIndices.Contains(device.OriginalIndex))
                    {
                        usedDevices.Add(device.Name);
                    }
                }

                if (usedDevices.Count > 0)
                {
                    return Ok(ApiResponse.Error($"Nie można usunąć wszystkich urządzeń - następujące urządzenia są używane w bindach: {string.Join(", ", usedDevices)}. Najpierw usuń bindy."));
                }

                // Usuń wszystkie urządzenia
                int count = _controls.Devices.Count;
                _controls.Devices.Clear();

                // Zapisz stan
                StateHelper.SaveCurrentState();

                return Ok(ApiResponse.Ok($"Usunięto wszystkie urządzenia ({count})"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.Error(ex.Message));
            }
        }

        /// <summary>
        /// Zapisuje ustawienia kontrolerów do pliku controls.ini
        /// </summary>
        /// <returns>Potwierdzenie zapisania ustawień</returns>
        [HttpPost]
        [Route("save")]
        public IHttpActionResult SaveControls()
        {
            try
            {
                // Zapisuj do C:\Users\{current-user}\Documents\asseto-manager
                var documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var managerDir = System.IO.Path.Combine(documentsDir, "asseto-manager");
                var outputPath = System.IO.Path.Combine(managerDir, "controls.ini");

                if (!System.IO.Directory.Exists(managerDir))
                {
                    System.IO.Directory.CreateDirectory(managerDir);
                }

                _controls.SaveToFile(outputPath);
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [CONTROLS] Zapisano controls.ini do: {outputPath}");
                return Ok(ApiResponse.Ok($"Ustawienia zapisane do: {outputPath}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [CONTROLS] Błąd zapisywania controls.ini: {ex.Message}");
                return Ok(ApiResponse.Error(ex.Message));
            }
        }

        /// <summary>
        /// Pobiera aktualny stan ustawień kontrolerów
        /// </summary>
        /// <returns>Stan wszystkich bindów kontrolerów</returns>
        [HttpGet]
        [Route("state")]
        public IHttpActionResult GetControlsState()
        {
            try
            {
                // Konwertuj urządzenia z Controls na format API z zachowaniem kolejności i indeksów
                var devices = new List<IQPowerContentManager.Api.Models.DeviceInfo>();
                if (_controls.Devices != null && _controls.Devices.Count > 0)
                {
                    for (int i = 0; i < _controls.Devices.Count; i++)
                    {
                        devices.Add(new IQPowerContentManager.Api.Models.DeviceInfo
                        {
                            Index = i,
                            Name = _controls.Devices[i].Name,
                            Guid = _controls.Devices[i].InstanceGuid,
                            ProductGuid = _controls.Devices[i].ProductGuid
                        });
                    }
                }

                var state = new ControlsState
                {
                    Devices = devices,
                    Steer = new SteerBinding
                    {
                        ControllerIndex = _controls.SteerAxleEntry.ControllerIndex,
                        AxleIndex = _controls.SteerAxleEntry.AxleIndex >= 0 ? _controls.SteerAxleEntry.AxleIndex + 1 : (int?)null, // Indeksujemy od 1 dla użytkownika
                        DegreesOfRotation = _controls.SteerAxleEntry.DegreesOfRotation,
                        Scale = _controls.SteerAxleEntry.Scale
                    },
                    Throttle = new PedalBinding
                    {
                        ControllerIndex = _controls.ThrottleAxleEntry.ControllerIndex,
                        AxleIndex = _controls.ThrottleAxleEntry.AxleIndex >= 0 ? _controls.ThrottleAxleEntry.AxleIndex + 1 : (int?)null, // Indeksujemy od 1 dla użytkownika
                        RangeFrom = _controls.ThrottleAxleEntry.RangeFrom,
                        RangeTo = _controls.ThrottleAxleEntry.RangeTo
                    },
                    Brakes = new PedalBinding
                    {
                        ControllerIndex = _controls.BrakesAxleEntry.ControllerIndex,
                        AxleIndex = _controls.BrakesAxleEntry.AxleIndex >= 0 ? _controls.BrakesAxleEntry.AxleIndex + 1 : (int?)null, // Indeksujemy od 1 dla użytkownika
                        RangeFrom = _controls.BrakesAxleEntry.RangeFrom,
                        RangeTo = _controls.BrakesAxleEntry.RangeTo
                    },
                    Clutch = new PedalBinding
                    {
                        ControllerIndex = _controls.ClutchAxleEntry.ControllerIndex,
                        AxleIndex = _controls.ClutchAxleEntry.AxleIndex >= 0 ? _controls.ClutchAxleEntry.AxleIndex + 1 : (int?)null, // Indeksujemy od 1 dla użytkownika
                        RangeFrom = _controls.ClutchAxleEntry.RangeFrom,
                        RangeTo = _controls.ClutchAxleEntry.RangeTo
                    },
                    Handbrake = new Binding
                    {
                        ControllerIndex = _controls.HandbrakeAxleEntry.ControllerIndex,
                        ButtonIndex = null, // HANDBRAKE zawsze jako oś
                        AxleIndex = _controls.HandbrakeAxleEntry.AxleIndex >= 0 ? _controls.HandbrakeAxleEntry.AxleIndex + 1 : (int?)null // Indeksujemy od 1 dla użytkownika
                    },
                    GearUp = new Binding
                    {
                        ControllerIndex = _controls.GearUpButtonEntry.ControllerIndex,
                        ButtonIndex = _controls.GearUpButtonEntry.ButtonIndex >= 0 ? _controls.GearUpButtonEntry.ButtonIndex : (int?)null
                    },
                    PaddleUp = new Binding
                    {
                        ControllerIndex = _controls.PaddleUpButtonEntry.ControllerIndex,
                        ButtonIndex = _controls.PaddleUpButtonEntry.ButtonIndex >= 0 ? _controls.PaddleUpButtonEntry.ButtonIndex : (int?)null
                    },
                    GearDown = new Binding
                    {
                        ControllerIndex = _controls.GearDnButtonEntry.ControllerIndex,
                        ButtonIndex = _controls.GearDnButtonEntry.ButtonIndex >= 0 ? _controls.GearDnButtonEntry.ButtonIndex : (int?)null
                    },
                    PaddleDown = new Binding
                    {
                        ControllerIndex = _controls.PaddleDnButtonEntry.ControllerIndex,
                        ButtonIndex = _controls.PaddleDnButtonEntry.ButtonIndex >= 0 ? _controls.PaddleDnButtonEntry.ButtonIndex : (int?)null
                    },
                    Camera = new Binding
                    {
                        ControllerIndex = _controls.CameraButtonEntry.ControllerIndex,
                        ButtonIndex = _controls.CameraButtonEntry.ButtonIndex >= 0 ? _controls.CameraButtonEntry.ButtonIndex : (int?)null
                    },
                    HShifter = new HShifterBinding
                    {
                        Active = _controls.ShifterActive == 1,
                        ControllerIndex = _controls.ShifterJoy,
                        Gears = new Dictionary<string, int>(_controls.ShifterGears)
                    }
                };

                return Ok(ApiResponse<ControlsState>.Ok(state));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<ControlsState>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Wczytuje ustawienia kontrolerów z pliku controls.ini
        /// </summary>
        /// <returns>Potwierdzenie wczytania ustawień</returns>
        [HttpPost]
        [Route("load")]
        public IHttpActionResult LoadControls()
        {
            try
            {
                // Wczytuj z C:\Users\{current-user}\Documents\asseto-manager, jeśli nie istnieje, to z domyślnego miejsca
                var documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var managerDir = System.IO.Path.Combine(documentsDir, "asseto-manager");
                var managerPath = System.IO.Path.Combine(managerDir, "controls.ini");
                var defaultPath = AcPaths.GetCfgControlsFilename();
                var inputPath = System.IO.File.Exists(managerPath) ? managerPath : defaultPath;

                if (!System.IO.File.Exists(inputPath))
                {
                    return Ok(ApiResponse.Error($"Plik nie istnieje: {inputPath}"));
                }

                if (_controls.LoadFromFile(inputPath))
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [CONTROLS] Wczytano controls.ini z: {inputPath}");
                    return Ok(ApiResponse.Ok($"Ustawienia wczytane pomyślnie z: {inputPath}"));
                }
                else
                {
                    return Ok(ApiResponse.Error("Nie udało się wczytać ustawień"));
                }
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.Error(ex.Message));
            }
        }

        /// <summary>
        /// Usuwa bind dla określonej akcji
        /// </summary>
        /// <param name="actionName">Nazwa akcji (np. "STEER", "THROTTLE", "GEARUP")</param>
        /// <returns>Potwierdzenie usunięcia binda</returns>
        [HttpDelete]
        [Route("bind/{actionName}")]
        public IHttpActionResult Unbind(string actionName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(actionName))
                {
                    return Ok(ApiResponse.Error("Action nie może być pusty"));
                }

                if (_controls.UnbindAction(actionName.ToUpper()))
                {
                    StateHelper.SaveCurrentState();
                    return Ok(ApiResponse.Ok("Control unbound successfully"));
                }
                else
                {
                    return Ok(ApiResponse.Error($"Nie znaleziono akcji: {actionName}"));
                }
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.Error(ex.Message));
            }
        }

        /// <summary>
        /// Usuwa konkretny bind na podstawie jego ID
        /// </summary>
        /// <param name="request">Request zawierający ID binda do usunięcia</param>
        /// <returns>Potwierdzenie usunięcia binda</returns>
        [HttpDelete]
        [Route("bind")]
        public IHttpActionResult UnbindSpecific([FromBody] UnbindSpecificRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.BindingId))
                {
                    return Ok(ApiResponse.Error("BindingId nie może być pusty"));
                }

                bool success = false;
                string actionName = "";

                // Parsuj ID binda (np. "GEARUP_1", "GEARUP_2", "GEAR_1", etc.)
                var parts = request.BindingId.Split('_');
                if (parts.Length < 2)
                {
                    return Ok(ApiResponse.Error($"Nieprawidłowy format BindingId: {request.BindingId}"));
                }

                var action = parts[0];
                var index = parts[1];

                if (action == "GEARUP")
                {
                    _controls.GearUpButtonEntry.Clear();
                    success = true;
                    actionName = "GEARUP";
                }
                else if (action == "PADDLEUP")
                {
                    _controls.PaddleUpButtonEntry.Clear();
                    success = true;
                    actionName = "PADDLEUP";
                }
                else if (action == "GEARDN")
                {
                    _controls.GearDnButtonEntry.Clear();
                    success = true;
                    actionName = "GEARDN";
                }
                else if (action == "PADDLEDN")
                {
                    _controls.PaddleDnButtonEntry.Clear();
                    success = true;
                    actionName = "PADDLEDN";
                }
                else if (action == "GEAR")
                {
                    // Usuń bind dla konkretnego biegu H-shiftera
                    var gearKey = $"GEAR_{index}";
                    if (_controls.ShifterGears.ContainsKey(gearKey))
                    {
                        _controls.ShifterGears[gearKey] = -1;
                        success = true;
                        actionName = gearKey;

                        // Sprawdź czy wszystkie biegi są usunięte - jeśli tak, wyłącz shifter
                        bool anyGearActive = _controls.ShifterGears.Values.Any(v => v >= 0);
                        if (!anyGearActive)
                        {
                            _controls.ShifterActive = 0;
                            _controls.ShifterJoy = -1;
                        }
                    }
                }
                else
                {
                    // Dla innych akcji, usuń standardowy bind
                    // Mapowanie akcji API na akcje w Controls
                    string actionToUse = action.ToUpper();
                    if (actionToUse == "CAMERA")
                    {
                        actionToUse = "ACTION_CHANGE_CAMERA";
                    }

                    if (_controls.UnbindAction(actionToUse))
                    {
                        success = true;
                        actionName = action;
                    }
                }

                if (success)
                {
                    StateHelper.SaveCurrentState();
                    return Ok(ApiResponse.Ok($"Bind {actionName} został usunięty"));
                }
                else
                {
                    return Ok(ApiResponse.Error($"Nie znaleziono binda: {request.BindingId}"));
                }
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.Error(ex.Message));
            }
        }

        /// <summary>
        /// Pobiera listę wszystkich dostępnych akcji i ich obecne bindy w bardziej strukturalnym formacie
        /// </summary>
        /// <returns>Lista wszystkich akcji z ich bindami</returns>
        [HttpGet]
        [Route("bindings")]
        public IHttpActionResult GetBindings()
        {
            try
            {
                var devices = JoystickInputHandler.GetAvailableDevices();
                var actions = new List<ActionBinding>();

                // Funkcja pomocnicza do pobierania nazwy kontrolera
                string GetControllerName(int controllerIndex)
                {
                    if (controllerIndex >= 0 && controllerIndex < devices.Count)
                    {
                        return devices[controllerIndex].Name;
                    }
                    return "Unknown";
                }

                // Funkcja pomocnicza do tworzenia BindingInfo z ID
                BindingInfo CreateBindingInfo(string bindingId, int? controllerIndex, string inputType, int? inputIndex)
                {
                    if (controllerIndex.HasValue && controllerIndex.Value >= 0)
                    {
                        var controllerName = GetControllerName(controllerIndex.Value);
                        string displayName;

                        if (inputType == "axis")
                        {
                            // Indeksujemy osie od 1 dla użytkownika
                            displayName = inputIndex.HasValue ? $"Axis {inputIndex.Value + 1}" : "Axis ?";
                        }
                        else
                        {
                            displayName = $"Button {inputIndex}";
                        }

                        return new BindingInfo
                        {
                            Id = bindingId,
                            ControllerIndex = controllerIndex,
                            ControllerName = controllerName,
                            InputType = inputType,
                            InputIndex = inputType == "axis" && inputIndex.HasValue ? inputIndex.Value + 1 : inputIndex, // Indeksujemy osie od 1
                            DisplayName = displayName
                        };
                    }
                    return null;
                }

                // STEER - oś
                var steerBindings = new List<BindingInfo>();
                var steerBinding = CreateBindingInfo("STEER_1",
                    _controls.SteerAxleEntry.ControllerIndex >= 0 ? _controls.SteerAxleEntry.ControllerIndex : (int?)null,
                    "axis",
                    _controls.SteerAxleEntry.AxleIndex >= 0 ? _controls.SteerAxleEntry.AxleIndex : (int?)null
                );
                if (steerBinding != null) steerBindings.Add(steerBinding);
                actions.Add(new ActionBinding
                {
                    Name = "STEER",
                    Type = "axis",
                    Description = "Steering wheel",
                    Bindings = steerBindings
                });

                // THROTTLE - oś
                var throttleBindings = new List<BindingInfo>();
                var throttleBinding = CreateBindingInfo("THROTTLE_1",
                    _controls.ThrottleAxleEntry.ControllerIndex >= 0 ? _controls.ThrottleAxleEntry.ControllerIndex : (int?)null,
                    "axis",
                    _controls.ThrottleAxleEntry.AxleIndex >= 0 ? _controls.ThrottleAxleEntry.AxleIndex : (int?)null
                );
                if (throttleBinding != null) throttleBindings.Add(throttleBinding);
                actions.Add(new ActionBinding
                {
                    Name = "THROTTLE",
                    Type = "axis",
                    Description = "Throttle pedal",
                    Bindings = throttleBindings
                });

                // BRAKES - oś
                var brakesBindings = new List<BindingInfo>();
                var brakesBinding = CreateBindingInfo("BRAKES_1",
                    _controls.BrakesAxleEntry.ControllerIndex >= 0 ? _controls.BrakesAxleEntry.ControllerIndex : (int?)null,
                    "axis",
                    _controls.BrakesAxleEntry.AxleIndex >= 0 ? _controls.BrakesAxleEntry.AxleIndex : (int?)null
                );
                if (brakesBinding != null) brakesBindings.Add(brakesBinding);
                actions.Add(new ActionBinding
                {
                    Name = "BRAKES",
                    Type = "axis",
                    Description = "Brake pedal",
                    Bindings = brakesBindings
                });

                // CLUTCH - oś
                var clutchBindings = new List<BindingInfo>();
                var clutchBinding = CreateBindingInfo("CLUTCH_1",
                    _controls.ClutchAxleEntry.ControllerIndex >= 0 ? _controls.ClutchAxleEntry.ControllerIndex : (int?)null,
                    "axis",
                    _controls.ClutchAxleEntry.AxleIndex >= 0 ? _controls.ClutchAxleEntry.AxleIndex : (int?)null
                );
                if (clutchBinding != null) clutchBindings.Add(clutchBinding);
                actions.Add(new ActionBinding
                {
                    Name = "CLUTCH",
                    Type = "axis",
                    Description = "Clutch pedal",
                    Bindings = clutchBindings
                });

                // HANDBRAKE - zawsze jako oś
                var handbrakeBindings = new List<BindingInfo>();
                if (_controls.HandbrakeAxleEntry.AxleIndex >= 0)
                {
                    var hbAxisBinding = CreateBindingInfo("HANDBRAKE_1",
                        _controls.HandbrakeAxleEntry.ControllerIndex >= 0 ? _controls.HandbrakeAxleEntry.ControllerIndex : (int?)null,
                        "axis",
                        _controls.HandbrakeAxleEntry.AxleIndex
                    );
                    if (hbAxisBinding != null) handbrakeBindings.Add(hbAxisBinding);
                }
                actions.Add(new ActionBinding
                {
                    Name = "HANDBRAKE",
                    Type = "axis",
                    Description = "Handbrake",
                    Bindings = handbrakeBindings
                });

                // GEARUP - osobna akcja
                var gearUpBindings = new List<BindingInfo>();
                if (_controls.GearUpButtonEntry.ButtonIndex >= 0)
                {
                    var guBinding = CreateBindingInfo("GEARUP_1",
                        _controls.GearUpButtonEntry.ControllerIndex >= 0 ? _controls.GearUpButtonEntry.ControllerIndex : (int?)null,
                        "button",
                        _controls.GearUpButtonEntry.ButtonIndex
                    );
                    if (guBinding != null) gearUpBindings.Add(guBinding);
                }
                actions.Add(new ActionBinding
                {
                    Name = "GEARUP",
                    Type = "button",
                    Description = "Gear up",
                    Bindings = gearUpBindings
                });

                // PADDLEUP - osobna akcja
                var paddleUpBindings = new List<BindingInfo>();
                if (_controls.PaddleUpButtonEntry.ButtonIndex >= 0)
                {
                    var puBinding = CreateBindingInfo("PADDLEUP_1",
                        _controls.PaddleUpButtonEntry.ControllerIndex >= 0 ? _controls.PaddleUpButtonEntry.ControllerIndex : (int?)null,
                        "button",
                        _controls.PaddleUpButtonEntry.ButtonIndex
                    );
                    if (puBinding != null) paddleUpBindings.Add(puBinding);
                }
                actions.Add(new ActionBinding
                {
                    Name = "PADDLEUP",
                    Type = "button",
                    Description = "Paddle up",
                    Bindings = paddleUpBindings
                });

                // GEARDN - osobna akcja
                var gearDnBindings = new List<BindingInfo>();
                if (_controls.GearDnButtonEntry.ButtonIndex >= 0)
                {
                    var gdBinding = CreateBindingInfo("GEARDN_1",
                        _controls.GearDnButtonEntry.ControllerIndex >= 0 ? _controls.GearDnButtonEntry.ControllerIndex : (int?)null,
                        "button",
                        _controls.GearDnButtonEntry.ButtonIndex
                    );
                    if (gdBinding != null) gearDnBindings.Add(gdBinding);
                }
                actions.Add(new ActionBinding
                {
                    Name = "GEARDN",
                    Type = "button",
                    Description = "Gear down",
                    Bindings = gearDnBindings
                });

                // PADDLEDN - osobna akcja
                var paddleDnBindings = new List<BindingInfo>();
                if (_controls.PaddleDnButtonEntry.ButtonIndex >= 0)
                {
                    var pdBinding = CreateBindingInfo("PADDLEDN_1",
                        _controls.PaddleDnButtonEntry.ControllerIndex >= 0 ? _controls.PaddleDnButtonEntry.ControllerIndex : (int?)null,
                        "button",
                        _controls.PaddleDnButtonEntry.ButtonIndex
                    );
                    if (pdBinding != null) paddleDnBindings.Add(pdBinding);
                }
                actions.Add(new ActionBinding
                {
                    Name = "PADDLEDN",
                    Type = "button",
                    Description = "Paddle down",
                    Bindings = paddleDnBindings
                });

                // Biegi H-shiftera (1-7 i R)
                var gearBindings = new List<BindingInfo>();
                var gearNames = new[] { "GEAR_1", "GEAR_2", "GEAR_3", "GEAR_4", "GEAR_5", "GEAR_6", "GEAR_7", "GEAR_R" };
                foreach (var gearName in gearNames)
                {
                    if (_controls.ShifterGears.ContainsKey(gearName) && _controls.ShifterGears[gearName] >= 0)
                    {
                        var gearBinding = CreateBindingInfo(gearName,
                            _controls.ShifterJoy >= 0 ? _controls.ShifterJoy : (int?)null,
                            "button",
                            _controls.ShifterGears[gearName]
                        );
                        if (gearBinding != null) gearBindings.Add(gearBinding);
                    }
                }
                actions.Add(new ActionBinding
                {
                    Name = "GEARS",
                    Type = "button",
                    Description = "H-Shifter gears (1-7 and R)",
                    Bindings = gearBindings
                });

                // CAMERA - przycisk
                var cameraBindings = new List<BindingInfo>();
                var cameraBinding = CreateBindingInfo("CAMERA_1",
                    _controls.CameraButtonEntry.ControllerIndex >= 0 ? _controls.CameraButtonEntry.ControllerIndex : (int?)null,
                    "button",
                    _controls.CameraButtonEntry.ButtonIndex >= 0 ? _controls.CameraButtonEntry.ButtonIndex : (int?)null
                );
                if (cameraBinding != null) cameraBindings.Add(cameraBinding);
                actions.Add(new ActionBinding
                {
                    Name = "CAMERA",
                    Type = "button",
                    Description = "Change camera",
                    Bindings = cameraBindings
                });

                var response = new BindingsResponse
                {
                    Actions = actions
                };

                return Ok(ApiResponse<BindingsResponse>.Ok(response));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<BindingsResponse>.Error(ex.Message));
            }
        }


        // ========== Endpointy do rozpoczęcia nasłuchiwania dla każdej akcji ==========

        /// <summary>
        /// Rozpoczyna nasłuchiwanie na input dla STEER
        /// </summary>
        [HttpPost]
        [Route("bind/steer/start")]
        public IHttpActionResult StartSteerBinding([FromBody] BindingDetectionRequest request)
        {
            return StartBindingDetection("STEER", request);
        }

        /// <summary>
        /// Rozpoczyna nasłuchiwanie na input dla THROTTLE
        /// </summary>
        [HttpPost]
        [Route("bind/throttle/start")]
        public IHttpActionResult StartThrottleBinding([FromBody] BindingDetectionRequest request)
        {
            return StartBindingDetection("THROTTLE", request);
        }

        /// <summary>
        /// Rozpoczyna nasłuchiwanie na input dla BRAKES
        /// </summary>
        [HttpPost]
        [Route("bind/brakes/start")]
        public IHttpActionResult StartBrakesBinding([FromBody] BindingDetectionRequest request)
        {
            return StartBindingDetection("BRAKES", request);
        }

        /// <summary>
        /// Rozpoczyna nasłuchiwanie na input dla CLUTCH
        /// </summary>
        [HttpPost]
        [Route("bind/clutch/start")]
        public IHttpActionResult StartClutchBinding([FromBody] BindingDetectionRequest request)
        {
            return StartBindingDetection("CLUTCH", request);
        }

        /// <summary>
        /// Rozpoczyna nasłuchiwanie na input dla HANDBRAKE
        /// </summary>
        [HttpPost]
        [Route("bind/handbrake/start")]
        public IHttpActionResult StartHandbrakeBinding([FromBody] BindingDetectionRequest request)
        {
            return StartBindingDetection("HANDBRAKE", request);
        }

        /// <summary>
        /// Rozpoczyna nasłuchiwanie na input dla GEARUP
        /// </summary>
        [HttpPost]
        [Route("bind/gearup/start")]
        public IHttpActionResult StartGearUpBinding([FromBody] BindingDetectionRequest request)
        {
            return StartBindingDetection("GEARUP", request);
        }

        /// <summary>
        /// Rozpoczyna nasłuchiwanie na input dla PADDLEUP
        /// </summary>
        [HttpPost]
        [Route("bind/paddleup/start")]
        public IHttpActionResult StartPaddleUpBinding([FromBody] BindingDetectionRequest request)
        {
            return StartBindingDetection("PADDLEUP", request);
        }

        /// <summary>
        /// Rozpoczyna nasłuchiwanie na input dla GEARDN
        /// </summary>
        [HttpPost]
        [Route("bind/geardn/start")]
        public IHttpActionResult StartGearDownBinding([FromBody] BindingDetectionRequest request)
        {
            return StartBindingDetection("GEARDN", request);
        }

        /// <summary>
        /// Rozpoczyna nasłuchiwanie na input dla PADDLEDN
        /// </summary>
        [HttpPost]
        [Route("bind/paddledn/start")]
        public IHttpActionResult StartPaddleDownBinding([FromBody] BindingDetectionRequest request)
        {
            return StartBindingDetection("PADDLEDN", request);
        }

        /// <summary>
        /// Rozpoczyna nasłuchiwanie na input dla CAMERA
        /// </summary>
        [HttpPost]
        [Route("bind/camera/start")]
        public IHttpActionResult StartCameraBinding([FromBody] BindingDetectionRequest request)
        {
            return StartBindingDetection("CAMERA", request);
        }

        // ========== Endpointy GET do sprawdzania statusu nasłuchiwania dla każdej akcji ==========

        /// <summary>
        /// Sprawdza status nasłuchiwania dla STEER
        /// </summary>
        [HttpGet]
        [Route("bind/steer")]
        public IHttpActionResult GetSteerBindingStatus()
        {
            return GetBindingStatus("STEER");
        }

        /// <summary>
        /// Sprawdza status nasłuchiwania dla THROTTLE
        /// </summary>
        [HttpGet]
        [Route("bind/throttle")]
        public IHttpActionResult GetThrottleBindingStatus()
        {
            return GetBindingStatus("THROTTLE");
        }

        /// <summary>
        /// Sprawdza status nasłuchiwania dla BRAKES
        /// </summary>
        [HttpGet]
        [Route("bind/brakes")]
        public IHttpActionResult GetBrakesBindingStatus()
        {
            return GetBindingStatus("BRAKES");
        }

        /// <summary>
        /// Sprawdza status nasłuchiwania dla CLUTCH
        /// </summary>
        [HttpGet]
        [Route("bind/clutch")]
        public IHttpActionResult GetClutchBindingStatus()
        {
            return GetBindingStatus("CLUTCH");
        }

        /// <summary>
        /// Sprawdza status nasłuchiwania dla HANDBRAKE
        /// </summary>
        [HttpGet]
        [Route("bind/handbrake")]
        public IHttpActionResult GetHandbrakeBindingStatus()
        {
            return GetBindingStatus("HANDBRAKE");
        }

        /// <summary>
        /// Sprawdza status nasłuchiwania dla GEARUP
        /// </summary>
        [HttpGet]
        [Route("bind/gearup")]
        public IHttpActionResult GetGearUpBindingStatus()
        {
            return GetBindingStatus("GEARUP");
        }

        /// <summary>
        /// Sprawdza status nasłuchiwania dla PADDLEUP
        /// </summary>
        [HttpGet]
        [Route("bind/paddleup")]
        public IHttpActionResult GetPaddleUpBindingStatus()
        {
            return GetBindingStatus("PADDLEUP");
        }

        /// <summary>
        /// Sprawdza status nasłuchiwania dla GEARDN
        /// </summary>
        [HttpGet]
        [Route("bind/geardn")]
        public IHttpActionResult GetGearDownBindingStatus()
        {
            return GetBindingStatus("GEARDN");
        }

        /// <summary>
        /// Sprawdza status nasłuchiwania dla PADDLEDN
        /// </summary>
        [HttpGet]
        [Route("bind/paddledn")]
        public IHttpActionResult GetPaddleDownBindingStatus()
        {
            return GetBindingStatus("PADDLEDN");
        }

        /// <summary>
        /// Sprawdza status nasłuchiwania dla CAMERA
        /// </summary>
        [HttpGet]
        [Route("bind/camera")]
        public IHttpActionResult GetCameraBindingStatus()
        {
            return GetBindingStatus("CAMERA");
        }

        // Metoda pomocnicza do sprawdzania statusu nasłuchiwania
        private IHttpActionResult GetBindingStatus(string action)
        {
            try
            {
                string actionUpper = action.ToUpper();

                lock (_detectionsLock)
                {
                    var status = _activeDetections.Values
                        .Where(d => d.Action == actionUpper)
                        .OrderByDescending(d => d.StartTime)
                        .FirstOrDefault();

                    if (status == null)
                    {
                        // Jeśli nie ma aktywnej sesji, zwróć pusty status
                        return Ok(ApiResponse<BindingDetectionStatus>.Ok(new BindingDetectionStatus
                        {
                            Action = actionUpper,
                            IsListening = false,
                            IsCompleted = false,
                            IsCancelled = false,
                            Success = false,
                            StatusMessage = "Brak aktywnej sesji nasłuchiwania"
                        }));
                    }

                    return Ok(ApiResponse<BindingDetectionStatus>.Ok(status));
                }
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<BindingDetectionStatus>.Error(ex.Message));
            }
        }

        // Metoda pomocnicza do rozpoczęcia nasłuchiwania
        private IHttpActionResult StartBindingDetection(string action, BindingDetectionRequest request)
        {
            try
            {
                string actionUpper = action.ToUpper();
                string detectionId = $"{actionUpper}_{request.ControllerIndex}_{DateTime.UtcNow.Ticks}";

                lock (_detectionsLock)
                {
                    // Sprawdź czy już istnieje aktywna sesja dla tej akcji
                    var existing = _activeDetections.Values.FirstOrDefault(d =>
                        d.Action == actionUpper && d.IsListening && !d.IsCompleted && !d.IsCancelled);

                    if (existing != null)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] Nasłuchiwanie dla {actionUpper} jest już aktywne. Anuluj je najpierw.");
                        return Ok(ApiResponse<BindingDetectionStatus>.Error(
                            $"Nasłuchiwanie dla {actionUpper} jest już aktywne. Anuluj je najpierw."));
                    }

                    // Utwórz nową sesję
                    var status = new BindingDetectionStatus
                    {
                        Action = actionUpper,
                        IsListening = true,
                        IsCompleted = false,
                        IsCancelled = false,
                        StatusMessage = "Czekam na input...",
                        StartTime = DateTime.UtcNow,
                        TimeoutSeconds = request.TimeoutSeconds ?? 15
                    };

                    _activeDetections[detectionId] = status;
                    var cts = new CancellationTokenSource();
                    _cancellationTokens[detectionId] = cts;

                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] Rozpoczęto nasłuchiwanie dla akcji: {actionUpper}, urządzenie: {request.ControllerIndex}, timeout: {status.TimeoutSeconds}s");

                    // Uruchom nasłuchiwanie w tle
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            if (actionUpper == "STEER" || actionUpper == "THROTTLE" ||
                                actionUpper == "BRAKES" || actionUpper == "CLUTCH" ||
                                actionUpper == "HANDBRAKE")
                            {
                                DetectAxisInBackground(detectionId, request.ControllerIndex,
                                    actionUpper, status.TimeoutSeconds.Value, cts.Token);
                            }
                            else if (actionUpper == "GEARUP" || actionUpper == "GEARDN" ||
                                     actionUpper == "PADDLEUP" || actionUpper == "PADDLEDN" ||
                                     actionUpper == "CAMERA")
                            {
                                DetectButtonInBackground(detectionId, request.ControllerIndex,
                                    actionUpper, status.TimeoutSeconds.Value, cts.Token);
                            }
                            else
                            {
                                lock (_detectionsLock)
                                {
                                    status.IsListening = false;
                                    status.IsCompleted = true;
                                    status.StatusMessage = $"Nieobsługiwana akcja: {actionUpper}";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] Błąd podczas nasłuchiwania dla {actionUpper}: {ex.Message}");
                            lock (_detectionsLock)
                            {
                                status.IsListening = false;
                                status.IsCompleted = true;
                                status.StatusMessage = $"Błąd: {ex.Message}";
                            }
                        }
                    });

                    return Ok(ApiResponse<BindingDetectionStatus>.Ok(new BindingDetectionStatus
                    {
                        Action = actionUpper,
                        IsListening = true,
                        IsCompleted = false,
                        IsCancelled = false,
                        StatusMessage = "Nasłuchiwanie rozpoczęte. Użyj GET /api/controls/bind/{action} aby sprawdzić wynik.",
                        StartTime = status.StartTime,
                        TimeoutSeconds = status.TimeoutSeconds
                    }));
                }
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<BindingDetectionStatus>.Error(ex.Message));
            }
        }

        // Metody pomocnicze do wykrywania w tle
        private void DetectAxisInBackground(string detectionId, int controllerIndex,
            string action, int timeoutSeconds, CancellationToken cancellationToken)
        {
            var status = _activeDetections[detectionId];

            try
            {
                var devices = JoystickInputHandler.GetAvailableDevices();

                // Jeśli controllerIndex == -1, nasłuchuj na wszystkich urządzeniach
                if (controllerIndex == -1)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] Nasłuchiwanie na wszystkich urządzeniach (wykrywanie osi)");
                    DetectAxisFromAllDevices(detectionId, action, timeoutSeconds, cancellationToken);
                    return;
                }

                if (controllerIndex < 0 || controllerIndex >= devices.Count)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] BŁĄD: Nieprawidłowy indeks urządzenia: {controllerIndex}");
                    lock (_detectionsLock)
                    {
                        status.IsListening = false;
                        status.IsCompleted = true;
                        status.StatusMessage = "Nieprawidłowy indeks urządzenia";
                    }
                    return;
                }

                var deviceName = devices[controllerIndex].Name;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] Rozpoczęto wykrywanie osi na urządzeniu: {deviceName} (index: {controllerIndex})");

                var handler = new JoystickInputHandler();
                if (!handler.Initialize(devices[controllerIndex].Guid))
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] BŁĄD: Nie udało się połączyć z urządzeniem: {deviceName}");
                    lock (_detectionsLock)
                    {
                        status.IsListening = false;
                        status.IsCompleted = true;
                        status.StatusMessage = "Nie udało się połączyć z urządzeniem";
                    }
                    return;
                }

                Thread.Sleep(200);
                handler.OnTick();

                double[] initialAxisValues = new double[handler.Axes.Count];
                double[] previousAxisValues = new double[handler.Axes.Count];
                for (int i = 0; i < handler.Axes.Count; i++)
                {
                    initialAxisValues[i] = handler.Axes[i].Value;
                    previousAxisValues[i] = handler.Axes[i].Value;
                }

                DateTime startTime = DateTime.Now;
                int detectedAxis = -1;
                double maxChange = 0;
                int axisWithMaxChange = -1;

                while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        handler.Dispose();
                        return;
                    }

                    handler.OnTick();

                    for (int i = 0; i < handler.Axes.Count; i++)
                    {
                        var axis = handler.Axes[i];
                        double currentValue = axis.Value;

                        // Sprawdź zmianę względem poprzedniego ticka (ruch użytkownika)
                        // Używamy tego samego progu co debug OnTick (0.001 = 0.1%)
                        double changeFromPrevious = Math.Abs(currentValue - previousAxisValues[i]);

                        // Jeśli zmiana jest większa niż 0.1% (0.001) - tak jak w debugu OnTick
                        // to znaczy że widzimy ruch i powinniśmy to wykryć
                        if (changeFromPrevious > 0.001)
                        {
                            if (changeFromPrevious > maxChange)
                            {
                                maxChange = changeFromPrevious;
                                axisWithMaxChange = i;
                            }
                        }

                        // Zapisz aktualną wartość jako poprzednią dla następnego ticka
                        previousAxisValues[i] = currentValue;
                    }

                    // Jeśli znaleziono oś z ruchem (tak jak w debugu), użyj jej od razu
                    // Używamy tego samego progu co debug - jeśli debug pokazuje ruch, to ustawiamy bind
                    if (axisWithMaxChange >= 0 && maxChange > 0.001)
                    {
                        detectedAxis = axisWithMaxChange;
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] Wykryto ruch osi {detectedAxis + 1} (zmiana: {maxChange:F4})");
                        break;
                    }

                    Thread.Sleep(50);
                }

                handler.Dispose();

                lock (_detectionsLock)
                {
                    status.IsListening = false;
                    status.IsCompleted = true;

                    if (detectedAxis >= 0)
                    {
                        status.DetectedAxis = detectedAxis + 1; // Indeksujemy od 1 dla użytkownika
                        status.DetectedControllerIndex = controllerIndex;
                        status.DetectedControllerName = deviceName;
                        status.Success = true;
                        status.StatusMessage = $"Wykryto oś {detectedAxis + 1} na urządzeniu {deviceName}";
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✓ Wykryto oś {detectedAxis + 1} na urządzeniu {deviceName} (index: {controllerIndex})");

                        // Automatyczne bindowanie po wykryciu
                        try
                        {
                            bool bindSuccess = _controls.AssignAxisBinding(action, controllerIndex, detectedAxis);
                            if (bindSuccess)
                            {
                                StateHelper.SaveCurrentState();
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✓ Automatycznie zbindowano oś {detectedAxis + 1}");
                                status.StatusMessage = $"Wykryto i zbindowano oś {detectedAxis + 1} na urządzeniu {deviceName}";
                            }
                            else
                            {
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✗ Nie udało się zbindować osi {detectedAxis + 1}");
                            }
                        }
                        catch (Exception bindEx)
                        {
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] BŁĄD podczas automatycznego bindowania: {bindEx.Message}");
                        }
                    }
                    else
                    {
                        status.StatusMessage = "Nie wykryto ruchu osi w czasie oczekiwania";
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✗ Nie wykryto ruchu osi w czasie oczekiwania ({timeoutSeconds}s)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] BŁĄD w DetectAxisInBackground: {ex.Message}");
                lock (_detectionsLock)
                {
                    status.IsListening = false;
                    status.IsCompleted = true;
                    status.StatusMessage = $"Błąd: {ex.Message}";
                }
            }
        }

        private void DetectAxisFromAllDevices(string detectionId, string action, int timeoutSeconds, CancellationToken cancellationToken)
        {
            var status = _activeDetections[detectionId];
            var devices = JoystickInputHandler.GetAvailableDevices();
            var handlers = new List<JoystickInputHandler>();
            var initialValues = new List<double[]>();

            try
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] Nasłuchiwanie na wszystkich urządzeniach ({devices.Count} urządzeń)");

                // Inicjalizuj wszystkie urządzenia
                for (int devIdx = 0; devIdx < devices.Count; devIdx++)
                {
                    var handler = new JoystickInputHandler();
                    if (handler.Initialize(devices[devIdx].Guid))
                    {
                        handlers.Add(handler);
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] Zainicjalizowano urządzenie: {devices[devIdx].Name}");
                        Thread.Sleep(50);
                        handler.OnTick();
                        var axisValues = new double[handler.Axes.Count];
                        for (int i = 0; i < handler.Axes.Count; i++)
                        {
                            axisValues[i] = handler.Axes[i].Value;
                        }
                        initialValues.Add(axisValues);
                    }
                }

                if (handlers.Count == 0)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] BŁĄD: Brak dostępnych urządzeń");
                    lock (_detectionsLock)
                    {
                        status.IsListening = false;
                        status.IsCompleted = true;
                        status.StatusMessage = "Brak dostępnych urządzeń";
                    }
                    return;
                }

                Thread.Sleep(200);

                DateTime startTime = DateTime.Now;
                int detectedDeviceIndex = -1;
                int detectedAxis = -1;
                double maxChange = 0;

                while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        foreach (var h in handlers) h.Dispose();
                        return;
                    }

                    // Sprawdź wszystkie urządzenia
                    for (int devIdx = 0; devIdx < handlers.Count; devIdx++)
                    {
                        var handler = handlers[devIdx];
                        handler.OnTick();

                        for (int axisIdx = 0; axisIdx < handler.Axes.Count; axisIdx++)
                        {
                            var axis = handler.Axes[axisIdx];
                            double currentValue = axis.Value;
                            double change = Math.Abs(currentValue - initialValues[devIdx][axisIdx]);

                            if (change > 0.05 && change > maxChange)
                            {
                                maxChange = change;
                                detectedDeviceIndex = devIdx;
                                detectedAxis = axisIdx;
                            }
                        }
                    }

                    if (detectedDeviceIndex >= 0 && maxChange > 0.1)
                    {
                        break;
                    }

                    Thread.Sleep(50);
                }

                // Zwolnij wszystkie handlery
                foreach (var h in handlers) h.Dispose();

                lock (_detectionsLock)
                {
                    status.IsListening = false;
                    status.IsCompleted = true;

                    if (detectedDeviceIndex >= 0 && detectedAxis >= 0)
                    {
                        var deviceName = devices[detectedDeviceIndex].Name;
                        status.DetectedAxis = detectedAxis + 1; // Indeksujemy od 1 dla użytkownika
                        status.DetectedControllerIndex = detectedDeviceIndex;
                        status.DetectedControllerName = deviceName;
                        status.Success = true;
                        status.StatusMessage = $"Wykryto oś {detectedAxis + 1} na urządzeniu {deviceName}";
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✓ Wykryto oś {detectedAxis + 1} na urządzeniu {deviceName} (index: {detectedDeviceIndex})");

                        // Automatyczne bindowanie po wykryciu
                        try
                        {
                            bool bindSuccess = _controls.AssignAxisBinding(action, detectedDeviceIndex, detectedAxis);
                            if (bindSuccess)
                            {
                                StateHelper.SaveCurrentState();
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✓ Automatycznie zbindowano oś {detectedAxis + 1}");
                                status.StatusMessage = $"Wykryto i zbindowano oś {detectedAxis + 1} na urządzeniu {deviceName}";
                            }
                            else
                            {
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✗ Nie udało się zbindować osi {detectedAxis + 1}");
                            }
                        }
                        catch (Exception bindEx)
                        {
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] BŁĄD podczas automatycznego bindowania: {bindEx.Message}");
                        }
                    }
                    else
                    {
                        status.StatusMessage = "Nie wykryto ruchu osi w czasie oczekiwania";
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✗ Nie wykryto ruchu osi w czasie oczekiwania ({timeoutSeconds}s)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] BŁĄD w DetectAxisFromAllDevices: {ex.Message}");
                foreach (var h in handlers) h.Dispose();
                lock (_detectionsLock)
                {
                    status.IsListening = false;
                    status.IsCompleted = true;
                    status.StatusMessage = $"Błąd: {ex.Message}";
                }
            }
        }

        private void DetectButtonInBackground(string detectionId, int controllerIndex,
            string action, int timeoutSeconds, CancellationToken cancellationToken)
        {
            var status = _activeDetections[detectionId];

            try
            {
                var devices = JoystickInputHandler.GetAvailableDevices();

                // Jeśli controllerIndex == -1, nasłuchuj na wszystkich urządzeniach
                if (controllerIndex == -1)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] Nasłuchiwanie na wszystkich urządzeniach (wykrywanie przycisku)");
                    DetectButtonFromAllDevices(detectionId, action, timeoutSeconds, cancellationToken);
                    return;
                }

                if (controllerIndex < 0 || controllerIndex >= devices.Count)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] BŁĄD: Nieprawidłowy indeks urządzenia: {controllerIndex}");
                    lock (_detectionsLock)
                    {
                        status.IsListening = false;
                        status.IsCompleted = true;
                        status.StatusMessage = "Nieprawidłowy indeks urządzenia";
                    }
                    return;
                }

                var deviceName = devices[controllerIndex].Name;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] Rozpoczęto wykrywanie przycisku na urządzeniu: {deviceName} (index: {controllerIndex})");

                var handler = new JoystickInputHandler();
                if (!handler.Initialize(devices[controllerIndex].Guid))
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] BŁĄD: Nie udało się połączyć z urządzeniem: {deviceName}");
                    lock (_detectionsLock)
                    {
                        status.IsListening = false;
                        status.IsCompleted = true;
                        status.StatusMessage = "Nie udało się połączyć z urządzeniem";
                    }
                    return;
                }

                Thread.Sleep(200);

                DateTime startTime = DateTime.Now;
                int detectedButton = -1;
                bool buttonWasPressed = false;

                while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        handler.Dispose();
                        return;
                    }
                    handler.OnTick();

                    for (int i = 0; i < handler.Buttons.Count; i++)
                    {
                        if (handler.Buttons[i].Value)
                        {
                            if (!buttonWasPressed)
                            {
                                buttonWasPressed = true;
                                detectedButton = i;
                                Thread.Sleep(100);
                                handler.OnTick();

                                if (handler.Buttons[i].Value)
                                {
                                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] Wykryto wciśnięcie przycisku {i}");
                                    break;
                                }
                                else
                                {
                                    buttonWasPressed = false;
                                    detectedButton = -1;
                                }
                            }
                            else if (detectedButton == i)
                            {
                                break;
                            }
                        }
                    }

                    if (detectedButton >= 0 && buttonWasPressed)
                    {
                        Thread.Sleep(200);
                        handler.OnTick();
                        if (!handler.Buttons[detectedButton].Value)
                        {
                            break;
                        }
                    }

                    Thread.Sleep(50);
                }

                handler.Dispose();

                lock (_detectionsLock)
                {
                    status.IsListening = false;
                    status.IsCompleted = true;

                    if (detectedButton >= 0)
                    {
                        status.DetectedButton = detectedButton;
                        status.DetectedControllerIndex = controllerIndex;
                        status.DetectedControllerName = deviceName;
                        status.Success = true;
                        status.StatusMessage = $"Wykryto przycisk {detectedButton} na urządzeniu {deviceName}";
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✓ Wykryto przycisk {detectedButton} na urządzeniu {deviceName} (index: {controllerIndex})");

                        // Automatyczne bindowanie po wykryciu
                        try
                        {
                            string actionToUse = action;
                            if (action == "CAMERA")
                            {
                                actionToUse = "ACTION_CHANGE_CAMERA";
                            }

                            bool bindSuccess = _controls.AssignButtonBinding(actionToUse, controllerIndex, detectedButton);
                            if (bindSuccess)
                            {
                                StateHelper.SaveCurrentState();
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✓ Automatycznie zbindowano przycisk {detectedButton}");
                                status.StatusMessage = $"Wykryto i zbindowano przycisk {detectedButton} na urządzeniu {deviceName}";
                            }
                            else
                            {
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✗ Nie udało się zbindować przycisku {detectedButton}");
                            }
                        }
                        catch (Exception bindEx)
                        {
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] BŁĄD podczas automatycznego bindowania: {bindEx.Message}");
                        }
                    }
                    else
                    {
                        status.StatusMessage = "Nie wykryto wciśnięcia przycisku w czasie oczekiwania";
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✗ Nie wykryto wciśnięcia przycisku w czasie oczekiwania ({timeoutSeconds}s)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] BŁĄD w DetectButtonInBackground: {ex.Message}");
                lock (_detectionsLock)
                {
                    status.IsListening = false;
                    status.IsCompleted = true;
                    status.StatusMessage = $"Błąd: {ex.Message}";
                }
            }
        }

        private void DetectButtonFromAllDevices(string detectionId, string action, int timeoutSeconds, CancellationToken cancellationToken)
        {
            var status = _activeDetections[detectionId];
            var devices = JoystickInputHandler.GetAvailableDevices();
            var handlers = new List<JoystickInputHandler>();

            try
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] Nasłuchiwanie na wszystkich urządzeniach ({devices.Count} urządzeń)");

                // Inicjalizuj wszystkie urządzenia
                for (int devIdx = 0; devIdx < devices.Count; devIdx++)
                {
                    var handler = new JoystickInputHandler();
                    if (handler.Initialize(devices[devIdx].Guid))
                    {
                        handlers.Add(handler);
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] Zainicjalizowano urządzenie: {devices[devIdx].Name}");
                    }
                }

                if (handlers.Count == 0)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] BŁĄD: Brak dostępnych urządzeń");
                    lock (_detectionsLock)
                    {
                        status.IsListening = false;
                        status.IsCompleted = true;
                        status.StatusMessage = "Brak dostępnych urządzeń";
                    }
                    return;
                }

                Thread.Sleep(200);

                DateTime startTime = DateTime.Now;
                int detectedDeviceIndex = -1;
                int detectedButton = -1;
                bool buttonWasPressed = false;

                while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        foreach (var h in handlers) h.Dispose();
                        return;
                    }

                    // Sprawdź wszystkie urządzenia
                    for (int devIdx = 0; devIdx < handlers.Count; devIdx++)
                    {
                        var handler = handlers[devIdx];
                        handler.OnTick();

                        for (int btnIdx = 0; btnIdx < handler.Buttons.Count; btnIdx++)
                        {
                            if (handler.Buttons[btnIdx].Value)
                            {
                                if (!buttonWasPressed)
                                {
                                    buttonWasPressed = true;
                                    detectedDeviceIndex = devIdx;
                                    detectedButton = btnIdx;
                                    Thread.Sleep(100);
                                    handler.OnTick();

                                    if (handler.Buttons[btnIdx].Value)
                                    {
                                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] Wykryto wciśnięcie przycisku {btnIdx} na urządzeniu {devices[devIdx].Name}");
                                        break;
                                    }
                                    else
                                    {
                                        buttonWasPressed = false;
                                        detectedDeviceIndex = -1;
                                        detectedButton = -1;
                                    }
                                }
                                else if (detectedDeviceIndex == devIdx && detectedButton == btnIdx)
                                {
                                    break;
                                }
                            }
                        }

                        if (detectedDeviceIndex >= 0 && detectedButton >= 0 && buttonWasPressed)
                        {
                            Thread.Sleep(200);
                            handler.OnTick();
                            if (!handler.Buttons[detectedButton].Value)
                            {
                                break;
                            }
                        }
                    }

                    if (detectedDeviceIndex >= 0 && detectedButton >= 0 && buttonWasPressed)
                    {
                        break;
                    }

                    Thread.Sleep(50);
                }

                // Zwolnij wszystkie handlery
                foreach (var h in handlers) h.Dispose();

                lock (_detectionsLock)
                {
                    status.IsListening = false;
                    status.IsCompleted = true;

                    if (detectedDeviceIndex >= 0 && detectedButton >= 0)
                    {
                        var deviceName = devices[detectedDeviceIndex].Name;
                        status.DetectedButton = detectedButton;
                        status.DetectedControllerIndex = detectedDeviceIndex;
                        status.DetectedControllerName = deviceName;
                        status.Success = true;
                        status.StatusMessage = $"Wykryto przycisk {detectedButton} na urządzeniu {deviceName}";
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✓ Wykryto przycisk {detectedButton} na urządzeniu {deviceName} (index: {detectedDeviceIndex})");

                        // Automatyczne bindowanie po wykryciu
                        try
                        {
                            string actionToUse = action;
                            if (action == "CAMERA")
                            {
                                actionToUse = "ACTION_CHANGE_CAMERA";
                            }

                            bool bindSuccess = _controls.AssignButtonBinding(actionToUse, detectedDeviceIndex, detectedButton);
                            if (bindSuccess)
                            {
                                StateHelper.SaveCurrentState();
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✓ Automatycznie zbindowano przycisk {detectedButton}");
                                status.StatusMessage = $"Wykryto i zbindowano przycisk {detectedButton} na urządzeniu {deviceName}";
                            }
                            else
                            {
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✗ Nie udało się zbindować przycisku {detectedButton}");
                            }
                        }
                        catch (Exception bindEx)
                        {
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] BŁĄD podczas automatycznego bindowania: {bindEx.Message}");
                        }
                    }
                    else
                    {
                        status.StatusMessage = "Nie wykryto wciśnięcia przycisku w czasie oczekiwania";
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] ✗ Nie wykryto wciśnięcia przycisku w czasie oczekiwania ({timeoutSeconds}s)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [{action}] BŁĄD w DetectButtonFromAllDevices: {ex.Message}");
                foreach (var h in handlers) h.Dispose();
                lock (_detectionsLock)
                {
                    status.IsListening = false;
                    status.IsCompleted = true;
                    status.StatusMessage = $"Błąd: {ex.Message}";
                }
            }
        }

        private void DetectHandbrakeInBackground(string detectionId, int controllerIndex,
            int timeoutSeconds, CancellationToken cancellationToken)
        {
            var status = _activeDetections[detectionId];

            try
            {
                var devices = JoystickInputHandler.GetAvailableDevices();

                // Jeśli controllerIndex == -1, nasłuchuj na wszystkich urządzeniach
                if (controllerIndex == -1)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] Nasłuchiwanie na wszystkich urządzeniach");
                    DetectHandbrakeFromAllDevices(detectionId, timeoutSeconds, cancellationToken);
                    return;
                }

                if (controllerIndex < 0 || controllerIndex >= devices.Count)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] BŁĄD: Nieprawidłowy indeks urządzenia: {controllerIndex}");
                    lock (_detectionsLock)
                    {
                        status.IsListening = false;
                        status.IsCompleted = true;
                        status.StatusMessage = "Nieprawidłowy indeks urządzenia";
                    }
                    return;
                }

                var deviceName = devices[controllerIndex].Name;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] Rozpoczęto wykrywanie (przycisk/oś) na urządzeniu: {deviceName} (index: {controllerIndex})");

                var handler = new JoystickInputHandler();
                if (!handler.Initialize(devices[controllerIndex].Guid))
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] BŁĄD: Nie udało się połączyć z urządzeniem: {deviceName}");
                    lock (_detectionsLock)
                    {
                        status.IsListening = false;
                        status.IsCompleted = true;
                        status.StatusMessage = "Nie udało się połączyć z urządzeniem";
                    }
                    return;
                }

                Thread.Sleep(200);

                // Najpierw spróbuj wykryć przycisk
                DateTime startTime = DateTime.Now;
                int detectedButton = -1;
                bool buttonWasPressed = false;

                while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds / 2)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        handler.Dispose();
                        return;
                    }

                    handler.OnTick();

                    for (int i = 0; i < handler.Buttons.Count; i++)
                    {
                        if (handler.Buttons[i].Value)
                        {
                            if (!buttonWasPressed)
                            {
                                buttonWasPressed = true;
                                detectedButton = i;
                                Thread.Sleep(100);
                                handler.OnTick();

                                if (handler.Buttons[i].Value)
                                {
                                    break;
                                }
                                else
                                {
                                    buttonWasPressed = false;
                                    detectedButton = -1;
                                }
                            }
                        }
                    }

                    if (detectedButton >= 0 && buttonWasPressed)
                    {
                        Thread.Sleep(200);
                        handler.OnTick();
                        if (!handler.Buttons[detectedButton].Value)
                        {
                            break;
                        }
                    }

                    Thread.Sleep(50);
                }

                if (detectedButton >= 0)
                {
                    handler.Dispose();
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] ✓ Wykryto przycisk {detectedButton} na urządzeniu {deviceName} (index: {controllerIndex})");
                    lock (_detectionsLock)
                    {
                        status.IsListening = false;
                        status.IsCompleted = true;
                        status.DetectedButton = detectedButton;
                        status.DetectedControllerIndex = controllerIndex;
                        status.DetectedControllerName = deviceName;
                        status.Success = true;
                        status.StatusMessage = $"Wykryto przycisk {detectedButton} na urządzeniu {deviceName}";
                    }
                    return;
                }

                // Jeśli przycisk nie został wykryty, spróbuj oś
                handler.OnTick();
                double[] initialAxisValues = new double[handler.Axes.Count];
                double[] previousAxisValues = new double[handler.Axes.Count];
                for (int i = 0; i < handler.Axes.Count; i++)
                {
                    initialAxisValues[i] = handler.Axes[i].Value;
                    previousAxisValues[i] = handler.Axes[i].Value;
                }

                startTime = DateTime.Now;
                int detectedAxis = -1;
                double maxChange = 0;
                int axisWithMaxChange = -1;

                while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds / 2)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        handler.Dispose();
                        return;
                    }

                    handler.OnTick();

                    for (int i = 0; i < handler.Axes.Count; i++)
                    {
                        var axis = handler.Axes[i];
                        double currentValue = axis.Value;

                        // Sprawdź zmianę względem poprzedniego ticka (ruch użytkownika)
                        // Używamy tego samego progu co debug OnTick (0.001 = 0.1%)
                        double changeFromPrevious = Math.Abs(currentValue - previousAxisValues[i]);

                        // Jeśli zmiana jest większa niż 0.1% (0.001) - tak jak w debugu OnTick
                        // to znaczy że widzimy ruch i powinniśmy to wykryć
                        if (changeFromPrevious > 0.001)
                        {
                            if (changeFromPrevious > maxChange)
                            {
                                maxChange = changeFromPrevious;
                                axisWithMaxChange = i;
                            }
                        }

                        // Zapisz aktualną wartość jako poprzednią dla następnego ticka
                        previousAxisValues[i] = currentValue;
                    }

                    // Jeśli znaleziono oś z ruchem (tak jak w debugu), użyj jej od razu
                    // Używamy tego samego progu co debug - jeśli debug pokazuje ruch, to ustawiamy bind
                    if (axisWithMaxChange >= 0 && maxChange > 0.001)
                    {
                        detectedAxis = axisWithMaxChange;
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] Wykryto ruch osi {detectedAxis + 1} (zmiana: {maxChange:F4})");
                        break;
                    }

                    Thread.Sleep(50);
                }

                handler.Dispose();

                lock (_detectionsLock)
                {
                    status.IsListening = false;
                    status.IsCompleted = true;

                    if (detectedAxis >= 0)
                    {
                        status.DetectedAxis = detectedAxis + 1; // Indeksujemy od 1 dla użytkownika
                        status.DetectedControllerIndex = controllerIndex;
                        status.DetectedControllerName = deviceName;
                        status.Success = true;
                        status.StatusMessage = $"Wykryto oś {detectedAxis + 1} na urządzeniu {deviceName}";
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] ✓ Wykryto oś {detectedAxis + 1} na urządzeniu {deviceName} (index: {controllerIndex})");
                    }
                    else
                    {
                        status.StatusMessage = "Nie wykryto przycisku ani osi dla HANDBRAKE";
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] ✗ Nie wykryto przycisku ani osi w czasie oczekiwania ({timeoutSeconds}s)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] BŁĄD w DetectHandbrakeInBackground: {ex.Message}");
                lock (_detectionsLock)
                {
                    status.IsListening = false;
                    status.IsCompleted = true;
                    status.StatusMessage = $"Błąd: {ex.Message}";
                }
            }
        }

        private void DetectHandbrakeFromAllDevices(string detectionId, int timeoutSeconds, CancellationToken cancellationToken)
        {
            var status = _activeDetections[detectionId];
            var devices = JoystickInputHandler.GetAvailableDevices();
            var handlers = new List<JoystickInputHandler>();
            var initialValues = new List<double[]>();
            var previousValues = new List<double[]>();

            try
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] Nasłuchiwanie na wszystkich urządzeniach ({devices.Count} urządzeń)");

                // Inicjalizuj wszystkie urządzenia
                for (int devIdx = 0; devIdx < devices.Count; devIdx++)
                {
                    var handler = new JoystickInputHandler();
                    if (handler.Initialize(devices[devIdx].Guid))
                    {
                        handlers.Add(handler);
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] Zainicjalizowano urządzenie: {devices[devIdx].Name}");
                        Thread.Sleep(50);
                        handler.OnTick();
                        var axisValues = new double[handler.Axes.Count];
                        var prevValues = new double[handler.Axes.Count];
                        for (int i = 0; i < handler.Axes.Count; i++)
                        {
                            axisValues[i] = handler.Axes[i].Value;
                            prevValues[i] = handler.Axes[i].Value;
                        }
                        initialValues.Add(axisValues);
                        previousValues.Add(prevValues);
                    }
                }

                if (handlers.Count == 0)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] BŁĄD: Brak dostępnych urządzeń");
                    lock (_detectionsLock)
                    {
                        status.IsListening = false;
                        status.IsCompleted = true;
                        status.StatusMessage = "Brak dostępnych urządzeń";
                    }
                    return;
                }

                Thread.Sleep(200);

                DateTime startTime = DateTime.Now;
                int detectedDeviceIndex = -1;
                int detectedButton = -1;
                int detectedAxis = -1;
                bool buttonWasPressed = false;
                bool isButton = false;

                // Najpierw spróbuj wykryć przycisk
                while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds / 2)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        foreach (var h in handlers) h.Dispose();
                        return;
                    }

                    for (int devIdx = 0; devIdx < handlers.Count; devIdx++)
                    {
                        var handler = handlers[devIdx];
                        handler.OnTick();

                        for (int btnIdx = 0; btnIdx < handler.Buttons.Count; btnIdx++)
                        {
                            if (handler.Buttons[btnIdx].Value)
                            {
                                if (!buttonWasPressed)
                                {
                                    buttonWasPressed = true;
                                    detectedDeviceIndex = devIdx;
                                    detectedButton = btnIdx;
                                    isButton = true;
                                    Thread.Sleep(100);
                                    handler.OnTick();

                                    if (handler.Buttons[btnIdx].Value)
                                    {
                                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] Wykryto wciśnięcie przycisku {btnIdx} na urządzeniu {devices[devIdx].Name}");
                                        break;
                                    }
                                    else
                                    {
                                        buttonWasPressed = false;
                                        detectedDeviceIndex = -1;
                                        detectedButton = -1;
                                        isButton = false;
                                    }
                                }
                            }
                        }

                        if (detectedDeviceIndex >= 0 && detectedButton >= 0 && buttonWasPressed)
                        {
                            Thread.Sleep(200);
                            handler.OnTick();
                            if (!handler.Buttons[detectedButton].Value)
                            {
                                break;
                            }
                        }
                    }

                    if (detectedDeviceIndex >= 0 && detectedButton >= 0 && buttonWasPressed)
                    {
                        break;
                    }

                    Thread.Sleep(50);
                }

                // Jeśli przycisk nie został wykryty, spróbuj oś
                if (detectedDeviceIndex < 0 || !isButton)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] Przycisk nie wykryty, próba wykrycia osi...");
                    startTime = DateTime.Now;
                    double maxChange = 0;
                    int axisWithMaxChange = -1;

                    while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds / 2)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            foreach (var h in handlers) h.Dispose();
                            return;
                        }

                        for (int devIdx = 0; devIdx < handlers.Count; devIdx++)
                        {
                            var handler = handlers[devIdx];
                            handler.OnTick();

                            for (int axisIdx = 0; axisIdx < handler.Axes.Count; axisIdx++)
                            {
                                var axis = handler.Axes[axisIdx];
                                double currentValue = axis.Value;

                                // Sprawdź zmianę względem poprzedniego ticka (ruch użytkownika)
                                // Używamy tego samego progu co debug OnTick (0.001 = 0.1%)
                                double changeFromPrevious = Math.Abs(currentValue - previousValues[devIdx][axisIdx]);

                                // Jeśli zmiana jest większa niż 0.1% (0.001) - tak jak w debugu OnTick
                                // to znaczy że widzimy ruch i powinniśmy to wykryć
                                if (changeFromPrevious > 0.001 && changeFromPrevious > maxChange)
                                {
                                    maxChange = changeFromPrevious;
                                    detectedDeviceIndex = devIdx;
                                    axisWithMaxChange = axisIdx;
                                }

                                // Zapisz aktualną wartość jako poprzednią dla następnego ticka
                                previousValues[devIdx][axisIdx] = currentValue;
                            }
                        }

                        // Jeśli znaleziono oś z ruchem (tak jak w debugu), użyj jej od razu
                        // Używamy tego samego progu co debug - jeśli debug pokazuje ruch, to ustawiamy bind
                        if (detectedDeviceIndex >= 0 && axisWithMaxChange >= 0 && maxChange > 0.001)
                        {
                            detectedAxis = axisWithMaxChange;
                            isButton = false;
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] Wykryto ruch osi {detectedAxis + 1} (zmiana: {maxChange:F4})");
                            break;
                        }

                        Thread.Sleep(50);
                    }
                }

                // Zwolnij wszystkie handlery
                foreach (var h in handlers) h.Dispose();

                lock (_detectionsLock)
                {
                    status.IsListening = false;
                    status.IsCompleted = true;

                    if (detectedDeviceIndex >= 0)
                    {
                        var deviceName = devices[detectedDeviceIndex].Name;

                        if (isButton && detectedButton >= 0)
                        {
                            status.DetectedButton = detectedButton;
                            status.DetectedControllerIndex = detectedDeviceIndex;
                            status.DetectedControllerName = deviceName;
                            status.Success = true;
                            status.StatusMessage = $"Wykryto przycisk {detectedButton} na urządzeniu {deviceName}";
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] ✓ Wykryto przycisk {detectedButton} na urządzeniu {deviceName} (index: {detectedDeviceIndex})");
                        }
                        else if (!isButton && detectedAxis >= 0)
                        {
                            status.DetectedAxis = detectedAxis + 1; // Indeksujemy od 1 dla użytkownika
                            status.DetectedControllerIndex = detectedDeviceIndex;
                            status.DetectedControllerName = deviceName;
                            status.Success = true;
                            status.StatusMessage = $"Wykryto oś {detectedAxis + 1} na urządzeniu {deviceName}";
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] ✓ Wykryto oś {detectedAxis + 1} na urządzeniu {deviceName} (index: {detectedDeviceIndex})");
                        }
                        else
                        {
                            status.StatusMessage = "Nie wykryto przycisku ani osi dla HANDBRAKE";
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] ✗ Nie wykryto przycisku ani osi w czasie oczekiwania ({timeoutSeconds}s)");
                        }
                    }
                    else
                    {
                        status.StatusMessage = "Nie wykryto przycisku ani osi dla HANDBRAKE";
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] ✗ Nie wykryto przycisku ani osi w czasie oczekiwania ({timeoutSeconds}s)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [BINDING] [HANDBRAKE] BŁĄD w DetectHandbrakeFromAllDevices: {ex.Message}");
                foreach (var h in handlers) h.Dispose();
                lock (_detectionsLock)
                {
                    status.IsListening = false;
                    status.IsCompleted = true;
                    status.StatusMessage = $"Błąd: {ex.Message}";
                }
            }
        }
    }
}


