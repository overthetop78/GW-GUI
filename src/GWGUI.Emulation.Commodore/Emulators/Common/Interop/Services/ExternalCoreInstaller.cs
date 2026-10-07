using System.IO;

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Services;

public sealed class ExternalCoreInstaller
{
    private readonly CoreDefinition _definition;
    private string CoreRevision => _definition.RequiredRelease?.Id ?? CoreReleaseConstants.Latest;
    private string DownloadUrl => _definition.DownloadUrl ?? throw new InvalidOperationException(nameof(CoreDefinition.DownloadUrl));
    private readonly HttpClient _httpClient;
    private readonly string _directory;

    internal ExternalCoreInstaller(HttpClient httpClient, string directory, CoreDefinition definition)
    {
        _definition = definition;
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
    }

    public string LibraryPath => Path.Combine(_directory, _definition.LibraryFile);

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
                             BufferConstants.FileTransferBufferSize, FileOptions.Asynchronous))
                await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);

            using (var archive = ZipFile.OpenRead(package))
            {
                var entry = archive.Entries.FirstOrDefault(item =>
                    Path.GetFileName(item.FullName).Equals(_definition.LibraryFile, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException(CoreExceptions.ArchiveMissingLibrary());
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
