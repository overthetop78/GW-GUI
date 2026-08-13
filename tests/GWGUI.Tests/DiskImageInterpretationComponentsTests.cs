using System.IO;
using GWGUI.MediaEngine.Exploration.Metadata;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Documents;
using GWGUI.MediaEngine.Exploration.Interpretation;
using GWGUI.MediaEngine.Exploration.Interpretation.Contracts;
using GWGUI.MediaEngine.Exploration.Scoring;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>VÃ©rifie les composants extraits de l'interprÃ©tation et de la construction des documents.</summary>
public sealed class DiskImageInterpretationComponentsTests
{
    [Fact]
    public void NormalizerRegistryUsesFirstSuccessfulPolicyAndKeepsSourceWithoutResult()
    {
        var calls = new List<string>();
        var source = Image(1, []);
        var expected = new SectorImage("normalized", 1, 1, 1, 1, [], logicalBlockCount: 1);
        var registry = new RecognizedImageNormalizerRegistry([new FakeNormalizer("first", calls, null), new FakeNormalizer("second", calls, expected), new FakeNormalizer("third", calls, source)]);
        Assert.Same(expected, registry.Normalize(source, "reader", Volume("VOL", [])));
        Assert.Equal(["first", "second"], calls);
        Assert.Same(source, new RecognizedImageNormalizerRegistry([]).Normalize(source, "reader", Volume("VOL", [])));
    }

    [Fact]
    public void AdditionalInterpretationRegistryPreservesPolicyOrderAndSupportsEmptyCollection()
    {
        var source = new SectorImage(DiskImageFormatIds.Ibm160, 1, 1, 1, 1, [], logicalBlockCount: 1);
        var registry = new AdditionalImageInterpretationRegistry([new FakeAdditionalPolicy("first"), new FakeAdditionalPolicy("second")]);
        Assert.Equal(["first", "second"], registry.Create(source).Select(image => image.FormatId));
        Assert.Empty(new AdditionalImageInterpretationRegistry([]).Create(source));
    }

    [Fact]
    public void DocumentFactoryBuildsRecognizedPhysicalAndUnknownDocuments()
    {
        var factory = new DiskImageDocumentFactory(new DiskImageMetadataFactory(new DiskSystemResolver(), new DiskProtectionResolver()));
        var image = Image(2, [Block(0, 0, 0, 1), Block(1, 0, 1, 2)]);
        var volume = Volume("VOL", [Entry("FILE", 1)]);
        var recognized = factory.Create("disk.img", image, [new("ibm.160", "fat12", image, volume)]);
        Assert.True(recognized.FileSystemRecognized);
        Assert.Same(volume, recognized.Volume);

        var physical = factory.Create("disk.img", image, []);
        Assert.False(physical.FileSystemRecognized);
        Assert.Equal("T00 H00", Assert.Single(physical.Volume.Entries).Name);
        Assert.Equal("S01.bin", physical.Volume.Entries[0].Children[1].Name);

        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, [1, 2, 3]);
            var unknown = factory.CreateUnknown(path);
            Assert.Equal(DiskImageFormatIds.Unknown, unknown.Image.FormatId);
            Assert.Equal(3, unknown.Volume.Capacity);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void DocumentFactoryHidesPhysicalSectorsForAnIdentifiedCustomAmigaLoader()
    {
        var factory = new DiskImageDocumentFactory(new DiskImageMetadataFactory(new DiskSystemResolver(), new DiskProtectionResolver()));
        var document = factory.Create("elf.adf", CustomAmigaImage(true), []);

        Assert.True(document.UsesCustomSectorLoader);
        Assert.Empty(document.Volume.Entries);
        Assert.Equal(DiskContentIds.CrackTheCompany, document.Metadata.Content.ModificationId);
        Assert.Equal([DiskContentIds.CompressionFire], document.Metadata.Content.CompressionIds);
        Assert.Null(document.Metadata.ProtectionId);
    }

    [Fact]
    public void DocumentFactoryKeepsPhysicalSectorsWhenTheAmigaBootLoaderIsInvalid()
    {
        var factory = new DiskImageDocumentFactory(new DiskImageMetadataFactory(new DiskSystemResolver(), new DiskProtectionResolver()));
        var document = factory.Create("damaged.adf", CustomAmigaImage(false), []);

        Assert.False(document.UsesCustomSectorLoader);
        Assert.NotEmpty(document.Volume.Entries);
    }

