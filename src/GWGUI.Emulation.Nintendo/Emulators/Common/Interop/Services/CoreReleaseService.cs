using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Services;
using System.IO;
using System.Text.Json;


namespace GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Services;


public sealed class CoreReleaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _directory;
    private readonly CoreReleaseSettings _settings;

    internal CoreReleaseService(HttpClient httpClient, string directory, CoreReleaseSettings settings)
    {
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
        _settings = settings;
    }

    public string RequiredLibraryPath => Path.Combine(_directory, _settings.InstalledLibraryName);

    public string? GetInstalledVersion()
    {
        if (!File.Exists(RequiredLibraryPath)) return null;
        var manifestPath = Path.Combine(_directory, CoreReleaseConstants.CoreJson);
        return EmulationCoreReleaseFunctions.ReadInstalledVersion(manifestPath, CoreReleaseConstants.Version, CoreReleaseConstants.Unknown);
    }

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await EmulationCoreReleaseFunctions.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, _settings.OfficialArchiveUri, response =>
        {
            var published = response.Content.Headers.LastModified ?? response.Headers.Date;
            var suffix = published?.UtcDateTime.ToString(CoreReleaseConstants.YyyyMMddHHmm) ?? CoreReleaseConstants.Latest;
            return[new CoreRelease($"official-{suffix}", published is null ? $"{_settings.DisplayName} · {CoreReleaseConstants.Latest}" : $"{published.Value.LocalDateTime:dd/MM/yyyy HH:mm} · {_settings.DisplayName}", _settings.OfficialArchiveUri, published, true, true)];
        }, cancellationToken).ConfigureAwait(false);
    }

    public bool IsInstalled(CoreRelease release)
    {
        return EmulationCoreDiagnosticFunctions.FindInstalledLibraryPath(RequiredLibraryPath, VerifyWindowsX64Library) is not null;
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
            await EmulationCoreReleaseFunctions.InstallLibraryAsync(
                () => EmulationCoreReleaseFunctions.DownloadAsync(_httpClient, release.DownloadUri, download,
                    (written, total) =>
                    {
                        if (total > 0) progress?.Report(written / (double)total.Value);
                    }, cancellationToken),
                () =>
                {
                    if (release.IsZip)
                    {
                        EmulationCoreReleaseFunctions.ExtractExpectedLibrary(download, _settings.ArchiveLibraryName, extracted,
                            static _ => CoreExceptions.ArchiveMissingLibrary(), preserveLastWriteTime: true);
                    }
                    else File.Copy(download, extracted, true);
                },
                archiveSize =>
                {
                    VerifyWindowsX64Library(extracted);
                    return EmulationCoreDiagnosticFunctions.CalculateSha256(extracted);
                },
                () => File.Move(extracted, destination, true),
                sha256 => CoreManifestFunctions.WriteManifestAsync(release.Id, release.DownloadUri.AbsoluteUri, destination, sha256, cancellationToken),
                () => Task.CompletedTask,
                archiveSize => progress?.Report(1)).ConfigureAwait(false);
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
        EmulationCoreDiagnosticFunctions.VerifyWindowsX64Library(path,
            CoreExceptions.DownloadedCoreNotPe, CoreExceptions.DownloadedCoreInvalidPe, CoreExceptions.DownloadedCoreWrongArchitecture);
}
