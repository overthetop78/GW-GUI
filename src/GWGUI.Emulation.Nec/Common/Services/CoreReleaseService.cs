using EmulatorCatalogFunctions = GWGUI.Emulation.Nec.Common.Functions.EmulatorCatalogFunctions;
using CoreReleaseFunctions = GWGUI.Emulation.Nec.Common.Functions.CoreReleaseFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Nec.Common.Exceptions;
using System.IO;
using System.Text.Json;


namespace GWGUI.Emulation.Nec.Common.Services;


internal sealed class CoreReleaseService : ICoreReleaseService
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

    private string RequiredLibraryPath => Path.Combine(_directory, _settings.InstalledLibraryName);

    /// <summary>Résout les chemins et la version de l'installation présente sans charger le cœur.</summary>
    public Task<CoreInstallationPaths?> GetActiveInstallationAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!File.Exists(RequiredLibraryPath)) return Task.FromResult<CoreInstallationPaths?>(null);
        var manifestPath = Path.Combine(_directory, EmulatorCatalogConstants.ManifestFileName);
        var version = CoreReleaseFunctions.Releases.ReadInstalledVersion(manifestPath, EmulatorCatalogConstants.VersionPropertyName, CoreReleaseConstants.UnknownVersion);
        return Task.FromResult<CoreInstallationPaths?>(new(_directory, RequiredLibraryPath, manifestPath) { InstalledVersion = version });
    }

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(Emulator emulator, CancellationToken cancellationToken = default)
    {
        return await CoreReleaseFunctions.Releases.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, _settings.OfficialArchiveUri, response =>
        [CoreReleaseFunctions.Releases.ReadOfficialRelease(emulator, _settings, response)], cancellationToken).ConfigureAwait(false);
    }


    public async Task<CoreInstallationPaths> InstallAsync(CoreRelease release, IProgress<CoreInstallProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(release);
        Directory.CreateDirectory(_directory);
        var destination = RequiredLibraryPath;
        var download = destination + CoreReleaseConstants.TemporaryDownloadExtension;
        var extracted = destination + CoreReleaseConstants.TemporaryExtractExtension;
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
                            _ => ExceptionText.Get(_errorResourcePrefix + ".ArchiveMissingLibrary"), preserveLastWriteTime: true);
                    }
                    else File.Copy(download, extracted, true);
                },
                archiveSize =>
                {
                    VerifyWindowsX64Library(extracted, _errorResourcePrefix);
                    return EmulatorCatalogFunctions.Diagnostics.CalculateSha256(extracted);
                },
                () => File.Move(extracted, destination, true),
                sha256 => CoreReleaseFunctions.WriteManifestAsync(release.Id, release.DownloadUri.AbsoluteUri, destination, sha256, cancellationToken),
                () => Task.CompletedTask,
                archiveSize => progress?.Report(new CoreInstallProgress(archiveSize, archiveSize))).ConfigureAwait(false);
            return new CoreInstallationPaths(_directory, destination, Path.Combine(_directory, EmulatorCatalogConstants.ManifestFileName)) { InstalledVersion = release.DeclaredVersion };
        }
        finally
        {
            if (File.Exists(download)) File.Delete(download);
            if (File.Exists(extracted)) File.Delete(extracted);
        }
    }



    internal static void VerifyWindowsX64Library(string path, string errorResourcePrefix)
        => EmulatorCatalogFunctions.Diagnostics.VerifyWindowsX64Library(path,
            () => ExceptionText.Get(errorResourcePrefix + ".DownloadedCoreNotPe"),
            () => ExceptionText.Get(errorResourcePrefix + ".DownloadedCoreInvalidPe"),
            () => ExceptionText.Get(errorResourcePrefix + ".DownloadedCoreWrongArchitecture"));
}