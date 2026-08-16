namespace GWGUI.Emulation.Atari;

internal sealed record AtariHatariContent(
    AtariMediaConfiguration Configuration,
    string RuntimePath,
    AtariSessionMedia? SessionMedia,
    AtariHatariStorage? Storage);
