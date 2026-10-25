namespace GWGUI.Emulation.Acorn.Emulators.Common.Contracts;





internal sealed record SessionMedia(MediaConfiguration Configuration, string RuntimePath, IReadOnlyList<string> SourcePaths, IReadOnlyList<string> RuntimePaths, bool RequiresExplicitSave);
