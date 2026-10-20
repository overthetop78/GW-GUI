namespace GWGUI.Emulation.Atari.Emulators.Common.Contracts;

internal sealed record PreparedCartridge(MediaConfiguration Configuration, Emulator Core, string RuntimePath, bool NeedsFullPath);

internal sealed record SessionMedia(MediaConfiguration Configuration, string RuntimePath, IReadOnlyList<string> SourcePaths, IReadOnlyList<string> RuntimePaths, bool RequiresExplicitSave);
