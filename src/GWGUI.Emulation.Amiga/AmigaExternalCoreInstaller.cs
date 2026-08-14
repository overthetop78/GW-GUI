using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace GWGUI.Emulation.Amiga;

public sealed class AmigaExternalCoreInstaller
{
    public const string DownloadUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/puae_libretro.dll.zip";
    public const string ArchiveSha256 = "CC2AF29C684B39B80E65E974B9927C58685F7BC06F5AED5FE9B0F4725DD001A3";
    public const string LibrarySha256 = "474A97533194C194107AFF6EDE2F4651E0E1D7ACF2ED4B57C3C9937433D1BD96";
    private readonly HttpClient _httpClient;
    private readonly string _directory;

    public AmigaExternalCoreInstaller(HttpClient httpClient, string directory)
    {
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
    }

    public string LibraryPath => Path.Combine(_directory, "puae_libretro.dll");
    public bool IsInstalled => File.Exists(LibraryPath) && Hash(LibraryPath).Equals(LibrarySha256, StringComparison.OrdinalIgnoreCase);

    public async Task<string> InstallAsync(CancellationToken cancellationToken = default)
    {
        if (IsInstalled) return LibraryPath;
        Directory.CreateDirectory(_directory);
        var archivePath = Path.Combine(_directory, "core.download");
        var temporaryLibrary = LibraryPath + ".download";
        try
        {
            await using (var source = await _httpClient.GetStreamAsync(DownloadUrl, cancellationToken))
            await using (var destination = new FileStream(archivePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                await source.CopyToAsync(destination, cancellationToken);
            Verify(archivePath, ArchiveSha256, "archive");
            using (var archive = ZipFile.OpenRead(archivePath))
            {
                var entry = archive.Entries.SingleOrDefault(item => item.Name.Equals("puae_libretro.dll", StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException("The downloaded Amiga archive does not contain the expected library.");
                await using var source = entry.Open();
                await using var destination = new FileStream(temporaryLibrary, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
                await source.CopyToAsync(destination, cancellationToken);
            }
            Verify(temporaryLibrary, LibrarySha256, "library");
            File.Move(temporaryLibrary, LibraryPath, true);
            var manifest = new { source = DownloadUrl, archiveSha256 = ArchiveSha256, librarySha256 = LibrarySha256, installedUtc = DateTime.UtcNow };
            await File.WriteAllTextAsync(Path.Combine(_directory, "core.json"), JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }), cancellationToken);
            return LibraryPath;
        }
        finally
        {
            if (File.Exists(archivePath)) File.Delete(archivePath);
            if (File.Exists(temporaryLibrary)) File.Delete(temporaryLibrary);
        }
    }

    private static void Verify(string path, string expected, string kind)
    {
        var actual = Hash(path);
        if (!actual.Equals(expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"The downloaded Amiga {kind} has SHA-256 {actual}; expected {expected}.");
    }

    private static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
