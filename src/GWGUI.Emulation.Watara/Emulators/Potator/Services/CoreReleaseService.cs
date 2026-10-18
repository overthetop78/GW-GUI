using GWGUI.Emulation.Watara.Emulators.Potator.Constants;
using GWGUI.Emulation.Watara.Emulators.Potator.Contracts;
using GWGUI.Emulation.Watara.Emulators.Potator.Dictionaries;
using GWGUI.Emulation.Watara.Emulators.Potator.Exceptions;
using GWGUI.Emulation.Watara.Emulators.Potator.Functions;
using GWGUI.Emulation.Watara.Emulators.Potator.Interop.Functions;
using System.Net.Http;
using System.IO;

namespace GWGUI.Emulation.Watara.Emulators.Potator.Services;
internal sealed class CoreReleaseService
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
            return[CoreReleaseFunctions.ParseRelease(entry, response)];
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
                var manifest = new CoreDiagnosticManifest(release.Id, release.DeclaredVersion, release.DownloadUri.AbsoluteUri, DateTimeOffset.UtcNow, archiveSize, new FileInfo(extracted).Length, CoreDiagnosticFunctions.CalculateSha256(extracted), CoreDiagnosticFunctions.ReadArchitecture(extracted), CoreDiagnosticFunctions.ReadDeclaredVersion(extracted), CoreDiagnosticFunctions.ReadExports(extracted));
                if (!string.Equals(manifest.Architecture, EmulatorCatalogConstants.WindowsX64Architecture, StringComparison.Ordinal))
                    throw new InvalidDataException(CoreReleaseErrors.InvalidArchitecture);
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
            try
            {
                CoreReleaseFunctions.DeleteIfExists(download);
            }
            finally
            {
                CoreReleaseFunctions.DeleteIfExists(extracted);
            }
        }
    }

    public async Task<CoreInstallationPaths?> GetActiveInstallationAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        var marker = await CoreReleaseFunctions.ReadJsonAsync<CoreActiveInstallation>(CoreCatalog.GetActiveManifestPath(emulator, _installationRoot), cancellationToken).ConfigureAwait(false);
        if (marker is null)
            return null;
        var paths = CoreCatalog.GetInstallationPaths(emulator, _installationRoot, marker.ReleaseVersion);
        return File.Exists(paths.LibraryPath) && File.Exists(paths.ManifestPath) ? paths : null;
    }
}