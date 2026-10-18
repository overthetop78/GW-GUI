namespace GWGUI.Emulation.Atari.Emulators.Common.Contracts;

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



public sealed record CoreRelease(Emulator Emulator, string Id, string DeclaredVersion, Uri DownloadUri, DateTimeOffset PublishedUtc, long? ExpectedArchiveSize);

internal sealed record HostError(string Type, string Message, ErrorCategory? Category, ErrorCode? Code, IReadOnlyDictionary<string, string> Context, bool IsLocalized);
