using GWGUI.App.Controls;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Metadata;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie la résolution et l'immuabilité des métadonnées techniques.</summary>
public sealed class DiskImageMetadataTests
{
    private readonly DiskImageMetadataFactory factory = new(new(), new());

    [Fact]
    public void FactoryPreservesDistinctSystemOrderAndResolvesProtection()
    {
        var image = Image(DiskImageFormatIds.AppleIIRwts18);
        var metadata = factory.Create(image, ["APPLE2.DOS33", DiskImageFormatIds.AtariSt720, DiskImageFormatIds.Ibm720, DiskImageFormatIds.AtariSt360]);
        Assert.Equal([DiskSystemIds.AppleII, DiskSystemIds.AtariSt, DiskSystemIds.IbmPc], metadata.SystemIds);
        Assert.Equal(DiskImageFormatIds.AppleIIRwts18, metadata.ProtectionId);
        Assert.Null(factory.Create(Image(DiskImageFormatIds.AppleIIDos33)).ProtectionId);
        Assert.Empty(factory.Create(Image(DiskImageFormatIds.Unknown)).SystemIds);
    }

    [Fact]
    public void MetadataAndExploredResultCopyTheirCollections()
    {
        var systems = new List<string> { DiskSystemIds.AppleII };
        var metadata = new DiskImageMetadata(systems, null);
        systems.Add(DiskSystemIds.Amiga);
        Assert.Equal([DiskSystemIds.AppleII], metadata.SystemIds);

        var formats = new List<string> { DiskImageFormatIds.Ibm160, "IBM.160", DiskImageFormatIds.AtariSt720 };
        var fileSystems = new List<ExploredFileSystem> { new(DiskImageFormatIds.Ibm160, "reader", Volume()) };
        var result = new ExploredDiskImage("disk.img", Image(DiskImageFormatIds.Ibm160), Volume(), metadata, true, fileSystems, formats);
        formats.Clear();
        fileSystems.Clear();
        Assert.Equal([DiskImageFormatIds.Ibm160, DiskImageFormatIds.AtariSt720], result.DetectedImageFormatIds);
        Assert.Single(result.DetectedFileSystems);
        Assert.Same(metadata, result.Metadata);
    }

    [Theory]
    [InlineData("apple2.dos33", DiskSystemIds.AppleII)]
    [InlineData("apple3.sos", DiskSystemIds.AppleIII)]
    [InlineData("mac.400", DiskSystemIds.Macintosh)]
    [InlineData("applemac.hfs", DiskSystemIds.Macintosh)]
    [InlineData("lisa.office", DiskSystemIds.Lisa)]
    [InlineData("applelisa.office", DiskSystemIds.Lisa)]
    [InlineData("amiga.amigados", DiskSystemIds.Amiga)]
    [InlineData("atarist.720", DiskSystemIds.AtariSt)]
    [InlineData("atari.90", DiskSystemIds.Atari8Bit)]
    [InlineData("ibm.720", DiskSystemIds.IbmPc)]
    [InlineData("commodore.1541", DiskSystemIds.Commodore)]
    [InlineData("amstrad.cpc", DiskSystemIds.Amstrad)]
    [InlineData("acorn.dfs.ss", DiskSystemIds.AcornBbc)]
    [InlineData("epson.qx10.320", DiskSystemIds.EpsonQx10)]
    [InlineData("msx.1d", DiskSystemIds.Msx)]
    [InlineData("dec.rx02", DiskSystemIds.Dec)]
    [InlineData("commodore900.coherent", DiskSystemIds.Coherent)]
    [InlineData("ucsd.ibm.mfm", DiskSystemIds.Ucsd)]
    public void SystemResolverSupportsEveryTechnicalFamily(string formatId, string expected) => Assert.Equal(expected, new DiskSystemResolver().ResolveId(formatId));

    [Fact]
    public void ResolversIgnoreCaseAndRejectUnknownValues()
    {
        Assert.Equal(DiskSystemIds.AtariSt, new DiskSystemResolver().ResolveId("ATARIST.720"));
        Assert.Null(new DiskSystemResolver().ResolveId("unknown.family"));
        Assert.Equal(DiskImageFormatIds.AppleIIRwts18, new DiskProtectionResolver().ResolveId(["APPLE2.RWTS18"]));
        Assert.Null(new DiskProtectionResolver().ResolveId([]));
    }

    [Fact]
    public void ApplicationPresentsTechnicalIdentifiersThroughResources()
    {
        var metadata = new DiskImageMetadata([DiskSystemIds.AppleII, DiskSystemIds.AtariSt], DiskImageFormatIds.AppleIIRwts18);
        Assert.Equal("Apple II + Atari ST", ExplorerMetadataPresenter.Systems(metadata));
        Assert.Contains("RWTS18", ExplorerMetadataPresenter.Protection(metadata));
        Assert.Equal("—", ExplorerMetadataPresenter.Systems(new DiskImageMetadata([], null)));
        Assert.Equal("—", ExplorerMetadataPresenter.Protection(new DiskImageMetadata([], null)));
    }

    private static SectorImage Image(string formatId) => new(formatId, 1, 1, 1, 1, [], logicalBlockCount: 1);
    private static FileSystemVolume Volume() => new("VOL", "test", 1, 0, null, null, [], []);
}
