using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Conversion;
using GWGUI.MediaEngine.Interfaces.Conversion;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Representations.Blocks;
using GWGUI.MediaEngine.Representations.Sectors;
using GWGUI.MediaEngine.Writing;

namespace GWGUI.Tests.MediaEngine.Conversion;

public sealed class MediaConversionServiceTests
{
    [Fact]
    public async Task DirectWriterReceivesTheAlreadyReadDocument()
    {
        var source = Document(new BlockMediaImageRepresentation(512, [(0, 512)]));
        var writer = new TestWriter("direct", MediaRepresentationKind.Blocks);
        var service = Service([], writer);

        var result = await service.ConvertAsync(new MediaConversionRequest(source, "result.img", "target"));

        Assert.Same(source, writer.ReceivedDocument);
        Assert.Equal(["result.img"], result.ProducedFiles);
    }

    [Fact]
    public async Task ConverterPreparesTheRepresentationRequiredByTheWriter()
    {
        var source = Document(new BlockMediaImageRepresentation(512, [(0, 512)]));
        var converter = new TestConverter();
        var writer = new TestWriter("sector", MediaRepresentationKind.Sectors);
        var service = Service([converter], writer);

        var result = await service.ConvertAsync(new MediaConversionRequest(source, "result.img", "target"));

        Assert.Same(source, converter.ReceivedDocument);
        Assert.IsType<SectorMediaImageRepresentation>(writer.ReceivedDocument!.Representation);
        Assert.Contains("converted", result.Diagnostics);
        Assert.Contains("block-layout", result.Losses);
    }

    private static MediaConversionService Service(
        IReadOnlyList<IMediaRepresentationConverter> converters,
        params IMediaImageWriter[] writers)
    {
        var writerRegistry = new MediaImageWriterRegistry(writers);
        return new MediaConversionService(
            new MediaRepresentationConverterRegistry(converters),
            writerRegistry,
            new MediaImageWritingService(writerRegistry));
    }

    private static MediaImageDocument Document(GWGUI.MediaEngine.Interfaces.IMediaImageRepresentation representation) =>
        new(
            new MediaSourceDescriptor("already-read.img", [], 512),
            "source",
            MediaKind.HardDisk,
            representation,
            [],
            ["source-diagnostic"],
            new Dictionary<string, string>());

    private sealed class TestWriter(string id, MediaRepresentationKind representationKind) : IMediaImageWriter
    {
        public string Id => id;

        public IReadOnlySet<string> FormatIds { get; } = new HashSet<string> { "target" };

        public IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; } =
            new HashSet<MediaRepresentationKind> { representationKind };

        public IReadOnlySet<string> ProducedFileExtensions { get; } = new HashSet<string> { ".img" };

        public bool ProducesMultipleFiles => false;

        public MediaImageDocument? ReceivedDocument { get; private set; }

        public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension) =>
            RepresentationKinds.Contains(document.Representation.RepresentationKind) &&
            targetFormatId == "target" &&
            targetExtension.Equals(".img", StringComparison.OrdinalIgnoreCase);

        public Task<IReadOnlyList<string>> WriteAsync(
            MediaImageDocument document,
            string outputPath,
            string targetFormatId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReceivedDocument = document;
            return Task.FromResult<IReadOnlyList<string>>([outputPath]);
        }
    }

    private sealed class TestConverter : IMediaRepresentationConverter
    {
        public string Id => "blocks-to-sectors";

        public IReadOnlySet<MediaRepresentationKind> SourceRepresentationKinds { get; } =
            new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Blocks };

        public IReadOnlySet<MediaRepresentationKind> TargetRepresentationKinds { get; } =
            new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sectors };

        public MediaImageDocument? ReceivedDocument { get; private set; }

        public bool CanConvert(
            MediaImageDocument source,
            string targetFormatId,
            MediaRepresentationKind targetRepresentationKind) =>
            targetFormatId == "target" && targetRepresentationKind == MediaRepresentationKind.Sectors;

        public Task<MediaRepresentationConversionResult> ConvertAsync(
            MediaImageDocument source,
            string targetFormatId,
            MediaRepresentationKind targetRepresentationKind,
            IReadOnlyDictionary<string, string> options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReceivedDocument = source;
            var image = new SectorImage(
                targetFormatId,
                512,
                1,
                1,
                1,
                [new SectorBlock(0, new SectorAddress(0, 0, 1), new byte[512])]);
            var converted = new MediaImageDocument(
                source.Source,
                targetFormatId,
                source.MediaKind,
                new SectorMediaImageRepresentation(image),
                source.Volumes,
                source.Diagnostics,
                source.Metadata);
            return Task.FromResult(new MediaRepresentationConversionResult(
                converted,
                ["converted"],
                ["block-layout"]));
        }
    }
}
