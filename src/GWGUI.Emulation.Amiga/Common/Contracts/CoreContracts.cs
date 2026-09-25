namespace GWGUI.Emulation.Amiga.Common.Contracts;

public sealed record CoreOption(string Key, string Name, string? Description, string? Category,
    string DefaultValue, IReadOnlyList<CoreOptionValue> Values, bool IsVisible = true);

public sealed record CoreOptionValue(string Value, string Label);
