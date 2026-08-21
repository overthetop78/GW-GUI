namespace GWGUI.Emulation;

public sealed record EmulationSettingsBlock(
    string Id,
    EmulationMachineTab Tab,
    string TitleResourceKey,
    IReadOnlyList<EmulationSettingsField> Fields,
    string? Icon = null,
    int Columns = 1,
    bool IsVisible = true);
