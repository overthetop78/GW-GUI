namespace GWGUI.Emulation.Atari;

internal sealed record AtariPreparedCartridge(
    AtariMediaConfiguration Configuration,
    AtariEmulator Core,
    string RuntimePath,
    bool NeedsFullPath);
