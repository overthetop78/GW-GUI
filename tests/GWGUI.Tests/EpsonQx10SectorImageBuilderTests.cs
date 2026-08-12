using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Epson;
using GWGUI.MediaEngine.Reconstruction.EpsonQx10;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie la construction sectorielle de chaque géométrie Epson QX-10.</summary>
public sealed class EpsonQx10SectorImageBuilderTests
{
    /// <summary>Vérifie chaque sélection Epson cataloguée.</summary>
    [Theory]
    [InlineData(DiskImageFormatIds.EpsonQx10_320)]
    [InlineData(DiskImageFormatIds.EpsonQx10_400)]
    [InlineData(DiskImageFormatIds.EpsonQx10Booter)]
    [InlineData(DiskImageFormatIds.EpsonQx10_399)]
    [InlineData(DiskImageFormatIds.EpsonQx10_396)]
    [InlineData(DiskImageFormatIds.EpsonQx10Logo)]
    public void BuildsEveryCataloguedSelection(string formatId)
    {
        var geometry = EpsonQx10GeometryCatalog.Resolve(formatId);
        var location = Enumerable.Range(0, geometry.Cylinders).SelectMany(cylinder => Enumerable.Range(0, geometry.Heads).Select(head => (Cylinder: cylinder, Head: head, Track: geometry.Track(cylinder, head)))).First(item => item.Track.Count > 0);
        var address = new SectorAddress(location.Cylinder, location.Head, location.Track.FirstSector);
        var candidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>> { [address] = [Candidate(address, location.Track.SectorSize, true, 1)] };
        var image = new EpsonQx10IsoScpSectorImagePolicy().Build(formatId, new(candidates, candidates));
        Assert.Equal(formatId, image.FormatId);
        Assert.Single(image.AvailableBlocks);
        Assert.NotEmpty(image.MissingBlocks);
    }

    /// <summary>Vérifie les tailles variables, les doublons classés et les secteurs absents.</summary>
    [Fact]
    public void SelectsBestDuplicateAndPreservesVariableAndMissingSectors()
    {
        var address = new SectorAddress(0, 0, EpsonQx10GeometryCatalog.Layout320.FirstSector);
        var candidates = new Dictionary<SectorAddress, List<IsoSectorCandidate>> { [address] = [Candidate(address, EpsonQx10GeometryCatalog.Layout320.SectorSize, false, 1), Candidate(address, EpsonQx10GeometryCatalog.Layout320.SectorSize, true, 2)] };
        var image = EpsonQx10SectorImageBuilder.Create(DiskImageFormatIds.EpsonQx10_396, candidates);
        var block = Assert.Single(image.AvailableBlocks);
        Assert.True(block.IntegrityValid);
        Assert.Equal(2, block.Revolution);
        Assert.NotEmpty(image.MissingBlocks);
        Assert.Equal(EpsonQx10GeometryCatalog.Layout320.SectorSize, image.GetBlock(block.LogicalBlock).Length);
    }

    private static IsoSectorCandidate Candidate(SectorAddress address, int size, bool integrity, int revolution) => new(new((byte)address.Cylinder, (byte)address.Head, address.Number, 0, size, integrity, 0, Data: new byte[size]), revolution);
}
