using System.IO.Compression;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using GWGUI.Infrastructure.HostTools;
using GWGUI.Domain.HostTools;

namespace GWGUI.Tests;

public sealed class HostToolsTests
{
    [Fact]
    public async Task RealHostToolsInstallationsAreDetectedAndExposeFormatCapabilitiesWhenRequested()
    {
        var specification = Environment.GetEnvironmentVariable("GWGUI_REAL_HOST_TOOLS");
        if (string.IsNullOrWhiteSpace(specification)) return;

        var entries = specification.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(entry => entry.Split('|', 2, StringSplitOptions.TrimEntries))
            .ToArray();
        Assert.True(entries.Length >= 2, "At least two real Host Tools installations are required.");
        var managedRoot = Path.Combine(Path.GetTempPath(), "gwgui-real-host-tools-" + Guid.NewGuid().ToString("N"));
        try
        {
            var installed = new List<(string Version, string ExecutablePath)>();
            foreach (var entry in entries)
            {
                Assert.Equal(2, entry.Length);
                var expectedVersion = entry[0];
                var archivePath = Path.GetFullPath(entry[1]);
                Assert.True(File.Exists(archivePath), $"Real Host Tools archive is missing: {archivePath}");
                var bytes = await File.ReadAllBytesAsync(archivePath);
                var sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                var manager = new GwInstallationManager(new HttpClient(new StaticHandler(new ByteArrayContent(bytes))), managedRoot);
                var installation = await manager.InstallAsync(new(
                    expectedVersion,
                    new Uri("https://example.test/" + Path.GetFileName(archivePath)),
                    Path.GetFileName(archivePath),
                    sha256));
                Assert.True(installation.Managed);
                Assert.Equal(expectedVersion, installation.Version);
                Assert.True(File.Exists(installation.ExecutablePath));
                installed.Add((expectedVersion, installation.ExecutablePath));
            }

            var detector = new GwInstallationManager(new HttpClient(), managedRoot);
            var detected = detector.Detect();
            var reader = new GwFormatCapabilityReader();
            foreach (var (expectedVersion, executablePath) in installed)
            {
                Assert.Contains(detected, item =>
                    Path.GetFullPath(item.ExecutablePath).Equals(Path.GetFullPath(executablePath), StringComparison.OrdinalIgnoreCase)
                    && item.Version == expectedVersion);
                var capabilities = await reader.ReadAsync(executablePath);
                Assert.True(capabilities.IsKnown, $"No format capabilities were parsed from Host Tools {expectedVersion}.");
                Assert.Contains("amiga.amigados", capabilities.FormatIds);
                Assert.Contains(".scp", capabilities.ImageExtensions);
            }
        }
        finally { if (Directory.Exists(managedRoot)) Directory.Delete(managedRoot, true); }
    }

    [Fact]
    public async Task LatestReleaseSelectsTheWindowsX64Asset()
    {
        const string json = """{"tag_name":"v1.23","assets":[{"name":"greaseweazle-1.23.zip","browser_download_url":"https://example.test/all.zip"},{"name":"greaseweazle-1.23-win64.zip","browser_download_url":"https://example.test/win64.zip"}]}""";
        var manager = new GwInstallationManager(new HttpClient(new StaticHandler(new StringContent(json))), Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        var release = await manager.GetLatestReleaseAsync();
        Assert.Equal("1.23", release.Version);
        Assert.Equal("https://example.test/win64.zip", release.DownloadUri.AbsoluteUri);
    }

    [Fact]
    public async Task InstallExtractsGwIntoAVersionedManagedFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var manager = new GwInstallationManager(new HttpClient(new StaticHandler(new ByteArrayContent(CreateArchive(("greaseweazle/gw.exe", "fake"))))), root);
            var installed = await manager.InstallAsync(new("1.23", new Uri("https://example.test/win64.zip"), "greaseweazle-1.23-win64.zip"));
            Assert.True(File.Exists(installed.ExecutablePath));
            Assert.True(installed.Managed);
            Assert.Equal("1.23", installed.Version);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public async Task InstallRejectsZipEntriesEscapingTheManagedFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var manager = new GwInstallationManager(new HttpClient(new StaticHandler(new ByteArrayContent(CreateArchive(("../outside.exe", "bad"))))), root);
            await Assert.ThrowsAsync<InvalidOperationException>(() => manager.InstallAsync(new("1.23", new Uri("https://example.test/win64.zip"), "asset.zip")));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public async Task InstallRejectsAnArchiveWithTheWrongPublishedChecksum()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var manager = new GwInstallationManager(new HttpClient(new StaticHandler(new ByteArrayContent(CreateArchive(("gw.exe", "fake"))))), root);
            await Assert.ThrowsAsync<InvalidDataException>(() => manager.InstallAsync(new("1.23", new Uri("https://example.test/win64.zip"), "asset.zip", new string('0', 64))));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void SelectionAndRollbackPreserveBothValidInstallations()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-selection-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var first = Path.Combine(root, "1.22", "gw.exe");
            var second = Path.Combine(root, "1.23", "gw.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(first)!);
            Directory.CreateDirectory(Path.GetDirectoryName(second)!);
            File.WriteAllText(first, "first");
            File.WriteAllText(second, "second");
            IGwInstallationManager manager = new GwInstallationManager(new HttpClient(), root);

            var selected = manager.Select(first, null, new(second, "1.23", true));
            Assert.Equal(second, selected.ExecutablePath);
            Assert.Equal(first, selected.PreviousExecutablePath);
            Assert.Equal("1.23", selected.InstalledVersion);

            var unchanged = manager.Select(selected.ExecutablePath, selected.PreviousExecutablePath, new(second, "1.23", true));
            Assert.Equal(first, unchanged.PreviousExecutablePath);

            var rolledBack = manager.Rollback(unchanged.ExecutablePath, unchanged.PreviousExecutablePath);
            Assert.Equal(first, rolledBack.ExecutablePath);
            Assert.Equal(second, rolledBack.PreviousExecutablePath);
            Assert.Null(rolledBack.InstalledVersion);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void SelectionAndRollbackRejectMissingExecutables()
    {
        IGwInstallationManager manager = new GwInstallationManager(new HttpClient(), Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        Assert.Throws<FileNotFoundException>(() => manager.Select(null, null, new("missing-gw.exe", null, false)));
        Assert.Throws<FileNotFoundException>(() => manager.Rollback(null, "missing-previous-gw.exe"));
    }

    private static byte[] CreateArchive(params (string Name, string Content)[] entries)
    {
        using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, true)) foreach (var item in entries)
        { var entry = archive.CreateEntry(item.Name); using var writer = new StreamWriter(entry.Open(), Encoding.UTF8); writer.Write(item.Content); }
        return memory.ToArray();
    }

    private sealed class StaticHandler(HttpContent content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
    }
}
