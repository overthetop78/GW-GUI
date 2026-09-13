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
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.Exploration.Metadata;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Decoding.Scp.Sectors;
using GWGUI.MediaEngine.Recognition.Scp;
using GWGUI.MediaEngine.Conversion.Migration;
using GWGUI.MediaEngine.FileSystems.Fat12;

namespace GWGUI.Tests.MediaEngine.Exploration;

public sealed class MediaExplorerTests
{
    [Fact]
    public void ScpTrackSamplerCoversBothSidesAndTheMiddleOfAHybridCapture()
    {
        var revolution = new ScpRevolution(8_000_000, 3, [80, 120, 160]);
        var tracks = Enumerable.Range(0, 82).SelectMany(cylinder => new[]
        {
            new ScpTrack((byte)(cylinder * 2), cylinder, 0, [revolution]),
            new ScpTrack((byte)(cylinder * 2 + 1), cylinder, 1, [revolution])
        }).ToArray();

        var sample = ScpTrackSampler.Sample(tracks);

        Assert.Equal(ScpTrackSampler.MaximumTrackCount, sample.Count);
        Assert.Equal(tracks[0].TrackNumber, sample[0].TrackNumber);
        Assert.Equal(tracks[^1].TrackNumber, sample[^1].TrackNumber);
        Assert.Equal([0, 1], sample.Select(track => track.Head).Distinct().Order().ToArray());
        Assert.Contains(sample, track => track.Cylinder is >= 41 and <= 53);
    }

    [Fact]
    public void ScpAutomaticCandidatesKeepEveryProbedFamilyAndPruneTheOthers()
    {
        static ScpSectorImageCandidate Candidate(string id, ScpFormatFamily family) =>
            new(id, family, (_, _, _) => Task.FromResult(new SectorImage(id, 512, 80, 2, 9, [])));
        var amiga = Candidate(ScpCandidateIds.Amiga, ScpFormatFamily.Amiga);
        var iso = Candidate(ScpCandidateIds.IsoAutomatic, ScpFormatFamily.Iso);
        var apple = Candidate(ScpCandidateIds.Apple, ScpFormatFamily.Apple);
        var registry = new ScpCandidateRegistry(
            [],
            [amiga, iso, apple],
            [
                new(ScpFormatFamily.Amiga, [amiga]),
                new(ScpFormatFamily.Iso, [iso]),
                new(ScpFormatFamily.Apple, [apple])
            ],
            [ScpFormatFamily.Amiga, ScpFormatFamily.Iso, ScpFormatFamily.Apple],
            iso);

        var selected = registry.Automatic(new HashSet<ScpFormatFamily>
        {
            ScpFormatFamily.Amiga,
            ScpFormatFamily.Iso
        });

        Assert.Equal([ScpCandidateIds.Amiga, ScpCandidateIds.IsoAutomatic], selected.Select(candidate => candidate.Id));
    }

    [Fact]
    public void ScpProgressNamesProbeCandidateRevolutionAndCompletionStages()
    {
        var stages = new[]
        {
            new ScpExplorationProgress(ScpExplorationProgressKind.FormatProbeStarted, "Amiga", 0, 5),
            new ScpExplorationProgress(ScpExplorationProgressKind.FormatProbeCompleted, "Amiga", 1, 5, true),
            new ScpExplorationProgress(ScpExplorationProgressKind.CandidateStarted, "Amiga", 0, 2),
            new ScpExplorationProgress(ScpExplorationProgressKind.RevolutionDecoded, "0:0 · 1/3", 1, 492),
            new ScpExplorationProgress(ScpExplorationProgressKind.TrackDecoded, "0:0", 1, 164),
            new ScpExplorationProgress(ScpExplorationProgressKind.CandidateCompleted, "Amiga", 1, 2, true)
        };

        Assert.Equal(Enum.GetValues<ScpExplorationProgressKind>(), stages.Select(stage => stage.Kind));
        Assert.All(stages, stage => Assert.False(string.IsNullOrWhiteSpace(stage.Detail)));
        Assert.True(stages[1].Recognized);
        Assert.True(stages[^1].Recognized);
    }

    [Fact]
    public void NamedValidatedCatalogRemainsCredibleWhenMissingFileBlocksProduceManyWarnings()
    {
        var entries = Enumerable.Range(0, 3)
            .Select(index => new FileSystemEntry($"file-{index}", FileSystemEntryKind.File, 512, null, "", 0, index, true, []))
            .ToArray();
        var volume = new FileSystemVolume("POVRAY", "amigados.ofs", 901120, 0, null, null, entries, Enumerable.Repeat("Missing data block.", 100));

        Assert.True(GWGUI.MediaEngine.Exploration.Interpretation.FileSystemAlternativePolicy.IsCredible(volume));
    }

