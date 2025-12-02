using System;
using System.Collections.Generic;

namespace IQPowerContentManager.Api.Models
{
    public class DeviceInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Guid { get; set; }
        public string ProductGuid { get; set; } // Product GUID dla PGUID
    }

    public class SetDevicesRequest
    {
        public List<DeviceInfo> Devices { get; set; }
    }

    public class BindRequest
    {
        public string Action { get; set; } // "STEER", "THROTTLE", "BRAKES", "CLUTCH", "HANDBRAKE", "GEARUP", "GEARDN", "CAMERA"
        public int ControllerIndex { get; set; }
        public int? ButtonIndex { get; set; }
        public int? AxleIndex { get; set; }
    }

    public class BindHShifterRequest
    {
        public int ControllerIndex { get; set; }
        public Dictionary<string, int> Gears { get; set; } // "GEAR_1" -> buttonIndex, "GEAR_2" -> buttonIndex, etc.
    }

    public class BindSequentialRequest
    {
        public int ControllerIndex { get; set; }
        public int? GearUpButton { get; set; }
        public int? GearDownButton { get; set; }
    }

    public class ControlsState
    {
        public List<DeviceInfo> Devices { get; set; } = new List<DeviceInfo>();
        public SteerBinding Steer { get; set; }
        public PedalBinding Throttle { get; set; }
        public PedalBinding Brakes { get; set; }
        public PedalBinding Clutch { get; set; }
        public Binding Handbrake { get; set; }
        public Binding GearUp { get; set; }
        public Binding PaddleUp { get; set; }
        public Binding GearDown { get; set; }
        public Binding PaddleDown { get; set; }
        public Binding Camera { get; set; }
        public HShifterBinding HShifter { get; set; }
    }

    public class Binding
    {
        public int ControllerIndex { get; set; }
        public int? ButtonIndex { get; set; }
        public int? AxleIndex { get; set; }
    }

    public class SteerBinding : Binding
    {
        public int DegreesOfRotation { get; set; }
        public int Scale { get; set; }
    }

    public class PedalBinding : Binding
    {
        public int RangeFrom { get; set; }
        public int RangeTo { get; set; }
    }

    public class HShifterBinding
    {
        public bool Active { get; set; }
        public int ControllerIndex { get; set; }
        public Dictionary<string, int> Gears { get; set; }
    }

    // Modele dla endpointu /api/controls/bindings
    public class BindingsResponse
    {
        public List<ActionBinding> Actions { get; set; }
    }

    public class BindingInfo
    {
        public string Id { get; set; } // Unikalny ID binda (np. "GEARUP_1", "GEARUP_2", "GEAR_1", etc.)
        public int? ControllerIndex { get; set; }
        public string ControllerName { get; set; }
        public string InputType { get; set; } // "axis" lub "button"
        public int? InputIndex { get; set; }
        public string DisplayName { get; set; }
    }

    public class ActionBinding
    {
        public string Name { get; set; }
        public string Type { get; set; } // "axis" lub "button"
        public string Description { get; set; }
        public List<BindingInfo> Bindings { get; set; } = new List<BindingInfo>(); // Lista wszystkich bindów dla tej akcji
    }

    // Model do usuwania konkretnego binda
    public class UnbindSpecificRequest
    {
        public string BindingId { get; set; } // ID binda do usunięcia (np. "GEARUP_1", "GEAR_1")
    }

    // Model do rozpoczęcia nasłuchiwania na input (wykrywa oś/przycisk)
    public class BindingDetectionRequest
    {
        public int ControllerIndex { get; set; } // Indeks urządzenia z listy dostępnych urządzeń
        public int? TimeoutSeconds { get; set; } // Opcjonalny timeout w sekundach (domyślnie 15)
    }

    // Model statusu nasłuchiwania
    public class BindingDetectionStatus
    {
        public string Action { get; set; }
        public bool IsListening { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCancelled { get; set; }
        public string StatusMessage { get; set; }
        public int? DetectedAxis { get; set; }
        public int? DetectedButton { get; set; }
        public int? DetectedControllerIndex { get; set; } // Indeks wykrytego urządzenia
        public string DetectedControllerName { get; set; } // Nazwa wykrytego urządzenia
        public bool Success { get; set; } // Czy bindowanie się powiodło
        public DateTime? StartTime { get; set; }
        public int? TimeoutSeconds { get; set; }
    }

    // Modele dla Setup Controller
    public class GearboxTypeInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class SetGearboxTypesRequest
    {
        public List<GearboxTypeInfo> GearboxTypes { get; set; }
    }
}

