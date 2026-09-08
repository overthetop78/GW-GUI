namespace GWGUI.App.Contracts.Machine;

internal sealed record MachineViewDeviceCommand(
    EmulationCassetteCommand Command,
    string Label,
    string Glyph,
    bool IsSupported,
    bool IsEnabled,
    bool IsActive,
    bool IsBlinking,
    Func<Task> Execute);
