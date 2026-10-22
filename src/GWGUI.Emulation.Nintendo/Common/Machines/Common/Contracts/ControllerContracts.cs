namespace GWGUI.Emulation.Nintendo.Common.Machines.Common.Contracts;

public sealed record ControllerBinding(int Port, PeripheralCategory Type, string? DeviceId = null, IReadOnlyDictionary<string, string>? ButtonMappings = null, string? VisualId = null);

