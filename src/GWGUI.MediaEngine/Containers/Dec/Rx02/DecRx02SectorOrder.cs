using GWGUI.MediaEngine.Geometries.Dec;

namespace GWGUI.MediaEngine.Containers.Dec.Rx02;

/// <summary>Convertit l'ordre logique RX02 vers l'ordre physique entrelacé du dump.</summary>
internal static class DecRx02SectorOrder
{
    /// <summary>Facteur d'entrelacement des secteurs.</summary>
    public const int InterleaveFactor = 2;
    /// <summary>Décalage appliqué à la seconde moitié de piste.</summary>
    public const int SecondHalfOffset = 1;
    /// <summary>Décalage rotatif appliqué à chaque piste.</summary>
    public const int TrackSkew = 6;
    /// <summary>Premier numéro de secteur physique.</summary>
    public const int FirstPhysicalSectorNumber = 1;
    /// <summary>Copie un secteur logique depuis un dump physique RX02.</summary>
    public static void CopyLogicalSector(ReadOnlySpan<byte> source, int logicalSector, Span<byte> destination)
    {
        var (track, sector) = LogicalToPhysical(logicalSector);
        source.Slice((track * DecRx02Geometry.PhysicalSectorsPerTrack + sector - 1) * DecRx02Geometry.PhysicalSectorSize, DecRx02Geometry.PhysicalSectorSize).CopyTo(destination);
    }

    /// <summary>Copie un secteur logique vers sa position physique entrelacée dans un dump RX02.</summary>
    public static void WriteLogicalSector(Span<byte> destination, int logicalSector, ReadOnlySpan<byte> source)
    {
        if (source.Length != DecRx02Geometry.PhysicalSectorSize) throw new ArgumentException($"RX02 physical sectors contain {DecRx02Geometry.PhysicalSectorSize} bytes.", nameof(source));
        var (track, sector) = LogicalToPhysical(logicalSector);
        source.CopyTo(destination.Slice((track * DecRx02Geometry.PhysicalSectorsPerTrack + sector - FirstPhysicalSectorNumber) * DecRx02Geometry.PhysicalSectorSize, DecRx02Geometry.PhysicalSectorSize));
    }

    /// <summary>Retourne la piste et le numéro de secteur physiques correspondant à un secteur logique.</summary>
    public static (int Track, int Sector) LogicalToPhysical(int logicalSector)
    {
        if (logicalSector is < 0 or >= DecRx02Geometry.PhysicalSectorCount) throw new ArgumentOutOfRangeException(nameof(logicalSector));
        var logicalTrack = logicalSector / DecRx02Geometry.PhysicalSectorsPerTrack;
        var position = logicalSector % DecRx02Geometry.PhysicalSectorsPerTrack;
        position = (InterleaveFactor * position + (position >= DecRx02Geometry.LogicalBlocksPerTrack ? SecondHalfOffset : 0)) % DecRx02Geometry.PhysicalSectorsPerTrack;
        var sector = FirstPhysicalSectorNumber + (position + TrackSkew * logicalTrack) % DecRx02Geometry.PhysicalSectorsPerTrack;
        var track = logicalTrack + 1;
        if (track >= DecRx02Geometry.TrackCount) track = 0;
        return (track, sector);
    }
}
