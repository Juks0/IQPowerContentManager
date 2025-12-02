using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using AcTools.Utils;

namespace IQPowerContentManager
{
    public class GameLauncher
    {
        /// <summary>
        /// Zwraca konfigurację sekcji [RACE] dla danej kombinacji samochodu i toru
        /// </summary>
        private static (string track, string configTrack, string model, string skin) GetRaceConfig(string carId, string trackId)
        {
            // Mapowanie kombinacji samochód+tor na konfigurację sekcji [RACE]
            var raceConfigMap = new Dictionary<string, Dictionary<string, (string track, string configTrack, string model, string skin)>>
            {
                ["cky_porsche992_gt3rs_2023"] = new Dictionary<string, (string, string, string, string)>
                {
                    ["ks_nordschleife"] = ("ks_nordschleife", "nordschleife", "cky_porsche992_gt3rs_2023", ""),
                    ["ks_nurburgring"] = ("ks_nurburgring", "layout_gp_a", "cky_porsche992_gt3rs_2023", "01_white'n'black")
                },
                ["ks_porsche_991_turbo_s"] = new Dictionary<string, (string, string, string, string)>
                {
                    ["ks_nordschleife"] = ("ks_nordschleife", "nordschleife", "ks_porsche_991_turbo_s", "00_miami_blue"),
                    ["ks_nurburgring"] = ("ks_nurburgring", "layout_gp_a", "ks_porsche_991_turbo_s", "00_miami_blue")
                },
                ["ks_nissan_gtr"] = new Dictionary<string, (string, string, string, string)>
                {
                    ["ks_nordschleife"] = ("ks_nordschleife", "nordschleife", "ks_nissan_gtr", "0_pearl_white"),
                    ["ks_nurburgring"] = ("ks_nurburgring", "layout_gp_a", "ks_nissan_gtr", "0_pearl_white")
                }
            };

            if (raceConfigMap.ContainsKey(carId) && raceConfigMap[carId].ContainsKey(trackId))
            {
                return raceConfigMap[carId][trackId];
            }

            // Domyślna konfiguracja jeśli nie ma w mapie
            string defaultConfigTrack = trackId == "ks_nordschleife" ? "nordschleife" : "";
            return (trackId, defaultConfigTrack, carId, "");
        }

