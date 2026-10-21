using GWGUI.Emulation;
using GWGUI.Emulation.Amstrad.Emulators.Common.Exceptions;
using GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Common.Services;

using System.IO;
using System.Text.Json;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Services;

public sealed class CoreReleaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _directory;
    private readonly CoreDefinition _definition;

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
        return await EmulationSdk.CoreReleases.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, _definition.DownloadUri, response =>
        {
            var published = response.Content.Headers.LastModified ?? response.Headers.Date;
            var suffix = published?.UtcDateTime.ToString(CoreReleaseConstants.ReleaseIdentifierDateFormat) ?? CoreReleaseConstants.Latest;
            return[new CoreRelease($"{CoreReleaseConstants.OfficialReleaseIdPrefix}{suffix}", published is null ? _definition.LatestReleaseDisplayName : $"{published.Value.LocalDateTime.ToString(CoreReleaseConstants.ReleaseDisplayDateFormat)}{_definition.ReleaseProviderSuffix}", _definition.DownloadUri, published, true, true)];
        }, cancellationToken).ConfigureAwait(false);
    }

    public bool IsInstalled(CoreRelease release)
    {
        return EmulationSdk.CoreDiagnostics.FindInstalledLibraryPath(RequiredLibraryPath, _definition.VerifyInstallation) is not null;
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
                            static _ => CommonExceptions.ArchiveMissingLibrary(), preserveLastWriteTime: true);
                    }
                    else File.Copy(download, extracted, true);
                },
                archiveSize =>
                {
                    _definition.VerifyInstallation?.Invoke(extracted);
                    return EmulationSdk.CoreDiagnostics.CalculateSha256(extracted);
                },
                () => File.Move(extracted, destination, true),
                sha256 => ExternalCoreInstaller.WriteManifestAsync(release.Id, release.DownloadUri.AbsoluteUri, destination, sha256, _definition.Architecture, cancellationToken),
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
}
