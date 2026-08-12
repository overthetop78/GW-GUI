using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Acorn;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie les quatre géométries BBC DFS reconstruites depuis des candidats ISO.</summary>
public sealed class BbcIsoScpSectorImagePolicyTests
{
    /// <summary>Vérifie SSD et DSD sur quarante et quatre-vingts pistes.</summary>
    [Theory]
    [InlineData(40, 1, DiskImageFormatIds.AcornDfsSingleSided)]
    [InlineData(80, 1, DiskImageFormatIds.AcornDfsSingleSided80)]
    [InlineData(40, 2, DiskImageFormatIds.AcornDfsDoubleSided)]
    [InlineData(80, 2, DiskImageFormatIds.AcornDfsDoubleSided80)]
    public void ResolvesObservedGeometry(int cylinders, int heads, string expectedFormatId)
    {
        var candidates = Enumerable.Range(0, BbcDfsGeometry.SectorsPerTrack).ToDictionary(number => new SectorAddress(cylinders - 1, heads - 1, number), number => new List<IsoSectorCandidate> { new(new((byte)(cylinders - 1), (byte)(heads - 1), number, 1, BbcDfsGeometry.SectorSize, true, 0, Data: new byte[BbcDfsGeometry.SectorSize]), 1) });
        var image = new BbcIsoScpSectorImagePolicy().Build(null, new(candidates, candidates));
        Assert.Equal(expectedFormatId, image.FormatId);
    }
}
