using GWGUI.Emulation.Sony.Emulators.Ppsspp.Exceptions;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Contracts;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Factories;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Functions;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Services;

using System.IO;
using System.Text.Json;

namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Services;


public sealed class CoreReleaseService
{
    public static readonly Uri LatestOfficialUri = new(CoreReleaseConstants.HttpsBuildbotLibretroComNightlyWindowsX8664LatestPpssppLibretroDllZip);

    private readonly HttpClient _httpClient;
    private readonly string _directory;

    public CoreReleaseService(HttpClient httpClient, string directory)
    {
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
    }

    public string RequiredLibraryPath => Path.Combine(_directory, CoreReleaseConstants.OptionLibretroDll);

    public string? GetInstalledVersion()
    {
        if (!File.Exists(RequiredLibraryPath) || !SystemFilesFunctions.IsInstalled(_directory)) return null;
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
        return await EmulationCoreReleaseFunctions.QueryReleaseAsync<IReadOnlyList<CoreRelease>>(_httpClient, LatestOfficialUri, response =>
        {
            var published = response.Content.Headers.LastModified ?? response.Headers.Date;
            var suffix = published?.UtcDateTime.ToString(CoreReleaseConstants.YyyyMMddHHmm) ?? CoreReleaseConstants.Latest;
            return[new CoreRelease($"official-{suffix}", published is null ? CoreReleaseConstants.PpssppLatest : $"{published.Value.LocalDateTime:dd/MM/yyyy HH:mm} · PPSSPP", LatestOfficialUri, published, true, true)];
        }, cancellationToken).ConfigureAwait(false);
    }

    public bool IsInstalled(CoreRelease release)
    {
        if (!File.Exists(RequiredLibraryPath) || !SystemFilesFunctions.IsInstalled(_directory)) return false;
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
                        EmulationCoreReleaseFunctions.ExtractExpectedLibrary(download, CoreReleaseConstants.OptionLibretroDll, extracted,
                            static _ => PpssppExceptions.ArchiveMissingLibrary(), preserveLastWriteTime: true);
                    }
                    else File.Copy(download, extracted, true);
                },
                archiveSize =>
                {
                    VerifyWindowsX64Library(extracted);
                    return EmulationCoreDiagnosticFunctions.CalculateSha256(extracted);
                },
                () => File.Move(extracted, destination, true),
                sha256 => ExternalCoreInstaller.WriteManifestAsync(release.Id, release.DownloadUri.AbsoluteUri, destination, sha256, cancellationToken),
                () => Task.CompletedTask,
                archiveSize => progress?.Report(1),
                prepareReplacement: () => SystemFilesFunctions.InstallAsync(_httpClient, _directory, cancellationToken)).ConfigureAwait(false);
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
            PpssppExceptions.DownloadedCoreNotPe, PpssppExceptions.DownloadedCoreInvalidPe, PpssppExceptions.DownloadedCoreWrongArchitecture);

}
