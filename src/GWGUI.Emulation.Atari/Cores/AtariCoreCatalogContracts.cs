namespace GWGUI.Emulation.Atari.Cores;

public sealed record AtariCoreCatalogEntry(
    AtariCoreKind Kind,
    string Id,
    string LibraryName,
    string DllName,
    string ArchiveName,
    Uri ArchiveUri,
    Uri SourceUri,
    string InspectedRevision,
    IReadOnlySet<AtariMachineModel> Models);

public sealed record AtariCoreDiagnosticManifest(
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

public sealed record AtariCoreInstallationPaths(
    string VersionDirectory,
    string LibraryPath,
    string ManifestPath);

public sealed record AtariCoreActiveInstallation(string ReleaseId, string ReleaseVersion);
