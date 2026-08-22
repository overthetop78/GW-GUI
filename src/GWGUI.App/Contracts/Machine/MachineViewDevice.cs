namespace GWGUI.App.Contracts.Machine;

internal sealed record MachineViewDevice(
    string Key,
    string Label,
    string Glyph,
    bool Removable,
    bool Present,
    Func<Task>? Insert,
    Func<Task>? Eject);
