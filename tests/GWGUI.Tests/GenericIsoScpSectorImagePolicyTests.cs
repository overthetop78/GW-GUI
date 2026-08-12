using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie la reconstruction ISO générique uniforme.</summary>
public sealed class GenericIsoScpSectorImagePolicyTests
{
    /// <summary>Vérifie plusieurs géométries uniformes mesurées.</summary>
    [Theory]
    [InlineData(40, 1, 8, 256)]
    [InlineData(80, 2, 9, 512)]
    [InlineData(77, 1, 26, 128)]
    public void BuildsUniformGeometry(int cylinders, int heads, int sectorsPerTrack, int sectorSize)
    {
        var candidates = Track(cylinders - 1, heads - 1, sectorsPerTrack, sectorSize);
        var image = new GenericIsoScpSectorImagePolicy().Build("custom.iso", new(candidates, candidates));
        Assert.Equal((cylinders, heads, sectorsPerTrack, sectorSize), (image.Cylinders, image.Heads, image.SectorsPerTrack, image.BlockSize));
    }

    /// <summary>Vérifie qu'une piste incohérente ne produit pas un bloc hors de la géométrie majoritaire.</summary>
    [Fact]
    public void FiltersIncoherentSectorNumber()
    {
        var candidates = Track(0, 0, 8, 256);
        var address = new SectorAddress(0, 0, 99);
        candidates[address] = [Candidate(address, 256)];
        var image = new GenericIsoScpSectorImagePolicy().Build("custom.iso", new(candidates, candidates));
        Assert.Equal(8, image.AvailableBlocks.Count);
    }

    private static Dictionary<SectorAddress, List<IsoSectorCandidate>> Track(int cylinder, int head, int sectors, int size) => Enumerable.Range(1, sectors).ToDictionary(number => new SectorAddress(cylinder, head, number), number => new List<IsoSectorCandidate> { Candidate(new(cylinder, head, number), size) });

    private static IsoSectorCandidate Candidate(SectorAddress address, int size) => new(new((byte)address.Cylinder, (byte)address.Head, address.Number, 0, size, true, 0, Data: new byte[size]), 1);
}
