using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace GWGUI.Emulation.Amiga;

public sealed class AmigaExternalCoreInstaller
{
    public const string CoreRevision = "ec639e2b";
    public const string DownloadUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/puae_libretro.dll.zip";
    public const string ArchiveSha256 = "CC2AF29C684B39B80E65E974B9927C58685F7BC06F5AED5FE9B0F4725DD001A3";
    public const long ArchiveSize = 6_636_879;
    public const string LibrarySha256 = "474A97533194C194107AFF6EDE2F4651E0E1D7ACF2ED4B57C3C9937433D1BD96";
    public const long LibrarySize = 17_632_256;
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

            VerifyPinnedArchive(package);
            using (var archive = ZipFile.OpenRead(package))
            {
                var entry = archive.Entries.FirstOrDefault(item =>
                    Path.GetFileName(item.FullName).Equals("puae_libretro.dll", StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException("The official Amiga core archive does not contain puae_libretro.dll.");
                entry.ExtractToFile(extracted, true);
            }
            VerifyPinnedLibrary(extracted);
            File.Move(extracted, LibraryPath, true);
            await WriteManifestAsync(CoreRevision, DownloadUrl, LibraryPath, LibrarySha256,
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

    private static void VerifyPinnedLibrary(string path)
    {
        var info = new FileInfo(path);
        if (info.Length != LibrarySize) throw new InvalidDataException($"The downloaded Amiga core has size {info.Length}; expected {LibrarySize}.");
        var actual = Hash(path);
        if (!actual.Equals(LibrarySha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"The downloaded Amiga core has SHA-256 {actual}; expected {LibrarySha256}.");
        AmigaCoreReleaseService.VerifyWindowsX64Library(path);
    }

    private static void VerifyPinnedArchive(string path)
    {
        var info = new FileInfo(path);
        if (info.Length != ArchiveSize)
            throw new InvalidDataException($"The downloaded Amiga core archive has size {info.Length}; expected {ArchiveSize}.");
        var actual = Hash(path);
        if (!actual.Equals(ArchiveSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"The downloaded Amiga core archive has SHA-256 {actual}; expected {ArchiveSha256}.");
    }

    internal static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
