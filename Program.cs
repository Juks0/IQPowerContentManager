#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using IQPowerContentManager.Api;
using IQPowerContentManager.Api.Controllers;
using IQPowerContentManager.Api.Models;
using AcTools.Utils;

namespace IQPowerContentManager
{
    class Program
    {
        private static Controls GetControlsInstance()
        {
            // Używamy refleksji, aby uzyskać dostęp do prywatnego pola _controls z ControlsController
            var controllerType = typeof(ControlsController);
            var controlsField = controllerType.GetField("_controls",
                BindingFlags.NonPublic | BindingFlags.Static);

            if (controlsField == null)
            {
                return new Controls();
            }

            var controls = controlsField.GetValue(null) as Controls;
            return controls ?? new Controls();
        }

        static void Main(string[] args)
        {
            // Uruchom API automatycznie przy starcie aplikacji w osobnym wątku w tle
            string baseUrl = args.Length > 0 ? args[0] : "http://localhost:8080";
            Thread apiThread = new Thread(() =>
            {
                try
                {
                    ApiProgram.StartApi(baseUrl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd uruchamiania API: {ex.Message}");
                }
            });
            apiThread.IsBackground = true;
            apiThread.Start();

            // Daj API czas na uruchomienie
            Thread.Sleep(500);

            // Uruchom interaktywny interfejs bindowania w głównym wątku
            RunBindingInterface();

            ApiProgram.StopApi();
        }

        static void RunBindingInterface()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine("\n=== IQPower Content Manager - Bindowanie Kontrolerów ===");
                        Console.WriteLine("1. Lista dostępnych urządzeń");
                        Console.WriteLine("2. Binduj STEER (kierownica - oś)");
                        Console.WriteLine("3. Binduj THROTTLE (gaz - oś)");
                        Console.WriteLine("4. Binduj BRAKES (hamulce - oś)");
                        Console.WriteLine("5. Binduj CLUTCH (sprzęgło - oś)");
                        Console.WriteLine("6. Binduj HANDBRAKE (hamulec ręczny - przycisk lub oś)");
                        Console.WriteLine("7. Binduj GEARUP (bieg w górę - przycisk)");
                        Console.WriteLine("8. Binduj GEARDN (bieg w dół - przycisk)");
                        Console.WriteLine("9. Binduj CAMERA (zmiana kamery - przycisk)");
                        Console.WriteLine("10. Binduj H-Shifter (biegi 1-7 i R)");
                        Console.WriteLine("11. Pokaż aktualne bindy");
                        Console.WriteLine("12. Zapisz ustawienia do controls.ini");
                        Console.WriteLine("13. Wczytaj ustawienia z controls.ini");
                        Console.WriteLine("14. Usuń wszystkie bindy");
                        Console.WriteLine("0. Wyjście");
                        Console.Write("\nWybierz opcję: ");

                        string input = Console.ReadLine() ?? "";
                        if (string.IsNullOrEmpty(input))
                            continue;

                        switch (input.Trim())
                        {
                            case "1":
                                ShowAvailableDevices();
                                break;
                            case "2":
                                BindAxis("STEER");
                                break;
                            case "3":
                                BindAxis("THROTTLE");
                                break;
                            case "4":
                                BindAxis("BRAKES");
                                break;
                            case "5":
                                BindAxis("CLUTCH");
                                break;
                            case "6":
                                BindHandbrake();
                                break;
                            case "7":
                                BindButton("GEARUP");
                                break;
                            case "8":
                                BindButton("GEARDN");
                                break;
                            case "9":
                                BindButton("CAMERA");
                                break;
                            case "10":
                                BindHShifter();
                                break;
                            case "11":
                                ShowCurrentBindings();
                                break;
                            case "12":
                                SaveControls();
                                break;
                            case "13":
                                LoadControls();
                                break;
                            case "14":
                                UnbindAll();
                                break;
                            case "0":
                                Console.WriteLine("Zamykanie programu...");
                                return;
                            default:
                                Console.WriteLine("Nieprawidłowa opcja!");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\n✗ Błąd: {ex.Message}");
                        Console.WriteLine($"Szczegóły: {ex}");
                        Console.WriteLine("Naciśnij Enter aby kontynuować...");
                        Console.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Krytyczny błąd w interfejsie bindowania: {ex.Message}");
                Console.WriteLine($"Szczegóły: {ex}");
            }
        }

