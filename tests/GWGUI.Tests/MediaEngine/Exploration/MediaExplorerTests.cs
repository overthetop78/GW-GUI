using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sectors;
using GWGUI.MediaEngine.Visualization;
using GWGUI.MediaEngine.Visualization.Providers;

namespace GWGUI.Tests.MediaEngine.Exploration;

public sealed class MediaExplorerTests
{
    [Fact]
    public async Task OneReadDocumentFeedsExplorationAndVisualization()
    {
        var reader = new CountingReader();
        var reading = new MediaImageReadingService(new MediaRecognitionRegistry([reader]));
        var source = new MediaSourceDescriptor("memory.img", [], 512, "test.sector");

        var document = await reading.ReadAsync(source);
        var explored = new MediaExplorer(new FileSystemRegistry([])).Explore(document);
        var descriptor = new MediaVisualizationProviderRegistry([new SectorMediaVisualizationProvider()])
            .CreateDescriptor(document);

        Assert.Equal(1, reader.ReadCount);
        Assert.Same(document, explored.Document);
        Assert.Equal(MediaRepresentationKind.Sectors, descriptor.RepresentationKind);
        Assert.Single(descriptor.Elements);
    }

    private sealed class CountingReader : IMediaImageReader
    {
        public int ReadCount { get; private set; }

        public IReadOnlySet<string> FormatIds { get; } = new HashSet<string> { "test.sector" };

        public IReadOnlySet<string> Extensions { get; } = new HashSet<string> { ".img" };

        public IReadOnlyList<ReadOnlyMemory<byte>> Signatures { get; } = [];

        public IReadOnlySet<string> AssociatedFileExtensions { get; } = new HashSet<string>();

        public IReadOnlySet<MediaKind> MediaKinds { get; } = new HashSet<MediaKind> { MediaKind.Floppy };

        public IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; } =
            new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sectors };

        public bool SupportsFormatId(string formatId) => FormatIds.Contains(formatId);

        public ValueTask<bool> CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken) =>
            ValueTask.FromResult(true);

        public Task<MediaImageDocument> ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReadCount++;
            var sectorImage = new SectorImage(
                "test.sector",
                512,
                1,
                1,
                1,
                [new SectorBlock(0, new SectorAddress(0, 0, 1), new byte[512])]);
            return Task.FromResult(new MediaImageDocument(
                context.Source,
                "test.sector",
                MediaKind.Floppy,
                new SectorMediaImageRepresentation(sectorImage),
                [],
                [],
                new Dictionary<string, string>()));
        }
    }
}
