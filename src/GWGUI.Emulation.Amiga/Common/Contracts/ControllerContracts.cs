namespace GWGUI.Emulation.Amiga.Common.Contracts;

public sealed record ControllerBinding(
    int Port,
    ControllerType Type,
    string? DeviceId = null,
    IReadOnlyDictionary<string, string>? ButtonMappings = null,
    string? VisualId = null);

internal sealed record ControllerDevice(string Name, uint Id);
