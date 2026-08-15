using System.IO.Compression;
using System.IO;
using System.Net;
using System.Net.Http;
using GWGUI.Emulation.Amiga;

namespace GWGUI.Tests;

public sealed class AmigaCoreReleaseServiceTests
{
    [Fact]
    public async Task Catalog_ContainsRequiredVersionFirstAndLatestOfficialBuild()
    {
        var modified = new DateTimeOffset(2026, 8, 14, 5, 0, 0, TimeSpan.Zero);
        using var client = new HttpClient(new CoreHandler(modified, CreateArchive()));
        var service = new AmigaCoreReleaseService(client, TemporaryRoot());

        var releases = await service.GetAvailableAsync();

        Assert.Equal(2, releases.Count);
        Assert.True(releases[0].IsRequired);
        Assert.Equal(AmigaCoreReleaseService.RequiredReleaseId, releases[0].Id);
        Assert.False(releases[1].IsRequired);
        Assert.Equal("official-20260814-0500", releases[1].Id);
        Assert.Equal(modified, releases[1].PublishedUtc);
    }

    [Fact]
    public async Task OfficialZip_ReplacesTheActiveLibraryAndWritesManifest()
    {
        var root = TemporaryRoot();
        Directory.CreateDirectory(root);
        var activeLibrary = Path.Combine(root, "puae_libretro.dll");
        await File.WriteAllBytesAsync(activeLibrary, [9, 8, 7]);
        await File.WriteAllTextAsync(Path.Combine(root, "core.json"), "{\"version\":\"previous\"}");
        var modified = new DateTimeOffset(2026, 8, 14, 5, 0, 0, TimeSpan.Zero);
        using var client = new HttpClient(new CoreHandler(modified, CreateArchive()));
        var service = new AmigaCoreReleaseService(client, root);
        try
        {
            var release = (await service.GetAvailableAsync()).Single(item => !item.IsRequired);
            var installed = await service.InstallAsync(release);

            Assert.Equal(activeLibrary, installed);
            Assert.True(File.Exists(installed));
            var installedBytes = await File.ReadAllBytesAsync(installed);
            Assert.False(new byte[] { 9, 8, 7 }.SequenceEqual(installedBytes));
            Assert.True(File.Exists(Path.Combine(root, "core.json")));
            Assert.Contains(release.Id, await File.ReadAllTextAsync(Path.Combine(root, "core.json")),
                StringComparison.Ordinal);
            Assert.True(service.IsInstalled(release));
            Assert.False(File.Exists(installed + ".download"));
            Assert.False(File.Exists(installed + ".extract"));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static byte[] CreateArchive()
    {
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("puae_libretro.dll");
            using var stream = entry.Open();
            stream.Write(CreatePeX64());
        }
        return output.ToArray();
    }

    private static byte[] CreatePeX64()
    {
        var bytes = new byte[256];
        bytes[0] = (byte)'M';
        bytes[1] = (byte)'Z';
        BitConverter.GetBytes(0x80).CopyTo(bytes, 0x3c);
        bytes[0x80] = (byte)'P';
        bytes[0x81] = (byte)'E';
        BitConverter.GetBytes((ushort)0x8664).CopyTo(bytes, 0x84);
        return bytes;
    }

    private static string TemporaryRoot() => Path.Combine(Path.GetTempPath(),
        "GWGUI-Amiga-Core-Releases", Guid.NewGuid().ToString("N"));

    private sealed class CoreHandler(DateTimeOffset modified, byte[] archive) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            if (request.Method == HttpMethod.Get)
            {
                response.Content = new ByteArrayContent(archive);
                response.Content.Headers.ContentLength = archive.Length;
            }
            else response.Content = new ByteArrayContent([]);
            response.Content.Headers.LastModified = modified;
            return Task.FromResult(response);
        }
    }
}
