using GWGUI.MediaEngine.Geometries.Dec;

namespace GWGUI.MediaEngine.Containers.Dec.Rx02;

/// <summary>Convertit l'ordre logique RX02 vers l'ordre physique entrelacé du dump.</summary>
internal static class DecRx02SectorOrder
{
    /// <summary>Copie un secteur logique depuis un dump physique RX02.</summary>
    public static void CopyLogicalSector(ReadOnlySpan<byte> source, int logicalSector, Span<byte> destination)
    {
        var (track, sector) = LogicalToPhysical(logicalSector);
        source.Slice((track * DecRx02Geometry.PhysicalSectorsPerTrack + sector - 1) * DecRx02Geometry.PhysicalSectorSize, DecRx02Geometry.PhysicalSectorSize).CopyTo(destination);
    }

    /// <summary>Retourne la piste et le numéro de secteur physiques correspondant à un secteur logique.</summary>
    private static (int Track, int Sector) LogicalToPhysical(int logicalSector)
    {
        var logicalTrack = logicalSector / DecRx02Geometry.PhysicalSectorsPerTrack;
        var position = logicalSector % DecRx02Geometry.PhysicalSectorsPerTrack;
        position = (2 * position + (position >= DecRx02Geometry.LogicalBlocksPerTrack ? 1 : 0)) % DecRx02Geometry.PhysicalSectorsPerTrack;
        var sector = 1 + (position + 6 * logicalTrack) % DecRx02Geometry.PhysicalSectorsPerTrack;
        var track = logicalTrack + 1;
        if (track >= DecRx02Geometry.TrackCount) track = 0;
        return (track, sector);
    }
}
