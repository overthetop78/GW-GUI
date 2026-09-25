namespace GWGUI.Emulation.Atari.Emulators.Atari800.Contracts;


internal sealed record Atari800PreparedMedia(
    AtariMediaConfiguration Configuration,
    Atari800ContentType ContentType,
    string RuntimePath,
    AtariSessionMedia? SessionMedia);
