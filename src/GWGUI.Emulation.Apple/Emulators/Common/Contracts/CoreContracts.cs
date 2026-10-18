namespace GWGUI.Emulation.Apple.Emulators.Common.Contracts;

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

public sealed record CoreOptionCategory(string Key, string Name, string? Description);

public sealed record CoreRelease(Emulator Emulator, string Id, string DeclaredVersion, Uri DownloadUri, DateTimeOffset PublishedUtc, long? ExpectedArchiveSize);
