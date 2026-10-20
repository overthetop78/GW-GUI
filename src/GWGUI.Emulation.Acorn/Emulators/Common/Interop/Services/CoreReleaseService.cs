using GWGUI.Emulation.Acorn.Emulators.Common.Exceptions;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Dictionaries;
using GWGUI.Emulation.Acorn.Emulators.Common.Interfaces;
using GWGUI.Emulation.Acorn.Emulators.Common.Functions;
using GWGUI.Emulation.Acorn.Emulators.Common.Contracts;
using GWGUI.Emulation.Acorn.Emulators.Common.Constants;
using System.IO;
namespace GWGUI.Emulation.Acorn.Emulators.Common.Interop.Services;

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

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        var entry = CoreCatalog.Get(emulator);
        return await EmulationCoreReleaseFunctions.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, entry.ArchiveUri, response =>
        {
            return[EmulationCoreReleaseFunctions.ParseRelease(entry, response, CoreReleaseConstants.ReleaseIdPrefix,
                CoreReleaseConstants.ReleaseVersionFormat, static () => CoreReleaseErrors.MissingPublishedDate)];
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
            await EmulationCoreReleaseFunctions.InstallLibraryAsync(() => CoreReleaseFunctions.DownloadAsync(_httpClient, release.DownloadUri, download, progress, cancellationToken), () => CoreReleaseFunctions.ExtractExpectedLibrary(download, entry.DllName, extracted), archiveSize =>
            {
                var manifest = EmulationCoreDiagnosticFunctions.CreateManifest(release.Id, release.DeclaredVersion, release.DownloadUri.AbsoluteUri,
                    archiveSize, extracted);
                return manifest;
            }, () => CoreReleaseFunctions.ReplaceLibraryAtomically(extracted, paths.LibraryPath), manifest => CoreReleaseFunctions.WriteManifestAtomicallyAsync(paths.ManifestPath, manifest, cancellationToken), async () =>
            {
                var activeManifestPath = CoreCatalog.GetActiveManifestPath(release.Emulator, _installationRoot);
                await CoreReleaseFunctions.WriteActiveInstallationAtomicallyAsync(activeManifestPath, new CoreActiveInstallation(release.Id, release.DeclaredVersion), cancellationToken).ConfigureAwait(false);
            }, archiveSize => progress?.Report(new CoreInstallProgress(archiveSize, archiveSize))).ConfigureAwait(false);
            return paths;
        }
        finally
        {
            CoreReleaseFunctions.DeleteIfExists(download);
            CoreReleaseFunctions.DeleteIfExists(extracted);
        }
    }

    public async Task<CoreInstallationPaths?> GetActiveInstallationAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        return await EmulationCoreReleaseFunctions.GetActiveInstallationAsync(
            CoreCatalog.GetActiveManifestPath(emulator, _installationRoot),
            version => CoreCatalog.GetInstallationPaths(emulator, _installationRoot, version), cancellationToken).ConfigureAwait(false);
    }
}

