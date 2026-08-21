namespace GWGUI.Emulation;

public sealed record EmulationSettingsVisibility(
    IReadOnlyDictionary<EmulationMachineTab, bool> Tabs,
    IReadOnlyDictionary<string, bool>? Blocks = null,
    IReadOnlyDictionary<string, bool>? Fields = null);
