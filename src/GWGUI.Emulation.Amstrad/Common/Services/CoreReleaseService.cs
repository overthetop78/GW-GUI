using EmulatorCatalogFunctions = GWGUI.Emulation.Amstrad.Common.Functions.EmulatorCatalogFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Amstrad.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Amstrad.Common.Exceptions;

using System.IO;
using System.Text.Json;

namespace GWGUI.Emulation.Amstrad.Common.Services;

internal sealed class CoreReleaseService : ICoreReleaseService
{
    /// <summary>Identité du profil dont ce service gère le dossier et les métadonnées.</summary>
    private readonly Emulator _emulator;
    private readonly HttpClient _httpClient;
    private readonly string _directory;
    private readonly string _libraryFile;
    private readonly Uri _downloadUri;
    private readonly string _latestReleaseDisplayName;
    private readonly string _releaseProviderSuffix;
    private readonly string _architecture;
    private readonly Action<string>? _verifyInstallation;

    internal CoreReleaseService(HttpClient httpClient, string directory, Emulator emulator, string libraryFile, Uri downloadUri, string latestReleaseDisplayName, string releaseProviderSuffix, string architecture, Action<string>? verifyInstallation)
    {
        if (!Enum.IsDefined(emulator)) throw new ArgumentOutOfRangeException(nameof(emulator));
        _emulator = emulator;
        _libraryFile = libraryFile;
        _downloadUri = downloadUri;
        _latestReleaseDisplayName = latestReleaseDisplayName;
        _releaseProviderSuffix = releaseProviderSuffix;
        _architecture = architecture;
        _verifyInstallation = verifyInstallation;
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
    }

    /// <summary>Refuse une demande destinée à un autre profil avant tout accès à son installation.</summary>
    private void ValidateEmulator(Emulator emulator)
    {
        if (emulator != _emulator) throw new ArgumentOutOfRangeException(nameof(emulator));
    }
    private string RequiredLibraryPath => Path.Combine(_directory, _libraryFile);

    /// <summary>Résout les chemins et la version de l'installation présente sans charger le cœur.</summary>
    public Task<CoreInstallationPaths?> GetActiveInstallationAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateEmulator(emulator);
        if (!File.Exists(RequiredLibraryPath)) return Task.FromResult<CoreInstallationPaths?>(null);
        var manifestPath = Path.Combine(_directory, EmulatorCatalogConstants.ManifestFileName);
        var version = CoreReleaseFunctions.Releases.ReadInstalledVersion(manifestPath, EmulatorCatalogConstants.VersionPropertyName, CoreReleaseConstants.UnknownVersion);
        return Task.FromResult<CoreInstallationPaths?>(new(_directory, RequiredLibraryPath, manifestPath) { InstalledVersion = version });
    }

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateEmulator(emulator);
        return await CoreReleaseFunctions.Releases.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, _downloadUri, response =>
        {
            var published = response.Content.Headers.LastModified ?? response.Headers.Date;
            var suffix = published?.UtcDateTime.ToString(CoreReleaseConstants.ReleaseVersionFormat) ?? CoreReleaseConstants.Latest;
            var id = $"{CoreReleaseConstants.ReleaseIdPrefix}{suffix}";
            return[new CoreRelease(emulator, id, id, _downloadUri, published, response.Content.Headers.ContentLength) { DisplayName = published is null ? _latestReleaseDisplayName : $"{published.Value.LocalDateTime.ToString(CoreReleaseConstants.ReleaseDisplayDateFormat)}{_releaseProviderSuffix}", IsRequired = true, IsZip = true }];
        }, cancellationToken).ConfigureAwait(false);
    }


    public async Task<CoreInstallationPaths> InstallAsync(CoreRelease release, IProgress<CoreInstallProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(release);
        ValidateEmulator(release.Emulator);
        Directory.CreateDirectory(_directory);
        var destination = RequiredLibraryPath;
        var download = destination + CoreReleaseConstants.TemporaryDownloadExtension;
        var extracted = destination + CoreReleaseConstants.TemporaryExtractExtension;
        try
        {
            await CoreReleaseFunctions.Releases.InstallLibraryAsync(
                () => CoreReleaseFunctions.Releases.DownloadAsync(_httpClient, release.DownloadUri, download,
                    (written, total) =>
                    {
                        progress?.Report(new CoreInstallProgress(written, total));
                    }, cancellationToken),
                () =>
                {
                    if (release.IsZip)
                    {
                        CoreReleaseFunctions.Releases.ExtractExpectedLibrary(download, _libraryFile, extracted,
                            static _ => ErrorMessages.ArchiveMissingLibrary(), preserveLastWriteTime: true);
                    }
                    else File.Copy(download, extracted, true);
                },
                archiveSize =>
                {
                    _verifyInstallation?.Invoke(extracted);
                    return EmulatorCatalogFunctions.Diagnostics.CalculateSha256(extracted);
                },
                () => File.Move(extracted, destination, true),
                sha256 => CoreReleaseFunctions.WriteManifestAsync(release.Id, release.DownloadUri.AbsoluteUri, destination, sha256, _architecture, cancellationToken),
                () => Task.CompletedTask,
                archiveSize => progress?.Report(new CoreInstallProgress(archiveSize, archiveSize))).ConfigureAwait(false);
            return new CoreInstallationPaths(_directory, destination, Path.Combine(_directory, EmulatorCatalogConstants.ManifestFileName)) { InstalledVersion = release.DeclaredVersion };
        }
        finally
        {
            if (File.Exists(download)) File.Delete(download);
            if (File.Exists(extracted)) File.Delete(extracted);
        }
    }
}
