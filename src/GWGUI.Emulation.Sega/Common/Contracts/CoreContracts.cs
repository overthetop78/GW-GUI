namespace GWGUI.Emulation.Sega.Common.Contracts;

public sealed record CoreOption(
    string Key,
    string Name,
    string? Description,
    string? Category,
    string DefaultValue,
    string CurrentValue,
    IReadOnlyList<CoreOptionValue> Values,
    bool IsVisible = true,
    string? CategorizedName = null,
    string? CategorizedDescription = null,
    EmulationMachineTab Tab = EmulationMachineTab.General,
    bool RequiresRestart = false,
    IReadOnlySet<string>? MachineIds = null);

public sealed record CoreOptionValue(string Value, string Label,
    IReadOnlySet<string>? MachineIds = null);

