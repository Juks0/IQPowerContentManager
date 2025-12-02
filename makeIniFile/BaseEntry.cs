using System;
using System.Collections.Generic;
using System.Text;

namespace IQPowerContentManager
{
    /// <summary>
    /// Bazowa klasa dla wszystkich bindów (osie, przyciski)
    /// </summary>
    public abstract class BaseEntry
    {
        /// <summary>
        /// ID binda (np. "STEER", "THROTTLE", "GEARUP")
        /// </summary>
        public string Id { get; protected set; }

        /// <summary>
        /// Nazwa wyświetlana w UI
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Indeks kontrolera (JOY)
        /// </summary>
        public int ControllerIndex { get; set; } = -1;

        /// <summary>
        /// Czy czeka na przypisanie wejścia
        /// </summary>
        public bool IsWaiting { get; set; } = false;

        protected BaseEntry(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        /// <summary>
        /// Wyczyść bind
        /// </summary>
        public virtual void Clear()
        {
            IsWaiting = false;
            ControllerIndex = -1;
        }

        /// <summary>
        /// Zapisuje bind do pliku INI
        /// </summary>
        public abstract void Save(StringBuilder sb);

        /// <summary>
        /// Ładuje bind z pliku INI
        /// </summary>
        public abstract void Load(Dictionary<string, Dictionary<string, string>> iniData);
    }
}

