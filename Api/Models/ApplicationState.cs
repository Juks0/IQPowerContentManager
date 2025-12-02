using System.Collections.Generic;

namespace IQPowerContentManager.Api.Models
{
    /// <summary>
    /// Stan aplikacji przechowywany między sesjami
    /// </summary>
    public class ApplicationState
    {
        /// <summary>
        /// Stan ustawień kontrolerów
        /// </summary>
        public ControlsState Controls { get; set; } = new ControlsState();

        /// <summary>
        /// Ostatnio wybrany samochód
        /// </summary>
        public string LastSelectedCar { get; set; }

        /// <summary>
        /// Ostatnio wybrany tor
        /// </summary>
        public string LastSelectedTrack { get; set; }

        /// <summary>
        /// Ostatnio używany nick
        /// </summary>
        public string LastNick { get; set; }

        /// <summary>
        /// Ostatnio wybrana skórka
        /// </summary>
        public string LastSkin { get; set; }

        /// <summary>
        /// Ostatnio wybrany typ skrzyni biegów
        /// </summary>
        public string LastShifterType { get; set; }

        /// <summary>
        /// Czy assetofolder został wgrany
        /// </summary>
        public bool AssetofolderUploaded { get; set; }

        /// <summary>
        /// Lista ostatnio używanych samochodów
        /// </summary>
        public List<string> RecentCars { get; set; } = new List<string>();

        /// <summary>
        /// Lista ostatnio używanych torów
        /// </summary>
        public List<string> RecentTracks { get; set; } = new List<string>();

        /// <summary>
        /// Dostępne typy skrzyni biegów (konfigurowalne)
        /// </summary>
        public List<GearboxTypeInfo> AvailableGearboxTypes { get; set; } = new List<GearboxTypeInfo>();
    }
}

