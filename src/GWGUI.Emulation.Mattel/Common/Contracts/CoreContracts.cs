namespace GWGUI.Emulation.Mattel.Common.Contracts;

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
    string? ResourceKey = null,
    string? ExplanationResourceKey = null,
    bool RequiresRuntimeReload = false);

public sealed record CoreOptionValue(string Value, string Label, string? ResourceKey = null, EmulationVideoStandard? VideoStandard = null, double? FrequencyHz = null, string? InvariantDisplayValue = null);

public sealed record CoreOptionCategory(string Key, string Name, string? Description);

internal sealed record HostError(string Type, string Message, ErrorCategory? Category, ErrorCode? Code, IReadOnlyDictionary<string, string> Context, bool IsLocalized);
