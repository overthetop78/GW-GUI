namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;

internal sealed record CoreDefinition(string Id, string LibraryName, string LibraryFileName,
    Func<IReadOnlyList<MediaConfiguration>, string, string, string>? PrepareContent = null,
    IReadOnlyDictionary<int, uint>? Subsystems = null);
