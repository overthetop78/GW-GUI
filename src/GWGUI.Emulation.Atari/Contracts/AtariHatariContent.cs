namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariHatariContent(
    AtariMediaConfiguration Configuration,
    string RuntimePath,
    AtariSessionMedia? SessionMedia,
    AtariHatariStorage? Storage);
