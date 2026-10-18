namespace GWGUI.Emulation.Snk.Common.Contracts;
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
    bool RequiresRestart = false,
    bool IsEnabled = true,
    string? ResourceKey = null,
    string? ExplanationResourceKey = null
);
public sealed record CoreOptionValue(string Value, string Label, string? ResourceKey = null, FrameSkipRatio? FrameSkipRatio = null);



internal sealed record HostError( string Type, string Message, ErrorCategory? Category, ErrorCode? Code, IReadOnlyDictionary<string, string> Context, bool IsLocalized);
