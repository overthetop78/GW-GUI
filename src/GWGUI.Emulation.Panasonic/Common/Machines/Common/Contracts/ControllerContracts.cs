namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Contracts;

public sealed record ControllerBinding(int Port, PeripheralCategory Peripheral = PeripheralCategory.ThreeDOPad, string? DeviceId = null, IReadOnlyDictionary<string, string>? Mappings = null, int DeadZonePercent = ControllerConstants.DefaultDeadZonePercent, string? VisualId = null);
