using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie les géométries IBM reconstruites par la politique ISO.</summary>
public sealed class IbmPcIsoScpSectorImagePolicyTests
{
    /// <summary>Expose toutes les géométries du catalogue IBM.</summary>
    public static IEnumerable<object[]> Geometries => IbmPcGeometryCatalog.ByCapacity.Values.Select(geometry => new object[] { geometry });

    /// <summary>Vérifie chaque géométrie IBM cataloguée via la sélection d'analyse.</summary>
    [Theory]
    [MemberData(nameof(Geometries))]
    public void BuildsEveryCataloguedGeometry(IbmPcGeometry geometry)
    {
        var candidates = Enumerable.Range(1, geometry.SectorsPerTrack).ToDictionary(number => new SectorAddress(geometry.Cylinders - 1, geometry.Heads - 1, number), number => new List<IsoSectorCandidate> { new(new((byte)(geometry.Cylinders - 1), (byte)(geometry.Heads - 1), number, 2, FatBpbLayout.SectorSize, true, 0, Data: new byte[FatBpbLayout.SectorSize]), 1) });
        var image = new IbmPcIsoScpSectorImagePolicy(true).Build(DiskImageFormatIds.IbmScan, new(candidates, candidates));
        Assert.Equal(geometry.FormatId, image.FormatId);
    }
}
