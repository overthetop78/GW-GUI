namespace GWGUI.Emulation.Amiga;

public sealed record AmigaMediaConfiguration(
    string Path,
    AmigaMediaCategory Category,
    string? Label = null,
    bool IsReadOnly = false);
