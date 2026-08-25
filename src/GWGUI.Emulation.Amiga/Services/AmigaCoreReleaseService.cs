using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace GWGUI.Emulation.Amiga.Services;


public sealed class AmigaCoreReleaseService
{
    public const string RequiredReleaseId = AmigaCoreReleaseServiceConstants.Validated96ebfcfc;
    public const string RequiredDisplayName = AmigaCoreReleaseServiceConstants.Value96ebfcfc31072026GWGUI;
    public static readonly Uri LatestOfficialUri = new(
        AmigaCoreReleaseServiceConstants.HttpsBuildbotLibretroComNightlyWindowsX8664LatestPuaeLibretroDllZip);

    private readonly HttpClient _httpClient;
    private readonly string _directory;

    public AmigaCoreReleaseService(HttpClient httpClient, string directory)
    {
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
    }

    public string RequiredLibraryPath => Path.Combine(_directory, AmigaCoreReleaseServiceConstants.OptionLibretroDll);

    public string? GetInstalledVersion()
    {
        if (!File.Exists(RequiredLibraryPath)) return null;
        var manifestPath = Path.Combine(_directory, AmigaCoreReleaseServiceConstants.CoreJson);
        if (!File.Exists(manifestPath)) return AmigaCoreReleaseServiceConstants.Unknown;
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
            return document.RootElement.TryGetProperty(AmigaCoreReleaseServiceConstants.Version, out var version)
                ? version.GetString() ?? AmigaCoreReleaseServiceConstants.Unknown
                : AmigaCoreReleaseServiceConstants.Unknown;
        }
        catch (JsonException) { return AmigaCoreReleaseServiceConstants.Unknown; }
        catch (IOException) { return AmigaCoreReleaseServiceConstants.Unknown; }
    }

    public async Task<IReadOnlyList<AmigaCoreRelease>> GetAvailableAsync(
        CancellationToken cancellationToken = default)
    {
        var releases = new List<AmigaCoreRelease>
        {
            new(RequiredReleaseId, RequiredDisplayName, new Uri(AmigaExternalCoreInstaller.DownloadUrl),
                new DateTimeOffset(2026, 7, 31, 1, 0, 0, TimeSpan.Zero), true, true)
        };

        using var request = new HttpRequestMessage(HttpMethod.Head, LatestOfficialUri);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var published = response.Content.Headers.LastModified ?? response.Headers.Date;
        var suffix = published?.UtcDateTime.ToString(AmigaCoreReleaseServiceConstants.YyyyMMddHHmm) ?? AmigaCoreReleaseServiceConstants.Latest;
        releases.Add(new AmigaCoreRelease($"official-{suffix}",
            published is null ? AmigaCoreReleaseServiceConstants.LibretroLatest
                : $"{published.Value.LocalDateTime:dd/MM/yyyy HH:mm} · Libretro",
            LatestOfficialUri, published, false, true));
        return releases;
    }

    public bool IsInstalled(AmigaCoreRelease release)
    {
        if (!File.Exists(RequiredLibraryPath)) return false;
        try { VerifyWindowsX64Library(RequiredLibraryPath); return true; }
        catch (IOException) { return false; }
        catch (InvalidDataException) { return false; }
    }

    public string GetLibraryPath(AmigaCoreRelease release) => RequiredLibraryPath;

    public async Task<string> InstallAsync(AmigaCoreRelease release,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(release);
        Directory.CreateDirectory(_directory);
        if (release.IsRequired)
            return await new AmigaExternalCoreInstaller(_httpClient, _directory)
                .InstallAsync(cancellationToken).ConfigureAwait(false);

        var destination = RequiredLibraryPath;
        var download = destination + AmigaCoreReleaseServiceConstants.Download;
        var extracted = destination + AmigaCoreReleaseServiceConstants.Extract;
        try
        {
            using var response = await _httpClient.GetAsync(release.DownloadUri,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var total = response.Content.Headers.ContentLength;
            await using (var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
            await using (var target = new FileStream(download, FileMode.Create, FileAccess.Write, FileShare.None,
                             81920, FileOptions.Asynchronous))
            {
                var buffer = new byte[81920];
                long written = 0;
                int read;
                while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                    written += read;
                    if (total > 0) progress?.Report(written / (double)total.Value);
                }
            }

            if (release.IsZip)
            {
                using var archive = ZipFile.OpenRead(download);
                var entry = archive.Entries.FirstOrDefault(item =>
                    Path.GetFileName(item.FullName).Equals(AmigaCoreReleaseServiceConstants.OptionLibretroDll, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException(AmigaCoreReleaseServiceConstants.TheOfficialArchiveDoesNotContainPuaeLibretroDll);
                entry.ExtractToFile(extracted, true);
            }
            else File.Copy(download, extracted, true);

            VerifyWindowsX64Library(extracted);
            var sha256 = Hash(extracted);
            File.Move(extracted, destination, true);
            await AmigaExternalCoreInstaller.WriteManifestAsync(release.Id, release.DownloadUri.AbsoluteUri,
                destination, sha256, cancellationToken).ConfigureAwait(false);
            progress?.Report(1);
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
        if (stream.Length < 0x40 || reader.ReadUInt16() != 0x5A4D)
            throw new InvalidDataException(AmigaCoreReleaseServiceConstants.TheDownloadedAmigaCoreIsNotAPEFile);
        stream.Position = 0x3c;
        var peOffset = reader.ReadInt32();
        if (peOffset < 0x40 || peOffset > stream.Length - 6)
            throw new InvalidDataException(AmigaCoreReleaseServiceConstants.TheDownloadedAmigaCoreHasAnInvalidPEHeader);
        stream.Position = peOffset;
        if (reader.ReadUInt32() != 0x00004550 || reader.ReadUInt16() != 0x8664)
            throw new InvalidDataException(AmigaCoreReleaseServiceConstants.TheDownloadedAmigaCoreIsNotAWindowsX64Library);
    }

    private static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
