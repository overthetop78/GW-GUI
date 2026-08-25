namespace GWGUI.Emulation.Contracts;

public sealed record EmulationMachineSettings(
    string MachineId,
    EmulationSettingsVisibility Visibility,
    IReadOnlyList<EmulationSettingsBlock> Blocks,
    IReadOnlyList<EmulationSettingsRule>? Rules = null);
