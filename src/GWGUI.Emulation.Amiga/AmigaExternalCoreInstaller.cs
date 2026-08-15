using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace GWGUI.Emulation.Amiga;

public sealed class AmigaExternalCoreInstaller
{
    public const string CoreRevision = "96ebfcfc";
    public const string DownloadUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/puae_libretro.dll.zip";
    private readonly HttpClient _httpClient;
    private readonly string _directory;

    public AmigaExternalCoreInstaller(HttpClient httpClient, string directory)
    {
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
    }

    public string LibraryPath => Path.Combine(_directory, "puae_libretro.dll");

    public bool IsInstalled
    {
        get
        {
            if (!File.Exists(LibraryPath)) return false;
            try { AmigaCoreReleaseService.VerifyWindowsX64Library(LibraryPath); return true; }
            catch (IOException) { return false; }
            catch (InvalidDataException) { return false; }
        }
    }

    public async Task<string> InstallAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var package = LibraryPath + ".download";
        var extracted = LibraryPath + ".extract";
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
                    Path.GetFileName(item.FullName).Equals("puae_libretro.dll", StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException("The official Amiga core archive does not contain puae_libretro.dll.");
                entry.ExtractToFile(extracted, true);
            }
            AmigaCoreReleaseService.VerifyWindowsX64Library(extracted);
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
            architecture = "x64",
            installedUtc = DateTimeOffset.UtcNow
        };
        await File.WriteAllTextAsync(Path.Combine(Path.GetDirectoryName(libraryPath)!, "core.json"),
            JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }),
            cancellationToken).ConfigureAwait(false);
    }

    internal static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
