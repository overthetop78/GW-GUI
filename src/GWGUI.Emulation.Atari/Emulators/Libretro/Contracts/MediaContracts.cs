namespace GWGUI.Emulation.Atari.Emulators.Libretro.Contracts;

internal sealed record DiskImageStatus(int Index, string? Path, string? Label);

internal sealed record DiskStatus(
    int ImageCount,
    int CurrentIndex,
    bool IsEjected,
    IReadOnlyList<DiskImageStatus> Images);

internal sealed record PreparedCartridge(
    MediaConfiguration Configuration,
    Emulator Core,
    string RuntimePath,
    bool NeedsFullPath);

internal sealed record SessionMedia(
    MediaConfiguration Configuration,
    string RuntimePath,
    IReadOnlyList<string> SourcePaths,
    IReadOnlyList<string> RuntimePaths,
    bool RequiresExplicitSave);
