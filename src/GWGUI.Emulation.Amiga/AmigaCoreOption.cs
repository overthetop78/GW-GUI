namespace GWGUI.Emulation.Amiga;

public sealed record AmigaCoreOptionValue(string Value, string Label);

public sealed record AmigaCoreOption(string Key, string Name, string? Description, string? Category,
    string DefaultValue, IReadOnlyList<AmigaCoreOptionValue> Values);
