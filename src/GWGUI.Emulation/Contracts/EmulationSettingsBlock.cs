namespace GWGUI.Emulation.Contracts;

public sealed record EmulationSettingsBlock(
    string Id,
    EmulationMachineTab Tab,
    string TitleResourceKey,
    IReadOnlyList<EmulationSettingsField> Fields,
    string? Icon = null,
    int Columns = 1,
    bool IsVisible = true);
