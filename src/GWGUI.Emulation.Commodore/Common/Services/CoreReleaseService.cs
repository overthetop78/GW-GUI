using EmulatorCatalogFunctions = GWGUI.Emulation.Commodore.Common.Functions.EmulatorCatalogFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Commodore.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using System.IO;

using System.Text.Json;

namespace GWGUI.Emulation.Commodore.Common.Services;


internal sealed class CoreReleaseService
{
    private readonly string _libraryFile;
    private readonly string? _downloadUrl;
    private readonly CoreRelease? _requiredRelease;
    public Uri? LatestOfficialUri => _downloadUrl is { } url ? new(url) : null;
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

    public string RequiredLibraryPath => Path.Combine(_directory, _libraryFile);

    public string? GetInstalledVersion()
    {
        if (!File.Exists(RequiredLibraryPath)) return null;
        var manifestPath = Path.Combine(_directory, CoreReleaseConstants.CoreJson);
        return CoreReleaseFunctions.Releases.ReadInstalledVersion(manifestPath, CoreReleaseConstants.Version, CoreReleaseConstants.Unknown);
    }

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        var releases = new List<CoreRelease>();
        if (_requiredRelease is { } required)
            releases.Add(required);
        if (LatestOfficialUri is not { } latestUri)
            return releases;
        return await CoreReleaseFunctions.Releases.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, latestUri, response =>
        {
            var published = response.Content.Headers.LastModified ?? response.Headers.Date;
            var suffix = published?.UtcDateTime.ToString(CoreReleaseConstants.YyyyMMddHHmm) ?? CoreReleaseConstants.Latest;
            releases.Add(new CoreRelease(string.Concat(CoreReleaseConstants.OfficialReleaseIdPrefix, suffix), published is null ? CoreReleaseConstants.LibretroLatest : string.Format(CoreReleaseConstants.PublishedDisplayFormat, published.Value.LocalDateTime), latestUri, published, false, true));
            return releases;
        }, cancellationToken).ConfigureAwait(false);
    }


    public async Task<string> InstallAsync(CoreRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
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
                        if (total > 0) progress?.Report(written / (double)total.Value);
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
                archiveSize => progress?.Report(CoreReleaseConstants.CompletedDownloadProgress)).ConfigureAwait(false);
            return destination;
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
