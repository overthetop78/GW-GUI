namespace GWGUI.Emulation.Sinclair.Common.Contracts;
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




internal sealed record HostError( string Type, string Message, ErrorCategory? Category, ErrorCode? Code, IReadOnlyDictionary<string, string> Context, bool IsLocalized);
