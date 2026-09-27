namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Contracts;

public sealed record FloppyConfiguration(string Path, string? Label = null, bool IsReadOnly = false);

public sealed record MediaConfiguration(
    string Path,
    MediaCategory Category,
    string? Label = null,
    bool IsReadOnly = false);
