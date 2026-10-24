using CommonCoreHostFunctions = GWGUI.Emulation.Epoch.Common.Functions.CoreHostFunctions;
using System.Globalization;
using GWGUI.Emulation;
using GWGUI.Emulation.Epoch.Common.Functions;
using System.Net.Http;
using System.IO;
namespace GWGUI.Emulation.Epoch.Common.Services;

internal sealed class CoreReleaseService : ICoreReleaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _installationRoot;
    /// <summary>Résout les métadonnées fournies par le catalogue du backend, sans en connaître le type concret.</summary>
    private readonly Func<Emulator, EmulatorCatalogEntry> _getEntry;

    public CoreReleaseService(HttpClient httpClient, string installationRoot, Func<Emulator, EmulatorCatalogEntry> getEntry)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        _getEntry = getEntry;
        _installationRoot = Path.GetFullPath(installationRoot);
    }

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        var entry = _getEntry(emulator);
        return await CoreReleaseFunctions.Releases.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, entry.ArchiveUri, response =>
        {
            return[CoreReleaseFunctions.Releases.ParseRelease(entry, response, CoreReleaseConstants.ReleaseIdPrefix,
                CoreReleaseConstants.ReleaseVersionFormat, static () => ErrorMessages.MissingPublishedDate)];
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<CoreInstallationPaths> InstallAsync(CoreRelease release, IProgress<CoreInstallProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(release);
        var entry = _getEntry(release.Emulator);
        var paths = EmulatorCatalogFunctions.GetInstallationPaths(entry, _installationRoot, release.DeclaredVersion);
        Directory.CreateDirectory(paths.VersionDirectory);
        var download = paths.LibraryPath + CoreReleaseConstants.TemporaryDownloadExtension;
        var extracted = paths.LibraryPath + CoreReleaseConstants.TemporaryExtractExtension;
        try
        {
            await CoreReleaseFunctions.Releases.InstallLibraryAsync(() => CoreReleaseFunctions.Releases.DownloadAsync(_httpClient, release.DownloadUri, download, progress is null ? null : (written, total) => progress.Report(new CoreInstallProgress(written, total)), cancellationToken), () => CoreReleaseFunctions.Releases.ExtractExpectedLibrary(download, entry.DllName, extracted, name => string.Format(CultureInfo.InvariantCulture, ErrorMessages.MissingExpectedLibraryFormat, name)), archiveSize =>
            {
                var manifest = EmulatorCatalogFunctions.Diagnostics.CreateManifest(release.Id, release.DeclaredVersion, release.DownloadUri.AbsoluteUri,
                    archiveSize, extracted);
                EmulatorCatalogFunctions.Diagnostics.RequireArchitecture(manifest.Architecture, EmulatorCatalogConstants.WindowsX64Architecture,
                    static () => ErrorMessages.InvalidArchitecture);
                return manifest;
            }, () => CoreReleaseFunctions.Releases.ReplaceLibraryAtomically(extracted, paths.LibraryPath, path => string.Format(CultureInfo.InvariantCulture, ErrorMessages.InstalledLibraryLockedFormat, path)), manifest => CoreReleaseFunctions.Releases.WriteJsonAtomicallyAsync(paths.ManifestPath, manifest, cancellationToken), async () =>
            {
                var activeManifestPath = EmulatorCatalogFunctions.GetActiveManifestPath(entry, _installationRoot);
                await CoreReleaseFunctions.Releases.WriteJsonAtomicallyAsync(activeManifestPath, new CoreActiveInstallation(release.Id, release.DeclaredVersion), cancellationToken).ConfigureAwait(false);
            }, archiveSize => progress?.Report(new CoreInstallProgress(archiveSize, archiveSize))).ConfigureAwait(false);
            return paths with { InstalledVersion = release.DeclaredVersion };
        }
        finally
        {
            CommonCoreHostFunctions.ConfigurationFiles.DeleteIfExists(download);
            CommonCoreHostFunctions.ConfigurationFiles.DeleteIfExists(extracted);
        }
    }

    public async Task<CoreInstallationPaths?> GetActiveInstallationAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        return await CoreReleaseFunctions.Releases.GetActiveInstallationAsync(
            EmulatorCatalogFunctions.GetActiveManifestPath(_getEntry(emulator), _installationRoot),
            version => EmulatorCatalogFunctions.GetInstallationPaths(_getEntry(emulator), _installationRoot, version), cancellationToken).ConfigureAwait(false);
    }
}
