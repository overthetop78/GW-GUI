namespace GWGUI.Emulation.Amiga.Contracts;

public sealed record AmigaFloppyConfiguration(string Path, string? Label = null, bool IsReadOnly = false);
