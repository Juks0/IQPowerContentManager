using System;
using Microsoft.Owin.Hosting;
using IQPowerContentManager.Api;

namespace IQPowerContentManager
{
    public class ApiProgram
    {
        private static IDisposable _webApp;

        public static void StartApi(string baseUrl = "http://localhost:8080")
        {
            try
            {
                _webApp = WebApp.Start<Startup>(baseUrl);
                Console.WriteLine($"API uruchomione na: {baseUrl}");
                Console.WriteLine($"Swagger UI dostępny na: {baseUrl}/swagger");
                Console.WriteLine($"Swagger JSON: {baseUrl}/swagger/docs/v1");
                Console.WriteLine($"Przykładowy endpoint: {baseUrl}/api/cars");
                Console.WriteLine("Naciśnij Enter aby zatrzymać API...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd uruchamiania API: {ex.Message}");
                Console.WriteLine($"Szczegóły: {ex}");
            }
        }

        public static void StopApi()
        {
            _webApp?.Dispose();
            Console.WriteLine("API zatrzymane");
        }
    }
}