    [Fact]
    public void CataloglessBootDetectorRejectsSparseAndIncompleteImages()
    {
        var factory = new DiskImageDocumentFactory(new DiskImageMetadataFactory(new DiskSystemResolver(), new DiskProtectionResolver()));
        var sparse = new byte[2048];
        sparse.AsSpan(12, 64).Fill(1);
        var blocks = new[]
        {
            new SectorBlock(0, new(0, 0, 0), sparse[..512]),
            new SectorBlock(1, new(0, 0, 1), sparse[512..1024]),
            new SectorBlock(2, new(0, 0, 2), sparse[1024..1536]),
            new SectorBlock(3, new(0, 0, 3), sparse[1536..])
        };
        var sparseImage = new SectorImage(DiskImageFormatIds.AmigaDos, 512, 1, 1, 4, blocks);

        var document = factory.Create("sparse.adf", sparseImage, []);

        Assert.False(document.UsesCustomSectorLoader);
        Assert.NotEmpty(document.Volume.Entries);
    }

    [Fact]
    public void CataloglessBootDetectorAcceptsANearlyCompleteCapture()
    {
        const int blockCount = 100;
        var bytes = Enumerable.Repeat((byte)1, blockCount * 512).ToArray();
        var blocks = Enumerable.Range(0, 96)
            .Select(index => new SectorBlock(index, new(index / 2, index % 2, 0), bytes.AsMemory(index * 512, 512).ToArray()))
            .ToArray();
        var image = new SectorImage(DiskImageFormatIds.AmigaDos, 512, 50, 2, 1, blocks, logicalBlockCount: blockCount);
        var factory = new DiskImageDocumentFactory(new DiskImageMetadataFactory(new DiskSystemResolver(), new DiskProtectionResolver()));

        var document = factory.Create("partial.adf", image, []);

        Assert.True(document.UsesCustomSectorLoader);
        Assert.Empty(document.Volume.Entries);
    }

    [Fact]
    public void DocumentFactoryRecognizesAnAlignedAtnImploderArchiveWithoutInventingFiles()
    {
        var factory = new DiskImageDocumentFactory(new DiskImageMetadataFactory(new DiskSystemResolver(), new DiskProtectionResolver()));
        var document = factory.Create("archive.adf", AtnArchiveImage(), []);

        Assert.True(document.UsesCustomSectorLoader);
        Assert.Empty(document.Volume.Entries);
        Assert.Equal(DiskContentIds.OrganizationAtnArchive, document.Metadata.Content.OrganizationId);
        Assert.Equal(2, document.Metadata.Content.OrganizationMemberCount);
        Assert.Contains(DiskContentIds.CompressionAtnImploder, document.Metadata.Content.CompressionIds);
    }

