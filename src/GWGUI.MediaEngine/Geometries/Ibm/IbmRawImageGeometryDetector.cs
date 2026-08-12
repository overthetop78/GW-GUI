using GWGUI.MediaEngine.Containers.Ibm.Raw;
using GWGUI.MediaEngine.FileSystems.Fat;

namespace GWGUI.MediaEngine.Geometries.Ibm;

/// <summary>Détecte la géométrie d'une image brute IBM complète depuis son BPB puis sa capacité.</summary>
public static class IbmRawImageGeometryDetector
{
    /// <summary>Détecte la géométrie ou signale précisément pourquoi elle ne peut pas l'être.</summary>
    public static IbmPcGeometry Detect(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0 || data.Length % FatBpbLayout.SectorSize != 0) throw IbmRawImageExceptions.InvalidLength(data.Length, FatBpbLayout.SectorSize);
        if (FatBpbGeometryDetector.TryDetect(data, data.Length, out var bpb)) return new(IbmPcGeometryCatalog.FormatIdForGeometry(bpb.Cylinders, bpb.Heads, bpb.SectorsPerTrack), bpb.Cylinders, bpb.Heads, bpb.SectorsPerTrack);
        if (IbmPcGeometryCatalog.TryFromCapacity(data.Length, out var geometry)) return geometry;
        throw IbmRawImageExceptions.UnknownGeometry(data.Length);
    }
}