        static void ShowAvailableDevices()
        {
            try
            {
                Console.WriteLine("\n=== Dostępne urządzenia ===");
                var devices = JoystickInputHandler.GetAvailableDevices();
                if (devices.Count == 0)
                {
                    Console.WriteLine("Brak dostępnych urządzeń.");
                    return;
                }

                for (int i = 0; i < devices.Count; i++)
                {
                    Console.WriteLine($"{i}. {devices[i].Name}");
                    Console.WriteLine($"   GUID: {JoystickInputHandler.FormatGuid(devices[i].Guid)}");
                    Console.WriteLine($"   Product GUID: {JoystickInputHandler.FormatGuid(devices[i].ProductGuid)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Błąd podczas pobierania listy urządzeń: {ex.Message}");
            }
        }

        static void BindAxis(string actionName)
        {
            try
            {
                Console.WriteLine($"\n=== Bindowanie {actionName} (oś) ===");
                var devices = JoystickInputHandler.GetAvailableDevices();
                if (devices.Count == 0)
                {
                    Console.WriteLine("Brak dostępnych urządzeń.");
                    return;
                }

                Console.WriteLine("Dostępne urządzenia:");
                for (int i = 0; i < devices.Count; i++)
                {
                    Console.WriteLine($"{i}. {devices[i].Name}");
                }

                Console.Write("Wybierz numer urządzenia: ");
                string? deviceInput = Console.ReadLine();
                if (string.IsNullOrEmpty(deviceInput) || !int.TryParse(deviceInput, out int deviceIndex) || deviceIndex < 0 || deviceIndex >= devices.Count)
                {
                    Console.WriteLine("Nieprawidłowy numer urządzenia.");
                    return;
                }

                Console.WriteLine($"\nPoruszaj {GetActionDescription(actionName)} na urządzeniu '{devices[deviceIndex].Name}'...");
                Console.WriteLine("Czekam na wykrycie ruchu osi... (Naciśnij ESC aby anulować)");

                var handler = new JoystickInputHandler();
                if (!handler.Initialize(devices[deviceIndex].Guid))
                {
                    Console.WriteLine("Nie udało się połączyć z urządzeniem.");
                    return;
                }

                // Poczekaj chwilę, aby urządzenie się zainicjalizowało
                Thread.Sleep(200);

                // Zapisz początkowe wartości osi jako referencję
                handler.OnTick();
                double[] initialAxisValues = new double[handler.Axes.Count];
                for (int i = 0; i < handler.Axes.Count; i++)
                {
                    initialAxisValues[i] = handler.Axes[i].Value;
                }

                DateTime startTime = DateTime.Now;
                int detectedAxis = -1;
                double maxChange = 0;
                int axisWithMaxChange = -1;

                Console.WriteLine("Wykrywanie osi... (poruszaj kontrolerem)");

                while ((DateTime.Now - startTime).TotalSeconds < 15)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Escape)
                        {
                            handler.Dispose();
                            Console.WriteLine("Anulowano.");
                            return;
                        }
                    }

                    handler.OnTick();

                    // Sprawdź wszystkie osie i znajdź tę z największą zmianą
                    for (int i = 0; i < handler.Axes.Count; i++)
                    {
                        var axis = handler.Axes[i];
                        double currentValue = axis.Value;
                        double change = Math.Abs(currentValue - initialAxisValues[i]);

                        // Jeśli zmiana jest większa niż 5% (0.05), uznaj to za aktywną oś
                        if (change > 0.05)
                        {
                            if (change > maxChange)
                            {
                                maxChange = change;
                                axisWithMaxChange = i;
                            }
                        }
                    }

                    // Jeśli znaleziono oś z wystarczająco dużą zmianą, użyj jej
                    if (axisWithMaxChange >= 0 && maxChange > 0.1)
                    {
                        detectedAxis = axisWithMaxChange;
                        break;
                    }

                    Thread.Sleep(50);
                }

                handler.Dispose();

                if (detectedAxis >= 0)
                {
                    var controls = GetControlsInstance();
                    if (controls.AssignAxisBinding(actionName, deviceIndex, detectedAxis))
                    {
                        Console.WriteLine($"✓ Zbindowano {actionName} do urządzenia {deviceIndex}, oś {detectedAxis}");
                        StateHelper.SaveCurrentState();
                    }
                    else
                    {
                        Console.WriteLine($"✗ Nie udało się zbindować {actionName}");
                    }
                }
                else
                {
                    Console.WriteLine("Nie wykryto ruchu osi. Spróbuj ponownie.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Błąd podczas bindowania {actionName}: {ex.Message}");
            }
        }

