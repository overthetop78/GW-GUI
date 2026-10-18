using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Contracts;

public sealed record EmulationMachineOption(
    string Key,
    string Name,
    string? Description,
    string? Category,
    string DefaultValue,
    string CurrentValue,
    IReadOnlyList<EmulationMachineOptionValue> Values,
    bool IsVisible = true,
    string? CategorizedName = null,
    string? CategorizedDescription = null,
    EmulationMachineTab Tab = EmulationMachineTab.General,
    bool RequiresRestart = false,
    IReadOnlySet<string>? MachineIds = null);
