using GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;


namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;


public sealed class CoreReleaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _directory;
    private readonly CoreReleaseSettings _settings;

    internal CoreReleaseService(HttpClient httpClient, string directory,
        CoreReleaseSettings settings)
    {
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
        _settings = settings;
    }

    public string RequiredLibraryPath => Path.Combine(_directory, _settings.InstalledLibraryName);

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
        using var request = new HttpRequestMessage(HttpMethod.Head, _settings.OfficialArchiveUri);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var published = response.Content.Headers.LastModified ?? response.Headers.Date;
        var suffix = published?.UtcDateTime.ToString(CoreReleaseConstants.YyyyMMddHHmm) ?? CoreReleaseConstants.Latest;
        return [new CoreRelease(CoreReleaseConstants.OfficialReleasePrefix + suffix,
            published is null ? _settings.DisplayName + CoreReleaseConstants.ReleaseDisplaySeparator + CoreReleaseConstants.Latest
                : published.Value.LocalDateTime.ToString(CoreReleaseConstants.PublicationDateFormat)
                    + CoreReleaseConstants.ReleaseDisplaySeparator + _settings.DisplayName,
            _settings.OfficialArchiveUri, published, true, true)];
    }

    public bool IsInstalled(CoreRelease release)
    {
        if (!File.Exists(RequiredLibraryPath)) return false;
        try { VerifyWindowsX64Library(RequiredLibraryPath); return true; }
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
                long written = 0;
                int read;
                while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await target.WriteAsync(buffer.AsMemory(BufferConstants.FirstBufferIndex, read),
                        cancellationToken).ConfigureAwait(false);
                    written += read;
                    if (total > 0) progress?.Report(written / (double)total.Value);
                }
            }

            if (release.IsZip)
            {
                using var archive = ZipFile.OpenRead(download);
                var entry = archive.Entries.FirstOrDefault(item =>
                    Path.GetFileName(item.FullName).Equals(_settings.ArchiveLibraryName, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException(CoreExceptions.ArchiveMissingLibrary());
                entry.ExtractToFile(extracted, true);
            }
            else File.Copy(download, extracted, true);

            VerifyWindowsX64Library(extracted);
            var sha256 = Hash(extracted);
            File.Move(extracted, destination, true);
            await CoreManifestFunctions.WriteManifestAsync(release.Id, release.DownloadUri.AbsoluteUri,
                destination, sha256, cancellationToken).ConfigureAwait(false);
            progress?.Report(CoreReleaseConstants.CompletedProgress);
            return destination;
        }
        finally
        {
            if (File.Exists(download)) File.Delete(download);
            if (File.Exists(extracted)) File.Delete(extracted);
        }
    }

    internal static void VerifyWindowsX64Library(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        if (stream.Length < CoreReleaseConstants.MinimumPeHeaderSize || reader.ReadUInt16() != CoreReleaseConstants.DosSignature)
            throw new InvalidDataException(CoreExceptions.DownloadedCoreNotPe());
        stream.Position = CoreReleaseConstants.PeHeaderOffsetPosition;
        var peOffset = reader.ReadInt32();
        if (peOffset < CoreReleaseConstants.MinimumPeHeaderSize || peOffset > stream.Length - CoreReleaseConstants.PeSignatureAndArchitectureSize)
            throw new InvalidDataException(CoreExceptions.DownloadedCoreInvalidPe());
        stream.Position = peOffset;
        if (reader.ReadUInt32() != CoreReleaseConstants.PeSignature || reader.ReadUInt16() != CoreReleaseConstants.Amd64MachineType)
            throw new InvalidDataException(CoreExceptions.DownloadedCoreWrongArchitecture());
    }

    private static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
