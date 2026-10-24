using EmulatorCatalogFunctions = GWGUI.Emulation.Sony.Common.Functions.EmulatorCatalogFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Sony.Common.Exceptions;
using System.IO;
using System.Text.Json;


namespace GWGUI.Emulation.Sony.Common.Services;


internal sealed class CoreReleaseService
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
        return CoreReleaseFunctions.Releases.ReadInstalledVersion(manifestPath, CoreReleaseConstants.Version, CoreReleaseConstants.Unknown);
    }

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await CoreReleaseFunctions.Releases.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, _settings.OfficialArchiveUri, response =>
        [CoreReleaseFunctions.Releases.ReadOfficialRelease(_settings, response)], cancellationToken).ConfigureAwait(false);
    }

internal string? FindInstalledLibraryPath()
    {
        return EmulatorCatalogFunctions.Diagnostics.FindInstalledLibraryPath(RequiredLibraryPath,
            path => VerifyWindowsX64Library(path, _emulatorId),
            _isInstallationComplete is { } complete ? () => complete(_directory) : null);
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
                        CoreReleaseFunctions.Releases.ExtractExpectedLibrary(download, _settings.ArchiveLibraryName, extracted,
                            _ => ErrorMessages.ForEmulator(_emulatorId, CoreErrorResourceKeys.ArchiveMissingLibrary), preserveLastWriteTime: true);
                    }
                    else File.Copy(download, extracted, true);
                },
                archiveSize =>
                {
                    VerifyWindowsX64Library(extracted, _emulatorId);
                    return EmulatorCatalogFunctions.Diagnostics.CalculateSha256(extracted);
                },
                () => File.Move(extracted, destination, true),
                sha256 => CoreReleaseFunctions.WriteManifestAsync(release.Id, release.DownloadUri.AbsoluteUri, destination, sha256, cancellationToken),
                () => Task.CompletedTask,
                archiveSize => progress?.Report(new CoreInstallProgress(archiveSize, archiveSize)),
                prepareReplacement: _prepareInstallation is { } prepare
                    ? () => prepare(_httpClient, _directory, cancellationToken) : null).ConfigureAwait(false);
            return new CoreInstallationPaths(_directory, destination, Path.Combine(_directory, CoreReleaseConstants.CoreJson));
        }
        finally
        {
            if (File.Exists(download)) File.Delete(download);
            if (File.Exists(extracted)) File.Delete(extracted);
        }
    }

    private static void VerifyWindowsX64Library(string path, string? emulatorId) =>
        EmulatorCatalogFunctions.Diagnostics.VerifyWindowsX64Library(path,
            () => ErrorMessages.ForEmulator(emulatorId, CoreErrorResourceKeys.DownloadedCoreNotPe),
            () => ErrorMessages.ForEmulator(emulatorId, CoreErrorResourceKeys.DownloadedCoreInvalidPe),
            () => ErrorMessages.ForEmulator(emulatorId, CoreErrorResourceKeys.DownloadedCoreWrongArchitecture));
}
