using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using IQPowerContentManager.Api.Models;
using IQPowerContentManager.Api;
using IQPowerContentManager;
using AcTools.Utils;

namespace IQPowerContentManager.Api.Controllers
{
    /// <summary>
    /// Kontroler do zarządzania konfiguracją przed uruchomieniem gry
    /// </summary>
    [RoutePrefix("api/setup")]
    public class SetupController : ApiController
    {
        /// <summary>
        /// Ustawia nick gracza
        /// </summary>
        [HttpPost]
        [Route("nick")]
        public IHttpActionResult SetNick([FromBody] SetNickRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Nick))
                {
                    return Ok(ApiResponse<string>.Error("Nick nie może być pusty"));
                }

                var state = ApplicationStateManager.LoadState();
                state.LastNick = request.Nick.Trim();
                ApplicationStateManager.SaveState(state);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Nick ustawiony: {state.LastNick}");

                return Ok(ApiResponse<string>.Ok($"Nick ustawiony: {state.LastNick}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Błąd ustawiania nicku: {ex.Message}");
                return Ok(ApiResponse<string>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Pobiera aktualny nick
        /// </summary>
        [HttpGet]
        [Route("nick")]
        public IHttpActionResult GetNick()
        {
            try
            {
                var state = ApplicationStateManager.LoadState();
                return Ok(ApiResponse<string>.Ok(state.LastNick ?? ""));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Ustawia typ skrzyni biegów
        /// </summary>
        [HttpPost]
        [Route("gearbox")]
        public IHttpActionResult SetGearbox([FromBody] SetGearboxRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.ShifterType))
                {
                    return Ok(ApiResponse<string>.Error("Typ skrzyni nie może być pusty"));
                }

                var state = ApplicationStateManager.LoadState();
                var shifterType = request.ShifterType.ToLower().Trim();

                // Sprawdź czy typ jest w dostępnych typach (lub użyj domyślnych jeśli nie są skonfigurowane)
                var availableTypes = state.AvailableGearboxTypes != null && state.AvailableGearboxTypes.Count > 0
                    ? state.AvailableGearboxTypes.Select(t => t.Id.ToLower()).ToList()
                    : new List<string> { "sequential", "automatic", "h-pattern" };

                if (!availableTypes.Contains(shifterType))
                {
                    return Ok(ApiResponse<string>.Error($"Nieprawidłowy typ skrzyni. Dozwolone: {string.Join(", ", availableTypes)}"));
                }

                state.LastShifterType = shifterType;
                ApplicationStateManager.SaveState(state);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Typ skrzyni ustawiony: {shifterType}");

                return Ok(ApiResponse<string>.Ok($"Typ skrzyni ustawiony: {shifterType}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Błąd ustawiania typu skrzyni: {ex.Message}");
                return Ok(ApiResponse<string>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Pobiera aktualny typ skrzyni biegów
        /// </summary>
        [HttpGet]
        [Route("gearbox")]
        public IHttpActionResult GetGearbox()
        {
            try
            {
                var state = ApplicationStateManager.LoadState();
                return Ok(ApiResponse<string>.Ok(state.LastShifterType ?? ""));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Pobiera listę dostępnych typów skrzyni biegów
        /// </summary>
        [HttpGet]
        [Route("gearbox-types")]
        public IHttpActionResult GetGearboxTypes()
        {
            try
            {
                var state = ApplicationStateManager.LoadState();

                // Jeśli nie ma skonfigurowanych typów, użyj domyślnych
                if (state.AvailableGearboxTypes == null || state.AvailableGearboxTypes.Count == 0)
                {
                    var defaultTypes = new List<GearboxTypeInfo>
                    {
                        new GearboxTypeInfo { Id = "automatic", Name = "Automatyczna", Description = "Automatyczna skrzynia biegów" },
                        new GearboxTypeInfo { Id = "sequential", Name = "Sekwencyjna", Description = "Sekwencyjna skrzynia biegów" },
                        new GearboxTypeInfo { Id = "h-pattern", Name = "H-Pattern", Description = "Manualna skrzynia biegów H-Pattern" }
                    };

                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Pobrano domyślną listę typów skrzyni: {defaultTypes.Count} dostępnych");
                    return Ok(ApiResponse<List<GearboxTypeInfo>>.Ok(defaultTypes));
                }

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Pobrano skonfigurowaną listę typów skrzyni: {state.AvailableGearboxTypes.Count} dostępnych");
                return Ok(ApiResponse<List<GearboxTypeInfo>>.Ok(state.AvailableGearboxTypes));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Błąd pobierania listy typów skrzyni: {ex.Message}");
                return Ok(ApiResponse<List<GearboxTypeInfo>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Ustawia dostępne typy skrzyni biegów
        /// </summary>
        [HttpPost]
        [Route("gearbox-types")]
        public IHttpActionResult SetGearboxTypes([FromBody] SetGearboxTypesRequest request)
        {
            try
            {
                if (request?.GearboxTypes == null || request.GearboxTypes.Count == 0)
                {
                    return Ok(ApiResponse<string>.Error("Lista typów skrzyni nie może być pusta"));
                }

                // Walidacja - sprawdź czy wszystkie typy mają wymagane pola
                foreach (var gearboxType in request.GearboxTypes)
                {
                    if (string.IsNullOrWhiteSpace(gearboxType.Id))
                    {
                        return Ok(ApiResponse<string>.Error("Wszystkie typy skrzyni muszą mieć ID"));
                    }
                    if (string.IsNullOrWhiteSpace(gearboxType.Name))
                    {
                        return Ok(ApiResponse<string>.Error("Wszystkie typy skrzyni muszą mieć nazwę"));
                    }
                }

                var state = ApplicationStateManager.LoadState();
                state.AvailableGearboxTypes = request.GearboxTypes;
                ApplicationStateManager.SaveState(state);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Ustawiono dostępne typy skrzyni: {state.AvailableGearboxTypes.Count} typów");

                return Ok(ApiResponse<string>.Ok($"Ustawiono {state.AvailableGearboxTypes.Count} dostępnych typów skrzyni"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Błąd ustawiania typów skrzyni: {ex.Message}");
                return Ok(ApiResponse<string>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Ustawia wybrany tor
        /// </summary>
        [HttpPost]
        [Route("track")]
        public IHttpActionResult SetTrack([FromBody] SetTrackRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.TrackId))
                {
                    return Ok(ApiResponse<string>.Error("ID toru nie może być puste"));
                }

                // Sprawdź czy tor jest dostępny
                if (!ContentManager.AvailableTracks.Contains(request.TrackId))
                {
                    return Ok(ApiResponse<string>.Error($"Tor '{request.TrackId}' nie jest dostępny"));
                }

                var state = ApplicationStateManager.LoadState();
                state.LastSelectedTrack = request.TrackId.Trim();

                // Dodaj do listy ostatnio używanych
                if (!state.RecentTracks.Contains(state.LastSelectedTrack))
                {
                    state.RecentTracks.Insert(0, state.LastSelectedTrack);
                    // Ogranicz do 10 ostatnich
                    if (state.RecentTracks.Count > 10)
                    {
                        state.RecentTracks = state.RecentTracks.Take(10).ToList();
                    }
                }
                else
                {
                    // Przenieś na początek listy
                    state.RecentTracks.Remove(state.LastSelectedTrack);
                    state.RecentTracks.Insert(0, state.LastSelectedTrack);
                }

                ApplicationStateManager.SaveState(state);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Tor ustawiony: {state.LastSelectedTrack}");

                return Ok(ApiResponse<string>.Ok($"Tor ustawiony: {state.LastSelectedTrack}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Błąd ustawiania toru: {ex.Message}");
                return Ok(ApiResponse<string>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Pobiera aktualny wybrany tor
        /// </summary>
        [HttpGet]
        [Route("track")]
        public IHttpActionResult GetTrack()
        {
            try
            {
                var state = ApplicationStateManager.LoadState();
                return Ok(ApiResponse<string>.Ok(state.LastSelectedTrack ?? ""));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Pobiera listę dostępnych torów
        /// </summary>
        [HttpGet]
        [Route("tracks")]
        public IHttpActionResult GetAvailableTracks()
        {
            try
            {
                var tracks = ContentManager.AvailableTracks.Select(trackId => new TrackInfo
                {
                    Id = trackId,
                    Name = trackId
                }).ToList();

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Pobrano listę torów: {tracks.Count} dostępnych");

                return Ok(ApiResponse<List<TrackInfo>>.Ok(tracks));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Błąd pobierania listy torów: {ex.Message}");
                return Ok(ApiResponse<List<TrackInfo>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Ustawia wybrany samochód
        /// </summary>
        [HttpPost]
        [Route("car")]
        public IHttpActionResult SetCar([FromBody] SetCarRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.CarId))
                {
                    return Ok(ApiResponse<string>.Error("ID samochodu nie może być puste"));
                }

                // Sprawdź czy samochód jest dostępny
                if (!ContentManager.AvailableCars.Contains(request.CarId))
                {
                    return Ok(ApiResponse<string>.Error($"Samochód '{request.CarId}' nie jest dostępny"));
                }

                var state = ApplicationStateManager.LoadState();
                state.LastSelectedCar = request.CarId.Trim();

                // Dodaj do listy ostatnio używanych
                if (!state.RecentCars.Contains(state.LastSelectedCar))
                {
                    state.RecentCars.Insert(0, state.LastSelectedCar);
                    // Ogranicz do 10 ostatnich
                    if (state.RecentCars.Count > 10)
                    {
                        state.RecentCars = state.RecentCars.Take(10).ToList();
                    }
                }
                else
                {
                    // Przenieś na początek listy
                    state.RecentCars.Remove(state.LastSelectedCar);
                    state.RecentCars.Insert(0, state.LastSelectedCar);
                }

                ApplicationStateManager.SaveState(state);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Samochód ustawiony: {state.LastSelectedCar}");

                return Ok(ApiResponse<string>.Ok($"Samochód ustawiony: {state.LastSelectedCar}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Błąd ustawiania samochodu: {ex.Message}");
                return Ok(ApiResponse<string>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Pobiera aktualny wybrany samochód
        /// </summary>
        [HttpGet]
        [Route("car")]
        public IHttpActionResult GetCar()
        {
            try
            {
                var state = ApplicationStateManager.LoadState();
                return Ok(ApiResponse<string>.Ok(state.LastSelectedCar ?? ""));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Pobiera listę dostępnych samochodów
        /// </summary>
        [HttpGet]
        [Route("cars")]
        public IHttpActionResult GetAvailableCars()
        {
            try
            {
                // Mapowanie ID samochodów na czytelne nazwy
                var carNameMap = new Dictionary<string, string>
                {
                    { "ks_porsche_991_turbo_s", "Porsche 911 Turbo S" },
                    { "cky_porsche992_gt3rs_2023", "Porsche 992 GT3 RS" },
                    { "ks_nissan_gtr", "Nissan GT-R NISMO" }
                };

                var cars = ContentManager.AvailableCars.Select(carId => new VehicleInfo
                {
                    Id = carId,
                    Name = carNameMap.ContainsKey(carId) ? carNameMap[carId] : carId
                }).ToList();

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Pobrano listę samochodów: {cars.Count} dostępnych");

                return Ok(ApiResponse<List<VehicleInfo>>.Ok(cars));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Błąd pobierania listy samochodów: {ex.Message}");
                return Ok(ApiResponse<List<VehicleInfo>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Pobiera podsumowanie konfiguracji
        /// </summary>
        [HttpGet]
        [Route("summary")]
        public IHttpActionResult GetSummary()
        {
            try
            {
                var state = ApplicationStateManager.LoadState();
                var summary = new SetupSummary
                {
                    Nick = state.LastNick ?? "",
                    ShifterType = state.LastShifterType ?? "",
                    TrackId = state.LastSelectedTrack ?? "",
                    CarId = state.LastSelectedCar ?? "",
                    IsComplete = !string.IsNullOrWhiteSpace(state.LastNick) &&
                                !string.IsNullOrWhiteSpace(state.LastShifterType) &&
                                !string.IsNullOrWhiteSpace(state.LastSelectedTrack) &&
                                !string.IsNullOrWhiteSpace(state.LastSelectedCar)
                };

                return Ok(ApiResponse<SetupSummary>.Ok(summary));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<SetupSummary>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Uruchamia grę z aktualną konfiguracją
        /// </summary>
        [HttpPost]
        [Route("launch")]
        public IHttpActionResult LaunchGame()
        {
            try
            {
                var state = ApplicationStateManager.LoadState();

                // Walidacja - sprawdź czy wszystkie wymagane pola są ustawione
                if (string.IsNullOrWhiteSpace(state.LastNick))
                {
                    return Ok(ApiResponse<string>.Error("Nick nie jest ustawiony. Użyj POST /api/setup/nick"));
                }

                if (string.IsNullOrWhiteSpace(state.LastShifterType))
                {
                    return Ok(ApiResponse<string>.Error("Typ skrzyni nie jest ustawiony. Użyj POST /api/setup/gearbox"));
                }

                if (string.IsNullOrWhiteSpace(state.LastSelectedTrack))
                {
                    return Ok(ApiResponse<string>.Error("Tor nie jest wybrany. Użyj POST /api/setup/track"));
                }

                if (string.IsNullOrWhiteSpace(state.LastSelectedCar))
                {
                    return Ok(ApiResponse<string>.Error("Samochód nie jest wybrany. Użyj POST /api/setup/car"));
                }

                // Pobierz ścieżkę do AC root (musi być ustawiona w konfiguracji)
                // TODO: Dodać konfigurację ścieżki AC root
                var acRoot = GetAcRoot();
                if (string.IsNullOrEmpty(acRoot))
                {
                    return Ok(ApiResponse<string>.Error("Ścieżka do Assetto Corsa nie jest skonfigurowana"));
                }

                if (!AcPaths.IsAcRoot(acRoot))
                {
                    return Ok(ApiResponse<string>.Error($"Ścieżka do Assetto Corsa jest nieprawidłowa: {acRoot}"));
                }

                // Konwertuj typ skrzyni na autoShifter (true dla automatic, false dla sequential/h-pattern)
                bool autoShifter = state.LastShifterType == "automatic";

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Uruchamianie gry:");
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP]   Nick: {state.LastNick}");
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP]   Samochód: {state.LastSelectedCar}");
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP]   Tor: {state.LastSelectedTrack}");
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP]   Skrzynia: {state.LastShifterType} (autoShifter: {autoShifter})");
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP]   AC Root: {acRoot}");

                // Uruchom grę
                bool success = GameLauncher.PrepareAndLaunch(
                    acRoot: acRoot,
                    carId: state.LastSelectedCar,
                    trackId: state.LastSelectedTrack,
                    driverName: state.LastNick,
                    skinId: state.LastSkin,
                    trackLayoutId: null,
                    enableCsp: true,
                    autoShifter: autoShifter
                );

                if (success)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] ✓ Gra uruchomiona pomyślnie");
                    return Ok(ApiResponse<string>.Ok("Gra uruchomiona pomyślnie"));
                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] ✗ Nie udało się uruchomić gry");
                    return Ok(ApiResponse<string>.Error("Nie udało się uruchomić gry"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] BŁĄD podczas uruchamiania gry: {ex.Message}");
                return Ok(ApiResponse<string>.Error($"Błąd podczas uruchamiania gry: {ex.Message}"));
            }
        }

        /// <summary>
        /// Pobiera ścieżkę do root folderu Assetto Corsa
        /// Sprawdza typowe lokalizacje Steam i inne możliwe ścieżki
        /// </summary>
        private string GetAcRoot()
        {
            var state = ApplicationStateManager.LoadState();

            // Najpierw sprawdź czy jest zapisana ścieżka w stanie (można dodać pole AcRootPath do ApplicationState)
            // Na razie sprawdzamy typowe lokalizacje

            var possiblePaths = new List<string>();

            // Typowe lokalizacje Steam
            var steamCommonPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Steam", "steamapps", "common", "assettocorsa"
            );
            possiblePaths.Add(steamCommonPath);

            // Alternatywna lokalizacja Steam (64-bit)
            var steamCommonPath64 = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Steam", "steamapps", "common", "assettocorsa"
            );
            possiblePaths.Add(steamCommonPath64);

            // Sprawdź assetofolder w projekcie (może być używany jako AC root w środowisku deweloperskim)
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var assetofolderPaths = new[]
            {
                Path.Combine(baseDir, "..", "..", "..", "assetofolder"), // Debug
                Path.Combine(baseDir, "..", "..", "assetofolder"), // Release
                Path.Combine(baseDir, "assetofolder"), // W tym samym katalogu
                Path.Combine(Directory.GetCurrentDirectory(), "assetofolder"), // Bieżący katalog
                "assetofolder" // Względna ścieżka
            };
            possiblePaths.AddRange(assetofolderPaths);

            foreach (var path in possiblePaths)
            {
                try
                {
                    var fullPath = Path.GetFullPath(path);
                    if (Directory.Exists(fullPath) && AcPaths.IsAcRoot(fullPath))
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Znaleziono AC root: {fullPath}");
                        return fullPath;
                    }
                }
                catch
                {
                    // Ignoruj błędy ścieżek
                }
            }

            // Jeśli nie znaleziono, zwróć null
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SETUP] Nie znaleziono AC root w typowych lokalizacjach");
            return null;
        }
    }

    // Modele requestów
    public class SetNickRequest
    {
        public string Nick { get; set; }
    }

    public class SetGearboxRequest
    {
        public string ShifterType { get; set; } // "sequential", "automatic", "h-pattern"
    }

    public class SetTrackRequest
    {
        public string TrackId { get; set; }
    }

    public class SetCarRequest
    {
        public string CarId { get; set; }
    }

    public class SetupSummary
    {
        public string Nick { get; set; }
        public string ShifterType { get; set; }
        public string TrackId { get; set; }
        public string CarId { get; set; }
        public bool IsComplete { get; set; }
    }

    public class VehicleInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class TrackInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

}

