using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Exceptions;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Services;

using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace GWGUI.Emulation.Nintendo.Emulators.Dolphin.Services;

public sealed class ExternalCoreInstaller
{
    public const string CoreRevision = CoreReleaseConstants.Latest;
    public const string DownloadUrl = CoreReleaseConstants.HttpsBuildbotLibretroComNightlyWindowsX8664LatestDolphinLibretroDllZip;
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
            if (!File.Exists(LibraryPath)) return false;
            try { CoreReleaseService.VerifyWindowsX64Library(LibraryPath); return true; }
            catch (IOException) { return false; }
            catch (InvalidDataException) { return false; }
        }
    }

    public async Task<string> InstallAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var package = LibraryPath + CoreReleaseConstants.Download;
        var extracted = LibraryPath + CoreReleaseConstants.Extract;
        try
        {
            using var response = await _httpClient.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using (var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
            await using (var destination = new FileStream(package, FileMode.Create, FileAccess.Write, FileShare.None,
                             81920, FileOptions.Asynchronous))
                await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);

            using (var archive = ZipFile.OpenRead(package))
            {
                var entry = archive.Entries.FirstOrDefault(item =>
                    Path.GetFileName(item.FullName).Equals(CoreReleaseConstants.OptionLibretroDll, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException(DolphinExceptions.ArchiveMissingLibrary());
                entry.ExtractToFile(extracted, true);
            }
            CoreReleaseService.VerifyWindowsX64Library(extracted);
            var sha256 = Hash(extracted);
            File.Move(extracted, LibraryPath, true);
            await WriteManifestAsync(CoreRevision, DownloadUrl, LibraryPath, sha256,
                cancellationToken).ConfigureAwait(false);
            return LibraryPath;
        }
        finally
        {
            if (File.Exists(package)) File.Delete(package);
            if (File.Exists(extracted)) File.Delete(extracted);
        }
    }

    internal static async Task WriteManifestAsync(string version, string source, string libraryPath,
        string sha256, CancellationToken cancellationToken)
    {
        var manifest = new
        {
            version,
            source,
            librarySize = new FileInfo(libraryPath).Length,
            librarySha256 = sha256,
            architecture = CoreReleaseConstants.X64,
            installedUtc = DateTimeOffset.UtcNow
        };
        await File.WriteAllTextAsync(Path.Combine(Path.GetDirectoryName(libraryPath)!, CoreReleaseConstants.CoreJson),
            JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }),
            cancellationToken).ConfigureAwait(false);
    }

    internal static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}


