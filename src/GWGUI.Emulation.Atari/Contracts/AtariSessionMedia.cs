namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariSessionMedia(
    AtariMediaConfiguration Configuration,
    string RuntimePath,
    IReadOnlyList<string> SourcePaths,
    IReadOnlyList<string> RuntimePaths,
    bool RequiresExplicitSave);
