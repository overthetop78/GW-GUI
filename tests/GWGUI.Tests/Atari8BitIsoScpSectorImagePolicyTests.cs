using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Reconstruction.Atari;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Decoding;

namespace GWGUI.Tests;

/// <summary>Vérifie les géométries reconnues par la politique ISO Atari 8 bits.</summary>
public sealed class Atari8BitIsoScpSectorImagePolicyTests
{
    /// <summary>Vérifie les trois géométries cataloguées et le repli sur une capacité inconnue.</summary>
    [Theory]
    [InlineData(128, 18, DiskImageFormatIds.Atari90)]
    [InlineData(128, 26, DiskImageFormatIds.Atari130)]
    [InlineData(256, 18, DiskImageFormatIds.Atari180)]
    [InlineData(512, 7, "atari.scp.512.7")]
    public void ResolvesMeasuredGeometry(int sectorSize, int sectorsPerTrack, string expectedFormatId)
    {
        var candidates = Enumerable.Range(1, sectorsPerTrack).ToDictionary(number => new SectorAddress(0, 0, number), number =>
        {
            var dataSize = sectorSize > 128 && number <= 3 ? 128 : sectorSize;
            return new List<IsoSectorCandidate> { new(new(0, 0, number, 0, dataSize, true, 0, Data: new byte[dataSize]), 1) };
        });
        var image = new Atari8BitIsoScpSectorImagePolicy(null).Build(null, new(candidates, candidates));
        Assert.Equal(expectedFormatId, image.FormatId);
        Assert.Equal(sectorsPerTrack, image.SectorsPerTrack);
    }
}
