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
        var marker = await CoreReleaseFunctions.ReadJsonAsync<CoreActiveInstallation>(CoreCatalog.GetActiveManifestPath(emulator, _installationRoot), cancellationToken).ConfigureAwait(false);
        if (marker is null) return null;
        var paths = CoreCatalog.GetInstallationPaths(emulator, _installationRoot, marker.ReleaseVersion);
        return File.Exists(paths.LibraryPath) && File.Exists(paths.ManifestPath) ? paths : null;
    }
}

