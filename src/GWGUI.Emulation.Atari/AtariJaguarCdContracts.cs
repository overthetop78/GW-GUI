namespace GWGUI.Emulation.Atari;

internal sealed record AtariPreparedJaguarCd(
    AtariMediaConfiguration Configuration,
    string RuntimePath,
    bool NeedsFullPath);
