namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariPreparedCartridge(
    AtariMediaConfiguration Configuration,
    AtariEmulator Core,
    string RuntimePath,
    bool NeedsFullPath);
