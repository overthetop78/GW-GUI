namespace GWGUI.Emulation.Atari.Emulators.Common.Contracts;

public sealed record CoreActiveInstallation(string ReleaseId, string ReleaseVersion);

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

public sealed record CoreDiagnosticManifest(
    string ReleaseId,
    string ReleaseVersion,
    string DownloadUrl,
    DateTimeOffset DownloadedUtc,
    long ArchiveSize,
    long LibrarySize,
    string LibrarySha256,
    string Architecture,
    string DeclaredVersion,
    IReadOnlyList<string> Exports);

public sealed record CoreInstallationPaths(
    string VersionDirectory,
    string LibraryPath,
    string ManifestPath);

public sealed record CoreInstallProgress(long DownloadedBytes, long? TotalBytes)
{
    public double? Fraction => TotalBytes is > 0 ? DownloadedBytes / (double)TotalBytes.Value : null;
}

public sealed record CoreOptionCategory(string Key, string Name, string? Description);

public sealed record CoreRelease(
    Emulator Emulator,
    string Id,
    string DeclaredVersion,
    Uri DownloadUri,
    DateTimeOffset PublishedUtc,
    long? ExpectedArchiveSize);

internal sealed record HostError(
    string Type,
    string Message,
    ErrorCategory? Category,
    ErrorCode? Code,
    IReadOnlyDictionary<string, string> Context,
    bool IsLocalized);
