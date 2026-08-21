namespace GWGUI.Emulation;

public sealed record FloppyDriveModelChoice(
    string Value,
    string DisplayResourceKey,
    string? InvariantDisplayValue = null,
    long BlankImageSize = 0);
