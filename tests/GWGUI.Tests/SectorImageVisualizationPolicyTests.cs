using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Geometries.Atari;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Visualization;
using GWGUI.MediaEngine.Visualization.Policies;
using System.IO;

namespace GWGUI.Tests;

public sealed class SectorImageVisualizationPolicyTests
{
    [Fact]
    public void ExactPolicyCopiesAndValidatesItsFormats()
    {
        var formats = new[] { "format.one", "format.two" };
        var policy = new ExactVisualizationPolicy("encoder", formats);
        formats[0] = "changed";

        Assert.True(policy.CanHandle(Image("FORMAT.ONE")));
        Assert.False(policy.CanHandle(Image("other")));
        Assert.Equal("encoder", policy.EncoderId(Image("format.two")));
        Assert.Throws<ArgumentException>(() => new ExactVisualizationPolicy("", "format"));
        Assert.Throws<ArgumentException>(() => new ExactVisualizationPolicy("encoder"));
        Assert.Throws<ArgumentException>(() => new ExactVisualizationPolicy("encoder", ""));
    }

    [Fact]
    public void PrefixPolicyCopiesAndValidatesItsPrefixes()
    {
        var prefixes = new[] { "family.", "other." };
        var policy = new PrefixVisualizationPolicy("encoder", prefixes);
        prefixes[0] = "changed";

        Assert.True(policy.CanHandle(Image("FAMILY.FORMAT")));
        Assert.False(policy.CanHandle(Image("unknown")));
        Assert.Throws<ArgumentException>(() => new PrefixVisualizationPolicy("", "family."));
        Assert.Throws<ArgumentException>(() => new PrefixVisualizationPolicy("encoder"));
        Assert.Throws<ArgumentException>(() => new PrefixVisualizationPolicy("encoder", ""));
    }

    [Fact]
    public void BasePolicyCreatesSizeCodesTagsAndDefaults()
    {
        var policy = new TestPolicy();
        var blocks = new[] { new SectorBlock(0, new SectorAddress(0, 0, 1), new byte[512], Tag: new byte[] { 3, 4 }) };
        var image = Image("test", 512, blocks: blocks);
        var sector = Assert.Single(policy.CreateTrackSectors(image, [(blocks[0], blocks[0].Address)]));

        Assert.Equal((byte)2, sector.SizeCode);
        Assert.Equal(3, sector.Attributes![TrackEncodingAttributeKeys.Tag(0)]);
        Assert.Equal(4, sector.Attributes[TrackEncodingAttributeKeys.Tag(1)]);
        Assert.Equal(TrackEncodingDefaults.BitCellTicks, policy.BitCellTicks(image, 0));
        Assert.Null(policy.TrackAttributes(image, 1));
    }

    [Theory]
    [InlineData(128, 0)]
    [InlineData(256, 1)]
    [InlineData(512, 2)]
    [InlineData(1024, 3)]
    [InlineData(2048, 4)]
    [InlineData(4096, 5)]
    [InlineData(8192, 6)]
    [InlineData(16384, 7)]
    public void BasePolicyUsesEveryIsoSectorSizeCode(int size, byte code)
    {
        var block = new SectorBlock(0, new SectorAddress(0, 0, 1), new byte[size]);
        var sector = Assert.Single(new TestPolicy().CreateTrackSectors(Image("test", size, blocks: [block]), [(block, block.Address)]));
        Assert.Equal(code, sector.SizeCode);
    }

    [Theory]
    [InlineData(DiskImageFormatIds.AppleIIRwts18, FluxCodecIds.AppleRwts18)]
    [InlineData(DiskImageFormatIds.AppleIIProDos, FluxCodecIds.AppleIIGcr)]
    [InlineData(DiskImageFormatIds.Mac400, FluxCodecIds.AppleMacGcr)]
    [InlineData(DiskImageFormatIds.AppleLisaPrefix, FluxCodecIds.AppleLisaFileWareGcr)]
    [InlineData(DiskImageFormatIds.Mac1440, FluxCodecIds.IsoMfm)]
    public void ApplePolicySelectsExpectedEncoder(string formatId, string encoderId)
    {
        var image = formatId == DiskImageFormatIds.AppleLisaPrefix ? Image(formatId, 512, 46, 2, 1) : Image(formatId);
        Assert.Equal(encoderId, new AppleVisualizationPolicy().EncoderId(image));
    }

    [Fact]
    public void ApplePolicySplitsProDosBlockAndIgnoresShortBlock()
    {
        var policy = new AppleVisualizationPolicy();
        var full = new SectorBlock(0, new SectorAddress(0, 0, 4), Enumerable.Range(0, 512).Select(value => (byte)value).ToArray());
        var shortBlock = new SectorBlock(1, new SectorAddress(0, 0, 5), new byte[511]);
        var sectors = policy.CreateTrackSectors(Image(DiskImageFormatIds.AppleIIProDos, 512, sectorsPerTrack: 2, blocks: [full, shortBlock], allowVariable: true), [(full, full.Address), (shortBlock, shortBlock.Address)]);

        Assert.Equal([8, 9], sectors.Select(sector => sector.Number));
        Assert.Equal(256, sectors[0].Data.Count);
        Assert.Equal(full.Data.Take(256), sectors[0].Data);
        Assert.Equal(full.Data.Skip(256), sectors[1].Data);
    }