    [Fact]
    public void UnnamedValidatedFatCatalogRemainsCredibleWhenMissingFileBlocksProduceManyWarnings()
    {
        var entries = Enumerable.Range(0, 3)
            .Select(index => new FileSystemEntry($"FILE{index}.BIN", FileSystemEntryKind.File, 512, null, "", 0, index, true, []))
            .ToArray();
        var volume = new FileSystemVolume(string.Empty, "fat12", 737280, 0, null, null, entries, Enumerable.Repeat("Missing data sector.", 100));

        Assert.True(GWGUI.MediaEngine.Exploration.Interpretation.FileSystemAlternativePolicy.IsCredible(volume));
    }

    [Fact]
    public void PhysicalAlternativeDoesNotBorrowAnotherFormatsLogicalCatalog()
    {
        var amiga = new SectorImage(DiskImageFormatIds.AmigaDos, 512, 80, 2, 11, []);
        var atari = new SectorImage(DiskImageFormatIds.AtariSt720, 512, 80, 2, 9, []);
        var file = new FileSystemEntry("CCIII", FileSystemEntryKind.File, 89908, null, "", 0, 0, true, []);
        var volume = new FileSystemVolume("CCIII", "amigados.ofs", amiga.Capacity, 0, null, null, [file], []);
        var recognized = new ExploredFileSystem("amigados.ofs", amiga, volume);
        var source = new ExploredDiskImage(
            "hybrid.scp",
            amiga,
            volume,
            new DiskImageMetadata(["amiga", "atari-st"], null),
            true,
            [recognized],
            [atari],
            DiskImageFormatIds.AmigaDos);

        var selected = source.SelectFormat(DiskImageFormatIds.AtariSt720);

        Assert.NotNull(selected);
        Assert.Same(atari, selected.Image);
        Assert.Equal(DiskImageFormatIds.AtariSt720, selected.PrimaryFormatId);
        Assert.False(selected.FileSystemRecognized);
        Assert.NotSame(volume, selected.Volume);
        Assert.Equal(DiskImageFormatIds.AtariSt720, selected.Volume.FileSystemId);
        Assert.Empty(selected.Volume.Entries);
    }

    [Fact]
    public void ScpAlternativesKeepEveryValidatedPhysicalFamily()
    {
        static SectorImage Image(string formatId) => new(formatId, 512, 80, 2, 9, []);
        var amiga = Image(DiskImageFormatIds.AmigaDos);
        var ibm = Image(DiskImageFormatIds.Ibm720);
        var atari = Image(DiskImageFormatIds.AtariSt720);
        var ranking = new ScpCandidateRanker.Result(amiga, amiga, null, [], [amiga, ibm, atari], []);

        var alternatives = ScpAutomaticImageExplorer.CredibleImages(ranking, []);

        Assert.Equal(
            [DiskImageFormatIds.AmigaDos, DiskImageFormatIds.Ibm720, DiskImageFormatIds.AtariSt720],
            alternatives.Select(image => image.FormatId));
    }

