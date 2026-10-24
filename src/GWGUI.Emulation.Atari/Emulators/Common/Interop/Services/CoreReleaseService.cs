using CommonCoreHostFunctions = GWGUI.Emulation.Atari.Common.Functions.CoreHostFunctions;
using System.Globalization;
using EmulatorCatalogFunctions = GWGUI.Emulation.Atari.Common.Functions.EmulatorCatalogFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Atari.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using System.IO;
namespace GWGUI.Emulation.Atari.Emulators.Common.Interop.Services;

internal sealed class CoreReleaseService : ICoreReleaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _installationRoot;

    public CoreReleaseService(HttpClient httpClient, string installationRoot)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
        _installationRoot = Path.GetFullPath(installationRoot);
    }

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        var entry = CoreCatalog.Get(emulator);
        return await CoreReleaseFunctions.Releases.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, entry.ArchiveUri, response =>
        {
            return[CoreReleaseFunctions.Releases.ParseRelease(entry, response, CoreReleaseConstants.ReleaseIdPrefix,
                CoreReleaseConstants.ReleaseVersionFormat, static () => ErrorMessages.MissingPublishedDate)];
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<CoreInstallationPaths> InstallAsync(CoreRelease release, IProgress<CoreInstallProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(release);
        var entry = CoreCatalog.Get(release.Emulator);
        var paths = CoreCatalog.GetInstallationPaths(release.Emulator, _installationRoot, release.DeclaredVersion);
        Directory.CreateDirectory(paths.VersionDirectory);
        var download = paths.LibraryPath + CoreReleaseConstants.TemporaryDownloadExtension;
        var extracted = paths.LibraryPath + CoreReleaseConstants.TemporaryExtractExtension;
        try
        {
            await CoreReleaseFunctions.Releases.InstallLibraryAsync(() => CoreReleaseFunctions.Releases.DownloadAsync(_httpClient, release.DownloadUri, download, progress is null ? null : (written, total) => progress.Report(new CoreInstallProgress(written, total)), cancellationToken), () => CoreReleaseFunctions.Releases.ExtractExpectedLibrary(download, entry.DllName, extracted, name => string.Format(CultureInfo.InvariantCulture, ErrorMessages.MissingExpectedLibraryFormat, name)), archiveSize =>
            {
                var manifest = EmulatorCatalogFunctions.Diagnostics.CreateManifest(release.Id, release.DeclaredVersion, release.DownloadUri.AbsoluteUri,
                    archiveSize, extracted);
                return manifest;
            }, () => CoreReleaseFunctions.Releases.ReplaceLibraryAtomically(extracted, paths.LibraryPath, path => string.Format(CultureInfo.InvariantCulture, ErrorMessages.InstalledLibraryLockedFormat, path)), manifest => CoreReleaseFunctions.Releases.WriteJsonAtomicallyAsync(paths.ManifestPath, manifest, cancellationToken), async () =>
            {
                var activeManifestPath = CoreCatalog.GetActiveManifestPath(release.Emulator, _installationRoot);
                await CoreReleaseFunctions.Releases.WriteJsonAtomicallyAsync(activeManifestPath, new CoreActiveInstallation(release.Id, release.DeclaredVersion), cancellationToken).ConfigureAwait(false);
            }, archiveSize => progress?.Report(new CoreInstallProgress(archiveSize, archiveSize))).ConfigureAwait(false);
            return paths;
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
            CoreCatalog.GetActiveManifestPath(emulator, _installationRoot),
            version => CoreCatalog.GetInstallationPaths(emulator, _installationRoot, version), cancellationToken).ConfigureAwait(false);
    }
}
