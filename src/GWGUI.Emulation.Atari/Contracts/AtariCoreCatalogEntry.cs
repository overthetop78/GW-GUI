namespace GWGUI.Emulation.Atari.Contracts;

public sealed record AtariCoreCatalogEntry(
    AtariEmulator Emulator,
    string Id,
    string LibraryName,
    string DllName,
    string ArchiveName,
    Uri ArchiveUri,
    Uri SourceUri,
    string InspectedRevision,
    IReadOnlySet<AtariMachineModel> Models);
