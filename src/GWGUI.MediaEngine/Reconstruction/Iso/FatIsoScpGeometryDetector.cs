using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Déduit une géométrie logique FAT depuis le BPB décodé, indépendamment de la taille physique du secteur qui le porte.</summary>
internal static class FatIsoScpGeometryDetector
{
    /// <summary>Tente de lire une géométrie FAT dans le secteur C0/H0/R1.</summary>
    public static bool TryDetect(IReadOnlyDictionary<SectorAddress, List<IsoSectorCandidate>> candidates, out FatBpbGeometry geometry)
    {
        geometry = default;
        var boot = IsoSectorImageBuilder.BestData(candidates, new(FatBootSectorLayout.SystemCylinder, FatBootSectorLayout.SystemHead, FatBootSectorLayout.BootSectorNumber));
        return FatBpbGeometryDetector.TryDetect(boot, null, out geometry);
    }
}
