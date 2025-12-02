using System;
using System.Collections.Generic;
using System.Text;

namespace IQPowerContentManager
{
    /// <summary>
    /// Bindowanie osi kontrolera - kierownica lub pedały (gaz, hamulce, sprzęgło)
    /// </summary>
    public class WheelAxleEntry : BaseEntry
    {
        /// <summary>
        /// Indeks osi (AXLE)
        /// </summary>
        public int AxleIndex { get; set; } = -1;

        /// <summary>
        /// true dla pedałów (używa MIN/MAX), false dla kierownicy (używa LOCK/SCALE)
        /// </summary>
        public bool RangeMode { get; private set; }

        // Właściwości dla pedałów (RangeMode = true)
        /// <summary>
        /// Minimalna wartość osi (0-100, konwertowane do MIN -1.0 do 1.0)
        /// </summary>
        public int RangeFrom { get; set; } = 0;

        /// <summary>
        /// Maksymalna wartość osi (0-100, konwertowane do MAX -1.0 do 1.0)
        /// </summary>
        public int RangeTo { get; set; } = 100;

        /// <summary>
        /// Czy odwrócić oś
        /// </summary>
        public bool Invert { get; set; } = false;

        // Właściwości dla kierownicy (RangeMode = false)
        /// <summary>
        /// Zakres obrotu w stopniach (40-9000)
        /// </summary>
        public int DegreesOfRotation { get; set; } = 900;

        /// <summary>
        /// Skala kierownicy (0-100, konwertowane do SCALE -1.0 do 1.0)
        /// </summary>
        public int Scale { get; set; } = 100;

        /// <summary>
        /// Filtr kierownicy (0-100, konwertowane do STEER_FILTER 0.0 do 1.0)
        /// </summary>
        public int Filter { get; set; } = 0;

        /// <summary>
        /// Czułość zależna od prędkości (0-100, konwertowane do SPEED_SENSITIVITY 0.0 do 1.0)
        /// </summary>
        public int SpeedSensitivity { get; set; } = 0;

        /// <summary>
        /// Wzmocnienie force feedback (FF_GAIN)
        /// </summary>
        public double FfGain { get; set; } = 1.0;

        /// <summary>
        /// Filtr force feedback (FILTER_FF)
        /// </summary>
        public double FilterFf { get; set; } = 0.0;

        /// <summary>
        /// Debouncing w milisekundach (DEBOUNCING_MS)
        /// </summary>
        public int DebouncingMs { get; set; } = 0;

        public WheelAxleEntry(string id, string displayName, bool rangeMode = false)
            : base(id, displayName)
        {
            RangeMode = rangeMode;
        }

        /// <summary>
        /// Konwertuje wartość procentową (0-100) do wartości double (-1.0 do 1.0)
        /// </summary>
        private double ToDoublePercentage(int percentage)
        {
            return percentage / 100.0;
        }

        /// <summary>
        /// Konwertuje wartość double (-1.0 do 1.0) do wartości procentowej (0-100)
        /// </summary>
        private int ToIntPercentage(double value)
        {
            return (int)Math.Round(value * 100.0);
        }

        public override void Save(StringBuilder sb)
        {
            sb.AppendLine($"[{Id}]");
            sb.AppendLine($"JOY = {ControllerIndex}");
            sb.AppendLine($"AXLE = {AxleIndex}");

            if (RangeMode)
            {
                // Dla pedałów - MIN/MAX
                var min = 0.02 * RangeFrom - 1.0;
                var max = 0.02 * RangeTo - 1.0;

                sb.AppendLine($"MIN = {(Invert ? -min : min):F1}");
                sb.AppendLine($"MAX = {(Invert ? -max : max):F1}");
            }
            else
            {
                // Dla kierownicy - LOCK, SCALE, filtry
                sb.AppendLine($"SCALE = {(Invert ? -1.0 : 1.0) * ToDoublePercentage(Scale):F1}");
                sb.AppendLine($"LOCK = {DegreesOfRotation}");
                sb.AppendLine($"DEBOUNCING_MS = {DebouncingMs}");
                sb.AppendLine($"FF_GAIN = {FfGain:F1}");
                sb.AppendLine($"FILTER_FF = {FilterFf:F1}");
                sb.AppendLine($"STEER_FILTER = {ToDoublePercentage(Filter):F1}");
                sb.AppendLine($"SPEED_SENSITIVITY = {ToDoublePercentage(SpeedSensitivity):F1}");
            }
            sb.AppendLine();
        }

        public override void Load(Dictionary<string, Dictionary<string, string>> iniData)
        {
            if (!iniData.ContainsKey(Id))
                return;

            var section = iniData[Id];

            // Wczytaj JOY i AXLE
            if (section.ContainsKey("JOY") && int.TryParse(section["JOY"], out int joy))
                ControllerIndex = joy;

            if (section.ContainsKey("AXLE") && int.TryParse(section["AXLE"], out int axle))
                AxleIndex = axle;

            if (RangeMode)
            {
                // Dla pedałów - MIN/MAX
                if (section.ContainsKey("MIN") && double.TryParse(section["MIN"], out double min))
                {
                    var from = (int)((min + 1.0) / 0.02);
                    RangeFrom = Math.Max(0, Math.Min(100, from));
                }

                if (section.ContainsKey("MAX") && double.TryParse(section["MAX"], out double max))
                {
                    var to = (int)((max + 1.0) / 0.02);
                    RangeTo = Math.Max(0, Math.Min(100, to));
                }

                Invert = RangeFrom > RangeTo;
            }
            else
            {
                // Dla kierownicy - LOCK, SCALE, filtry
                if (section.ContainsKey("LOCK") && int.TryParse(section["LOCK"], out int lockValue))
                    DegreesOfRotation = lockValue;

                if (section.ContainsKey("SCALE") && double.TryParse(section["SCALE"], out double scale))
                {
                    Invert = scale < 0;
                    Scale = ToIntPercentage(Math.Abs(scale));
                }

                if (section.ContainsKey("STEER_FILTER") && double.TryParse(section["STEER_FILTER"], out double filter))
                    Filter = ToIntPercentage(filter);

                if (section.ContainsKey("SPEED_SENSITIVITY") && double.TryParse(section["SPEED_SENSITIVITY"], out double speedSens))
                    SpeedSensitivity = ToIntPercentage(speedSens);

                if (section.ContainsKey("FF_GAIN") && double.TryParse(section["FF_GAIN"], out double ffGain))
                    FfGain = ffGain;

                if (section.ContainsKey("FILTER_FF") && double.TryParse(section["FILTER_FF"], out double filterFf))
                    FilterFf = filterFf;

                if (section.ContainsKey("DEBOUNCING_MS") && int.TryParse(section["DEBOUNCING_MS"], out int debouncing))
                    DebouncingMs = debouncing;
            }
        }

        public override void Clear()
        {
            base.Clear();
            AxleIndex = -1;
        }
    }
}

