namespace GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Contracts;

internal sealed record AtariPreparedJaguarCd(
    AtariMediaConfiguration Configuration,
    string RuntimePath,
    bool NeedsFullPath,
    IReadOnlySet<string> ActivityPaths);
