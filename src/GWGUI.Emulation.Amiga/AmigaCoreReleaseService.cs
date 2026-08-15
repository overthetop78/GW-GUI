using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace GWGUI.Emulation.Amiga;

public sealed record AmigaCoreRelease(
    string Id,
    string DisplayName,
    Uri DownloadUri,
    DateTimeOffset? PublishedUtc,
    bool IsRequired,
    bool IsZip)
{
    public override string ToString() => DisplayName;
}

public sealed class AmigaCoreReleaseService
{
    public const string RequiredReleaseId = "validated-ec639e2b";
    public const string RequiredDisplayName = "ec639e2b · 31/07/2026 · GW GUI";
    public static readonly Uri LatestOfficialUri = new(
        "https://buildbot.libretro.com/nightly/windows/x86_64/latest/puae_libretro.dll.zip");

    private readonly HttpClient _httpClient;
    private readonly string _directory;

    public AmigaCoreReleaseService(HttpClient httpClient, string directory)
    {
        _httpClient = httpClient;
        _directory = Path.GetFullPath(directory);
    }

    public string RequiredLibraryPath => Path.Combine(_directory, "puae_libretro.dll");

    public string? GetInstalledVersion()
    {
        if (!File.Exists(RequiredLibraryPath)) return null;
        var manifestPath = Path.Combine(_directory, "core.json");
        if (!File.Exists(manifestPath)) return "unknown";
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
            return document.RootElement.TryGetProperty("version", out var version)
                ? version.GetString() ?? "unknown"
                : "unknown";
        }
        catch (JsonException) { return "unknown"; }
        catch (IOException) { return "unknown"; }
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
        var suffix = published?.UtcDateTime.ToString("yyyyMMdd-HHmm") ?? "latest";
        releases.Add(new AmigaCoreRelease($"official-{suffix}",
            published is null ? "Libretro · latest"
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
        var download = destination + ".download";
        var extracted = destination + ".extract";
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
                    Path.GetFileName(item.FullName).Equals("puae_libretro.dll", StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException("The official archive does not contain puae_libretro.dll.");
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
            throw new InvalidDataException("The downloaded Amiga core is not a PE file.");
        stream.Position = 0x3c;
        var peOffset = reader.ReadInt32();
        if (peOffset < 0x40 || peOffset > stream.Length - 6)
            throw new InvalidDataException("The downloaded Amiga core has an invalid PE header.");
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
