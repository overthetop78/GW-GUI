using EmulatorCatalogFunctions = GWGUI.Emulation.Commodore.Common.Functions.EmulatorCatalogFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Commodore.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using System.IO;

using System.Text.Json;

namespace GWGUI.Emulation.Commodore.Common.Services;


internal sealed class CoreReleaseService : ICoreReleaseService
{
    private readonly string _libraryFile;
    private readonly string? _downloadUrl;
    private readonly CoreRelease? _requiredRelease;
    private Uri? LatestOfficialUri => _downloadUrl is { } url ? new(url) : null;
    private readonly HttpClient _httpClient;
    private readonly string _directory;

    internal CoreReleaseService(HttpClient httpClient, string directory, string libraryFile, string? downloadUrl, CoreRelease? requiredRelease)
    {
        _libraryFile = libraryFile;
        _downloadUrl = downloadUrl;
        _requiredRelease = requiredRelease;
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
    }

    private string RequiredLibraryPath => Path.Combine(_directory, _libraryFile);

    /// <summary>Résout les chemins et la version de l'installation présente sans charger le cœur.</summary>
    public Task<CoreInstallationPaths?> GetActiveInstallationAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!File.Exists(RequiredLibraryPath)) return Task.FromResult<CoreInstallationPaths?>(null);
        var manifestPath = Path.Combine(_directory, CoreReleaseConstants.CoreJson);
        var version = CoreReleaseFunctions.Releases.ReadInstalledVersion(manifestPath, CoreReleaseConstants.Version, CoreReleaseConstants.Unknown);
        return Task.FromResult<CoreInstallationPaths?>(new(_directory, RequiredLibraryPath, manifestPath) { InstalledVersion = version });
    }

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        var releases = new List<CoreRelease>();
        if (_requiredRelease is { } required)
            releases.Add(required with { Emulator = emulator });
        if (LatestOfficialUri is not { } latestUri)
            return releases;
        return await CoreReleaseFunctions.Releases.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, latestUri, response =>
        {
            var published = response.Content.Headers.LastModified ?? response.Headers.Date;
            var suffix = published?.UtcDateTime.ToString(CoreReleaseConstants.YyyyMMddHHmm) ?? CoreReleaseConstants.Latest;
            var id = string.Concat(CoreReleaseConstants.OfficialReleaseIdPrefix, suffix);
            releases.Add(new CoreRelease(emulator, id, id, latestUri, published, response.Content.Headers.ContentLength) { DisplayName = published is null ? CoreReleaseConstants.LibretroLatest : string.Format(CoreReleaseConstants.PublishedDisplayFormat, published.Value.LocalDateTime), IsRequired = false, IsZip = true });
            return releases;
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
                    VerifyWindowsX64Library(extracted);
                    return EmulatorCatalogFunctions.Diagnostics.CalculateSha256(extracted);
                },
                () => File.Move(extracted, destination, true),
                sha256 => CoreReleaseFunctions.WriteManifestAsync(release.Id, release.DownloadUri.AbsoluteUri, destination, sha256, cancellationToken),
                () => Task.CompletedTask,
                archiveSize => progress?.Report(new CoreInstallProgress(archiveSize, archiveSize))).ConfigureAwait(false);
            return new CoreInstallationPaths(_directory, destination, Path.Combine(_directory, CoreReleaseConstants.CoreJson)) { InstalledVersion = release.DeclaredVersion };
        }
        finally
        {
            if (File.Exists(download)) File.Delete(download);
            if (File.Exists(extracted)) File.Delete(extracted);
        }
    }

    internal static void VerifyWindowsX64Library(string path)
        =>
        EmulatorCatalogFunctions.Diagnostics.VerifyWindowsX64Library(path,
            ErrorMessages.DownloadedCoreNotPe, ErrorMessages.DownloadedCoreInvalidPe, ErrorMessages.DownloadedCoreWrongArchitecture);
}
