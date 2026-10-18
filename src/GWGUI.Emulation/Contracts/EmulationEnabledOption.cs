namespace GWGUI.Emulation.Contracts;

public sealed record EmulationEnabledOption(string Key, string Name, string? Description, string? Category, string DefaultValue, string CurrentValue, IReadOnlyList<EmulationOptionValue> Values, bool IsVisible = true, string? CategorizedName = null, string? CategorizedDescription = null, bool RequiresRestart = false, bool IsEnabled = true);
