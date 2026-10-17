using GWGUI.Emulation.Sega.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;
using System.IO;
using System.Text.Json;


namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Services;


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
        if (!File.Exists(manifestPath)) return CoreReleaseConstants.Unknown;
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
            return document.RootElement.TryGetProperty(CoreReleaseConstants.Version, out var version) ? version.GetString() ?? CoreReleaseConstants.Unknown : CoreReleaseConstants.Unknown;
        }
        catch (JsonException) { return CoreReleaseConstants.Unknown; }
        catch (IOException) { return CoreReleaseConstants.Unknown; }
    }

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, _settings.OfficialArchiveUri);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var published = response.Content.Headers.LastModified ?? response.Headers.Date;
        var suffix = published?.UtcDateTime.ToString(CoreReleaseConstants.YyyyMMddHHmm) ?? CoreReleaseConstants.Latest;
        return [new CoreRelease($"official-{suffix}",
            published is null ? $"{_settings.DisplayName} · {CoreReleaseConstants.Latest}" : $"{published.Value.LocalDateTime:dd/MM/yyyy HH:mm} · {_settings.DisplayName}",
            _settings.OfficialArchiveUri, published, true, true)];
    }

    public bool IsInstalled(CoreRelease release)
    {
        if (!File.Exists(RequiredLibraryPath)) return false;
        try { VerifyWindowsX64Library(RequiredLibraryPath); return true; }
        catch (IOException) { return false; }
        catch (InvalidDataException) { return false; }
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
            await EmulationCoreReleaseFunctions.DownloadAsync(_httpClient, release.DownloadUri, download,
                (written, total) =>
                {
                    if (total > 0) progress?.Report(written / (double)total.Value);
                }, cancellationToken).ConfigureAwait(false);

            if (release.IsZip)
            {
                EmulationCoreReleaseFunctions.ExtractExpectedLibrary(download, _settings.ArchiveLibraryName, extracted,
                    static _ => CoreExceptions.ArchiveMissingLibrary(), preserveLastWriteTime: true);
            }
            else File.Copy(download, extracted, true);

            VerifyWindowsX64Library(extracted);
            var sha256 = EmulationCoreDiagnosticFunctions.CalculateSha256(extracted);
            File.Move(extracted, destination, true);
            await CoreManifestFunctions.WriteManifestAsync(release.Id, release.DownloadUri.AbsoluteUri, destination, sha256, cancellationToken).ConfigureAwait(false);
            progress?.Report(1);
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
