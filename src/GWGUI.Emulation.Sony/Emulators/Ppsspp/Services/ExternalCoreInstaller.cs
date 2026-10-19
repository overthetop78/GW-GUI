using GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Factories;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Functions;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Services;

using System.IO;
using System.Text.Json;

namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Services;

public sealed class ExternalCoreInstaller
{
    public const string CoreRevision = CoreReleaseConstants.Latest;
    public const string DownloadUrl = CoreReleaseConstants.HttpsBuildbotLibretroComNightlyWindowsX8664LatestPpssppLibretroDllZip;
    private readonly HttpClient _httpClient;
    private readonly string _directory;

    public ExternalCoreInstaller(HttpClient httpClient, string directory)
    {
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
    }

    public string LibraryPath => Path.Combine(_directory, CoreReleaseConstants.OptionLibretroDll);

    public bool IsInstalled
    {
        get
        {
            if (!File.Exists(LibraryPath) || !SystemFilesFunctions.IsInstalled(_directory)) return false;
            try { CoreReleaseService.VerifyWindowsX64Library(LibraryPath); return true; }
            catch (IOException) { return false; }
            catch (InvalidDataException) { return false; }
        }
    }

    public Task<string> InstallAsync(CancellationToken cancellationToken = default) =>
        new CoreReleaseService(_httpClient, _directory).InstallAsync(new CoreRelease(CoreRevision, CoreReleaseConstants.PpssppLatest, new Uri(DownloadUrl), null, true, true), cancellationToken: cancellationToken);

    internal static async Task WriteManifestAsync(string version, string source, string libraryPath, string sha256, CancellationToken cancellationToken)
    {
        var manifest = new { version, source, librarySize = new FileInfo(libraryPath).Length, librarySha256 = sha256, architecture = CoreReleaseConstants.X64, installedUtc = DateTimeOffset.UtcNow };
        await File.WriteAllTextAsync(Path.Combine(Path.GetDirectoryName(libraryPath)!, CoreReleaseConstants.CoreJson), JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }), cancellationToken).ConfigureAwait(false);
    }
}


