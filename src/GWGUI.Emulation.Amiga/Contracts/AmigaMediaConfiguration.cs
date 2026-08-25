namespace GWGUI.Emulation.Amiga.Contracts;

public sealed record AmigaMediaConfiguration(
    string Path,
    AmigaMediaCategory Category,
    string? Label = null,
    bool IsReadOnly = false);
