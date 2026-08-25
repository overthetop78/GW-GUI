namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariPreparedJaguarCd(
    AtariMediaConfiguration Configuration,
    string RuntimePath,
    bool NeedsFullPath);
