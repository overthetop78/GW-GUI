using System.Security.Cryptography;
using System.Text.Json;

namespace GWGUI.Emulation.Amiga;

public sealed class AmigaExternalCoreInstaller
{
    public const string CoreRevision = "ec639e2b";
    public const string DownloadUrl = "https://raw.githubusercontent.com/overthetop78/GW-GUI/ec639e2b/artifacts/ppua/puae_libretro.dll";
    public const string FallbackDownloadUrl = "https://cdn.jsdelivr.net/gh/overthetop78/GW-GUI@ec639e2b/artifacts/ppua/puae_libretro.dll";
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
            try { VerifyLibrary(LibraryPath); return true; }
            catch (IOException) { return false; }
            catch (InvalidDataException) { return false; }
        }
    }

    public async Task<string> InstallAsync(CancellationToken cancellationToken = default)
    {
        if (IsInstalled) return LibraryPath;
        Directory.CreateDirectory(_directory);
        var temporaryLibrary = LibraryPath + ".download";
        Exception? lastError = null;
        try
        {
            foreach (var url in new[] { DownloadUrl, FallbackDownloadUrl })
            {
                try
                {
                    await DownloadAsync(url, temporaryLibrary, cancellationToken).ConfigureAwait(false);
                    VerifyLibrary(temporaryLibrary);
                    File.Move(temporaryLibrary, LibraryPath, true);
                    var manifest = new
                    {
                        coreKind = "External",
                        revision = CoreRevision,
                        source = url,
                        librarySize = LibrarySize,
                        librarySha256 = LibrarySha256,
                        architecture = "x64",
                        installedUtc = DateTime.UtcNow
                    };
                    await File.WriteAllTextAsync(Path.Combine(_directory, "core.json"),
                        JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }), cancellationToken);
                    return LibraryPath;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                catch (Exception error)
                {
                    lastError = error;
                    if (File.Exists(temporaryLibrary)) File.Delete(temporaryLibrary);
                }
            }
            throw new InvalidOperationException("The pinned Amiga core could not be installed from either source.", lastError);
        }
        finally
        {
            if (File.Exists(temporaryLibrary)) File.Delete(temporaryLibrary);
        }
    }

    private async Task DownloadAsync(string url, string destinationPath, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var destination = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
        await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
    }

    private static void VerifyLibrary(string path)
    {
        var info = new FileInfo(path);
        if (info.Length != LibrarySize) throw new InvalidDataException($"The downloaded Amiga core has size {info.Length}; expected {LibrarySize}.");
        var actual = Hash(path);
        if (!actual.Equals(LibrarySha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"The downloaded Amiga core has SHA-256 {actual}; expected {LibrarySha256}.");
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        if (reader.ReadUInt16() != 0x5A4D) throw new InvalidDataException("The downloaded Amiga core is not a PE file.");
        stream.Position = 0x3c;
        var peOffset = reader.ReadInt32();
        if (peOffset < 0x40 || peOffset > stream.Length - 6) throw new InvalidDataException("The downloaded Amiga core has an invalid PE header.");
        stream.Position = peOffset;
        if (reader.ReadUInt32() != 0x00004550 || reader.ReadUInt16() != 0x8664)
            throw new InvalidDataException("The downloaded Amiga core is not a Windows x64 library.");
    }

    private static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
