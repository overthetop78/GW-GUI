using GWGUI.MediaEngine.FileSystems.Fat;
using GWGUI.MediaEngine.Geometries.Ibm;

namespace GWGUI.MediaEngine.Recognition.Ibm;

/// <summary>Combine le BPB, l'OEM DOS et les descripteurs de média FAT historiques.</summary>
internal static class IbmDosDiskProbe
{
    /// <summary>Tente d'identifier une géométrie IBM depuis un secteur d'amorçage issu du flux.</summary>
    public static bool TryIdentify(ReadOnlySpan<byte> boot, byte fatMedia, bool requireDosOem, out IbmPcGeometry geometry)
    {
        geometry = default;
        var hasBpb = FatBpbGeometryDetector.TryDetect(boot, null, out var bpb);
        var legacy = !hasBpb && IbmPcGeometryCatalog.TryFromMediaDescriptor(fatMedia, out geometry);
        if (requireDosOem && !IbmDosOemProbe.IsKnownDosOem(boot) && !legacy) { geometry = default; return false; }
        if (hasBpb)
        {
            geometry = new(IbmPcGeometryCatalog.FormatIdForGeometry(bpb.Cylinders, bpb.Heads, bpb.SectorsPerTrack, bpb.SectorSize), bpb.Cylinders, bpb.Heads, bpb.SectorsPerTrack);
            return true;
        }
        return legacy;
    }
}