        /// <summary>
        /// Zapisuje plik race.ini z ustawieniami samochodu i toru
        /// </summary>
        public static bool CreateRaceIni(string carId, string trackId, string driverName, string skinId = null, string trackLayoutId = null)
        {
            try
            {
                var raceIniPath = AcPaths.GetRaceIniFilename();
                var cfgDir = Path.GetDirectoryName(raceIniPath);

                // Utwórz katalog jeśli nie istnieje
                if (!Directory.Exists(cfgDir))
                {
                    Directory.CreateDirectory(cfgDir);
                }

                var sb = new StringBuilder();

                // Pobierz konfigurację sekcji [RACE] dla tej kombinacji
                var raceConfig = GetRaceConfig(carId, trackId);

                // Użyj skórki z parametru jeśli podano, w przeciwnym razie użyj z konfiguracji
                string finalSkin = !string.IsNullOrEmpty(skinId) ? skinId : raceConfig.skin;

                // Użyj trackLayoutId z parametru jeśli podano, w przeciwnym razie użyj z konfiguracji
                string finalConfigTrack = !string.IsNullOrEmpty(trackLayoutId) ? trackLayoutId : raceConfig.configTrack;

                // HEADER
                sb.AppendLine("[HEADER]");
                sb.AppendLine("VERSION=2");
                sb.AppendLine("__CM_FEATURE_SET=2");
                sb.AppendLine();

                // RACE - użyj konfiguracji z mapy
                sb.AppendLine("[RACE]");
                sb.AppendLine($"TRACK={raceConfig.track}");
                sb.AppendLine($"CONFIG_TRACK={finalConfigTrack}");
                sb.AppendLine($"MODEL={raceConfig.model}");
                sb.AppendLine("MODEL_CONFIG=");
                if (!string.IsNullOrEmpty(finalSkin))
                {
                    sb.AppendLine($"SKIN={finalSkin}");
                }
                else
                {
                    sb.AppendLine("SKIN=");
                }
                sb.AppendLine("CARS=1");  // TYLKO GRACZ - bez samochodów AI/traffic
                sb.AppendLine("AI_LEVEL=98");  // Opcjonalne (nie używane gdy CARS = 1)
                sb.AppendLine("FIXED_SETUP=0");
                sb.AppendLine("PENALTIES=1");
                sb.AppendLine("DRIFT_MODE=0");
                sb.AppendLine("JUMP_START_PENALTY=0");
                sb.AppendLine();

                // CAR_0
                sb.AppendLine("[CAR_0]");
                sb.AppendLine("SETUP=");
                if (!string.IsNullOrEmpty(finalSkin))
                {
                    sb.AppendLine($"SKIN={finalSkin}");
                }
                else
                {
                    sb.AppendLine("SKIN=");
                }
                sb.AppendLine("MODEL=-");
                sb.AppendLine("MODEL_CONFIG=");
                sb.AppendLine("BALLAST=0");
                sb.AppendLine("RESTRICTOR=0");
                sb.AppendLine($"DRIVER_NAME={driverName}");  // Zawsze używaj nicku z inputu użytkownika
                sb.AppendLine("NATIONALITY=Planet Earth");
                sb.AppendLine("NATION_CODE=PLA");
                sb.AppendLine();

                // REMOTE
                sb.AppendLine("[REMOTE]");
                sb.AppendLine("ACTIVE=0");
                sb.AppendLine("SERVER_IP=");
                sb.AppendLine("SERVER_PORT=");
                sb.AppendLine("NAME=");
                sb.AppendLine("TEAM=");
                sb.AppendLine("GUID=");
                sb.AppendLine("REQUESTED_CAR=");
                sb.AppendLine("PASSWORD=");
                sb.AppendLine();

                // GHOST_CAR
                sb.AppendLine("[GHOST_CAR]");
                sb.AppendLine("RECORDING=1");
                sb.AppendLine("PLAYING=1");
                sb.AppendLine("SECONDS_ADVANTAGE=0");
                sb.AppendLine("LOAD=1");
                sb.AppendLine("FILE=");
                sb.AppendLine("ENABLED=0");
                sb.AppendLine();

                // REPLAY
                sb.AppendLine("[REPLAY]");
                sb.AppendLine("ACTIVE=0");
                sb.AppendLine("FILENAME=");
                sb.AppendLine();

                // LIGHTING
                sb.AppendLine("[LIGHTING]");
                sb.AppendLine("SUN_ANGLE=16.00");
                sb.AppendLine("TIME_MULT=0.0");
                sb.AppendLine("CLOUD_SPEED=0.200");
                sb.AppendLine("__CM_WEATHER_TYPE=-1");
                sb.AppendLine("__CM_WEATHER_CONTROLLER=base");
                sb.AppendLine("__TRACK_GEOTAG_LAT=50.3356");
                sb.AppendLine("__TRACK_GEOTAG_LONG=6.9475");
                sb.AppendLine("__TRACK_TIMEZONE_BASE_OFFSET=3600");
                sb.AppendLine("__TRACK_TIMEZONE_OFFSET=3600");
                sb.AppendLine("__TRACK_TIMEZONE_DTS=0");
                sb.AppendLine();

                // TEMPERATURE
                sb.AppendLine("[TEMPERATURE]");
                sb.AppendLine("AMBIENT=12");
                sb.AppendLine("ROAD=11");
                sb.AppendLine();

                // WEATHER
                sb.AppendLine("[WEATHER]");
                sb.AppendLine("NAME=sol_24_smoke");
                sb.AppendLine();

                // DYNAMIC_TRACK
                sb.AppendLine("[DYNAMIC_TRACK]");
                sb.AppendLine("SESSION_START=200");
                sb.AppendLine("RANDOMNESS=200");
                sb.AppendLine("LAP_GAIN=132");
                sb.AppendLine("SESSION_TRANSFER=200");
                sb.AppendLine();

                // GROOVE
                sb.AppendLine("[GROOVE]");
                sb.AppendLine("VIRTUAL_LAPS=10");
                sb.AppendLine("MAX_LAPS=1");
                sb.AppendLine("STARTING_LAPS=1");
                sb.AppendLine();

                // SESSION_0 - MUSI być na końcu przed AUTOSPAWN
                sb.AppendLine("[SESSION_0]");
                sb.AppendLine("NAME=Practice");
                sb.AppendLine("TYPE=1");  // 1 = Practice (działa z AUTOSPAWN), 2 = Qualify, 3 = Race, 4 = Hotlap
                sb.AppendLine("DURATION_MINUTES=0");
                sb.AppendLine("SPAWN_SET=START");  // START dla AUTOSPAWN (PIT może nie działać)
                sb.AppendLine();

                // LAP_INVALIDATOR
                sb.AppendLine("[LAP_INVALIDATOR]");
                sb.AppendLine("ALLOWED_TYRES_OUT=-1");
                sb.AppendLine();

                // AUTOSPAWN - pomija menu główne i od razu wchodzi do wyścigu
                sb.AppendLine("[AUTOSPAWN]");
                sb.AppendLine("ACTIVE=1");
                sb.AppendLine();

                // BENCHMARK
                sb.AppendLine("[BENCHMARK]");
                sb.AppendLine("ACTIVE=0");
                sb.AppendLine();

                // RESTART
                sb.AppendLine("[RESTART]");
                sb.AppendLine("ACTIVE=0");
                sb.AppendLine();

                // __PREVIEW_GENERATION
                sb.AppendLine("[__PREVIEW_GENERATION]");
                sb.AppendLine("ACTIVE=0");
                sb.AppendLine();

                // OPTIONS
                sb.AppendLine("[OPTIONS]");
                sb.AppendLine("USE_MPH=0");
                sb.AppendLine();

                // WIND
                sb.AppendLine("[WIND]");
                sb.AppendLine("SPEED_KMH_MIN=10");
                sb.AppendLine("SPEED_KMH_MAX=10");
                sb.AppendLine("DIRECTION_DEG=0");

                var content = sb.ToString();

                // Zapisz plik z kodowaniem ASCII (Windows-1252) - wymagane przez Assetto Corsa
                // Użyj Encoding.Default (Windows-1252) zamiast UTF-8
                File.WriteAllText(raceIniPath, content, Encoding.Default);

                // Wyświetl zawartość pliku
                Console.WriteLine("\n=== ZAWARTOŚĆ PLIKU race.ini ===");
                Console.WriteLine(content);
                Console.WriteLine("================================\n");
                Console.WriteLine("✓ CARS = 1 (tylko gracz, bez samochodów AI/traffic)");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas tworzenia race.ini: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Zapisuje plik assists.ini z ustawieniami asystentów
        /// </summary>
        /// <param name="autoShifter">true = automatyczna skrzynia biegów, false = manualna</param>
        public static bool CreateAssistsIni(bool autoShifter = false)
        {
            try
            {
                var assistsIniPath = AcPaths.GetAssistsIniFilename();
                var cfgDir = Path.GetDirectoryName(assistsIniPath);

                // Utwórz katalog jeśli nie istnieje
                if (!Directory.Exists(cfgDir))
                {
                    Directory.CreateDirectory(cfgDir);
                }

                var sb = new StringBuilder();
                sb.AppendLine("[ASSISTS]");
                sb.AppendLine("IDEAL_LINE=0");
                sb.AppendLine("AUTO_BLIP=0");
                sb.AppendLine("STABILITY_CONTROL=0");
                sb.AppendLine("AUTO_BRAKE=0");
                sb.AppendLine($"AUTO_SHIFTER={(autoShifter ? 1 : 0)}");  // 0 = manualna, 1 = automatyczna
                sb.AppendLine("ABS=1");
                sb.AppendLine("TRACTION_CONTROL=1");
                sb.AppendLine("AUTO_CLUTCH=0");
                sb.AppendLine("VISUALDAMAGE=1");
                sb.AppendLine("DAMAGE=100");
                sb.AppendLine("FUEL_RATE=1");
                sb.AppendLine("TYRE_WEAR=1");
                sb.AppendLine("TYRE_BLANKETS=0");
                sb.AppendLine("SLIPSTREAM=1");

                File.WriteAllText(assistsIniPath, sb.ToString(), Encoding.Default);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas tworzenia assists.ini: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Uruchamia grę Assetto Corsa
        /// </summary>
        public static bool LaunchGame(string acRoot)
        {
            try
            {
                // Sprawdź czy katalog AC istnieje
                if (!AcPaths.IsAcRoot(acRoot))
                {
                    Console.WriteLine($"Błąd: Katalog Assetto Corsa jest nieprawidłowy: {acRoot}");
                    return false;
                }

                // WAŻNE: AUTOSPAWN działa tylko z bezpośrednim uruchomieniem acs.exe
                // AssettoCorsa.exe (launcher) może nie obsługiwać AUTOSPAWN
                // Spróbuj najpierw acs.exe (dla AUTOSPAWN)
                var acsPath = Path.Combine(acRoot, "acs.exe");
                if (File.Exists(acsPath))
                {
                    Console.WriteLine("Uruchamianie acs.exe (wymagane dla AUTOSPAWN)...");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = acsPath,
                        WorkingDirectory = acRoot
                    });
                    return true;
                }

                // Spróbuj acs_pro.exe
                var acsProPath = Path.Combine(acRoot, "acs_pro.exe");
                if (File.Exists(acsProPath))
                {
                    Console.WriteLine("Uruchamianie acs_pro.exe (wymagane dla AUTOSPAWN)...");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = acsProPath,
                        WorkingDirectory = acRoot
                    });
                    return true;
                }

                // Jeśli nie ma acs.exe, spróbuj AssettoCorsa.exe (launcher) - ale AUTOSPAWN może nie działać
                var launcherPath = Path.Combine(acRoot, "AssettoCorsa.exe");
                if (File.Exists(launcherPath))
                {
                    Console.WriteLine("Uwaga: Uruchamianie AssettoCorsa.exe (launcher) - AUTOSPAWN może nie działać!");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = launcherPath,
                        WorkingDirectory = acRoot
                    });
                    return true;
                }

