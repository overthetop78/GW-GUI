namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariExternalCoreInfo(
    AtariEmulator Emulator,
    string LibraryName,
    string LibraryVersion,
    IReadOnlySet<string> Extensions,
    bool NeedsFullPath,
    bool BlocksArchiveExtraction);
