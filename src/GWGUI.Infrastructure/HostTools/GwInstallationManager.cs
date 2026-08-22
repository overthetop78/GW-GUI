using GWGUI.Domain.HostTools;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace GWGUI.Infrastructure.HostTools;

public sealed partial class GwInstallationManager(HttpClient httpClient, string managedRoot) : IGwInstallationManager
{
    private static readonly Uri LatestReleaseApi = new("https://api.github.com/repos/keirf/greaseweazle/releases/latest");

    public IReadOnlyList<HostToolsInstallation> Detect(string? configuredPath = null)
    {
        var candidates = new List<string?> { configuredPath, Path.Combine(AppContext.BaseDirectory, "gw.exe") };
        var path = Environment.GetEnvironmentVariable("PATH") ?? "";
        candidates.AddRange(path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).Select(x => Path.Combine(x.Trim(), "gw.exe")));
        if (Directory.Exists(managedRoot))
        {
            foreach (var versionDirectory in Directory.EnumerateDirectories(managedRoot)
                         .Where(directory => !Path.GetFileName(directory).StartsWith(".", StringComparison.Ordinal)))
            {
                NormalizeManagedInstallation(versionDirectory);
            }
            candidates.AddRange(Directory.EnumerateFiles(managedRoot, "gw.exe", SearchOption.AllDirectories));
        }
        return candidates.Where(x => !string.IsNullOrWhiteSpace(x) && File.Exists(x)).Select(x => Path.GetFullPath(x!))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(x => new HostToolsInstallation(x, VersionFromPath(x), IsInside(x, managedRoot))).ToArray();
    }

    public async Task<HostToolsRelease> GetLatestReleaseAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, LatestReleaseApi);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("GW-GUI", "0.1"));
        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        var version = json.RootElement.GetProperty("tag_name").GetString()?.TrimStart('v') ?? throw new InvalidDataException("Release tag is missing.");
        foreach (var asset in json.RootElement.GetProperty("assets").EnumerateArray())
        {
            var name = asset.GetProperty("name").GetString() ?? "";
            if (!Win64AssetRegex().IsMatch(name)) continue;
            var url = asset.GetProperty("browser_download_url").GetString() ?? throw new InvalidDataException("Release URL is missing.");
            var digest = asset.TryGetProperty("digest", out var digestNode) ? digestNode.GetString() : null;
            return new(version, new Uri(url), name, digest?.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) == true ? digest[7..] : null);
        }
        throw new InvalidDataException("The release has no Windows x64 archive.");
    }

    public async Task<HostToolsInstallation> InstallAsync(HostToolsRelease release, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(managedRoot);
        var destination = Path.GetFullPath(Path.Combine(managedRoot, release.Version));
        EnsureInside(destination, managedRoot);
        var existing = NormalizeManagedInstallation(destination);
        if (existing is not null) return new(existing, release.Version, true);

        var temporary = Path.GetFullPath(Path.Combine(managedRoot, ".install-" + Guid.NewGuid().ToString("N")));
        EnsureInside(temporary, managedRoot);
        Directory.CreateDirectory(temporary);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, release.DownloadUri);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("GW-GUI", "0.1"));
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var length = response.Content.Headers.ContentLength;
            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var archiveMemory = new MemoryStream();
            var buffer = new byte[81920]; long copied = 0;
            while (true)
            {
                var read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false); if (read == 0) break;
                await archiveMemory.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false); copied += read;
                if (length > 0) progress?.Report(Math.Clamp((double)copied / length.Value, 0, 1));
            }
            archiveMemory.Position = 0;
            if (!string.IsNullOrWhiteSpace(release.Sha256))
            {
                var actual = Convert.ToHexString(SHA256.HashData(archiveMemory)).ToLowerInvariant();
                if (!actual.Equals(release.Sha256, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Downloaded archive checksum does not match the release metadata.");
                archiveMemory.Position = 0;
            }
            using var archive = new ZipArchive(archiveMemory, ZipArchiveMode.Read);
            foreach (var entry in archive.Entries)
            {
                var target = Path.GetFullPath(Path.Combine(temporary, entry.FullName)); EnsureInside(target, temporary);
                if (string.IsNullOrEmpty(entry.Name)) { Directory.CreateDirectory(target); continue; }
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                await using var input = entry.Open(); await using var output = File.Create(target); await input.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
            }
            var executable = Directory.EnumerateFiles(temporary, "gw.exe", SearchOption.AllDirectories).FirstOrDefault() ?? throw new InvalidDataException("gw.exe is missing from the downloaded archive.");
            var payload = Path.GetDirectoryName(executable)!;
            Directory.CreateDirectory(destination);
            PromotePayload(payload, destination);
            return new(Path.Combine(destination, "gw.exe"), release.Version, true);
        }
        finally { if (Directory.Exists(temporary)) Directory.Delete(temporary, recursive: true); }
    }

    public HostToolsSelection Select(string? currentPath, string? previousPath, HostToolsInstallation selected)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selected.ExecutablePath);
        if (!File.Exists(selected.ExecutablePath)) throw new FileNotFoundException("The selected gw executable does not exist.", selected.ExecutablePath);
        var previous = !string.Equals(currentPath, selected.ExecutablePath, StringComparison.OrdinalIgnoreCase) && File.Exists(currentPath)
            ? currentPath
            : previousPath;
        return new(selected.ExecutablePath, previous, selected.Version);
    }

    public HostToolsSelection Rollback(string? currentPath, string? previousPath)
    {
        if (!File.Exists(previousPath)) throw new FileNotFoundException("The previous gw executable does not exist.", previousPath);
        return new(previousPath, string.IsNullOrWhiteSpace(currentPath) ? null : currentPath, null);
    }

    private static string? VersionFromPath(string path) => VersionRegex().Match(path) is { Success: true } match ? match.Groups[1].Value : null;
    private static string? NormalizeManagedInstallation(string destination)
    {
        var direct = Path.Combine(destination, "gw.exe");
        if (File.Exists(direct)) return direct;
        if (!Directory.Exists(destination)) return null;
        var nested = Directory.EnumerateFiles(destination, "gw.exe", SearchOption.AllDirectories).FirstOrDefault();
        if (nested is null) return null;
        PromotePayload(Path.GetDirectoryName(nested)!, destination);
        return File.Exists(direct) ? direct : nested;
    }

    private static void PromotePayload(string payload, string destination)
    {
        if (Path.GetFullPath(payload).Equals(Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase)) return;
        foreach (var entry in Directory.EnumerateFileSystemEntries(payload).ToArray())
        {
            var target = Path.Combine(destination, Path.GetFileName(entry));
            if (Directory.Exists(entry)) Directory.Move(entry, target);
            else File.Move(entry, target);
        }
        var current = payload;
        while (!Path.GetFullPath(current).Equals(Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase)
               && Directory.Exists(current) && !Directory.EnumerateFileSystemEntries(current).Any())
        {
            var parent = Directory.GetParent(current)?.FullName;
            Directory.Delete(current);
            if (parent is null) break;
            current = parent;
        }
    }
    private static bool IsInside(string path, string root) { var relative = Path.GetRelativePath(Path.GetFullPath(root), Path.GetFullPath(path)); return relative != ".." && !relative.StartsWith(".." + Path.DirectorySeparatorChar); }
    private static void EnsureInside(string path, string root) { if (!IsInside(path, root)) throw new InvalidOperationException("Path escapes the managed Host Tools folder."); }
    [GeneratedRegex(@"^greaseweazle-.*-win64\.zip$", RegexOptions.IgnoreCase)] private static partial Regex Win64AssetRegex();
    [GeneratedRegex(@"(?:^|[\\/])(?:v)?(\d+\.\d+(?:\.\d+)?)(?:[\\/]|$)")] private static partial Regex VersionRegex();
}
