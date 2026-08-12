using System.Buffers.Binary;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Containers.Atari.St;

namespace GWGUI.MediaEngine.Geometries.Atari;

/// <summary>Détecte une géométrie Atari ST depuis son BPB puis, à défaut, depuis les replis de capacité ordonnés.</summary>
internal static class AtariStGeometryDetector
{
    /// <summary>Nombre maximal de secteurs par piste accepté dans un BPB Atari ST.</summary>
    public const int MaximumSectorsPerTrack = 36;
    /// <summary>Nombre minimal de cylindres accepté par les replis de capacité.</summary>
    public const int MinimumFallbackCylinderCount = 35;
    /// <summary>Nombre maximal de cylindres accepté par les replis de capacité.</summary>
    public const int MaximumFallbackCylinderCount = 90;
    /// <summary>Ordre prioritaire des secteurs par piste essayés lorsque le BPB ne valide rien.</summary>
    public static IReadOnlyList<int> FallbackSectorsPerTrack { get; } = Array.AsReadOnly(new[] { 9, 10, 11, 18 });
    /// <summary>Ordre prioritaire des faces essayées pour chaque nombre de secteurs : double face puis simple face.</summary>
    public static IReadOnlyList<int> FallbackHeadCounts { get; } = Array.AsReadOnly(new[] { DiskGeometryConstants.DoubleSidedHeadCount, DiskGeometryConstants.SingleSidedHeadCount });

    /// <summary>Retourne la géométrie et la preuve BPB ou capacité ayant permis sa sélection.</summary>
    public static AtariStGeometryDetection Detect(ReadOnlySpan<byte> data)
    {
        if (TryReadBpb(data, out var geometry)) return new(geometry, AtariStGeometryEvidence.Bpb);
        var sectorCount = data.Length / AtariStGeometry.SectorSize;
        foreach (var sectors in FallbackSectorsPerTrack)
            foreach (var heads in FallbackHeadCounts)
                if (sectorCount % (sectors * heads) == 0 && sectorCount / (sectors * heads) is >= MinimumFallbackCylinderCount and <= MaximumFallbackCylinderCount) return new(new(sectorCount / (sectors * heads), heads, sectors), AtariStGeometryEvidence.CapacityFallback);
        throw AtariStExceptions.GeometryNotDetected(data.Length);
    }

    /// <summary>Valide les champs géométriques du BPB et leur cohérence avec la longueur observée.</summary>
    private static bool TryReadBpb(ReadOnlySpan<byte> data, out AtariStGeometry geometry)
    {
        geometry = default;
        if (data.Length < FatBootSectorLayout.MinimumLength) return false;
        var bytesPerSector = BinaryPrimitives.ReadUInt16LittleEndian(data[FatBootSectorLayout.BytesPerSectorOffset..]);
        var totalSectors = BinaryPrimitives.ReadUInt16LittleEndian(data[FatBootSectorLayout.TotalSectors16Offset..]);
        if (totalSectors == 0) totalSectors = checked((ushort)Math.Min(ushort.MaxValue, BinaryPrimitives.ReadUInt32LittleEndian(data[FatBootSectorLayout.TotalSectors32Offset..])));
        var sectors = BinaryPrimitives.ReadUInt16LittleEndian(data[FatBootSectorLayout.SectorsPerTrackOffset..]);
        var heads = BinaryPrimitives.ReadUInt16LittleEndian(data[FatBootSectorLayout.HeadCountOffset..]);
        if (bytesPerSector != AtariStGeometry.SectorSize || totalSectors != data.Length / AtariStGeometry.SectorSize || sectors is 0 or > MaximumSectorsPerTrack || heads is 0 or > DiskGeometryConstants.DoubleSidedHeadCount || totalSectors % (sectors * heads) != 0) return false;
        geometry = new(totalSectors / (sectors * heads), heads, sectors);
        return true;
    }
}

/// <summary>Indique la preuve ayant déterminé une géométrie Atari ST.</summary>
internal enum AtariStGeometryEvidence { Bpb, CapacityFallback }

/// <summary>Associe une géométrie Atari ST à sa preuve de détection.</summary>
internal sealed record AtariStGeometryDetection(AtariStGeometry Geometry, AtariStGeometryEvidence Evidence);
