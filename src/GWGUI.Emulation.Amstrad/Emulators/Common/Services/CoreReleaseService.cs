using GWGUI.Emulation.Amstrad.Emulators.Common.Exceptions;
using GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;
using GWGUI.Emulation.Amstrad.Emulators.Common.Services;

using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Services;

public sealed class CoreReleaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _directory;
    private readonly CoreDefinition _definition;

    internal CoreReleaseService(HttpClient httpClient, string directory, CoreDefinition definition)
    {
        _definition = definition;
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
    }

    public string RequiredLibraryPath => Path.Combine(_directory, _definition.LibraryFile);

    public string? GetInstalledVersion()
    {
        if (!File.Exists(RequiredLibraryPath)) return null;
        var manifestPath = Path.Combine(_directory, CoreReleaseConstants.CoreJson);
        if (!File.Exists(manifestPath)) return CoreReleaseConstants.Unknown;
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
            return document.RootElement.TryGetProperty(CoreReleaseConstants.Version, out var version)
                ? version.GetString() ?? CoreReleaseConstants.Unknown
                : CoreReleaseConstants.Unknown;
        }
        catch (JsonException) { return CoreReleaseConstants.Unknown; }
        catch (IOException) { return CoreReleaseConstants.Unknown; }
    }

    public async Task<IReadOnlyList<CoreRelease>> GetAvailableAsync(
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, _definition.DownloadUri);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var published = response.Content.Headers.LastModified ?? response.Headers.Date;
        var suffix = published?.UtcDateTime.ToString(CoreReleaseConstants.ReleaseIdentifierDateFormat) ?? CoreReleaseConstants.Latest;
        return [new CoreRelease($"{CoreReleaseConstants.OfficialReleaseIdPrefix}{suffix}",
            published is null ? _definition.LatestReleaseDisplayName
                : $"{published.Value.LocalDateTime.ToString(CoreReleaseConstants.ReleaseDisplayDateFormat)}{_definition.ReleaseProviderSuffix}",
            _definition.DownloadUri, published, true, true)];
    }

    public bool IsInstalled(CoreRelease release)
    {
        if (!File.Exists(RequiredLibraryPath)) return false;
        try { _definition.VerifyInstallation?.Invoke(RequiredLibraryPath); return true; }
        catch (IOException) { return false; }
        catch (InvalidDataException) { return false; }
    }

    public string GetLibraryPath(CoreRelease release) => RequiredLibraryPath;

    public async Task<string> InstallAsync(CoreRelease release,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(release);
        Directory.CreateDirectory(_directory);
        var destination = RequiredLibraryPath;
        var download = destination + CoreReleaseConstants.Download;
        var extracted = destination + CoreReleaseConstants.Extract;
        try
        {
            using var response = await _httpClient.GetAsync(release.DownloadUri,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var total = response.Content.Headers.ContentLength;
            await using (var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
            await using (var target = new FileStream(download, FileMode.Create, FileAccess.Write, FileShare.None,
                             CoreReleaseConstants.DownloadBufferSize, FileOptions.Asynchronous))
            {
                var buffer = new byte[CoreReleaseConstants.DownloadBufferSize];
                long written = BufferConstants.EmptyCollectionCount;
                int read;
                while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > BufferConstants.EmptyCollectionCount)
                {
                    await target.WriteAsync(buffer.AsMemory(BufferConstants.FirstBufferIndex, read),
                        cancellationToken).ConfigureAwait(false);
                    written += read;
                    if (total > BufferConstants.EmptyCollectionCount) progress?.Report(written / (double)total.Value);
                }
            }

            if (release.IsZip)
            {
                using var archive = ZipFile.OpenRead(download);
                var entry = archive.Entries.FirstOrDefault(item =>
                    Path.GetFileName(item.FullName).Equals(_definition.LibraryFile, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException(CommonExceptions.ArchiveMissingLibrary());
                entry.ExtractToFile(extracted, true);
            }
            else File.Copy(download, extracted, true);

            _definition.VerifyInstallation?.Invoke(extracted);
            var sha256 = Hash(extracted);
            File.Move(extracted, destination, true);
            await ExternalCoreInstaller.WriteManifestAsync(release.Id, release.DownloadUri.AbsoluteUri,
                destination, sha256, _definition.Architecture, cancellationToken).ConfigureAwait(false);
            progress?.Report(CoreReleaseConstants.CompletedDownloadProgress);
            return destination;
        }
        finally
        {
            if (File.Exists(download)) File.Delete(download);
            if (File.Exists(extracted)) File.Delete(extracted);
        }
    }

    private static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
