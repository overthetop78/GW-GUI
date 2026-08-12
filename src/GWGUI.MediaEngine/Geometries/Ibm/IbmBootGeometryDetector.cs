using GWGUI.MediaEngine.FileSystems.Fat;

namespace GWGUI.MediaEngine.Geometries.Ibm;

/// <summary>Détecte une géométrie IBM explicitement demandée depuis son BPB ou son descripteur de média FAT.</summary>
internal static class IbmBootGeometryDetector
{
    /// <summary>Tente de résoudre une géométrie sans exiger la présence d'un OEM DOS connu.</summary>
    /// <param name="boot">Données du secteur d'amorçage.</param>
    /// <param name="fatMedia">Descripteur de média lu au début du premier secteur FAT.</param>
    /// <param name="geometry">Géométrie IBM détectée lorsque la méthode retourne <see langword="true"/>.</param>
    /// <returns><see langword="true"/> lorsqu'un BPB ou un descripteur historique permet de résoudre la géométrie.</returns>
    public static bool TryDetect(ReadOnlySpan<byte> boot, byte fatMedia, out IbmPcGeometry geometry)
    {
        if (FatBpbGeometryDetector.TryDetect(boot, null, out var bpb))
        {
            geometry = new(IbmPcGeometryCatalog.FormatIdForGeometry(bpb.Cylinders, bpb.Heads, bpb.SectorsPerTrack, bpb.SectorSize), bpb.Cylinders, bpb.Heads, bpb.SectorsPerTrack);
            return true;
        }
        return IbmPcGeometryCatalog.TryFromMediaDescriptor(fatMedia, out geometry);
    }
}
