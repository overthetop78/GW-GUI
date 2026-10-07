using GWGUI.Emulation.Amstrad.Emulators.Common.Exceptions;
using GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Common.Services;

using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Services;

public sealed class ExternalCoreInstaller
{
    private readonly HttpClient _httpClient;
    private readonly string _directory;
    private readonly CoreDefinition _definition;

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
            try { _definition.VerifyInstallation?.Invoke(LibraryPath); return true; }
            catch (IOException) { return false; }
            catch (InvalidDataException) { return false; }
        }
    }

    public async Task<string> InstallAsync(CancellationToken cancellationToken = default)
    {
        var service = new CoreReleaseService(_httpClient, _directory, _definition);
        var releases = await service.GetAvailableAsync(cancellationToken).ConfigureAwait(false);
        return await service.InstallAsync(releases.Single(), cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    internal static async Task WriteManifestAsync(string version, string source, string libraryPath,
        string sha256, string architecture, CancellationToken cancellationToken)
    {
        var manifest = new
        {
            version,
            source,
            librarySize = new FileInfo(libraryPath).Length,
            librarySha256 = sha256,
            architecture,
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
