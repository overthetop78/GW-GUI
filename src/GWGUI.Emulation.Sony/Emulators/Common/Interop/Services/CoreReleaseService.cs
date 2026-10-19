using GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;
using System.IO;
using System.Text.Json;


namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;


public sealed class CoreReleaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _directory;
    private readonly CoreReleaseSettings _settings;
    private readonly string? _emulatorId;
    private readonly Func<string, bool>? _isInstallationComplete;
    private readonly Func<HttpClient, string, CancellationToken, Task>? _prepareInstallation;

    internal CoreReleaseService(HttpClient httpClient, string directory, CoreReleaseSettings settings,
        string? emulatorId = null, Func<string, bool>? isInstallationComplete = null,
        Func<HttpClient, string, CancellationToken, Task>? prepareInstallation = null)
    {
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
        _settings = settings;
        _emulatorId = emulatorId;
        _isInstallationComplete = isInstallationComplete;
        _prepareInstallation = prepareInstallation;
    }

    public string RequiredLibraryPath => Path.Combine(_directory, _settings.InstalledLibraryName);

    public string? GetInstalledVersion()
    {
        if (!File.Exists(RequiredLibraryPath) || _isInstallationComplete?.Invoke(_directory) == false) return null;
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
            return[new CoreRelease(CoreReleaseConstants.OfficialReleasePrefix + suffix, published is null ? _settings.DisplayName + CoreReleaseConstants.ReleaseDisplaySeparator + CoreReleaseConstants.Latest : published.Value.LocalDateTime.ToString(CoreReleaseConstants.PublicationDateFormat) + CoreReleaseConstants.ReleaseDisplaySeparator + _settings.DisplayName, _settings.OfficialArchiveUri, published, true, true)];
        }, cancellationToken).ConfigureAwait(false);
    }

    public bool IsInstalled(CoreRelease release) => FindInstalledLibraryPath() is not null;

    internal string? FindInstalledLibraryPath()
    {
        if (!File.Exists(RequiredLibraryPath) || _isInstallationComplete?.Invoke(_directory) == false) return null;
        try { VerifyWindowsX64Library(RequiredLibraryPath, _emulatorId); return RequiredLibraryPath; }
        catch (IOException) { return null; }
        catch (InvalidDataException) { return null; }
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
                            _ => CoreExceptions.ForEmulator(_emulatorId, CoreExceptionsResourceConstants.ArchiveMissingLibrary), preserveLastWriteTime: true);
                    }
                    else File.Copy(download, extracted, true);
                },
                archiveSize =>
                {
                    VerifyWindowsX64Library(extracted, _emulatorId);
                    return EmulationCoreDiagnosticFunctions.CalculateSha256(extracted);
                },
                () => File.Move(extracted, destination, true),
                sha256 => CoreManifestFunctions.WriteManifestAsync(release.Id, release.DownloadUri.AbsoluteUri, destination, sha256, cancellationToken),
                () => Task.CompletedTask,
                archiveSize => progress?.Report(CoreReleaseConstants.CompletedProgress),
                prepareReplacement: _prepareInstallation is { } prepare
                    ? () => prepare(_httpClient, _directory, cancellationToken) : null).ConfigureAwait(false);
            return destination;
        }
        finally
        {
            if (File.Exists(download)) File.Delete(download);
            if (File.Exists(extracted)) File.Delete(extracted);
        }
    }

    internal static void VerifyWindowsX64Library(string path)
        => VerifyWindowsX64Library(path, null);

    private static void VerifyWindowsX64Library(string path, string? emulatorId) =>
        EmulationCoreDiagnosticFunctions.VerifyWindowsX64Library(path,
            () => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.DownloadedCoreNotPe),
            () => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.DownloadedCoreInvalidPe),
            () => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.DownloadedCoreWrongArchitecture));
}
