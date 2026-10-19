using GWGUI.Emulation.Nec.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;
using System.IO;
using System.Text.Json;


namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Services;


public sealed class CoreReleaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _directory;
    private readonly CoreReleaseSettings _settings;
    private readonly string _errorResourcePrefix;

    internal CoreReleaseService(HttpClient httpClient, string directory, CoreReleaseSettings settings, string errorResourcePrefix = "Emulation.Error.Core")
    {
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
        _settings = settings;
        _errorResourcePrefix = errorResourcePrefix;
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
        return await EmulationCoreReleaseFunctions.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, _settings.OfficialArchiveUri, response =>
        {
            var published = response.Content.Headers.LastModified ?? response.Headers.Date;
            var suffix = published?.UtcDateTime.ToString(CoreReleaseConstants.YyyyMMddHHmm) ?? CoreReleaseConstants.Latest;
            return[new CoreRelease($"official-{suffix}", published is null ? $"{_settings.DisplayName} · {CoreReleaseConstants.Latest}" : $"{published.Value.LocalDateTime:dd/MM/yyyy HH:mm} · {_settings.DisplayName}", _settings.OfficialArchiveUri, published, true, true)];
        }, cancellationToken).ConfigureAwait(false);
    }

    public bool IsInstalled(CoreRelease release)
    {
        if (!File.Exists(RequiredLibraryPath)) return false;
        try { VerifyWindowsX64Library(RequiredLibraryPath, _errorResourcePrefix); return true; }
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
                            _ => CoreExceptions.Text(_errorResourcePrefix + ".ArchiveMissingLibrary"), preserveLastWriteTime: true);
                    }
                    else File.Copy(download, extracted, true);
                },
                archiveSize =>
                {
                    VerifyWindowsX64Library(extracted, _errorResourcePrefix);
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

    internal string? FindInstalledLibraryPath()
    {
        if (!File.Exists(RequiredLibraryPath)) return null;
        try
        {
            VerifyWindowsX64Library(RequiredLibraryPath, _errorResourcePrefix);
            return RequiredLibraryPath;
        }
        catch (IOException) { return null; }
        catch (InvalidDataException) { return null; }
    }

    internal static void VerifyWindowsX64Library(string path)
        => VerifyWindowsX64Library(path, "Emulation.Error.Core");

    private static void VerifyWindowsX64Library(string path, string errorResourcePrefix)
        => EmulationCoreDiagnosticFunctions.VerifyWindowsX64Library(path,
            () => CoreExceptions.Text(errorResourcePrefix + ".DownloadedCoreNotPe"),
            () => CoreExceptions.Text(errorResourcePrefix + ".DownloadedCoreInvalidPe"),
            () => CoreExceptions.Text(errorResourcePrefix + ".DownloadedCoreWrongArchitecture"));
}