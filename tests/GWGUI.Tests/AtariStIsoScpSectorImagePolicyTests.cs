using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Atari;
using GWGUI.MediaEngine.Reconstruction.Atari;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie les capacités prises en charge par la politique ISO Atari ST.</summary>
public sealed class AtariStIsoScpSectorImagePolicyTests
{
    /// <summary>Vérifie chaque capacité Atari ST cataloguée.</summary>
    [Theory]
    [InlineData(40, 1, 9, DiskImageFormatIds.AtariSt180)]
    [InlineData(40, 2, 9, DiskImageFormatIds.AtariSt360)]
    [InlineData(80, 1, 10, DiskImageFormatIds.AtariSt400)]
    [InlineData(80, 1, 11, DiskImageFormatIds.AtariSt440)]
    [InlineData(80, 2, 9, DiskImageFormatIds.AtariSt720)]
    [InlineData(80, 2, 10, DiskImageFormatIds.AtariSt800)]
    [InlineData(90, 2, 9, DiskImageFormatIds.AtariSt810)]
    [InlineData(80, 2, 11, DiskImageFormatIds.AtariSt880)]
    [InlineData(80, 2, 18, DiskImageFormatIds.AtariSt1440)]
    public void ResolvesCataloguedCapacity(int cylinders, int heads, int sectorsPerTrack, string expectedFormatId)
    {
        var candidates = Enumerable.Range(1, sectorsPerTrack).ToDictionary(number => new SectorAddress(cylinders - 1, heads - 1, number), number => new List<IsoSectorCandidate> { new(new((byte)(cylinders - 1), (byte)(heads - 1), number, 2, AtariStGeometry.SectorSize, true, 0, Data: new byte[AtariStGeometry.SectorSize]), 1) });
        var image = new AtariStIsoScpSectorImagePolicy().Build(null, new(candidates, candidates));
        Assert.Equal(expectedFormatId, image.FormatId);
    }
}
