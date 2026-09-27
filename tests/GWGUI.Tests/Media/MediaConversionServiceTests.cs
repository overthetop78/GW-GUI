using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Conversion;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Decoding;
using SectorFileSystemRegistry = GWGUI.MediaFileSystems.Exploration.SectorFileSystemRegistry;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Conversion;
using GWGUI.MediaEngine.Interfaces.Writing;

namespace GWGUI.Tests.Media;

public sealed class MediaConversionServiceTests
{
    [Fact]
    public void EveryScpConversionCandidateProvidesLiveProgress()
    {
        var composition = new ScpSectorDecodingComposition(new ScpReader(), new SectorFileSystemRegistry());
        Assert.All(composition.Candidates.Default(), candidate => Assert.NotNull(candidate.ProgressiveReadAsync));

        var formats = new[]
        {
            DiskImageFormatIds.AmigaDos,
            DiskImageFormatIds.Commodore1541,
            DiskImageFormatIds.AmstradCpc,
            DiskImageFormatIds.IbmScan,
            DiskImageFormatIds.AcornAdfs800,
            DiskImageFormatIds.DecRx02,
            DiskImageFormatIds.EpsonQx10_320,
            DiskImageFormatIds.AtariSt720,
            DiskImageFormatIds.AppleIIDos33
        };
        Assert.All(formats, format => Assert.NotNull(composition.Candidates.Selected(format)?.ProgressiveReadAsync));
    }

    [Fact]
    public async Task DirectSectorConversionReportsEveryTrackForAnySectorFormat()
    {
        var blocks = Enumerable.Range(0, 4)
            .Select(logicalBlock => new SectorBlock(
                logicalBlock,
                new SectorAddress(logicalBlock / 2, logicalBlock % 2, 1),
                new byte[] { (byte)logicalBlock }))
            .ToArray();
        var source = new MediaImageDocument(
            new MediaSourceDescriptor("source.adf", []),
            TestWriter.TargetFormat,
            MediaKind.Floppy,
            new SectorMediaImageRepresentation(new SectorImage(
                TestWriter.TargetFormat,
                1,
                cylinders: 2,
                heads: 2,
                sectorsPerTrack: 1,
                blocks)),
            [],
            [],
            new Dictionary<string, string>());
        var writer = new TestWriter();
        var writers = new MediaImageWriterRegistry([writer]);
        var service = new MediaConversionService(
            new MediaRepresentationConverterRegistry([]),
            writers,
            new MediaImageWritingService(writers));
        var reported = new List<MediaExplorationProgress>();

        await service.ConvertAsync(new MediaConversionRequest(
            source,
            "converted.img",
            TestWriter.TargetFormat,
            progress: reported.Add));

        Assert.Contains(reported, item => item.Detail == "Converting c=0,1:h=0,1");
        Assert.Equal(
            ["T0.0", "T0.1", "T1.0", "T1.1"],
            reported.Where(item => item.Detail.StartsWith('T')).Select(item => item.Detail));
        Assert.Equal(100, reported[^1].Value);
    }

    [Fact]
    public async Task ConvertedRepresentationDestinationIsAdvertisedExecutedAndReported()
    {
        var source = new MediaImageDocument(
            new MediaSourceDescriptor("source.synthetic", []),
            "source.flux",
            MediaKind.Floppy,
            new TestRepresentation(MediaRepresentationKind.Flux),
            [],
            [],
            new Dictionary<string, string>());
        var converter = new TestConverter();
        var writer = new TestWriter();
        var writers = new MediaImageWriterRegistry([writer]);
        var service = new MediaConversionService(
            new MediaRepresentationConverterRegistry([converter]),
            writers,
            new MediaImageWritingService(writers));
        var reported = new List<MediaExplorationProgress>();

        var destination = Assert.Single(service.GetAvailableDestinations(source));
        Assert.Equal(TestWriter.TargetFormat, destination.FormatId);
        Assert.Equal(TestWriter.TargetExtension, destination.Extension);

        var result = await service.ConvertAsync(new MediaConversionRequest(
            source,
            "converted.img",
            TestWriter.TargetFormat,
            progress: reported.Add));

        Assert.True(converter.WasCalled);
        Assert.True(writer.WasCalled);
        Assert.Equal("converted.img", Assert.Single(result.ProducedFiles));
        Assert.Equal([0d, 50d, 95d, 100d], reported.Select(item => item.Value));
    }

    private sealed record TestRepresentation(MediaRepresentationKind RepresentationKind) : IMediaImageRepresentation
    {
        public long? LogicalLength => null;
        public bool SupportsRandomAccess => true;
        public bool SupportsSequentialAccess => true;
    }

    private sealed class TestConverter : IMediaRepresentationConverter
    {
        public bool WasCalled { get; private set; }
        public string Id => "test-flux-to-sectors";
        public IReadOnlySet<MediaRepresentationKind> SourceRepresentationKinds { get; } =
            new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Flux };
        public IReadOnlySet<MediaRepresentationKind> TargetRepresentationKinds { get; } =
            new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sectors };

        public bool CanConvert(MediaImageDocument source, string targetFormatId, MediaRepresentationKind targetRepresentationKind) =>
            source.Representation.RepresentationKind == MediaRepresentationKind.Flux &&
            targetFormatId == TestWriter.TargetFormat &&
            targetRepresentationKind == MediaRepresentationKind.Sectors;

        public Task<MediaRepresentationConversionResult> ConvertAsync(
            MediaImageDocument source,
            string targetFormatId,
            MediaRepresentationKind targetRepresentationKind,
            IReadOnlyDictionary<string, string> options,
            Action<MediaExplorationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            progress?.Invoke(new(
                MediaExplorationProgressStage.ReadingMedia,
                "test unit",
                50,
                source.MediaKind));
            var converted = new MediaImageDocument(
                source.Source,
                targetFormatId,
                source.MediaKind,
                new TestRepresentation(targetRepresentationKind),
                [],
                [],
                source.Metadata);
            return Task.FromResult(new MediaRepresentationConversionResult(converted, [], []));
        }
    }

    private sealed class TestWriter : IMediaImageWriter
    {
        internal const string TargetFormat = "target.sectors";
        internal const string TargetExtension = ".img";
        public bool WasCalled { get; private set; }
        public string Id => "test-sector-writer";
        public IReadOnlySet<string> FormatIds { get; } = new HashSet<string> { TargetFormat };
        public IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; } =
            new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sectors };
        public IReadOnlySet<string> ProducedFileExtensions { get; } = new HashSet<string> { TargetExtension };
        public bool ProducesMultipleFiles => false;

        public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension) =>
            document.Representation.RepresentationKind == MediaRepresentationKind.Sectors &&
            targetFormatId == TargetFormat &&
            targetExtension == TargetExtension;

        public Task<IReadOnlyList<string>> WriteAsync(
            MediaImageDocument document,
            string outputPath,
            string targetFormatId,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult<IReadOnlyList<string>>([outputPath]);
        }
    }
}
