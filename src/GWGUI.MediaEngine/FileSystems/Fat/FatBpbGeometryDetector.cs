using System.Buffers.Binary;

namespace GWGUI.MediaEngine.FileSystems.Fat;

/// <summary>Détecte et valide une géométrie depuis les champs communs du BPB FAT.</summary>
public static class FatBpbGeometryDetector
{
    /// <summary>Tente de lire la géométrie, avec contrôle facultatif de la longueur complète.</summary>
    public static bool TryDetect(ReadOnlySpan<byte> boot, int? imageLength, out FatBpbGeometry geometry)
    {
        geometry = default;
        if (boot.Length < FatBpbLayout.MinimumLength) return false;
        var sectorSize = BinaryPrimitives.ReadUInt16LittleEndian(boot[FatBpbLayout.BytesPerSectorOffset..]);
        var totalSectors = (int)BinaryPrimitives.ReadUInt16LittleEndian(boot[FatBpbLayout.TotalSectors16Offset..]);
        if (totalSectors == 0)
        {
            var largeTotal = BinaryPrimitives.ReadUInt32LittleEndian(boot[FatBpbLayout.TotalSectors32Offset..]);
            if (largeTotal > int.MaxValue) return false;
            totalSectors = (int)largeTotal;
        }
        var sectorsPerTrack = BinaryPrimitives.ReadUInt16LittleEndian(boot[FatBpbLayout.SectorsPerTrackOffset..]);
        var heads = BinaryPrimitives.ReadUInt16LittleEndian(boot[FatBpbLayout.HeadCountOffset..]);
        if (sectorSize != FatBpbLayout.SectorSize || totalSectors <= 0 || sectorsPerTrack is 0 or > FatBpbLayout.MaximumSectorsPerTrack || heads is 0 or > FatBpbLayout.MaximumHeadCount || totalSectors % (sectorsPerTrack * heads) != 0) return false;
        if (imageLength is { } length && (length % sectorSize != 0 || totalSectors != length / sectorSize)) return false;
        var cylinders = totalSectors / (sectorsPerTrack * heads);
        if (cylinders is <= 0 or > FatBpbLayout.MaximumCylinderCount) return false;
        geometry = new(sectorSize, totalSectors, cylinders, heads, sectorsPerTrack);
        return true;
    }
}
