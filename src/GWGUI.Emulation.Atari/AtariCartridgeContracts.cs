namespace GWGUI.Emulation.Atari;

internal sealed record AtariPreparedCartridge(
    AtariMediaConfiguration Configuration,
    AtariCoreKind Core,
    string RuntimePath,
    bool NeedsFullPath);
