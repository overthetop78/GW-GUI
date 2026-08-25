namespace GWGUI.Emulation.Atari.Contracts;


internal sealed record Atari800PreparedMedia(
    AtariMediaConfiguration Configuration,
    Atari800ContentType ContentType,
    string RuntimePath,
    AtariSessionMedia? SessionMedia);