    [Fact]
    public void AtnArchiveDetectorRejectsAnIsolatedSignatureAndInvalidSizes()
    {
        var bytes = new byte[1024];
        "ATN!"u8.CopyTo(bytes);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(4), 100);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(8), 2000);

        Assert.False(AtnImploderArchiveDetector.TryDetect(bytes, 512, out var memberCount));
        Assert.Equal(0, memberCount);
    }

    [Fact]
    public void DecodeScoreCoversEmptyPartialAndCompleteImages()
    {
        Assert.Equal(0, DiskImageDecodeScore.Calculate(Image(1, [])));
        Assert.Equal(.5, DiskImageDecodeScore.Calculate(Image(2, [Block(0, 0, 0, 1)])));
        Assert.Equal(1, DiskImageDecodeScore.Calculate(Image(2, [Block(0, 0, 0, 1), Block(1, 0, 1, 2)])));
    }

    [Fact]
    public void InterpretationIdentityIgnoresEntryOrderButPreservesHierarchyAndFamily()
    {
        var first = new ExploredFileSystem("ibm.160", "reader", Image(1, []), Volume("VOL", [Entry("B", 2), Entry("A", 1)]));
        var reordered = new ExploredFileSystem("ibm.360", "other", Image(1, []), Volume("VOL", [Entry("A", 1), Entry("B", 2)]));
        var otherFamily = reordered with { FormatId = "atari.360" };
        var otherHierarchy = new ExploredFileSystem("ibm.360", "reader", Image(1, []), Volume("VOL", [new FileSystemEntry("A", FileSystemEntryKind.Directory, 1, null, string.Empty, 0, 0, true, [Entry("B", 2)])]));
        Assert.Equal(FileSystemInterpretationIdentity.Create(first), FileSystemInterpretationIdentity.Create(reordered));
        Assert.NotEqual(FileSystemInterpretationIdentity.Create(first), FileSystemInterpretationIdentity.Create(otherFamily));
        Assert.NotEqual(FileSystemInterpretationIdentity.Create(first), FileSystemInterpretationIdentity.Create(otherHierarchy));
    }

    [Theory]
    [InlineData("ibm.720", "ibm")]
    [InlineData("atarist.720", "atarist")]
    [InlineData("amiga.amigados", "amiga")]
    public void InterpretationIdentityExtractsTheMachineFamily(string formatId, string expected)
    {
        Assert.Equal(expected, FileSystemInterpretationIdentity.FormatFamily(formatId));
    }

    [Fact]
    public void AlternativePolicyAppliesMinimumAndEntryCountThresholds()
    {
        Assert.True(FileSystemAlternativePolicy.IsCredible(Volume("VOL", [], ["1", "2"])));
        Assert.True(FileSystemAlternativePolicy.IsCredible(Volume("VOL", [], ["1", "2", "3"])));
        Assert.False(FileSystemAlternativePolicy.IsCredible(Volume("VOL", [], ["1", "2", "3", "4"])));
        Assert.True(FileSystemAlternativePolicy.IsCredible(Volume("VOL", [Entry("1", 1), Entry("2", 1), Entry("3", 1), Entry("4", 1)], ["1", "2", "3", "4"])));
    }

    private static SectorImage Image(int logicalBlocks, IEnumerable<SectorBlock> blocks) => new("test", 1, 1, 1, Math.Max(1, logicalBlocks), blocks, logicalBlockCount: logicalBlocks);
    private static SectorBlock Block(int logical, int cylinder, int sector, byte value) => new(logical, new(cylinder, 0, sector), [value]);
    private static FileSystemEntry Entry(string name, long size) => new(name, FileSystemEntryKind.File, size, null, string.Empty, 0, 0, true, []);
    private static FileSystemVolume Volume(string name, IEnumerable<FileSystemEntry> entries, IEnumerable<string>? warnings = null) => new(name, "test", 0, 0, null, null, entries, warnings ?? []);

    private static SectorImage CustomAmigaImage(bool validBootChecksum)
    {
        var bytes = new byte[1024];
        "DOS\0"u8.CopyTo(bytes);
        "CRACKED BY   THE COMPANY"u8.CopyTo(bytes.AsSpan(128));
        "FIRE"u8.CopyTo(bytes.AsSpan(512));
        if (validBootChecksum)
        {
            uint sum = 0;
            for (var offset = 0; offset < bytes.Length; offset += sizeof(uint))
            {
                var value = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset));
                var previous = sum;
                sum += value;
                if (sum < previous) sum++;
            }
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(4), ~sum);
        }
        return new(DiskImageFormatIds.AmigaDos, 512, 1, 1, 2, [new(0, new(0, 0, 0), bytes[..512]), new(1, new(0, 0, 1), bytes[512..])]);
    }

    private static SectorImage AtnArchiveImage()
    {
        var bytes = new byte[1024];
        WriteAtnHeader(bytes.AsSpan(0), 384, 128);
        WriteAtnHeader(bytes.AsSpan(512), 256, 96);
        return new(DiskImageFormatIds.AmigaDos, 512, 1, 1, 2, [new(0, new(0, 0, 0), bytes[..512]), new(1, new(0, 0, 1), bytes[512..])]);
    }

    private static void WriteAtnHeader(Span<byte> destination, uint expandedSize, uint compressedSize)
    {
        "ATN!"u8.CopyTo(destination);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(destination[4..], expandedSize);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(destination[8..], compressedSize);
    }

    private sealed class FakeNormalizer(string name, ICollection<string> calls, SectorImage? result) : IRecognizedImageNormalizer
    {
        public bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized)
        {
            calls.Add(name);
            normalized = result ?? image;
            return result is not null;
        }
    }

    private sealed class FakeAdditionalPolicy(string formatId) : IAdditionalImageInterpretationPolicy
    {
        public IEnumerable<SectorImage> CreateCandidates(SectorImage image)
        {
            yield return new(formatId, image.BlockSize, image.Cylinders, image.Heads, image.SectorsPerTrack, image.AvailableBlocks, logicalBlockCount: image.BlockCount);
        }
    }
}
