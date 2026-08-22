using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Geometries.Acorn;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;
using System.IO;
using GWGUI.MediaEngine.Decoding;

namespace GWGUI.Tests;

/// <summary>VÃ©rifie les quatre gÃ©omÃ©tries BBC DFS reconstruites depuis des candidats ISO.</summary>
public sealed class BbcIsoScpSectorImagePolicyTests
{
    /// <summary>VÃ©rifie SSD et DSD sur quarante et quatre-vingts pistes.</summary>
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

    /// <summary>VÃ©rifie les parcours public explicite et automatique sur une capture BBC DFS rÃ©elle.</summary>
    [Fact]
    public async Task PublicReaderSupportsExplicitAndAutomaticSelection()
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", "validated_images", "Acorn", "BBC Micro", "5.25 pouces - Acorn DFS - 200 Kio", "seeds-of-evil-bbc [test].scp"));
        Assert.True(File.Exists(path), $"Image SCP BBC obligatoire absente : {path}");
        var explorer = DiskImageExplorer.CreateDefault();
        Assert.Equal(DiskImageFormatIds.AcornDfsSingleSided80, (await explorer.ExploreAsync(path, DiskImageFormatIds.AcornDfsSingleSided80)).Image.FormatId);
        Assert.StartsWith(DiskImageFormatIds.AcornDfsPrefix, (await explorer.ExploreAsync(path)).Image.FormatId, StringComparison.Ordinal);
    }

    /// <summary>VÃ©rifie le rejet d'une capture ne contenant aucun secteur BBC utilisable.</summary>
    [Fact]
    public void RejectsCaptureWithoutValidSector()
    {
        var empty = new Dictionary<SectorAddress, List<IsoSectorCandidate>>();
        Assert.Throws<InvalidDataException>(() => new BbcIsoScpSectorImagePolicy().Build(DiskImageFormatIds.AcornDfsSingleSided, new(empty, empty)));
    }
}
