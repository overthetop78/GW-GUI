namespace GWGUI.Emulation.Amiga.Contracts;

public sealed record AmigaControllerBinding(
    int Port,
    AmigaControllerType Type,
    string? DeviceId = null,
    IReadOnlyDictionary<string, string>? ButtonMappings = null,
    string? VisualId = null);
