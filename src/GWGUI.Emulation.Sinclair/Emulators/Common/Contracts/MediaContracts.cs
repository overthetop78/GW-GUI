namespace GWGUI.Emulation.Sinclair.Emulators.Common.Contracts;

internal sealed record DiskImageStatus(int Index, string? Path, string? Label);

internal sealed record DiskStatus( int ImageCount, int CurrentIndex, bool IsEjected, IReadOnlyList<DiskImageStatus> Images);

internal sealed record PreparedContent( MediaConfiguration Configuration, Emulator Core, string RuntimePath, bool NeedsFullPath);

internal sealed record SessionMedia( MediaConfiguration Configuration, string RuntimePath, IReadOnlyList<string> SourcePaths, IReadOnlyList<string> RuntimePaths, bool RequiresExplicitSave);
