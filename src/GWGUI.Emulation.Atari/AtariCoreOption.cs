namespace GWGUI.Emulation.Atari;

public sealed record AtariCoreOptionValue(string Value, string Label);

public sealed record AtariCoreOptionCategory(string Key, string Name, string? Description);

public sealed record AtariCoreOption(
    string Key,
    string Name,
    string? Description,
    string? Category,
    string DefaultValue,
    string CurrentValue,
    IReadOnlyList<AtariCoreOptionValue> Values,
    bool IsVisible = true,
    string? CategorizedName = null,
    string? CategorizedDescription = null);
