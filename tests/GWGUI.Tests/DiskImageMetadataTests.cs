using GWGUI.App.Presenters.Explorer;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Metadata;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
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

        var formats = new List<SectorImage>
        {
            Image(DiskImageFormatIds.Ibm160),
            Image("IBM.160"),
            Image(DiskImageFormatIds.AtariSt720)
        };
        var image = Image(DiskImageFormatIds.Ibm160);
        var fileSystems = new List<ExploredFileSystem> { new(DiskImageFormatIds.Ibm160, "reader", image, Volume()) };
        var result = new ExploredDiskImage("disk.img", image, Volume(), metadata, true, fileSystems, formats);
        formats.Clear();
        fileSystems.Clear();
        Assert.Equal([DiskImageFormatIds.Ibm160, DiskImageFormatIds.AtariSt720], result.DetectedImageFormatIds);
        Assert.Single(result.DetectedFileSystems);
        Assert.Same(metadata, result.Metadata);
    }

    [Fact]
    public void MultiformatResultKeepsTheImageVolumeAndEntriesOfEveryDetectedFormat()
    {
        var amigaImage = ImageWithData(DiskImageFormatIds.AmigaDos, 0x11);
        var atariImage = ImageWithData(DiskImageFormatIds.AtariSt720, 0x22);
        var amigaVolume = Volume("AMIGA", "amigados.ofs", "AMIGA-FILE");
        var atariVolume = Volume("ATARI", "fat12", "ATARI.PRG");
        var fileSystems = new[]
        {
            new ExploredFileSystem(amigaImage.FormatId, "amigados.ofs", amigaImage, amigaVolume),
            new ExploredFileSystem(atariImage.FormatId, "fat12", atariImage, atariVolume)
        };
        var metadata = new DiskImageMetadata([DiskSystemIds.Amiga, DiskSystemIds.AtariSt], null);

        var result = new ExploredDiskImage(
            "multi.scp",
            amigaImage,
            amigaVolume,
            metadata,
            true,
            fileSystems,
            [amigaImage, atariImage],
            amigaImage.FormatId);

        Assert.Equal(2, result.FormatsDetectes.Count);
        var amiga = Assert.Single(result.FormatsDetectes, format => format.FormatId == amigaImage.FormatId);
        var atari = Assert.Single(result.FormatsDetectes, format => format.FormatId == atariImage.FormatId);
        Assert.Equal("AMIGA", amiga.NomVolume);
        Assert.Equal("AMIGA-FILE", Assert.Single(amiga.Entrees).Nom);
        Assert.Equal(0x11, Assert.Single(amiga.Secteurs).Donnees.Span[0]);
        Assert.Equal("ATARI", atari.NomVolume);
        Assert.Equal("ATARI.PRG", Assert.Single(atari.Entrees).Nom);
        Assert.Equal(0x22, Assert.Single(atari.Secteurs).Donnees.Span[0]);

        var selected = Assert.IsType<ExploredDiskImage>(result.SelectFormat(atariImage.FormatId));
        Assert.Same(atariImage, selected.Image);
        Assert.Same(atariVolume, selected.Volume);
        Assert.Equal(atariImage.FormatId, selected.PrimaryFormatId);
        Assert.Equal("ATARI.PRG", Assert.Single(selected.Volume.Entries).Name);
        Assert.Equal(2, selected.FormatsDetectes.Count);
    }

    [Fact]
    public void SectorImageContractExposesItsSourceSectorsOnLogicalTracks()
    {
        var image = ImageWithData(DiskImageFormatIds.AtariSt720, 0x5A);
        var result = new ExploredDiskImage(
            "disk.st",
            image,
            Volume("ATARI", "fat12", "AUTO.PRG"),
            new DiskImageMetadata([DiskSystemIds.AtariSt], null));

        var track = Assert.Single(result.Pistes);
        Assert.Empty(track.Revolutions);
        var sector = Assert.Single(track.SecteursSource);
        Assert.Equal(0x5A, sector.Donnees.Span[0]);
        Assert.Equal("ST", result.TypeImage);
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

    private static SectorImage ImageWithData(string formatId, byte value)
    {
        return new SectorImage(
            formatId,
            1,
            1,
            1,
            1,
            [new SectorBlock(0, new SectorAddress(0, 0, 0), [value])]);
    }

    private static FileSystemVolume Volume(string name, string fileSystemId, string entryName)
    {
        var entry = new FileSystemEntry(
            entryName,
            FileSystemEntryKind.File,
            1,
            null,
            string.Empty,
            0,
            0,
            true,
            [],
            [0x42]);
        return new FileSystemVolume(name, fileSystemId, 1, 0, null, null, [entry], []);
    }
}