    [Fact]
    public void ApplePolicyProducesNamedTrackAttributes()
    {
        var policy = new AppleVisualizationPolicy();

        Assert.Equal(16, policy.TrackAttributes(Image(DiskImageFormatIds.AppleIIProDos), 16)![TrackEncodingAttributeKeys.SectorsPerTrack]);
        Assert.Equal(0, policy.TrackAttributes(Image(DiskImageFormatIds.AppleIIProDos), 16)![TrackEncodingAttributeKeys.Format]);
        Assert.Equal(0x02, policy.TrackAttributes(Image(DiskImageFormatIds.Mac400), 12)![TrackEncodingAttributeKeys.Format]);
        Assert.Equal(0x22, policy.TrackAttributes(Image(DiskImageFormatIds.Mac800, heads: 2), 12)![TrackEncodingAttributeKeys.Format]);
        Assert.Equal(0x12, policy.TrackAttributes(Image(DiskImageFormatIds.AppleLisaPrefix), 22)![TrackEncodingAttributeKeys.Format]);
    }

    [Theory]
    [InlineData(DiskImageFormatIds.Atari90, FluxCodecIds.IsoFm)]
    [InlineData(DiskImageFormatIds.Atari130, FluxCodecIds.IsoMfm)]
    [InlineData(DiskImageFormatIds.AtariStPrefix, FluxCodecIds.IsoMfm)]
    public void AtariPolicySelectsExpectedEncoder(string formatId, string encoderId) => Assert.Equal(encoderId, new AtariVisualizationPolicy().EncoderId(Image(formatId)));

    [Fact]
    public void AtariPolicyConvertsLinearEnhancedDensityAddress()
    {
        var address = new AtariVisualizationPolicy().VisualAddress(Image(DiskImageFormatIds.Atari130, 128, Atari8BitGeometry.EnhancedSectorsPerTrack * 4, 1, 1), new SectorAddress(Atari8BitGeometry.EnhancedSectorsPerTrack, 0, 0));
        Assert.Equal(new SectorAddress(1, Atari8BitGeometry.FirstHead, Atari8BitGeometry.FirstSectorNumber), address);
    }

    [Fact]
    public void AtariPolicyPreservesStructuredAddress() => Assert.Equal(new SectorAddress(2, 1, 4), new AtariVisualizationPolicy().VisualAddress(Image(DiskImageFormatIds.AtariStPrefix, cylinders: 80, heads: 2), new SectorAddress(2, 1, 4)));

    [Fact]
    public void CommodorePolicyCombinesD81HalvesAndUsesZones()
    {
        var policy = new CommodoreVisualizationPolicy();
        var first = new SectorBlock(0, new SectorAddress(0, 0, 0), Enumerable.Repeat((byte)1, Commodore1581Geometry.LogicalBlockSize).ToArray());
        var second = new SectorBlock(1, new SectorAddress(0, 0, 1), Enumerable.Repeat((byte)2, Commodore1581Geometry.LogicalBlockSize).ToArray());
        var sector = Assert.Single(policy.CreateTrackSectors(Image(DiskImageFormatIds.Commodore1581, 256, 1, 1, 2, [first, second]), [(first, first.Address), (second, second.Address)]));

        Assert.Equal(Commodore1581Geometry.FirstPhysicalSectorNumber, sector.Number);
        Assert.Equal(Commodore1581Geometry.PhysicalSectorSize, sector.Data.Count);
        Assert.Equal(Commodore1581Geometry.PhysicalSectorSizeCode, sector.SizeCode);
        Assert.Equal([86u, 93u, 100u, 106u], new[] { 0, 39, 53, 64 }.Select(cylinder => policy.BitCellTicks(Image(DiskImageFormatIds.Commodore900Prefix), cylinder)));
    }

    [Fact]
    public void CommodorePolicyIgnoresIncompleteD81Half()
    {
        var block = new SectorBlock(0, new SectorAddress(0, 0, 0), new byte[Commodore1581Geometry.LogicalBlockSize]);
        Assert.Empty(new CommodoreVisualizationPolicy().CreateTrackSectors(Image(DiskImageFormatIds.Commodore1581, 256, blocks: [block]), [(block, block.Address)]));
    }

    [Fact]
    public void DecPolicySplitsLogicalBlockIntoPhysicalSectors()
    {
        var data = Enumerable.Range(0, DecRx02Geometry.LogicalBlockSize).Select(value => (byte)value).ToArray();
        var block = new SectorBlock(0, new SectorAddress(0, 0, DecRx02Geometry.FirstLogicalSectorNumber), data);
        var sectors = new DecRx02VisualizationPolicy().CreateTrackSectors(Image(DiskImageFormatIds.DecRx02, 512, blocks: [block]), [(block, block.Address)]);

        Assert.Equal(DecRx02Geometry.PhysicalSectorsPerLogicalBlock, sectors.Count);
        Assert.Equal([1, 2], sectors.Select(sector => sector.Number));
        Assert.Equal(data.Take(256), sectors[0].Data);
        Assert.Equal(data.Skip(256), sectors[1].Data);
    }

