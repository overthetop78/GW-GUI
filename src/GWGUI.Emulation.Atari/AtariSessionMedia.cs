namespace GWGUI.Emulation.Atari;

internal sealed record AtariSessionMedia(
    AtariMediaConfiguration Configuration,
    string RuntimePath,
    IReadOnlyList<string> SourcePaths,
    IReadOnlyList<string> RuntimePaths,
    bool RequiresExplicitSave);
