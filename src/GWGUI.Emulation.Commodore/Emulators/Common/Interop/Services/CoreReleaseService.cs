using GWGUI.Emulation;
using System.IO;

using System.Text.Json;

namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Services;


public sealed class CoreReleaseService
{
    private readonly CoreDefinition _definition;
    public Uri? LatestOfficialUri => _definition.DownloadUrl is { } url ? new(url) : null;
    private readonly HttpClient _httpClient;
    private readonly string _directory;

    internal CoreReleaseService(HttpClient httpClient, string directory, CoreDefinition definition)
    {
        _definition = definition;
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
    }

    public string RequiredLibraryPath => Path.Combine(_directory, _definition.LibraryFile);

    public string? GetInstalledVersion()
    {
        if (!File.Exists(RequiredLibraryPath)) return null;
        var manifestPath = Path.Combine(_directory, CoreReleaseConstants.CoreJson);
        return EmulationSdk.CoreReleases.ReadInstalledVersion(manifestPath, CoreReleaseConstants.Version, CoreReleaseConstants.Unknown);
    }

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        var releases = new List<CoreRelease>();
        if (_definition.RequiredRelease is { } required)
            releases.Add(required);
        if (LatestOfficialUri is not { } latestUri)
            return releases;
        return await EmulationSdk.CoreReleases.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, latestUri, response =>
        {
            var published = response.Content.Headers.LastModified ?? response.Headers.Date;
            var suffix = published?.UtcDateTime.ToString(CoreReleaseConstants.YyyyMMddHHmm) ?? CoreReleaseConstants.Latest;
            releases.Add(new CoreRelease(string.Concat(CoreReleaseConstants.OfficialReleaseIdPrefix, suffix), published is null ? CoreReleaseConstants.LibretroLatest : string.Format(CoreReleaseConstants.PublishedDisplayFormat, published.Value.LocalDateTime), latestUri, published, false, true));
            return releases;
        }, cancellationToken).ConfigureAwait(false);
    }

    public bool IsInstalled(CoreRelease release)
    {
        return EmulationSdk.CoreDiagnostics.FindInstalledLibraryPath(RequiredLibraryPath, VerifyWindowsX64Library) is not null;
    }

    public string GetLibraryPath(CoreRelease release) => RequiredLibraryPath;

    public async Task<string> InstallAsync(CoreRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(release);
        Directory.CreateDirectory(_directory);


        var destination = RequiredLibraryPath;
        var download = destination + CoreReleaseConstants.Download;
        var extracted = destination + CoreReleaseConstants.Extract;
        try
        {
            await EmulationSdk.CoreReleases.InstallLibraryAsync(
                () => EmulationSdk.CoreReleases.DownloadAsync(_httpClient, release.DownloadUri, download,
                    (written, total) =>
                    {
                        if (total > 0) progress?.Report(written / (double)total.Value);
                    }, cancellationToken),
                () =>
                {
                    if (release.IsZip)
                    {
                        EmulationSdk.CoreReleases.ExtractExpectedLibrary(download, _definition.LibraryFile, extracted,
                            static _ => CoreExceptions.ArchiveMissingLibrary(), preserveLastWriteTime: true);
                    }
                    else File.Copy(download, extracted, true);
                },
                archiveSize =>
                {
                    VerifyWindowsX64Library(extracted);
                    return EmulationSdk.CoreDiagnostics.CalculateSha256(extracted);
                },
                () => File.Move(extracted, destination, true),
                sha256 => ExternalCoreInstaller.WriteManifestAsync(release.Id, release.DownloadUri.AbsoluteUri, destination, sha256, cancellationToken),
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
        EmulationSdk.CoreDiagnostics.VerifyWindowsX64Library(path,
            CoreExceptions.DownloadedCoreNotPe, CoreExceptions.DownloadedCoreInvalidPe, CoreExceptions.DownloadedCoreWrongArchitecture);
}