    [Fact]
    public void ScpRankingNormalizesPartialTriFormatTracksAndRejectsContradictoryGeometry()
    {
        static SectorImage Image(string formatId, int cylinders, int heads, int sectorsPerTrack, int blockCount)
        {
            var blocks = Enumerable.Range(0, blockCount).Select(index => new SectorBlock(
                index,
                new SectorAddress(index / Math.Max(1, heads * sectorsPerTrack), index / Math.Max(1, sectorsPerTrack) % heads, index % sectorsPerTrack + 1),
                new byte[512],
                true));
            return new(formatId, 512, cylinders, heads, sectorsPerTrack, blocks);
        }

        var header = new ScpHeader(0, 0x80, 3, 0, 163, 0, 0, 0, 0, 0);
        var tracks = Enumerable.Range(0, 82).SelectMany(cylinder => new[]
        {
            new ScpTrack((byte)(cylinder * 2), cylinder, 0, []),
            new ScpTrack((byte)(cylinder * 2 + 1), cylinder, 1, [])
        }).ToArray();
        var source = new ScpImage(header, tracks, true, 1);
        var amiga = Image(DiskImageFormatIds.AmigaDos, 80, 2, 11, 1012);
        var atariPartial = Image("atarist.333", 37, 2, 9, 612);
        var ibmPartial = Image("ibm.333", 37, 2, 9, 612);
        var acornPartial = Image(DiskImageFormatIds.AcornAdfs800, 37, 2, 9, 612);
        var ucsdSingleSided = Image(DiskImageFormatIds.UcsdIbmMfm, 58, 1, 8, 273);
        var inspections = new[] { amiga, atariPartial, ibmPartial, acornPartial, ucsdSingleSided }
            .Select(image => new ScpCandidateInspection(image.FormatId, image, [], null));

        var ranking = ScpCandidateRanker.Rank(inspections, source);

        Assert.Equal(
            [DiskImageFormatIds.AmigaDos, DiskImageFormatIds.AtariSt720, DiskImageFormatIds.Ibm720],
            ranking.DecodedImages.Select(image => image.FormatId));
        Assert.All(ranking.DecodedImages, image =>
        {
            Assert.InRange(image.Cylinders, 80, 82);
            Assert.Equal(2, image.Heads);
        });
        Assert.Equal(2, ranking.Rejected.Count);
    }

    [Fact]
    public void ScpLogicalGeometryNormalizationPreservesACompleteFat12Catalog()
    {
        var entry = new MigrationEntry(
            "ENTOMBED.PRG",
            "ENTOMBED.PRG",
            FileSystemEntryKind.File,
            new byte[] { 1, 2, 3 },
            null,
            string.Empty,
            0,
            true,
            []);
        var logicalDisk = new Fat12VolumeWriter().Create(
            new MigrationPlan("synthetic", "fat12", "DEMO", [entry]),
            DiskImageFormatIds.AtariSt720);
        var capturedDisk = new SectorImage(
            DiskImageFormatIds.AtariSt720,
            512,
            82,
            2,
            9,
            logicalDisk.AvailableBlocks);
        var source = new ScpImage(
            new ScpHeader(0, 0x80, 3, 0, 163, 0, 0, 0, 0, 0),
            Enumerable.Range(0, 82).SelectMany(cylinder => new[]
            {
                new ScpTrack((byte)(cylinder * 2), cylinder, 0, []),
                new ScpTrack((byte)(cylinder * 2 + 1), cylinder, 1, [])
            }).ToArray(),
            true,
            1);
        var reader = new Fat12FileSystemReader();

        Assert.Equal(82, capturedDisk.Cylinders);
        var normalized = Assert.IsType<SectorImage>(ScpCandidateRanker.NormalizeCompatibleGeometry(capturedDisk, source));
        Assert.Equal(80, normalized.Cylinders);
        Assert.True(reader.CanRead(normalized));
        Assert.Equal("ENTOMBED.PRG", Assert.Single(reader.Read(normalized).Entries).Name);
    }

    [Fact]
    public void ScpRankingDoesNotPromoteResidualMfmTracksToARecognizedFormat()
    {
        static SectorImage Hybrid(string formatId, int completeTrackCount)
        {
            var blocks = Enumerable.Range(0, completeTrackCount)
                .SelectMany(track => Enumerable.Range(1, 9).Select(sector => new SectorBlock(
                    track * 9 + sector - 1,
                    new SectorAddress(41 + track / 2, track % 2, sector),
                    new byte[512],
                    true)))
                .ToArray();
            return new(formatId, 512, 80, 2, 9, blocks);
        }

        var header = new ScpHeader(0, 0x80, 3, 0, 163, 0, 0, 0, 0, 0);
        var tracks = Enumerable.Range(0, 82).SelectMany(cylinder => new[]
        {
            new ScpTrack((byte)(cylinder * 2), cylinder, 0, []),
            new ScpTrack((byte)(cylinder * 2 + 1), cylinder, 1, [])
        }).ToArray();
        var source = new ScpImage(header, tracks, true, 1);
        var ibmResidue = Hybrid(DiskImageFormatIds.Ibm720, 20);
        var atariResidue = Hybrid(DiskImageFormatIds.AtariSt720, 19);

        var ranking = ScpCandidateRanker.Rank([
            new(ibmResidue.FormatId, ibmResidue, [], null),
            new(atariResidue.FormatId, atariResidue, [], null)
        ], source);

        Assert.Empty(ranking.DecodedImages);
    }

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
        Assert.Same(document.Representation, explored.Document.Representation);
        Assert.Single(explored.Document.Volumes);
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
