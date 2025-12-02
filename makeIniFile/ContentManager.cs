using System;
using System.IO;
using AcTools.Utils;

namespace IQPowerContentManager
{
    public class ContentManager
    {
        private static string GetProjectContentPath()
        {
            // Spróbuj różne ścieżki w zależności od tego czy jesteśmy w debug czy release
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "..", "..", "..", "content"), // Debug (bin/Debug/net48/)
                Path.Combine(baseDir, "..", "..", "content"), // Release (bin/Release/net48/)
                Path.Combine(baseDir, "content"), // W tym samym katalogu co exe
                Path.Combine(Directory.GetCurrentDirectory(), "content"), // Bieżący katalog roboczy
                "content" // Względna ścieżka
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (Directory.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            // Jeśli nie znaleziono, zwróć domyślną ścieżkę (dla debug)
            return Path.Combine(baseDir, "..", "..", "..", "content");
        }

        private static readonly string PROJECT_CONTENT_PATH = GetProjectContentPath();

        // Dostępne samochody
        public static readonly string[] AvailableCars = new[]
        {
            "ks_porsche_991_turbo_s",      // Porsche 911 Turbo S
            "cky_porsche992_gt3rs_2023",   // Porsche 992 GT3 RS
            "ks_nissan_gtr",                // Nissan GT-R NISMO
        };

        // Dostępne tory
        public static readonly string[] AvailableTracks = new[]
        {
            "ks_nordschleife",
            "ks_nurburgring"
        };

        /// <summary>
        /// Kopiuje samochód z projektu do folderu Assetto Corsa
        /// </summary>
        public static bool CopyCarToAc(string acRoot, string carName, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                // Sprawdź czy katalog AC istnieje
                if (!AcPaths.IsAcRoot(acRoot))
                {
                    errorMessage = $"Katalog Assetto Corsa jest nieprawidłowy: {acRoot}";
                    return false;
                }

                // Ścieżka do samochodu w projekcie
                var projectCarPath = Path.Combine(PROJECT_CONTENT_PATH, "cars", carName);
                if (!Directory.Exists(projectCarPath))
                {
                    errorMessage = $"Samochód nie został znaleziony w projekcie: {carName}\n" +
                                   $"Szukano w: {projectCarPath}\n" +
                                   $"Folder content projektu: {PROJECT_CONTENT_PATH}";
                    return false;
                }

                // Ścieżka docelowa w AC
                var acCarPath = AcPaths.GetCarDirectory(acRoot, carName);

                // Jeśli folder już istnieje, usuń go (opcjonalnie - można dodać potwierdzenie)
                if (Directory.Exists(acCarPath))
                {
                    Console.WriteLine($"Uwaga: Folder samochodu już istnieje: {acCarPath}");
                    Console.Write("Czy chcesz go nadpisać? (T/N): ");
                    var response = Console.ReadLine();
                    if (response?.ToUpper() != "T")
                    {
                        errorMessage = "Operacja anulowana przez użytkownika";
                        return false;
                    }

                    // Usuń istniejący folder
                    Directory.Delete(acCarPath, true);
                }

                // Skopiuj cały folder samochodu
                CopyDirectory(projectCarPath, acCarPath);

                Console.WriteLine($"✓ Samochód '{carName}' został skopiowany do: {acCarPath}");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Błąd podczas kopiowania samochodu: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Kopiuje tor z projektu do folderu Assetto Corsa
        /// </summary>
        public static bool CopyTrackToAc(string acRoot, string trackName, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                // Sprawdź czy katalog AC istnieje
                if (!AcPaths.IsAcRoot(acRoot))
                {
                    errorMessage = $"Katalog Assetto Corsa jest nieprawidłowy: {acRoot}";
                    return false;
                }

                // Ścieżka do toru w projekcie
                var projectTrackPath = Path.Combine(PROJECT_CONTENT_PATH, "tracks", trackName);
                if (!Directory.Exists(projectTrackPath))
                {
                    errorMessage = $"Tor nie został znaleziony w projekcie: {trackName}\n" +
                                   $"Szukano w: {projectTrackPath}\n" +
                                   $"Folder content projektu: {PROJECT_CONTENT_PATH}";
                    return false;
                }

                // Ścieżka docelowa w AC
                var acTrackPath = Path.Combine(AcPaths.GetTracksDirectory(acRoot), trackName);

                // Jeśli folder już istnieje, usuń go (opcjonalnie - można dodać potwierdzenie)
                if (Directory.Exists(acTrackPath))
                {
                    Console.WriteLine($"Uwaga: Folder toru już istnieje: {acTrackPath}");
                    Console.Write("Czy chcesz go nadpisać? (T/N): ");
                    var response = Console.ReadLine();
                    if (response?.ToUpper() != "T")
                    {
                        errorMessage = "Operacja anulowana przez użytkownika";
                        return false;
                    }

                    // Usuń istniejący folder
                    Directory.Delete(acTrackPath, true);
                }

                // Skopiuj cały folder toru
                CopyDirectory(projectTrackPath, acTrackPath);

                Console.WriteLine($"✓ Tor '{trackName}' został skopiowany do: {acTrackPath}");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Błąd podczas kopiowania toru: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Rekurencyjnie kopiuje katalog z wszystkimi plikami i podkatalogami
        /// </summary>
        private static void CopyDirectory(string sourceDir, string destDir)
        {
            // Utwórz katalog docelowy jeśli nie istnieje
            Directory.CreateDirectory(destDir);

            // Skopiuj wszystkie pliki
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            // Rekurencyjnie skopiuj wszystkie podkatalogi
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
                CopyDirectory(dir, destSubDir);
            }
        }

        /// <summary>
        /// Pobiera ścieżkę do folderu content projektu (do wyświetlenia w komunikatach)
        /// </summary>
        public static string GetContentPath()
        {
            return PROJECT_CONTENT_PATH;
        }

        /// <summary>
        /// Pobiera ścieżkę do folderu assetofolder w projekcie
        /// </summary>
        private static string GetAssetofolderPath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "..", "..", "..", "assetofolder"), // Debug (bin/Debug/net48/)
                Path.Combine(baseDir, "..", "..", "assetofolder"), // Release (bin/Release/net48/)
                Path.Combine(baseDir, "assetofolder"), // W tym samym katalogu co exe
                Path.Combine(Directory.GetCurrentDirectory(), "assetofolder"), // Bieżący katalog roboczy
                "assetofolder" // Względna ścieżka
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (Directory.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            // Jeśli nie znaleziono, zwróć domyślną ścieżkę (dla debug)
            return Path.Combine(baseDir, "..", "..", "..", "assetofolder");
        }

