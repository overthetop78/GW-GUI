namespace GWGUI.Emulation.Panasonic.Common.Contracts;

public sealed record CoreOption(string Key, string Name, string? Description, string? Category, string DefaultValue, string CurrentValue, IReadOnlyList<CoreOptionValue> Values, bool IsVisible = true, string? CategorizedName = null, string? CategorizedDescription = null, bool RequiresRestart = false, bool IsEnabled = true);
public sealed record CoreOptionValue(string Value, string Label);
