namespace GWGUI.Emulation.Acorn.Emulators.Common.Contracts;

internal sealed record DiskImageStatus(int Index, string? Path, string? Label);

internal sealed record DiskStatus(int ImageCount, int CurrentIndex, bool IsEjected, IReadOnlyList<DiskImageStatus> Images);

internal sealed record SessionMedia(MediaConfiguration Configuration, string RuntimePath, IReadOnlyList<string> SourcePaths, IReadOnlyList<string> RuntimePaths, bool RequiresExplicitSave);