        /// <summary>
        /// Kopiuje całą zawartość folderu assetofolder do root folderu Assetto Corsa (z interakcją konsoli)
        /// </summary>
        /// <param name="acRoot">Ścieżka do root folderu Assetto Corsa</param>
        /// <param name="errorMessage">Komunikat błędu (jeśli wystąpi)</param>
        /// <returns>True jeśli operacja zakończyła się sukcesem</returns>
        public static bool CopyAssetofolderToAc(string acRoot, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                // Sprawdź czy katalog AC istnieje
                if (!AcPaths.IsAcRoot(acRoot))
                {
                    errorMessage = $"Katalog Assetto Corsa jest nieprawidłowy: {acRoot}";
                    return false;
                }

                // Ścieżka do folderu assetofolder w projekcie
                var assetofolderPath = GetAssetofolderPath();
                if (!Directory.Exists(assetofolderPath))
                {
                    errorMessage = $"Folder assetofolder nie został znaleziony w projekcie.\n" +
                                   $"Szukano w: {assetofolderPath}";
                    return false;
                }

                Console.WriteLine($"\n=== KOPIOWANIE ZAWARTOŚCI ASSETOFOLDER ===");
                Console.WriteLine($"Źródło: {assetofolderPath}");
                Console.WriteLine($"Cel: {acRoot}");
                Console.WriteLine("\nUwaga: Ta operacja skopiuje wszystkie pliki i foldery z assetofolder do root folderu AC.");
                Console.WriteLine("Jeśli pliki już istnieją, zostaną nadpisane.");
                Console.Write("Czy chcesz kontynuować? (T/N): ");
                var response = Console.ReadLine();
                if (response?.ToUpper() != "T")
                {
                    errorMessage = "Operacja anulowana przez użytkownika";
                    return false;
                }

                // Skopiuj całą zawartość folderu assetofolder do root folderu AC
                Console.WriteLine("\nKopiowanie plików...");
                CopyDirectoryContents(assetofolderPath, acRoot, true);

                Console.WriteLine($"\n✓ Cała zawartość assetofolder została skopiowana do: {acRoot}");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Błąd podczas kopiowania zawartości assetofolder: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Kopiuje całą zawartość folderu assetofolder do root folderu Assetto Corsa (bez interakcji, dla API)
        /// </summary>
        /// <param name="acRoot">Ścieżka do root folderu Assetto Corsa</param>
        /// <param name="errorMessage">Komunikat błędu (jeśli wystąpi)</param>
        /// <returns>True jeśli operacja zakończyła się sukcesem</returns>
        public static bool CopyAssetofolderToAcSilent(string acRoot, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                // Sprawdź czy katalog AC istnieje
                if (!AcPaths.IsAcRoot(acRoot))
                {
                    errorMessage = $"Katalog Assetto Corsa jest nieprawidłowy: {acRoot}";
                    return false;
                }

                // Ścieżka do folderu assetofolder w projekcie
                var assetofolderPath = GetAssetofolderPath();
                if (!Directory.Exists(assetofolderPath))
                {
                    errorMessage = $"Folder assetofolder nie został znaleziony w projekcie.\n" +
                                   $"Szukano w: {assetofolderPath}";
                    return false;
                }

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [CONTENT] Kopiowanie zawartości assetofolder do AC...");
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [CONTENT] Źródło: {assetofolderPath}");
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [CONTENT] Cel: {acRoot}");

                // Skopiuj całą zawartość folderu assetofolder do root folderu AC
                CopyDirectoryContents(assetofolderPath, acRoot, true);

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [CONTENT] ✓ Cała zawartość assetofolder została skopiowana do: {acRoot}");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Błąd podczas kopiowania zawartości assetofolder: {ex.Message}";
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [CONTENT] ✗ Błąd: {errorMessage}");
                return false;
            }
        }

