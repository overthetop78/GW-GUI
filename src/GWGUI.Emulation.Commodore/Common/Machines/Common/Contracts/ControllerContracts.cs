namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Contracts;

public sealed record ControllerBinding(
    int Port,
    ControllerType Type,
    string? DeviceId = null,
    IReadOnlyDictionary<string, string>? ButtonMappings = null,
    string? VisualId = null);
