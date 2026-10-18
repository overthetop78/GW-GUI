namespace GWGUI.Emulation.Contracts;

public sealed record EmulationResourceReloadOption(
    string Key,
    string Name,
    string? Description,
    string? Category,
    string DefaultValue,
    string CurrentValue,
    IReadOnlyList<EmulationResourceOptionValue> Values,
    bool IsVisible = true,
    string? CategorizedName = null,
    string? CategorizedDescription = null,
    string? ResourceKey = null,
    string? ExplanationResourceKey = null,
    bool RequiresRuntimeReload = false
);