        static void BindButton(string actionName)
        {
            try
            {
                Console.WriteLine($"\n=== Bindowanie {actionName} (przycisk) ===");
                var devices = JoystickInputHandler.GetAvailableDevices();
                if (devices.Count == 0)
                {
                    Console.WriteLine("Brak dostępnych urządzeń.");
                    return;
                }

                Console.WriteLine("Dostępne urządzenia:");
                for (int i = 0; i < devices.Count; i++)
                {
                    Console.WriteLine($"{i}. {devices[i].Name}");
                }

                Console.Write("Wybierz numer urządzenia: ");
                string? deviceInput = Console.ReadLine();
                if (string.IsNullOrEmpty(deviceInput) || !int.TryParse(deviceInput, out int deviceIndex) || deviceIndex < 0 || deviceIndex >= devices.Count)
                {
                    Console.WriteLine("Nieprawidłowy numer urządzenia.");
                    return;
                }

                Console.WriteLine($"\nNaciśnij przycisk na urządzeniu '{devices[deviceIndex].Name}'...");
                Console.WriteLine("Czekam na wykrycie przycisku... (Naciśnij ESC aby anulować)");

                var handler = new JoystickInputHandler();
                if (!handler.Initialize(devices[deviceIndex].Guid))
                {
                    Console.WriteLine("Nie udało się połączyć z urządzeniem.");
                    return;
                }

                // Poczekaj chwilę, aby urządzenie się zainicjalizowało
                Thread.Sleep(200);

                DateTime startTime = DateTime.Now;
                int detectedButton = -1;
                bool buttonWasPressed = false;

                Console.WriteLine("Wykrywanie przycisku... (naciśnij i przytrzymaj przycisk)");

                while ((DateTime.Now - startTime).TotalSeconds < 15)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Escape)
                        {
                            handler.Dispose();
                            Console.WriteLine("Anulowano.");
                            return;
                        }
                    }

                    handler.OnTick();

                    // Sprawdź wszystkie przyciski
                    for (int i = 0; i < handler.Buttons.Count; i++)
                    {
                        if (handler.Buttons[i].Value)
                        {
                            if (!buttonWasPressed)
                            {
                                // Pierwsze wykrycie wciśnięcia - poczekaj chwilę, aby upewnić się, że to nie jest szum
                                buttonWasPressed = true;
                                detectedButton = i;
                                Thread.Sleep(100); // Krótka pauza, aby upewnić się, że przycisk jest rzeczywiście wciśnięty
                                handler.OnTick(); // Odśwież stan

                                // Sprawdź ponownie, czy przycisk jest nadal wciśnięty
                                if (handler.Buttons[i].Value)
                                {
                                    break;
                                }
                                else
                                {
                                    // Jeśli przycisk nie jest już wciśnięty, zresetuj
                                    buttonWasPressed = false;
                                    detectedButton = -1;
                                }
                            }
                            else if (detectedButton == i)
                            {
                                // Przycisk nadal wciśnięty - potwierdź
                                break;
                            }
                        }
                    }

                    if (detectedButton >= 0 && buttonWasPressed)
                    {
                        // Poczekaj, aż przycisk zostanie zwolniony, aby potwierdzić wykrycie
                        Thread.Sleep(200);
                        handler.OnTick();
                        if (!handler.Buttons[detectedButton].Value)
                        {
                            // Przycisk został zwolniony - wykrycie potwierdzone
                            break;
                        }
                    }

