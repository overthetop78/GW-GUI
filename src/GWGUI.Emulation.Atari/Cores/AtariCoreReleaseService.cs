namespace GWGUI.Emulation.Atari.Cores;

public sealed class AtariCoreReleaseService : IAtariCoreReleaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _installationRoot;

    public AtariCoreReleaseService(HttpClient httpClient, string installationRoot)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        _installationRoot = Path.GetFullPath(installationRoot);
    }

    public async Task<IReadOnlyList<AtariCoreRelease>> GetAvailableAsync(AtariCoreKind kind,
        CancellationToken cancellationToken = default)
    {
        var entry = AtariCoreCatalog.Get(kind);
        using var request = new HttpRequestMessage(HttpMethod.Head, entry.ArchiveUri);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return [AtariCoreReleaseFunctions.ParseRelease(entry, response)];
    }

    public async Task<AtariCoreInstallationPaths> InstallAsync(AtariCoreRelease release,
        IProgress<AtariCoreInstallProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(release);
        var entry = AtariCoreCatalog.Get(release.Kind);
        var paths = AtariCoreCatalog.GetInstallationPaths(release.Kind, _installationRoot,
            release.DeclaredVersion);
        Directory.CreateDirectory(paths.VersionDirectory);
        var download = paths.LibraryPath + AtariCoreReleaseConstants.TemporaryDownloadExtension;
        var extracted = paths.LibraryPath + AtariCoreReleaseConstants.TemporaryExtractExtension;
        try
        {
            var archiveSize = await AtariCoreReleaseFunctions.DownloadAsync(_httpClient, release.DownloadUri,
                download, progress, cancellationToken).ConfigureAwait(false);
            AtariCoreReleaseFunctions.ExtractExpectedLibrary(download, entry.DllName, extracted);
            var manifest = new AtariCoreDiagnosticManifest(release.Id, release.DeclaredVersion,
                release.DownloadUri.AbsoluteUri,
                DateTimeOffset.UtcNow, archiveSize, new FileInfo(extracted).Length,
                AtariCoreDiagnosticFunctions.CalculateSha256(extracted),
                AtariCoreDiagnosticFunctions.ReadArchitecture(extracted),
                AtariCoreDiagnosticFunctions.ReadDeclaredVersion(extracted),
                AtariCoreDiagnosticFunctions.ReadExports(extracted));
            AtariCoreReleaseFunctions.ReplaceLibraryAtomically(extracted, paths.LibraryPath);
            await AtariCoreReleaseFunctions.WriteManifestAtomicallyAsync(paths.ManifestPath, manifest,
                cancellationToken).ConfigureAwait(false);
            var activeManifestPath = AtariCoreCatalog.GetActiveManifestPath(release.Kind, _installationRoot);
            await AtariCoreReleaseFunctions.WriteActiveInstallationAtomicallyAsync(activeManifestPath,
                new AtariCoreActiveInstallation(release.Id, release.DeclaredVersion), cancellationToken)
                .ConfigureAwait(false);
            progress?.Report(new AtariCoreInstallProgress(archiveSize, archiveSize));
            return paths;
        }
        finally
        {
            AtariCoreReleaseFunctions.DeleteIfExists(download);
            AtariCoreReleaseFunctions.DeleteIfExists(extracted);
        }
    }

    public async Task<AtariCoreInstallationPaths?> GetActiveInstallationAsync(AtariCoreKind kind,
        CancellationToken cancellationToken = default)
    {
        var marker = await AtariCoreReleaseFunctions.ReadJsonAsync<AtariCoreActiveInstallation>(
            AtariCoreCatalog.GetActiveManifestPath(kind, _installationRoot), cancellationToken)
            .ConfigureAwait(false);
        if (marker is null) return null;
        var paths = AtariCoreCatalog.GetInstallationPaths(kind, _installationRoot, marker.ReleaseVersion);
        return File.Exists(paths.LibraryPath) && File.Exists(paths.ManifestPath) ? paths : null;
    }
}
