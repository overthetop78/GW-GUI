namespace GWGUI.Emulation.Contracts;

public sealed record EmulationCoreDiagnosticManifest(
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