        /// <summary>
        /// Kopiuje zawartość katalogu źródłowego do katalogu docelowego (merge, nie nadpisuje całego katalogu)
        /// </summary>
        /// <param name="sourceDir">Katalog źródłowy</param>
        /// <param name="destDir">Katalog docelowy</param>
        /// <param name="overwrite">Czy nadpisać istniejące pliki (true = nadpisz, false = pomiń)</param>
        private static void CopyDirectoryContents(string sourceDir, string destDir, bool overwrite = true)
        {
            // Skopiuj wszystkie pliki z głównego katalogu
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(destDir, fileName);

                // Jeśli plik już istnieje i nie nadpisujemy, pomiń
                if (File.Exists(destFile) && !overwrite)
                {
                    Console.WriteLine($"  ⊘ Pominięto (już istnieje): {fileName}");
                    continue;
                }

                try
                {
                    File.Copy(file, destFile, true);
                    if (File.Exists(destFile) && overwrite)
                    {
                        Console.WriteLine($"  ✓ Nadpisano: {fileName}");
                    }
                    else
                    {
                        Console.WriteLine($"  ✓ Skopiowano: {fileName}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Błąd kopiowania {fileName}: {ex.Message}");
                }
            }

            // Rekurencyjnie skopiuj wszystkie podkatalogi
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(dir);
                var destSubDir = Path.Combine(destDir, dirName);

                // Jeśli katalog już istnieje, kontynuuj rekurencyjnie
                if (Directory.Exists(destSubDir))
                {
                    // Console.WriteLine($"  → Katalog: {dirName} (merge)");
                }
                else
                {
                    Directory.CreateDirectory(destSubDir);
                    Console.WriteLine($"  ✓ Utworzono katalog: {dirName}");
                }

                CopyDirectoryContents(dir, destSubDir, overwrite);
            }
        }

    }
}