    [Fact]
    public void DecPolicyIgnoresShortLogicalBlock()
    {
        var block = new SectorBlock(0, new SectorAddress(0, 0, 1), new byte[DecRx02Geometry.LogicalBlockSize - 1]);
        Assert.Empty(new DecRx02VisualizationPolicy().CreateTrackSectors(Image(DiskImageFormatIds.DecRx02, 512, blocks: [block], allowVariable: true), [(block, block.Address)]));
    }

    [Fact]
    public void RegistryCopiesPoliciesAndReturnsFirstMatch()
    {
        var first = new TestPolicy();
        var second = new TestPolicy();
        var source = new List<ISectorImageVisualizationPolicy> { first, second };
        var registry = new SectorImageVisualizationPolicyRegistry(source);
        source.Clear();

        Assert.Same(first, registry.Resolve(Image("test")));
        Assert.Throws<ArgumentNullException>(() => new SectorImageVisualizationPolicyRegistry(null!));
        Assert.Throws<ArgumentException>(() => new SectorImageVisualizationPolicyRegistry([null!]));
        Assert.Throws<ArgumentNullException>(() => registry.Resolve(null!));
    }

    [Theory]
    [InlineData(DiskImageFormatIds.AmigaPrefix, FluxCodecIds.AmigaMfm)]
    [InlineData(DiskImageFormatIds.AcornDfsPrefix, FluxCodecIds.IsoFm)]
    [InlineData(DiskImageFormatIds.AcornAdfsPrefix, FluxCodecIds.IsoMfm)]
    [InlineData(DiskImageFormatIds.IbmPrefix, FluxCodecIds.IsoMfm)]
    [InlineData(DiskImageFormatIds.Imd, FluxCodecIds.IsoMfm)]
    [InlineData(DiskImageFormatIds.Td0, FluxCodecIds.IsoMfm)]
    public void DefaultRegistryMapsFamiliesToExpectedCodecs(string formatId, string codecId)
    {
        var image = Image(formatId);
        Assert.Equal(codecId, new SectorImageVisualizationPolicyRegistry().Resolve(image)!.EncoderId(image));
    }

    [Fact]
    public void VisualizerUsesInjectedPolicyAndBuildsOrderedScpTracks()
    {
        var blocks = new[]
        {
            new SectorBlock(0, new SectorAddress(1, 1, 1), new byte[512]),
            new SectorBlock(1, new SectorAddress(0, 0, 1), new byte[512])
        };
        var image = Image("injected", 512, 1, 1, 2, blocks);
        var visualizer = new SectorImageFluxVisualizer(null, new SectorImageVisualizationPolicyRegistry([new TestPolicy()]));

        var scp = visualizer.Create(image);

        Assert.True(visualizer.CanVisualize(image));
        Assert.Equal([0, 3], scp.Tracks.Select(track => track.TrackNumber));
        Assert.Equal(ScpVisualizationDefaults.Version, scp.Header.Version);
        Assert.Equal(ScpVisualizationDefaults.RevolutionCount, scp.Header.Revolutions);
    }

    [Fact]
    public void VisualizerReportsMissingPolicyEncoderAndEmptyTrack()
    {
        var image = Image("test");
        Assert.Throws<NotSupportedException>(() => new SectorImageFluxVisualizer(null, new SectorImageVisualizationPolicyRegistry([])).Create(image));
        Assert.Contains(image.FormatId, Assert.Throws<NotSupportedException>(() => new SectorImageFluxVisualizer(new FluxEncoderRegistry([]), new SectorImageVisualizationPolicyRegistry([new TestPolicy()])).Create(image)).Message);
        Assert.Throws<InvalidDataException>(() => new SectorImageFluxVisualizer(null, new SectorImageVisualizationPolicyRegistry([new EmptyPolicy()])).Create(image));
    }

    private static SectorImage Image(string formatId, int blockSize = 512, int cylinders = 1, int heads = 1, int sectorsPerTrack = 1, IEnumerable<SectorBlock>? blocks = null, bool allowVariable = false) => new(formatId, blockSize, cylinders, heads, sectorsPerTrack, blocks ?? [new SectorBlock(0, new SectorAddress(0, 0, 1), new byte[blockSize])], allowVariable);

    private class TestPolicy : SectorImageVisualizationPolicy
    {
        public override bool CanHandle(SectorImage image) => true;
        public override string EncoderId(SectorImage image) => FluxCodecIds.IsoMfm;
    }

    private sealed class EmptyPolicy : TestPolicy
    {
        public override IReadOnlyList<TrackSector> CreateTrackSectors(SectorImage image, IReadOnlyList<(SectorBlock Block, SectorAddress Address)> items) => [];
    }
}
