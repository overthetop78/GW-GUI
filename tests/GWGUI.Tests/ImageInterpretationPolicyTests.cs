using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Interpretation;
using GWGUI.MediaEngine.Exploration.Interpretation.Contracts;
using GWGUI.MediaEngine.Exploration.Interpretation.Definitions;
using GWGUI.MediaEngine.Exploration.Interpretation.Policies;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie l'ordre, les copies et la conservation des images réinterprétées.</summary>
public sealed class ImageInterpretationPolicyTests
{
    [Theory]
    [InlineData("atarist.720")]
    [InlineData("atari.90")]
    [InlineData("ibm.720")]
    [InlineData("acorn.dfs.ss")]
    [InlineData("imd")]
    public void RegistryCallsPoliciesForEverySupportedSourceFamily(string formatId)
    {
        var policy = new TrackingPolicy("candidate");
        Assert.Single(new AdditionalImageInterpretationRegistry([policy]).Create(Image(formatId, 512)));
        Assert.Equal(1, policy.Calls);
    }

    [Fact]
    public void RegistryRejectsUnsupportedSourceAndCopiesPolicyCollection()
    {
        var first = new TrackingPolicy("first");
        var second = new TrackingPolicy("second");
        var source = new List<IAdditionalImageInterpretationPolicy> { first, second };
        var registry = new AdditionalImageInterpretationRegistry(source);
        source.Clear();
        Assert.Equal(["first", "second"], registry.Create(Image("ibm.720", 512)).Select(image => image.FormatId));
        Assert.Empty(registry.Create(Image("unknown", 512)));
        Assert.Equal(1, first.Calls);
        Assert.Equal(1, second.Calls);
    }

    [Theory]
    [InlineData(512, 5)]
    [InlineData(256, 5)]
    [InlineData(1024, 1)]
    [InlineData(123, 0)]
    public void CompatibleCatalogReturnsExpectedImmutableCandidates(int blockSize, int count)
    {
        var formats = CompatibleFormatCatalog.Resolve(blockSize);
        Assert.Equal(count, formats.Count);
        Assert.False(formats is IList<string> list && !list.IsReadOnly);
    }

    [Fact]
    public void CompatiblePolicyPreservesImageAndOmitsCurrentFormat()
    {
        var source = Image(DiskImageFormatIds.UcsdIbmMfm, 512);
        var candidates = new CompatibleFormatInterpretationPolicy().CreateCandidates(source).ToArray();
        Assert.Equal([DiskImageFormatIds.Commodore900Coherent, DiskImageFormatIds.EpsonQx10_396, DiskImageFormatIds.EpsonQx10_399, DiskImageFormatIds.EpsonQx10Logo], candidates.Select(image => image.FormatId));
        Assert.All(candidates, candidate => AssertImageContent(source, candidate));
    }

    [Fact]
    public void WithFormatIdPreservesEverySectorProperty()
    {
        var block = new SectorBlock(0, new SectorAddress(0, 0, 1), [1, 2], IntegrityValid: false, Revolution: 4, Tag: [3], FormatCode: 5, DiagnosticCode: 6);
        var source = new SectorImage("old", 1, 1, 1, 2, [block], true, 9, 2);
        var copy = source.WithFormatId("new");
        Assert.Equal("new", copy.FormatId);
        AssertImageContent(source, copy);
        Assert.Equal(source.MissingBlocks, copy.MissingBlocks);
    }

    private static SectorImage Image(string formatId, int blockSize) => new(formatId, blockSize, 1, 1, 1, [new SectorBlock(0, new SectorAddress(0, 0, 1), new byte[blockSize])]);

    private static void AssertImageContent(SectorImage expected, SectorImage actual)
    {
        Assert.Equal(expected.BlockSize, actual.BlockSize);
        Assert.Equal(expected.Cylinders, actual.Cylinders);
        Assert.Equal(expected.Heads, actual.Heads);
        Assert.Equal(expected.SectorsPerTrack, actual.SectorsPerTrack);
        Assert.Equal(expected.Capacity, actual.Capacity);
        Assert.Equal(expected.BlockCount, actual.BlockCount);
        var expectedBlocks = expected.AvailableBlocks.OrderBy(block => block.LogicalBlock).ToArray();
        var actualBlocks = actual.AvailableBlocks.OrderBy(block => block.LogicalBlock).ToArray();
        Assert.Equal(expectedBlocks.Length, actualBlocks.Length);
        for (var index = 0; index < expectedBlocks.Length; index++)
        {
            Assert.Equal(expectedBlocks[index].LogicalBlock, actualBlocks[index].LogicalBlock);
            Assert.Equal(expectedBlocks[index].Address, actualBlocks[index].Address);
            Assert.Equal(expectedBlocks[index].Data, actualBlocks[index].Data);
            Assert.Equal(expectedBlocks[index].Tag, actualBlocks[index].Tag);
            Assert.Equal(expectedBlocks[index].IntegrityValid, actualBlocks[index].IntegrityValid);
            Assert.Equal(expectedBlocks[index].Revolution, actualBlocks[index].Revolution);
            Assert.Equal(expectedBlocks[index].FormatCode, actualBlocks[index].FormatCode);
            Assert.Equal(expectedBlocks[index].DiagnosticCode, actualBlocks[index].DiagnosticCode);
        }
    }

    private sealed class TrackingPolicy(string formatId) : IAdditionalImageInterpretationPolicy
    {
        public int Calls { get; private set; }
        public IEnumerable<SectorImage> CreateCandidates(SectorImage image)
        {
            Calls++;
            yield return image.WithFormatId(formatId);
        }
    }
}
