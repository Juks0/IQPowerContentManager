using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using IQPowerContentManager.Api.Models;

namespace IQPowerContentManager.Api
{
    /// <summary>
    /// Zarządza stanem aplikacji - zapisuje i wczytuje dane z pliku JSON
    /// </summary>
    public static class ApplicationStateManager
    {
        private static readonly string StateFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "IQPowerContentManager",
            "application_state.json"
        );

        private static readonly object _lock = new object();

        /// <summary>
        /// Zapisuje stan aplikacji do pliku JSON
        /// </summary>
        public static void SaveState(ApplicationState state)
        {
            lock (_lock)
            {
                try
                {
                    var directory = Path.GetDirectoryName(StateFilePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    var json = JsonConvert.SerializeObject(state, Formatting.Indented);
                    File.WriteAllText(StateFilePath, json, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    // Log error but don't throw - we don't want to break the API if saving fails
                    System.Diagnostics.Debug.WriteLine($"Błąd zapisywania stanu aplikacji: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Wczytuje stan aplikacji z pliku JSON
        /// </summary>
        public static ApplicationState LoadState()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(StateFilePath))
                    {
                        return new ApplicationState();
                    }

                    var json = File.ReadAllText(StateFilePath, Encoding.UTF8);
                    var state = JsonConvert.DeserializeObject<ApplicationState>(json);
                    return state ?? new ApplicationState();
                }
                catch (Exception ex)
                {
                    // Log error and return empty state
                    System.Diagnostics.Debug.WriteLine($"Błąd wczytywania stanu aplikacji: {ex.Message}");
                    return new ApplicationState();
                }
            }
        }

        /// <summary>
        /// Pobiera ścieżkę do pliku stanu
        /// </summary>
        public static string GetStateFilePath()
        {
            return StateFilePath;
        }
    }
}

