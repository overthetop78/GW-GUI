using System.Collections.Frozen;
using System.IO;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

public sealed class FileSystemRegistryTests
{
    private static readonly SectorImage Image = new("format", 1, 1, 1, 1, [new SectorBlock(0, new(0, 0, 0), [0])]);

    [Fact]
    public void DefaultCatalogContainsTheEighteenReadersInOrder()
    {
        string[] expected = ["AmigaDosFileSystemReader", "AmigaFlatResourceArchiveReader", "AcornAdfsFileSystemReader", "BbcDfsFileSystemReader", "CoherentFileSystemReader", "Rt11FileSystemReader", "UcsdFileSystemReader", "AppleInformXzipFileSystemReader", "AppleDosFileSystemReader", "ProDosFileSystemReader", "MacMfsFileSystemReader", "MacHfsFileSystemReader", "LisaFileSystemReader", "AmstradCpmFileSystemReader", "CpmFileSystemReader", "CommodoreDosFileSystemReader", "Fat12FileSystemReader", "AtariDosFileSystemReader"];
        Assert.Equal(expected, new FileSystemRegistry().Readers.Select(reader => reader.GetType().Name));
    }

    [Fact]
    public void RegistryCopiesAndFreezesItsIndexes()
    {
        var source = new List<IFileSystemReader> { new FakeReader("one", ["format"], true) };
        var registry = new FileSystemRegistry(source);
        source.Add(new FakeReader("two", ["other"], true));
        Assert.Single(registry.Readers);
        Assert.IsAssignableFrom<FrozenSet<string>>(registry.SupportedFormatIds);
        Assert.Throws<NotSupportedException>(() => ((IList<IFileSystemReader>)registry.Readers).Add(source[1]));
    }

    [Fact]
    public void RegistryRejectsInvalidReadersAndDuplicateIds()
    {
        Assert.Throws<ArgumentException>(() => new FileSystemRegistry([null!]));
        Assert.Throws<ArgumentException>(() => new FileSystemRegistry([new FakeReader(" ", [], false)]));
        Assert.Throws<ArgumentException>(() => new FileSystemRegistry([new FakeReader("same", [], false), new FakeReader("SAME", [], false)]));
    }

    [Fact]
    public void RegistrySelectsByReaderAndFormatAndAllowsSharedFormats()
    {
        var first = new FakeReader("first", ["format"], true);
        var second = new FakeReader("second", ["format"], true);
        var registry = new FileSystemRegistry([first, second]);
        Assert.Equal(["second"], registry.ReadCandidates(Image, "second").Matches.Select(match => match.ReaderId));
        Assert.Equal(["first", "second"], registry.ReadCandidates(Image, "format").Matches.Select(match => match.ReaderId));
    }

    [Fact]
    public void RegistryKeepsFailureAndTriesTheNextCandidate()
    {
        var corrupt = new FakeReader("corrupt", ["format"], true, new InvalidDataException("corrupt"));
        var valid = new FakeReader("valid", ["format"], true);
        var registry = new FileSystemRegistry([corrupt, valid]);
        var report = registry.ReadAll(Image);
        Assert.Single(report.Failures);
        Assert.Equal("corrupt", report.Failures[0].ReaderId);
        Assert.Single(report.Matches);
        Assert.True(registry.TryRead(Image, null, out var match));
        Assert.NotNull(match);
    }

    [Fact]
    public void ReportDistinguishesUnrecognizedReadAndCorruptImages()
    {
        var unrecognized = new FileSystemRegistry([new FakeReader("reader", ["format"], false)]).ReadAll(Image);
        Assert.True(unrecognized.IsUnrecognized);
        var read = new FileSystemRegistry([new FakeReader("reader", ["format"], true)]).ReadAll(Image);
        Assert.True(read.HasMatches);
        var corrupt = new FileSystemRegistry([new FakeReader("reader", ["format"], true, new InvalidDataException())]).ReadAll(Image);
        Assert.True(corrupt.HasCorruption);
        Assert.False(corrupt.IsUnrecognized);
    }

    private sealed class FakeReader(string id, IEnumerable<string> formats, bool canRead, InvalidDataException? failure = null) : IFileSystemReader
    {
        public string Id { get; } = id;
        public IReadOnlySet<string> CatalogFormatIds { get; } = formats.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        public bool CanRead(SectorImage image) => canRead;
        public FileSystemVolume Read(SectorImage image) => failure is null ? new("volume", Id, image.Capacity, 0, null, null, [], []) : throw failure;
    }
}
