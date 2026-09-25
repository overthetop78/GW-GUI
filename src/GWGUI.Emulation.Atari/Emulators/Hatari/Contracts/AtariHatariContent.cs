namespace GWGUI.Emulation.Atari.Emulators.Hatari.Contracts;

internal sealed record AtariHatariContent(
    AtariMediaConfiguration Configuration,
    string RuntimePath,
    AtariSessionMedia? SessionMedia,
    AtariHatariStorage? Storage,
    AtariSessionMedia? BootFloppy = null);