                    Thread.Sleep(50);
                }

                handler.Dispose();

                if (detectedButton >= 0)
                {
                    string actionToUse = actionName;
                    if (actionName == "CAMERA")
                    {
                        actionToUse = "ACTION_CHANGE_CAMERA";
                    }

                    var controls = GetControlsInstance();
                    if (controls.AssignButtonBinding(actionToUse, deviceIndex, detectedButton))
                    {
                        Console.WriteLine($"✓ Zbindowano {actionName} do urządzenia {deviceIndex}, przycisk {detectedButton}");
                        StateHelper.SaveCurrentState();
                    }
                    else
                    {
                        Console.WriteLine($"✗ Nie udało się zbindować {actionName}");
                    }
                }
                else
                {
                    Console.WriteLine("Nie wykryto wciśnięcia przycisku. Spróbuj ponownie.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Błąd podczas bindowania {actionName}: {ex.Message}");
            }
        }

        static void BindHandbrake()
        {
            try
            {
                Console.WriteLine("\n=== Bindowanie HANDBRAKE ===");
                Console.WriteLine("1. Binduj jako przycisk");
                Console.WriteLine("2. Binduj jako oś");
                Console.Write("Wybierz opcję: ");

                string input = Console.ReadLine() ?? "";
                if (input == "1")
                {
                    BindButton("HANDBRAKE");
                }
                else if (input == "2")
                {
                    BindAxis("HANDBRAKE");
                }
                else
                {
                    Console.WriteLine("Nieprawidłowa opcja.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Błąd podczas bindowania HANDBRAKE: {ex.Message}");
            }
        }

        static void BindHShifter()
        {
            try
            {
                Console.WriteLine("\n=== Bindowanie H-Shifter ===");
                var devices = JoystickInputHandler.GetAvailableDevices();
                if (devices.Count == 0)
                {
                    Console.WriteLine("Brak dostępnych urządzeń.");
                    return;
                }

                Console.WriteLine("Dostępne urządzenia:");
                for (int i = 0; i < devices.Count; i++)
                {
                    Console.WriteLine($"{i}. {devices[i].Name}");
                }

                Console.Write("Wybierz numer urządzenia dla shiftera: ");
                string? deviceInput = Console.ReadLine();
                if (string.IsNullOrEmpty(deviceInput) || !int.TryParse(deviceInput, out int deviceIndex) || deviceIndex < 0 || deviceIndex >= devices.Count)
                {
                    Console.WriteLine("Nieprawidłowy numer urządzenia.");
                    return;
                }

                var controls = GetControlsInstance();
                controls.ShifterActive = 1;
                controls.ShifterJoy = deviceIndex;

                var gearNames = new[] { "GEAR_1", "GEAR_2", "GEAR_3", "GEAR_4", "GEAR_5", "GEAR_6", "GEAR_7", "GEAR_R" };
                var gearDisplayNames = new[] { "1", "2", "3", "4", "5", "6", "7", "R" };

                Console.WriteLine($"\nBinduj biegi na urządzeniu '{devices[deviceIndex].Name}'...");
                Console.WriteLine("Naciśnij ESC aby anulować, SPACE aby pominąć bieg.");

                var handler = new JoystickInputHandler();
                if (!handler.Initialize(devices[deviceIndex].Guid))
                {
                    Console.WriteLine("Nie udało się połączyć z urządzeniem.");
                    return;
                }

                for (int g = 0; g < gearNames.Length; g++)
                {
                    Console.WriteLine($"\nNaciśnij przycisk dla biegu {gearDisplayNames[g]}...");

                    DateTime startTime = DateTime.Now;
                    int detectedButton = -1;

                    while ((DateTime.Now - startTime).TotalSeconds < 10)
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Escape)
                            {
                                handler.Dispose();
                                Console.WriteLine("Anulowano.");
                                return;
                            }
                            if (key.Key == ConsoleKey.Spacebar)
                            {
                                Console.WriteLine($"Pominięto bieg {gearDisplayNames[g]}");
                                break;
                            }
                        }

                        handler.OnTick();

                        for (int i = 0; i < handler.Buttons.Count; i++)
                        {
                            if (handler.Buttons[i].Value)
                            {
                                detectedButton = i;
                                break;
                            }
                        }

                        if (detectedButton >= 0)
                            break;

                        Thread.Sleep(10);
                    }

                    if (detectedButton >= 0)
                    {
                        if (controls.ShifterGears.ContainsKey(gearNames[g]))
                        {
                            controls.ShifterGears[gearNames[g]] = detectedButton;
                            Console.WriteLine($"✓ Zbindowano bieg {gearDisplayNames[g]} do przycisku {detectedButton}");
                        }
                    }
                }

                handler.Dispose();
                StateHelper.SaveCurrentState();
                Console.WriteLine("\n✓ Zakończono bindowanie H-shiftera");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Błąd podczas bindowania H-Shifter: {ex.Message}");
            }
        }

        static void ShowCurrentBindings()
        {
            try
            {
                var controls = GetControlsInstance();
                Console.WriteLine("\n=== Aktualne bindy ===");
                Console.WriteLine($"STEER: Urządzenie {controls.SteerAxleEntry.ControllerIndex}, Oś {controls.SteerAxleEntry.AxleIndex}");
                Console.WriteLine($"THROTTLE: Urządzenie {controls.ThrottleAxleEntry.ControllerIndex}, Oś {controls.ThrottleAxleEntry.AxleIndex}");
                Console.WriteLine($"BRAKES: Urządzenie {controls.BrakesAxleEntry.ControllerIndex}, Oś {controls.BrakesAxleEntry.AxleIndex}");
                Console.WriteLine($"CLUTCH: Urządzenie {controls.ClutchAxleEntry.ControllerIndex}, Oś {controls.ClutchAxleEntry.AxleIndex}");
                Console.WriteLine($"HANDBRAKE (przycisk): Urządzenie {controls.HandbrakeButtonEntry.ControllerIndex}, Przycisk {controls.HandbrakeButtonEntry.ButtonIndex}");
                Console.WriteLine($"HANDBRAKE (oś): Urządzenie {controls.HandbrakeAxleEntry.ControllerIndex}, Oś {controls.HandbrakeAxleEntry.AxleIndex}");
                Console.WriteLine($"GEARUP: Urządzenie {controls.GearUpButtonEntry.ControllerIndex}, Przycisk {controls.GearUpButtonEntry.ButtonIndex}");
                Console.WriteLine($"GEARDN: Urządzenie {controls.GearDnButtonEntry.ControllerIndex}, Przycisk {controls.GearDnButtonEntry.ButtonIndex}");
                Console.WriteLine($"CAMERA: Urządzenie {controls.CameraButtonEntry.ControllerIndex}, Przycisk {controls.CameraButtonEntry.ButtonIndex}");

                if (controls.ShifterActive == 1)
                {
                    Console.WriteLine($"H-SHIFTER: Aktywny, Urządzenie {controls.ShifterJoy}");
                    foreach (var gear in controls.ShifterGears)
                    {
                        if (gear.Value >= 0)
                        {
                            Console.WriteLine($"  {gear.Key}: Przycisk {gear.Value}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Błąd podczas wyświetlania bindów: {ex.Message}");
            }
        }

        static void SaveControls()
        {
            try
            {
                var controls = GetControlsInstance();
                // Zapisuj do C:\Users\{current-user}\Documents\asseto-manager
                var documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var managerDir = System.IO.Path.Combine(documentsDir, "asseto-manager");
                var outputPath = System.IO.Path.Combine(managerDir, "controls.ini");

                if (!System.IO.Directory.Exists(managerDir))
                {
                    System.IO.Directory.CreateDirectory(managerDir);
                }

                controls.SaveToFile(outputPath);
                Console.WriteLine($"✓ Ustawienia zapisane do: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Błąd zapisu: {ex.Message}");
            }
        }

        static void LoadControls()
        {
            try
            {
                var controls = GetControlsInstance();
                // Wczytuj z C:\Users\{current-user}\Documents\asseto-manager, jeśli nie istnieje, to z domyślnego miejsca
                var documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var managerDir = System.IO.Path.Combine(documentsDir, "asseto-manager");
                var managerPath = System.IO.Path.Combine(managerDir, "controls.ini");
                var defaultPath = AcPaths.GetCfgControlsFilename();
                var inputPath = System.IO.File.Exists(managerPath) ? managerPath : defaultPath;

                if (!System.IO.File.Exists(inputPath))
                {
                    Console.WriteLine($"✗ Plik nie istnieje: {inputPath}");
                    return;
                }

                if (controls.LoadFromFile(inputPath))
                {
                    Console.WriteLine("✓ Ustawienia wczytane pomyślnie");
                    StateHelper.SaveCurrentState();
                }
                else
                {
                    Console.WriteLine("✗ Nie udało się wczytać ustawień");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Błąd wczytywania: {ex.Message}");
            }
        }

        static void UnbindAll()
        {
            try
            {
                Console.WriteLine("\n=== Usuwanie wszystkich bindów ===");
                Console.Write("Czy na pewno chcesz usunąć wszystkie bindy? (T/N): ");
                string? confirm = Console.ReadLine();

                if (confirm?.Trim().ToUpper() != "T")
                {
                    Console.WriteLine("Anulowano.");
                    return;
                }

                var controls = GetControlsInstance();
                controls.UnbindAll();
                StateHelper.SaveCurrentState();
                Console.WriteLine("✓ Wszystkie bindy zostały usunięte");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Błąd podczas usuwania bindów: {ex.Message}");
            }
        }

        static string GetActionDescription(string action)
        {
            return action switch
            {
                "STEER" => "kierownicą",
                "THROTTLE" => "pedałem gazu",
                "BRAKES" => "pedałem hamulca",
                "CLUTCH" => "pedałem sprzęgła",
                "HANDBRAKE" => "hamulcem ręcznym",
                _ => "kontrolerem"
            };
        }
    }
}
