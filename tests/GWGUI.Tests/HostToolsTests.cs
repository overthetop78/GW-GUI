using System.IO.Compression;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using GWGUI.Infrastructure.HostTools;

namespace GWGUI.Tests;

public sealed class HostToolsTests
{
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
