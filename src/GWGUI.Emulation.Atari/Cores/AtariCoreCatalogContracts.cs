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
    string DownloadUrl,
    DateTimeOffset DownloadedUtc,
    long ArchiveSize,
    long LibrarySize,
    string LibrarySha256,
    string Architecture,
    string DeclaredVersion);

public sealed record AtariCoreInstallationPaths(
    string VersionDirectory,
    string LibraryPath,
    string ManifestPath);
