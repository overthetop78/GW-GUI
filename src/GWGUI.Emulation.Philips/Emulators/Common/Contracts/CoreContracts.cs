namespace GWGUI.Emulation.Philips.Emulators.Common.Contracts;

public sealed record EmulatorCatalogEntry(
    Emulator Emulator,
    string Id,
    string LibraryName,
    string DllName,
    string ArchiveName,
    Uri ArchiveUri,
    Uri SourceUri,
    string InspectedRevision,
    IReadOnlySet<MachineModel> Models,
    string? DescriptionResourceKey = null);
