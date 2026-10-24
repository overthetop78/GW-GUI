using EmulatorCatalogFunctions = GWGUI.Emulation.Amstrad.Common.Functions.EmulatorCatalogFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Amstrad.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Amstrad.Common.Exceptions;

using System.IO;
using System.Text.Json;

namespace GWGUI.Emulation.Amstrad.Common.Services;

internal sealed class CoreReleaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _directory;
    private readonly string _libraryFile;
    private readonly Uri _downloadUri;
    private readonly string _latestReleaseDisplayName;
    private readonly string _releaseProviderSuffix;
    private readonly string _architecture;
    private readonly Action<string>? _verifyInstallation;

    internal CoreReleaseService(HttpClient httpClient, string directory, string libraryFile, Uri downloadUri, string latestReleaseDisplayName, string releaseProviderSuffix, string architecture, Action<string>? verifyInstallation)
    {
        _libraryFile = libraryFile;
        _downloadUri = downloadUri;
        _latestReleaseDisplayName = latestReleaseDisplayName;
        _releaseProviderSuffix = releaseProviderSuffix;
        _architecture = architecture;
        _verifyInstallation = verifyInstallation;
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
    }

    public string RequiredLibraryPath => Path.Combine(_directory, _libraryFile);

    public string? GetInstalledVersion()
    {
        if (!File.Exists(RequiredLibraryPath)) return null;
        var manifestPath = Path.Combine(_directory, CoreReleaseConstants.CoreJson);
        return CoreReleaseFunctions.Releases.ReadInstalledVersion(manifestPath, CoreReleaseConstants.Version, CoreReleaseConstants.Unknown);
    }

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        return await CoreReleaseFunctions.Releases.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, _downloadUri, response =>
        {
            var published = response.Content.Headers.LastModified ?? response.Headers.Date;
            var suffix = published?.UtcDateTime.ToString(CoreReleaseConstants.ReleaseIdentifierDateFormat) ?? CoreReleaseConstants.Latest;
            var id = $"{CoreReleaseConstants.OfficialReleaseIdPrefix}{suffix}";
            return[new CoreRelease(emulator, id, id, _downloadUri, published, response.Content.Headers.ContentLength) { DisplayName = published is null ? _latestReleaseDisplayName : $"{published.Value.LocalDateTime.ToString(CoreReleaseConstants.ReleaseDisplayDateFormat)}{_releaseProviderSuffix}", IsRequired = true, IsZip = true }];
        }, cancellationToken).ConfigureAwait(false);
    }


    public async Task<CoreInstallationPaths> InstallAsync(CoreRelease release, IProgress<CoreInstallProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(release);
        Directory.CreateDirectory(_directory);
        var destination = RequiredLibraryPath;
        var download = destination + CoreReleaseConstants.Download;
        var extracted = destination + CoreReleaseConstants.Extract;
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
            return new CoreInstallationPaths(_directory, destination, Path.Combine(_directory, CoreReleaseConstants.CoreJson));
        }
        finally
        {
            if (File.Exists(download)) File.Delete(download);
            if (File.Exists(extracted)) File.Delete(extracted);
        }
    }
}
