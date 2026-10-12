using GWGUI.Emulation.Interop;
namespace GWGUI.Emulation.Apple.Common.Contracts;

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
    string? ExplanationResourceKey = null
);

public sealed record CoreOptionValue(string Value, string Label, string? ResourceKey = null, string? InvariantDisplayValue = null);

internal sealed record HostError(
    string Type,
    string Message,
    ErrorCategory? Category,
    ErrorCode? Code,
    IReadOnlyDictionary<string, string> Context,
    bool IsLocalized);
