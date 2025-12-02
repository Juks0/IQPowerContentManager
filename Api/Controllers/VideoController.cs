using System;
using System.IO;
using System.Web.Http;
using IQPowerContentManager.Api.Models;
using IQPowerContentManager;
using AcTools.Utils;

namespace IQPowerContentManager.Api.Controllers
{
    [RoutePrefix("api/video")]
    public class VideoController : ApiController
    {
        private static VideoSettings _videoSettings = new VideoSettings();

        /// <summary>
        /// Ustawia tryb wyświetlania
        /// </summary>
        [HttpPost]
        [Route("display-mode")]
        public IHttpActionResult SetDisplayMode([FromBody] SetDisplayModeRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Mode))
                {
                    return Ok(ApiResponse<string>.Error("Tryb wyświetlania nie może być pusty"));
                }

                var validModes = new[] { "SINGLE_SCREEN", "TRIPLE_SCREEN", "OPENVR", "STEAMVR" };
                var upperMode = request.Mode.ToUpper().Trim();

                // Mapowanie na wartości używane w pliku video.ini
                string cameraMode;
                switch (upperMode)
                {
                    case "SINGLE_SCREEN":
                        cameraMode = "DEFAULT";
                        break;
                    case "TRIPLE_SCREEN":
                        cameraMode = "TRIPLE";
                        break;
                    case "OPENVR":
                        cameraMode = "OPENVR";
                        break;
                    case "STEAMVR":
                        cameraMode = "OPENVR"; // SteamVR używa OPENVR w Assetto Corsa
                        break;
                    default:
                        return Ok(ApiResponse<string>.Error($"Nieprawidłowy tryb wyświetlania. Dozwolone: {string.Join(", ", validModes)}"));
                }

