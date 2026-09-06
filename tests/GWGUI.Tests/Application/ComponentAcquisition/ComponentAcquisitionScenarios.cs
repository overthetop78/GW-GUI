using GWGUI.Domain.HostTools;
using GWGUI.Infrastructure.HostTools;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;

namespace GWGUI.Tests.Application.ComponentAcquisition;
internal static class ComponentAcquisitionScenarios
{
    public static async Task CancelDuringDownload()
    {
        var storage=new Storage(); using var cancellation=new CancellationTokenSource();
        using var client=new HttpClient(new ResponseHandler(_=>new(HttpStatusCode.OK){Content=new ByteArrayContent(Archive("gw.exe"))}));
        var progress=new CancelProgress(cancellation);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>new GwInstallationManager(client,Root,storage).InstallAsync(Release,progress,cancellation.Token));
        Assert.True(progress.Reported); Assert.Empty(storage.Files); Assert.DoesNotContain(storage.Directories,path=>path.Contains(".install-"));
        Assert.False(storage.DirectoryExists(Path.Combine(Root,"1.2")));
    }
    private sealed class CancelProgress(CancellationTokenSource source) : IProgress<double>
    {
        public bool Reported;
        public void Report(double value) { Assert.InRange(value,0,1); Reported=true; source.Cancel(); }
    }
    private static string Root => Path.GetFullPath("virtual-installation");
    private static HostToolsRelease Release => new("1.2", new Uri("https://synthetic.invalid/archive"), "greaseweazle-1.2-win64.zip");
    public static async Task Metadata()
    {
        const string metadata = """{"tag_name":"v1.2","assets":[{"name":"linux.zip"},{"name":"greaseweazle-1.2-win64.zip","browser_download_url":"https://synthetic.invalid/archive","digest":"sha256:abc"}]}""";
        using var client = new HttpClient(new ResponseHandler(_ => new(HttpStatusCode.OK) { Content = new StringContent(metadata) }));
        var result = await new GwInstallationManager(client, Root, new Storage()).GetLatestReleaseAsync();
        Assert.Equal("1.2", result.Version); Assert.Equal(Release.DownloadUri, result.DownloadUri); Assert.Equal("abc", result.Sha256);
    }
    private static byte[] Archive(string entryName)
    {
        using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, true))
        { using var stream = archive.CreateEntry(entryName).Open(); stream.Write(new byte[] { 42, 93 }); }
        return memory.ToArray();
    }
    public static async Task Install()
    {
        var bytes = Archive("payload/gw.exe"); var storage = new Storage(); var downloads = 0;
        using var client = new HttpClient(new ResponseHandler(request => { Assert.Equal(Release.DownloadUri, request.RequestUri); downloads++; return new(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) }; }));
        var manager = new GwInstallationManager(client, Root, storage);
        var progress = new Reports();
        var result = await manager.InstallAsync(Release with { Sha256 = Convert.ToHexString(SHA256.HashData(bytes)) }, progress);
        Assert.True(result.Managed); Assert.Equal("1.2", result.Version);
        Assert.Equal(Path.Combine(Root, "1.2", "gw.exe"), result.ExecutablePath);
        Assert.Equal(new byte[] { 42, 93 }, storage.Files[result.ExecutablePath]);
        Assert.Single(storage.Files); Assert.DoesNotContain(storage.Directories, path => path.Contains(".install-"));
        Assert.NotEmpty(progress.Values); Assert.All(progress.Values, value => Assert.InRange(value, 0, 1)); Assert.Equal(1, progress.Values.Last());
        Assert.Equal(result, await manager.InstallAsync(Release)); Assert.Equal(1, downloads);
    }
    public static async Task Invalid(int variant)
    {
        var bytes = variant == 0 ? new byte[] { 1, 2, 3 } : Archive(variant == 2 ? "../escaped" : variant == 3 ? "payload/readme.txt" : "payload/gw.exe");
        var storage = new Storage();
        using var client = new HttpClient(new ResponseHandler(_ => new(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) }));
        var manager = new GwInstallationManager(client, Root, storage);
        var release = variant == 1 ? Release with { Sha256 = "wrong" } : Release;
        if (variant == 2) await Assert.ThrowsAsync<InvalidOperationException>(() => manager.InstallAsync(release));
        else await Assert.ThrowsAsync<InvalidDataException>(() => manager.InstallAsync(release));
        Assert.Empty(storage.Files); Assert.DoesNotContain(storage.Directories, path => path.Contains(".install-"));
        Assert.False(storage.DirectoryExists(Path.Combine(Root, "1.2")));
    }
    public static void Selection()
    {
        var storage = new Storage(); var first = Path.Combine(Root, "1.0", "gw.exe"); var second = Path.Combine(Root, "1.2", "gw.exe");
        storage.Files[first] = [1]; storage.Files[second] = [2];
        using var client = new HttpClient(new ResponseHandler(_ => throw new InvalidOperationException("Unexpected download")));
        var manager = new GwInstallationManager(client, Root, storage);
        var selection = manager.Select(first, null, new(second, "1.2", true));
        Assert.Equal(second, selection.ExecutablePath); Assert.Equal(first, selection.PreviousExecutablePath);
        var restored = manager.Rollback(second, first); Assert.Equal(first, restored.ExecutablePath); Assert.Equal(second, restored.PreviousExecutablePath);
        Assert.Throws<FileNotFoundException>(() => manager.Rollback(second, "absent"));
    }
    public static async Task TransportFailure()
    {
        var storage = new Storage();
        using var client = new HttpClient(new ResponseHandler(_ => new(HttpStatusCode.ServiceUnavailable)));
        var manager = new GwInstallationManager(client, Root, storage);
        await Assert.ThrowsAsync<HttpRequestException>(() => manager.GetLatestReleaseAsync());
        await Assert.ThrowsAsync<HttpRequestException>(() => manager.InstallAsync(Release));
        using var source = new CancellationTokenSource(); source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => manager.InstallAsync(Release, cancellationToken: source.Token));
        Assert.Empty(storage.Files); Assert.DoesNotContain(storage.Directories, path => path.Contains(".install-"));
    }
    private sealed class Reports : IProgress<double> { public List<double> Values { get; } = []; public void Report(double value) => Values.Add(value); }
    private sealed class ResponseHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        { cancellationToken.ThrowIfCancellationRequested(); Assert.Equal(HttpMethod.Get, request.Method); Assert.NotEmpty(request.Headers.UserAgent); return Task.FromResult(respond(request)); }
    }
    private sealed class Storage : IInstallationFileSystem
    {
        public Dictionary<string, byte[]> Files { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> Directories { get; } = new(StringComparer.OrdinalIgnoreCase);
        public bool FileExists(string? path) => path is not null && Files.ContainsKey(path);
        public bool DirectoryExists(string path) => Directories.Contains(path);
        public IEnumerable<string> EnumerateDirectories(string path) => Directories.Where(item => Path.GetDirectoryName(item) == path).ToArray();
        public IEnumerable<string> EnumerateFiles(string path, string pattern, SearchOption option) => Files.Keys.Where(item => item.StartsWith(path + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && Path.GetFileName(item) == pattern && (option == SearchOption.AllDirectories || Path.GetDirectoryName(item) == path)).ToArray();
        public IEnumerable<string> EnumerateEntries(string path) => Files.Keys.Concat(Directories).Where(item => Path.GetDirectoryName(item) == path).ToArray();
        public void CreateDirectory(string path) { Directories.Add(path); var parent = Path.GetDirectoryName(path); if (parent is not null && parent != path) CreateDirectory(parent); }
        public Stream CreateFile(string path) => new CommitStream(bytes => Files[path] = bytes);
        public void MoveFile(string source, string destination) { Assert.False(Files.ContainsKey(destination)); Files.Add(destination, Files[source]); Files.Remove(source); }
        public void MoveDirectory(string source, string destination)
        {
            Assert.DoesNotContain(destination, Directories);
            var prefix = source + Path.DirectorySeparatorChar;
            foreach (var path in Files.Keys.Where(path => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray()) MoveFile(path, destination + path[source.Length..]);
            foreach (var path in Directories.Where(path => path == source || path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray()) { Directories.Remove(path); Directories.Add(destination + path[source.Length..]); }
        }
        public void DeleteDirectory(string path, bool recursive = false)
        {
            var prefix = path + Path.DirectorySeparatorChar;
            if (!recursive) Assert.Empty(EnumerateEntries(path));
            foreach (var file in Files.Keys.Where(file => file.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray()) Files.Remove(file);
            Directories.RemoveWhere(directory => directory == path || directory.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }
        private sealed class CommitStream(Action<byte[]> commit) : MemoryStream
        { protected override void Dispose(bool disposing) { if (disposing) commit(ToArray()); base.Dispose(disposing); } }
    }
}
