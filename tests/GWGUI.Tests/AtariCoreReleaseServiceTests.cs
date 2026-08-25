using GWGUI.Emulation.Atari;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Text.Json;

namespace GWGUI.Tests;

[SupportedOSPlatform("windows")]
public sealed class AtariCoreReleaseServiceTests
{
    private const string PublishedVersion = "20260816-120000";
    private const string LaterVersion = "20260817-120000";
    private const string PreviousLibraryContent = "previous";
    private const string ReplacementLibraryContent = "replacement";
    private const string MissingLibraryName = "other.dll";
    private const int ExpectedManifestArchiveSizeMinimum = 1;
    private const int ExpectedManifestLibrarySize = 11;
    private const int CancellationPayloadBufferCount = 2;
    private static readonly DateTimeOffset PublishedUtc =
        new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);

    public static TheoryData<string> ActualCoreFiles => new()
    {
        { "hatari.dll" },
        { "atari800.dll" },
        { "stella.dll" },
        { "prosystem.dll" },
        { "beetle-lynx.dll" },
        { "virtual-jaguar.dll" }
    };

    [Theory]
    [InlineData(AtariEmulator.Hatari)]
    [InlineData(AtariEmulator.Atari800)]
    [InlineData(AtariEmulator.Stella)]
    [InlineData(AtariEmulator.ProSystem)]
    [InlineData(AtariEmulator.BeetleLynx)]
    [InlineData(AtariEmulator.VirtualJaguar)]
    public async Task OfficialSourceReturnsEveryVersionItOffers(AtariEmulator kind)
    {
        using var client = new HttpClient(new ReleaseHandler(PublishedUtc, []));
        var service = new AtariCoreReleaseService(client, CreateTemporaryRoot());

        var releases = await service.GetAvailableAsync(kind);

        var release = Assert.Single(releases);
        Assert.Equal(kind, release.Emulator);
        Assert.Equal(PublishedVersion, release.DeclaredVersion);
        Assert.Equal(PublishedUtc, release.PublishedUtc);
        Assert.Equal(AtariCoreCatalog.Get(kind).ArchiveUri, release.DownloadUri);
    }

    [Fact]
    public async Task MissingSourceDateReportsTheFormatCause()
    {
        using var client = new HttpClient(new ReleaseHandler(null, []));
        var service = new AtariCoreReleaseService(client, CreateTemporaryRoot());

        var error = await Assert.ThrowsAsync<InvalidDataException>(() =>
            service.GetAvailableAsync(AtariEmulator.Hatari));

        Assert.Equal(AtariCoreReleaseErrors.MissingPublishedDate, error.Message);
    }

    [Fact]
    public async Task OfflineFailurePreservesTheHttpCause()
    {
        using var client = new HttpClient(new ReleaseHandler(PublishedUtc, [], HttpStatusCode.ServiceUnavailable));
        var service = new AtariCoreReleaseService(client, CreateTemporaryRoot());

        var error = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetAvailableAsync(AtariEmulator.Hatari));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, error.StatusCode);
    }

    [Fact]
    public async Task OfferedArchiveInstallsAndReplacesTheSameVersionWithoutDiagnosticBlocking()
    {
        var root = CreateTemporaryRoot();
        var entry = AtariCoreCatalog.Get(AtariEmulator.Hatari);
        var release = CreateRelease(entry.Emulator);
        try
        {
            using (var firstClient = new HttpClient(new ReleaseHandler(PublishedUtc,
                       CreateArchive(entry.DllName, PreviousLibraryContent))))
                await new AtariCoreReleaseService(firstClient, root).InstallAsync(release);
            var paths = AtariCoreCatalog.GetInstallationPaths(entry.Emulator, root, PublishedVersion);
            Assert.Equal(PreviousLibraryContent, await File.ReadAllTextAsync(paths.LibraryPath));

            var progress = new RecordingProgress();
            using (var secondClient = new HttpClient(new ReleaseHandler(PublishedUtc,
                       CreateArchive(entry.DllName, ReplacementLibraryContent))))
                await new AtariCoreReleaseService(secondClient, root).InstallAsync(release, progress);

            Assert.Equal(ReplacementLibraryContent, await File.ReadAllTextAsync(paths.LibraryPath));
            Assert.True(progress.Values.Count > 0);
            Assert.Equal(AtariCoreReleaseConstants.CompletedProgress,
                Assert.IsType<double>(progress.Values[^1].Fraction));
            using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(paths.ManifestPath));
            var document = manifest.RootElement;
            Assert.Equal(release.Id, document.GetProperty("releaseId").GetString());
            Assert.Equal(release.DeclaredVersion, document.GetProperty("releaseVersion").GetString());
            Assert.True(document.GetProperty("archiveSize").GetInt64()
                        >= ExpectedManifestArchiveSizeMinimum);
            Assert.Equal(ExpectedManifestLibrarySize, document.GetProperty("librarySize").GetInt64());
            Assert.False(string.IsNullOrWhiteSpace(document.GetProperty("librarySha256").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(document.GetProperty("architecture").GetString()));
            Assert.True(document.TryGetProperty("exports", out _));
            AssertNoTemporaryFiles(paths.VersionDirectory);
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task SelectingAnotherOfferedVersionAtomicallyChangesTheActiveInstallation()
    {
        var root = CreateTemporaryRoot();
        var entry = AtariCoreCatalog.Get(AtariEmulator.Hatari);
        var firstRelease = CreateRelease(entry.Emulator);
        var secondRelease = firstRelease with
        {
            Id = AtariCoreReleaseConstants.ReleaseIdPrefix + LaterVersion,
            DeclaredVersion = LaterVersion
        };
        using var client = new HttpClient(new ReleaseHandler(PublishedUtc,
            CreateArchive(entry.DllName, ReplacementLibraryContent)));
        var service = new AtariCoreReleaseService(client, root);
        try
        {
            var firstPaths = await service.InstallAsync(firstRelease);
            Assert.Equal(firstPaths, await service.GetActiveInstallationAsync(entry.Emulator));

            var secondPaths = await service.InstallAsync(secondRelease);

            Assert.NotEqual(firstPaths.VersionDirectory, secondPaths.VersionDirectory);
            Assert.Equal(secondPaths, await service.GetActiveInstallationAsync(entry.Emulator));
            using var marker = JsonDocument.Parse(await File.ReadAllTextAsync(
                AtariCoreCatalog.GetActiveManifestPath(entry.Emulator, root)));
            Assert.Equal(LaterVersion, marker.RootElement.GetProperty("releaseVersion").GetString());
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Theory]
    [MemberData(nameof(ActualCoreFiles))]
    [Trait("Category", "LocalAssets")]
    public void ActualCoreDiagnosticsCalculateArchitectureAndExports(string fileName)
    {
        var path = Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", fileName);

        Assert.Equal(AtariCoreCatalogConstants.WindowsX64Architecture,
            AtariCoreDiagnosticFunctions.ReadArchitecture(path));
        Assert.Contains(ExternalCoreExportNames.Initialize, AtariCoreDiagnosticFunctions.ReadExports(path));
        Assert.False(string.IsNullOrWhiteSpace(AtariCoreDiagnosticFunctions.CalculateSha256(path)));
    }

    [Fact]
    public async Task TruncatedArchiveReportsZipCauseAndCleansTemporaryFiles()
    {
        var root = CreateTemporaryRoot();
        var release = CreateRelease(AtariEmulator.Hatari);
        using var client = new HttpClient(new ReleaseHandler(PublishedUtc, [1, 2, 3]));
        try
        {
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                new AtariCoreReleaseService(client, root).InstallAsync(release));
            AssertNoTemporaryFiles(GetVersionDirectory(root, release));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task ArchiveWithoutExpectedLibraryIsRejectedAndCleaned()
    {
        var root = CreateTemporaryRoot();
        var release = CreateRelease(AtariEmulator.Hatari);
        using var client = new HttpClient(new ReleaseHandler(PublishedUtc,
            CreateArchive(MissingLibraryName, ReplacementLibraryContent)));
        try
        {
            var error = await Assert.ThrowsAsync<InvalidDataException>(() =>
                new AtariCoreReleaseService(client, root).InstallAsync(release));
            Assert.Contains(AtariCoreCatalog.Get(release.Emulator).DllName, error.Message, StringComparison.Ordinal);
            AssertNoTemporaryFiles(GetVersionDirectory(root, release));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task LockedInstalledLibraryIsNotDamagedAndTemporaryFilesAreCleaned()
    {
        var root = CreateTemporaryRoot();
        var release = CreateRelease(AtariEmulator.Hatari);
        var paths = AtariCoreCatalog.GetInstallationPaths(release.Emulator, root, release.DeclaredVersion);
        Directory.CreateDirectory(paths.VersionDirectory);
        await File.WriteAllTextAsync(paths.LibraryPath, PreviousLibraryContent);
        await using var locked = new FileStream(paths.LibraryPath, FileMode.Open, FileAccess.Read, FileShare.None);
        using var client = new HttpClient(new ReleaseHandler(PublishedUtc,
            CreateArchive(AtariCoreCatalog.Get(release.Emulator).DllName, ReplacementLibraryContent)));
        try
        {
            await Assert.ThrowsAnyAsync<IOException>(() =>
                new AtariCoreReleaseService(client, root).InstallAsync(release));
            Assert.Equal(PreviousLibraryContent, await ReadLockedFileAsync(locked));
            AssertNoTemporaryFiles(paths.VersionDirectory);
        }
        finally
        {
            await locked.DisposeAsync();
            DeleteDirectory(root);
        }
    }

    [Fact]
    public async Task CancellationCleansDownloadAndExtraction()
    {
        var root = CreateTemporaryRoot();
        var release = CreateRelease(AtariEmulator.Hatari);
        using var cancellation = new CancellationTokenSource();
        var progress = new CancelingProgress(cancellation);
        using var client = new HttpClient(new ReleaseHandler(PublishedUtc,
            new byte[AtariCoreReleaseConstants.DownloadBufferSize * CancellationPayloadBufferCount]));
        try
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                new AtariCoreReleaseService(client, root).InstallAsync(release, progress,
                    cancellationToken: cancellation.Token));
            Assert.True(progress.HasReported);
            AssertNoTemporaryFiles(GetVersionDirectory(root, release));
        }
        finally
        {
            DeleteDirectory(root);
        }
    }

    private static AtariCoreRelease CreateRelease(AtariEmulator kind) => new(kind,
        AtariCoreReleaseConstants.ReleaseIdPrefix + PublishedVersion, PublishedVersion,
        AtariCoreCatalog.Get(kind).ArchiveUri, PublishedUtc, null);

    private static byte[] CreateArchive(string libraryName, string content)
    {
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry(libraryName);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(content);
        }
        return output.ToArray();
    }

    private static string GetVersionDirectory(string root, AtariCoreRelease release) =>
        AtariCoreCatalog.GetInstallationPaths(release.Emulator, root, release.DeclaredVersion).VersionDirectory;

    private static async Task<string> ReadLockedFileAsync(FileStream stream)
    {
        stream.Position = AtariConstants.FirstBufferIndex;
        using var reader = new StreamReader(stream, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private static void AssertNoTemporaryFiles(string directory)
    {
        if (!Directory.Exists(directory)) return;
        Assert.DoesNotContain(Directory.EnumerateFiles(directory), path =>
            path.EndsWith(AtariCoreReleaseConstants.TemporaryDownloadExtension, StringComparison.Ordinal)
            || path.EndsWith(AtariCoreReleaseConstants.TemporaryExtractExtension, StringComparison.Ordinal)
            || path.EndsWith(AtariCoreReleaseConstants.TemporaryManifestExtension, StringComparison.Ordinal));
    }

    private static string CreateTemporaryRoot() => Path.Combine(Path.GetTempPath(),
        nameof(AtariCoreReleaseServiceTests), Guid.NewGuid().ToString("N"));

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "GWGUI.sln")))
            current = current.Parent;
        return current?.FullName ?? throw new DirectoryNotFoundException();
    }

    private static void DeleteDirectory(string root)
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }

    private sealed class RecordingProgress : IProgress<AtariCoreInstallProgress>
    {
        internal List<AtariCoreInstallProgress> Values { get; } = [];
        public void Report(AtariCoreInstallProgress value) => Values.Add(value);
    }

    private sealed class CancelingProgress(CancellationTokenSource cancellation)
        : IProgress<AtariCoreInstallProgress>
    {
        internal bool HasReported { get; private set; }

        public void Report(AtariCoreInstallProgress value)
        {
            HasReported = true;
            cancellation.Cancel();
        }
    }

    private sealed class ReleaseHandler(DateTimeOffset? modified, byte[] archive,
        HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = new HttpResponseMessage(statusCode) { Content = new ByteArrayContent(archive) };
            response.Content.Headers.ContentLength = archive.Length;
            response.Content.Headers.LastModified = modified;
            return Task.FromResult(response);
        }
    }
}