                _videoSettings.SetCameraMode(cameraMode);
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [VIDEO] Tryb wyświetlania ustawiony: {upperMode} (CAMERA.MODE={cameraMode})");
                return Ok(ApiResponse<string>.Ok($"Tryb wyświetlania ustawiony: {GetDisplayModeName(upperMode)}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [VIDEO] Błąd ustawiania trybu wyświetlania: {ex.Message}");
                return Ok(ApiResponse<string>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Pobiera aktualny tryb wyświetlania
        /// </summary>
        [HttpGet]
        [Route("display-mode")]
        public IHttpActionResult GetDisplayMode()
        {
            try
            {
                // Mapowanie z CAMERA.MODE na nazwę API
                string apiMode;
                switch (_videoSettings.CameraMode.ToUpper())
                {
                    case "DEFAULT":
                        apiMode = "SINGLE_SCREEN";
                        break;
                    case "TRIPLE":
                        apiMode = "TRIPLE_SCREEN";
                        break;
                    case "OPENVR":
                        // Nie możemy rozróżnić OpenVR od SteamVR, więc domyślnie zwracamy OPENVR
                        apiMode = "OPENVR";
                        break;
                    default:
                        apiMode = "SINGLE_SCREEN";
                        break;
                }

                return Ok(ApiResponse<DisplayModeInfo>.Ok(new DisplayModeInfo
                {
                    Mode = apiMode,
                    DisplayName = GetDisplayModeName(apiMode)
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [VIDEO] Błąd pobierania trybu wyświetlania: {ex.Message}");
                return Ok(ApiResponse<DisplayModeInfo>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Ustawia rozdzielczość ekranu i refresh rate
        /// </summary>
        [HttpPost]
        [Route("resolution")]
        public IHttpActionResult SetResolution([FromBody] SetResolutionRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Ok(ApiResponse<string>.Error("Request nie może być pusty"));
                }

                if (request.Width <= 0 || request.Height <= 0)
                {
                    return Ok(ApiResponse<string>.Error("Szerokość i wysokość muszą być większe od 0"));
                }

                if (request.RefreshRate <= 0)
                {
                    return Ok(ApiResponse<string>.Error("Refresh rate musi być większy od 0"));
                }

                _videoSettings.SetResolution(request.Width, request.Height, request.RefreshRate, request.Index ?? 0);
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [VIDEO] Rozdzielczość ustawiona: {request.Width}x{request.Height}@{request.RefreshRate}Hz");
                return Ok(ApiResponse<string>.Ok($"Rozdzielczość ustawiona: {request.Width}x{request.Height}@{request.RefreshRate}Hz"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [VIDEO] Błąd ustawiania rozdzielczości: {ex.Message}");
                return Ok(ApiResponse<string>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Pobiera aktualną rozdzielczość ekranu
        /// </summary>
        [HttpGet]
        [Route("resolution")]
        public IHttpActionResult GetResolution()
        {
            try
            {
                return Ok(ApiResponse<ResolutionInfo>.Ok(new ResolutionInfo
                {
                    Width = _videoSettings.Width,
                    Height = _videoSettings.Height,
                    RefreshRate = _videoSettings.Refresh,
                    Index = _videoSettings.Index
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [VIDEO] Błąd pobierania rozdzielczości: {ex.Message}");
                return Ok(ApiResponse<ResolutionInfo>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Zapisuje ustawienia wideo do pliku
        /// </summary>
        [HttpPost]
        [Route("save")]
        public IHttpActionResult SaveVideoSettings()
        {
            try
            {
                // Zapisuj do C:\Users\{current-user}\Documents\asseto-manager
                var documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var managerDir = Path.Combine(documentsDir, "asseto-manager");
                var outputPath = Path.Combine(managerDir, "video.ini");

                if (!Directory.Exists(managerDir))
                {
                    Directory.CreateDirectory(managerDir);
                }

                _videoSettings.SaveToFile(outputPath);
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [VIDEO] Zapisano video.ini do: {outputPath}");
                return Ok(ApiResponse<string>.Ok($"Ustawienia wideo zapisane do: {outputPath}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [VIDEO] Błąd zapisywania video.ini: {ex.Message}");
                return Ok(ApiResponse<string>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Pobiera aktualny stan ustawień wideo
        /// </summary>
        [HttpGet]
        [Route("state")]
        public IHttpActionResult GetVideoState()
        {
            try
            {
                // Mapowanie z CAMERA.MODE na nazwę API
                string apiMode;
                switch (_videoSettings.CameraMode.ToUpper())
                {
                    case "DEFAULT":
                        apiMode = "SINGLE_SCREEN";
                        break;
                    case "TRIPLE":
                        apiMode = "TRIPLE_SCREEN";
                        break;
                    case "OPENVR":
                        apiMode = "OPENVR";
                        break;
                    default:
                        apiMode = "SINGLE_SCREEN";
                        break;
                }

                return Ok(ApiResponse<VideoState>.Ok(new VideoState
                {
                    Width = _videoSettings.Width,
                    Height = _videoSettings.Height,
                    RefreshRate = _videoSettings.Refresh,
                    DisplayMode = apiMode,
                    DisplayModeName = GetDisplayModeName(apiMode),
                    Index = _videoSettings.Index
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [VIDEO] Błąd pobierania stanu wideo: {ex.Message}");
                return Ok(ApiResponse<VideoState>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Wczytuje ustawienia wideo z pliku
        /// </summary>
        [HttpPost]
        [Route("load")]
        public IHttpActionResult LoadVideoSettings()
        {
            try
            {
                // Wczytuj z C:\Users\{current-user}\Documents\asseto-manager, jeśli nie istnieje, to z domyślnego miejsca
                var documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var managerDir = Path.Combine(documentsDir, "asseto-manager");
                var managerPath = Path.Combine(managerDir, "video.ini");
                var defaultPath = AcPaths.GetCfgVideoFilename();
                var inputPath = File.Exists(managerPath) ? managerPath : defaultPath;

                if (!File.Exists(inputPath))
                {
                    return Ok(ApiResponse<string>.Error($"Plik nie istnieje: {inputPath}"));
                }

                if (_videoSettings.LoadFromFile(inputPath))
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [VIDEO] Wczytano video.ini z: {inputPath}");
                    return Ok(ApiResponse<string>.Ok($"Ustawienia wideo wczytane z: {inputPath}"));
                }
                else
                {
                    return Ok(ApiResponse<string>.Error("Nie udało się wczytać ustawień wideo"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [VIDEO] Błąd wczytywania video.ini: {ex.Message}");
                return Ok(ApiResponse<string>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Zwraca czytelną nazwę trybu wyświetlania
        /// </summary>
        private string GetDisplayModeName(string mode)
        {
            return mode.ToUpper() switch
            {
                "SINGLE_SCREEN" => "Single Screen",
                "TRIPLE_SCREEN" => "Triple Screen",
                "OPENVR" => "OpenVR",
                "STEAMVR" => "SteamVR",
                _ => mode
            };
        }
    }

    // Modele request/response
    public class SetDisplayModeRequest
    {
        public string Mode { get; set; } // SINGLE_SCREEN, TRIPLE_SCREEN, OPENVR, STEAMVR
    }

    public class DisplayModeInfo
    {
        public string Mode { get; set; }
        public string DisplayName { get; set; }
    }

    public class SetResolutionRequest
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int RefreshRate { get; set; }
        public int? Index { get; set; }
    }

    public class ResolutionInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int RefreshRate { get; set; }
        public int Index { get; set; }
    }

    public class VideoState
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int RefreshRate { get; set; }
        public string DisplayMode { get; set; }
        public string DisplayModeName { get; set; }
        public int Index { get; set; }
    }
}


