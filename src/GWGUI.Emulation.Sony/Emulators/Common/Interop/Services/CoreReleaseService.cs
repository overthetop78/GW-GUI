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
        return global::GWGUI.Emulation.EmulationSdk.CoreReleases.ReadInstalledVersion(manifestPath, CoreReleaseConstants.Version, CoreReleaseConstants.Unknown);
    }

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await global::GWGUI.Emulation.EmulationSdk.CoreReleases.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, _settings.OfficialArchiveUri, response =>
        [global::GWGUI.Emulation.EmulationSdk.CoreReleases.ReadOfficialRelease(_settings, response)], cancellationToken).ConfigureAwait(false);
    }

    public bool IsInstalled(CoreRelease release) => FindInstalledLibraryPath() is not null;

    internal string? FindInstalledLibraryPath()
    {
        return global::GWGUI.Emulation.EmulationSdk.CoreDiagnostics.FindInstalledLibraryPath(RequiredLibraryPath,
            path => VerifyWindowsX64Library(path, _emulatorId),
            _isInstallationComplete is { } complete ? () => complete(_directory) : null);
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
            await global::GWGUI.Emulation.EmulationSdk.CoreReleases.InstallLibraryAsync(
                () => global::GWGUI.Emulation.EmulationSdk.CoreReleases.DownloadAsync(_httpClient, release.DownloadUri, download,
                    (written, total) =>
                    {
                        if (total > 0) progress?.Report(written / (double)total.Value);
                    }, cancellationToken),
                () =>
                {
                    if (release.IsZip)
                    {
                        global::GWGUI.Emulation.EmulationSdk.CoreReleases.ExtractExpectedLibrary(download, _settings.ArchiveLibraryName, extracted,
                            _ => CoreExceptions.ForEmulator(_emulatorId, CoreExceptionsResourceConstants.ArchiveMissingLibrary), preserveLastWriteTime: true);
                    }
                    else File.Copy(download, extracted, true);
                },
                archiveSize =>
                {
                    VerifyWindowsX64Library(extracted, _emulatorId);
                    return global::GWGUI.Emulation.EmulationSdk.CoreDiagnostics.CalculateSha256(extracted);
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

    private static void VerifyWindowsX64Library(string path, string? emulatorId) =>
        global::GWGUI.Emulation.EmulationSdk.CoreDiagnostics.VerifyWindowsX64Library(path,
            () => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.DownloadedCoreNotPe),
            () => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.DownloadedCoreInvalidPe),
            () => CoreExceptions.ForEmulator(emulatorId, CoreExceptionsResourceConstants.DownloadedCoreWrongArchitecture));
}
