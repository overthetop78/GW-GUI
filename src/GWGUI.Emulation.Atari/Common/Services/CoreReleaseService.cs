namespace GWGUI.Emulation.Atari.Common.Services;

public sealed class CoreReleaseService : ICoreReleaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _installationRoot;

    public CoreReleaseService(HttpClient httpClient, string installationRoot)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        _installationRoot = Path.GetFullPath(installationRoot);
    }

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(Emulator emulator,
        CancellationToken cancellationToken = default)
    {
        var entry = EmulatorCatalog.Get(emulator);
        using var request = new HttpRequestMessage(HttpMethod.Head, entry.ArchiveUri);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return [CoreReleaseFunctions.ParseRelease(entry, response)];
    }

    public async Task<CoreInstallationPaths> InstallAsync(CoreRelease release,
        IProgress<CoreInstallProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(release);
        var entry = EmulatorCatalog.Get(release.Emulator);
        var paths = EmulatorCatalog.GetInstallationPaths(release.Emulator, _installationRoot,
            release.DeclaredVersion);
        Directory.CreateDirectory(paths.VersionDirectory);
        var download = paths.LibraryPath + CoreReleaseConstants.TemporaryDownloadExtension;
        var extracted = paths.LibraryPath + CoreReleaseConstants.TemporaryExtractExtension;
        try
        {
            var archiveSize = await CoreReleaseFunctions.DownloadAsync(_httpClient, release.DownloadUri,
                download, progress, cancellationToken).ConfigureAwait(false);
            CoreReleaseFunctions.ExtractExpectedLibrary(download, entry.DllName, extracted);
            var manifest = new CoreDiagnosticManifest(release.Id, release.DeclaredVersion,
                release.DownloadUri.AbsoluteUri,
                DateTimeOffset.UtcNow, archiveSize, new FileInfo(extracted).Length,
                CoreDiagnosticFunctions.CalculateSha256(extracted),
                CoreDiagnosticFunctions.ReadArchitecture(extracted),
                CoreDiagnosticFunctions.ReadDeclaredVersion(extracted),
                CoreDiagnosticFunctions.ReadExports(extracted));
            CoreReleaseFunctions.ReplaceLibraryAtomically(extracted, paths.LibraryPath);
            await CoreReleaseFunctions.WriteManifestAtomicallyAsync(paths.ManifestPath, manifest,
                cancellationToken).ConfigureAwait(false);
            var activeManifestPath = EmulatorCatalog.GetActiveManifestPath(release.Emulator, _installationRoot);
            await CoreReleaseFunctions.WriteActiveInstallationAtomicallyAsync(activeManifestPath,
                new CoreActiveInstallation(release.Id, release.DeclaredVersion), cancellationToken)
                .ConfigureAwait(false);
            progress?.Report(new CoreInstallProgress(archiveSize, archiveSize));
            return paths;
        }
        finally
        {
            CoreReleaseFunctions.DeleteIfExists(download);
            CoreReleaseFunctions.DeleteIfExists(extracted);
        }
    }

    public async Task<CoreInstallationPaths?> GetActiveInstallationAsync(Emulator emulator,
        CancellationToken cancellationToken = default)
    {
        var marker = await CoreReleaseFunctions.ReadJsonAsync<CoreActiveInstallation>(
            EmulatorCatalog.GetActiveManifestPath(emulator, _installationRoot), cancellationToken)
            .ConfigureAwait(false);
        if (marker is null) return null;
        var paths = EmulatorCatalog.GetInstallationPaths(emulator, _installationRoot, marker.ReleaseVersion);
        return File.Exists(paths.LibraryPath) && File.Exists(paths.ManifestPath) ? paths : null;
    }
}