                Console.WriteLine("Błąd: Nie znaleziono pliku wykonywalnego gry!");
                Console.WriteLine($"Szukano w: {acRoot}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas uruchamiania gry: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sprawdza czy CSP jest zainstalowany w Assetto Corsa
        /// </summary>
        public static bool IsCspInstalled(string acRoot)
        {
            // Sprawdź czy istnieje dwrite.dll (główny plik CSP)
            var dwritePath = Path.Combine(acRoot, "dwrite.dll");
            if (File.Exists(dwritePath))
            {
                return true;
            }

            // Sprawdź też czy istnieje folder extension/config
            var extensionConfigDir = Path.Combine(acRoot, "extension", "config");
            return Directory.Exists(extensionConfigDir);
        }

        /// <summary>
        /// Sprawdza czy CSP jest aktywny (ENABLED = 1)
        /// </summary>
        public static bool IsCspEnabled(string acRoot)
        {
            try
            {
                var generalIniPath = Path.Combine(acRoot, "extension", "config", "general.ini");
                if (!File.Exists(generalIniPath))
                {
                    return false;
                }

                var lines = File.ReadAllLines(generalIniPath);
                bool inBasicSection = false;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        inBasicSection = trimmed.Equals("[BASIC]", StringComparison.OrdinalIgnoreCase);
                        continue;
                    }

                    if (inBasicSection && trimmed.StartsWith("ENABLED", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = trimmed.Split('=');
                        if (parts.Length >= 2)
                        {
                            var value = parts[1].Trim();
                            return value == "1";
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Włącza CSP (ustawia ENABLED = 1 w general.ini)
        /// </summary>
        public static bool EnableCsp(string acRoot)
        {
            try
            {
                var generalIniPath = Path.Combine(acRoot, "extension", "config", "general.ini");
                if (!File.Exists(generalIniPath))
                {
                    Console.WriteLine("⚠ CSP nie jest zainstalowany - nie można włączyć");
                    return false;
                }

                var lines = File.ReadAllLines(generalIniPath);
                var newLines = new List<string>();
                bool inBasicSection = false;
                bool enabledFound = false;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        inBasicSection = trimmed.Equals("[BASIC]", StringComparison.OrdinalIgnoreCase);
                        newLines.Add(line);
                        continue;
                    }

                    if (inBasicSection && trimmed.StartsWith("ENABLED", StringComparison.OrdinalIgnoreCase))
                    {
                        newLines.Add("ENABLED = 1");
                        enabledFound = true;
                    }
                    else
                    {
                        newLines.Add(line);
                    }
                }

                // Jeśli nie znaleziono sekcji BASIC lub ENABLED, dodaj je
                if (!inBasicSection || !enabledFound)
                {
                    // Znajdź miejsce na końcu pliku lub dodaj nową sekcję
                    if (!newLines.Any(l => l.Trim().Equals("[BASIC]", StringComparison.OrdinalIgnoreCase)))
                    {
                        newLines.Add("");
                        newLines.Add("[BASIC]");
                    }
                    if (!enabledFound)
                    {
                        newLines.Add("ENABLED = 1");
                    }
                }

                File.WriteAllLines(generalIniPath, newLines);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas włączania CSP: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kopiuje CSP z projektu do Assetto Corsa (jeśli istnieje w projekcie)
        /// </summary>
        public static bool CopyCspFromProject(string acRoot, string projectContentPath)
        {
            try
            {
                var projectCspPath = Path.Combine(projectContentPath, "csp");
                if (!Directory.Exists(projectCspPath))
                {
                    // CSP nie istnieje w projekcie - to nie jest błąd
                    return true;
                }

                // Sprawdź czy istnieje dwrite.dll i extension/
                var projectDwrite = Path.Combine(projectCspPath, "dwrite.dll");
                var projectExtension = Path.Combine(projectCspPath, "extension");

                if (!File.Exists(projectDwrite) || !Directory.Exists(projectExtension))
                {
                    Console.WriteLine("⚠ CSP w projekcie jest niekompletny (brak dwrite.dll lub extension/)");
                    return true; // Nie jest to błąd krytyczny
                }

                // Skopiuj dwrite.dll
                var acDwrite = Path.Combine(acRoot, "dwrite.dll");
                if (File.Exists(acDwrite))
                {
                    Console.WriteLine("Uwaga: dwrite.dll już istnieje w AC");
                    Console.Write("Czy chcesz go nadpisać? (T/N): ");
                    var response = Console.ReadLine();
                    if (response?.ToUpper() != "T")
                    {
                        Console.WriteLine("Pomijam kopiowanie dwrite.dll");
                    }
                    else
                    {
                        File.Copy(projectDwrite, acDwrite, true);
                        Console.WriteLine("✓ dwrite.dll skopiowany");
                    }
                }
                else
                {
                    File.Copy(projectDwrite, acDwrite, true);
                    Console.WriteLine("✓ dwrite.dll skopiowany");
                }

                // Skopiuj extension/
                var acExtension = Path.Combine(acRoot, "extension");
                if (Directory.Exists(acExtension))
                {
                    Console.WriteLine("Uwaga: folder extension/ już istnieje w AC");
                    Console.Write("Czy chcesz go nadpisać? (T/N): ");
                    var response = Console.ReadLine();
                    if (response?.ToUpper() != "T")
                    {
                        Console.WriteLine("Pomijam kopiowanie extension/");
                    }
                    else
                    {
                        CopyDirectory(projectExtension, acExtension);
                        Console.WriteLine("✓ extension/ skopiowany");
                    }
                }
                else
                {
                    CopyDirectory(projectExtension, acExtension);
                    Console.WriteLine("✓ extension/ skopiowany");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas kopiowania CSP: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Rekurencyjnie kopiuje katalog
        /// </summary>
        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
                CopyDirectory(dir, destSubDir);
            }
        }

        /// <summary>
        /// Przygotowuje pliki konfiguracyjne i uruchamia grę
        /// </summary>
        public static bool PrepareAndLaunch(string acRoot, string carId, string trackId, string driverName, string skinId = null, string trackLayoutId = null, bool enableCsp = true, bool autoShifter = false)
        {
            Console.WriteLine("\n=== PRZYGOTOWYWANIE PLIKÓW KONFIGURACYJNYCH ===");

            // Sprawdź czy CSP jest zainstalowany (dwrite.dll istnieje w AC)
            if (IsCspInstalled(acRoot))
            {
                Console.WriteLine("CSP wykryty w Assetto Corsa");

                // Włącz CSP jeśli jest zainstalowany
                if (enableCsp)
                {
                    Console.WriteLine("Włączanie CSP...");
                    if (EnableCsp(acRoot))
                    {
                        Console.WriteLine("✓ CSP włączony");
                    }
                    else
                    {
                        Console.WriteLine("⚠ Nie udało się włączyć CSP");
                    }
                }
            }
            else
            {
                Console.WriteLine("CSP nie jest zainstalowany w Assetto Corsa (opcjonalne)");
            }

            // Utwórz race.ini
            Console.WriteLine("Tworzenie race.ini...");
            if (!CreateRaceIni(carId, trackId, driverName, skinId, trackLayoutId))
            {
                return false;
            }
            Console.WriteLine("✓ race.ini utworzony");

            // Utwórz assists.ini
            Console.WriteLine("Tworzenie assists.ini...");
            if (!CreateAssistsIni(autoShifter))
            {
                return false;
            }
            Console.WriteLine($"✓ assists.ini utworzony (AUTO_SHIFTER={(autoShifter ? 1 : 0)} - {(autoShifter ? "automatyczna" : "manualna")})");

            // Zapisz controls.ini do C:\Users\{current-user}\Documents\asseto-manager przed uruchomieniem gry
            Console.WriteLine("Zapisywanie controls.ini do Documents\\asseto-manager...");
            try
            {
                IQPowerContentManager.Api.StateHelper.SaveControlsToIniFile();
                Console.WriteLine("✓ controls.ini zapisany do Documents\\asseto-manager");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Błąd podczas zapisywania controls.ini: {ex.Message}");
                // Nie przerywamy uruchamiania gry - kontynuujemy mimo błędu
            }

            // Skopiuj controls.ini z Documents\asseto-manager do właściwego miejsca dla gry
            Console.WriteLine("Kopiowanie controls.ini do folderu gry...");
            try
            {
                var documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var managerDir = Path.Combine(documentsDir, "asseto-manager");
                var managerControlsPath = Path.Combine(managerDir, "controls.ini");
                var gameControlsPath = AcPaths.GetCfgControlsFilename();
                var gameCfgDir = Path.GetDirectoryName(gameControlsPath);

                if (!Directory.Exists(gameCfgDir))
                {
                    Directory.CreateDirectory(gameCfgDir);
                }

                if (File.Exists(managerControlsPath))
                {
                    File.Copy(managerControlsPath, gameControlsPath, overwrite: true);
                    Console.WriteLine($"✓ controls.ini skopiowany z {managerControlsPath} do: {gameControlsPath}");
                }
                else
                {
                    Console.WriteLine($"⚠ Plik controls.ini nie istnieje w Documents\\asseto-manager - gra użyje domyślnych ustawień");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Błąd podczas kopiowania controls.ini: {ex.Message}");
                // Nie przerywamy uruchamiania gry - kontynuujemy mimo błędu
            }

            // Skopiuj video.ini z Documents\asseto-manager do właściwego miejsca dla gry
            Console.WriteLine("Kopiowanie video.ini do folderu gry...");
            try
            {
                var documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var managerDir = Path.Combine(documentsDir, "asseto-manager");
                var managerVideoPath = Path.Combine(managerDir, "video.ini");
                var gameVideoPath = AcPaths.GetCfgVideoFilename();
                var gameCfgDir = Path.GetDirectoryName(gameVideoPath);

                if (!Directory.Exists(gameCfgDir))
                {
                    Directory.CreateDirectory(gameCfgDir);
                }

                if (File.Exists(managerVideoPath))
                {
                    File.Copy(managerVideoPath, gameVideoPath, overwrite: true);
                    Console.WriteLine($"✓ video.ini skopiowany z {managerVideoPath} do: {gameVideoPath}");
                }
                else
                {
                    Console.WriteLine($"⚠ Plik video.ini nie istnieje w Documents\\asseto-manager - gra użyje domyślnych ustawień");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Błąd podczas kopiowania video.ini: {ex.Message}");
                // Nie przerywamy uruchamiania gry - kontynuujemy mimo błędu
            }

            // Uruchom grę
            Console.WriteLine("\n=== URUCHAMIANIE GRY ===");
            if (!LaunchGame(acRoot))
            {
                return false;
            }

            Console.WriteLine("✓ Gra uruchomiona!");
            return true;
        }
    }
}

