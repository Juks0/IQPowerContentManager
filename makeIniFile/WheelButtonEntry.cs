using System;
using System.Collections.Generic;
using System.Text;

namespace IQPowerContentManager
{
    /// <summary>
    /// Bindowanie przycisku na kole kierowniczym
    /// </summary>
    public class WheelButtonEntry : BaseEntry
    {
        /// <summary>
        /// Indeks przycisku (BUTTON)
        /// </summary>
        public int ButtonIndex { get; set; } = -1;

        /// <summary>
        /// Kod klawisza klawiatury (KEY) w formacie hex (np. "0x57")
        /// </summary>
        public string KeyCode { get; set; } = "";

        public WheelButtonEntry(string id, string displayName)
            : base(id, displayName)
        {
        }

        public override void Save(StringBuilder sb)
        {
            sb.AppendLine($"[{Id}]");
            sb.AppendLine($"JOY = {ControllerIndex}");
            sb.AppendLine($"BUTTON = {ButtonIndex}");
            if (!string.IsNullOrEmpty(KeyCode))
            {
                sb.AppendLine($"KEY = {KeyCode}");
            }
            sb.AppendLine();
        }

        public override void Load(Dictionary<string, Dictionary<string, string>> iniData)
        {
            if (!iniData.ContainsKey(Id))
                return;

            var section = iniData[Id];

            if (section.ContainsKey("JOY") && int.TryParse(section["JOY"], out int joy))
                ControllerIndex = joy;

            if (section.ContainsKey("BUTTON") && int.TryParse(section["BUTTON"], out int button))
                ButtonIndex = button;

            if (section.ContainsKey("KEY"))
                KeyCode = section["KEY"];
        }

        public override void Clear()
        {
            base.Clear();
            ButtonIndex = -1;
            KeyCode = "";
        }
    }
}

