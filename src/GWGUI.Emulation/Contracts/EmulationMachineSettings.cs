namespace GWGUI.Emulation;

public sealed record EmulationMachineSettings(
    string MachineId,
    EmulationSettingsVisibility Visibility,
    IReadOnlyList<EmulationSettingsBlock> Blocks,
    IReadOnlyList<EmulationSettingsRule>? Rules = null);
