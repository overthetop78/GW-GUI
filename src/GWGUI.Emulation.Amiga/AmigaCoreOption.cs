namespace GWGUI.Emulation.Amiga;

public sealed record AmigaCoreOption(string Key, string Name, string? Description, string? Category,
    string DefaultValue, IReadOnlyList<AmigaCoreOptionValue> Values, bool IsVisible = true);
