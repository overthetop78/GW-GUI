namespace GWGUI.Emulation.Watara.Common.Machines.Common.Contracts;

public sealed record ControllerBinding(int Port, PeripheralCategory Peripheral = PeripheralCategory.Supervision, string? DeviceId = null, IReadOnlyDictionary<string, string>? Mappings = null, int DeadZonePercent = ControllerConstants.DefaultDeadZonePercent, string? VisualId = null);
