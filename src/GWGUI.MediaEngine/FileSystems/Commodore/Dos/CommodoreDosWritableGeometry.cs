using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Décrit les géométries standard créées par le Writer Commodore DOS.</summary>
internal sealed record CommodoreDosWritableGeometry(string FormatId, int Cylinders, int Heads, int SectorsPerTrack, int BlockCount)
{
    /// <summary>Résout D64, D71 ou D81 sans déduire la géométrie du contenu.</summary>
    public static CommodoreDosWritableGeometry Resolve(string formatId)
    {
        if (formatId.Equals(DiskImageFormatIds.Commodore1541, StringComparison.OrdinalIgnoreCase))
        {
            var tracks = Commodore1541Geometry.StandardTrackCount;
            return new(DiskImageFormatIds.Commodore1541, tracks, 1, Commodore1541Geometry.MaximumSectorsPerTrack, Commodore1541Geometry.BlocksPerSide(tracks));
        }
        if (formatId.Equals(DiskImageFormatIds.Commodore1571, StringComparison.OrdinalIgnoreCase))
        {
            var tracks = Commodore1541Geometry.StandardTrackCount;
            return new(DiskImageFormatIds.Commodore1571, tracks, Commodore1571Geometry.SideCount, Commodore1541Geometry.MaximumSectorsPerTrack, Commodore1541Geometry.BlocksPerSide(tracks) * Commodore1571Geometry.SideCount);
        }
        if (formatId.Equals(DiskImageFormatIds.Commodore1581, StringComparison.OrdinalIgnoreCase)) return new(DiskImageFormatIds.Commodore1581, Commodore1581Geometry.LogicalCylinderCount, Commodore1581Geometry.LogicalHeadCount, Commodore1581Geometry.LogicalBlocksPerTrack, Commodore1581Geometry.LogicalCylinderCount * Commodore1581Geometry.LogicalBlocksPerTrack);
        throw CommodoreDosVolumeWriterExceptions.UnsupportedFormat(formatId);
    }

    /// <summary>Convertit une adresse Commodore DOS en bloc logique.</summary>
    public int ToLogicalBlock(int track, int sector)
    {
        if (FormatId == DiskImageFormatIds.Commodore1581) return Commodore1581Geometry.ToLogicalBlock(track, sector);
        var side = track > Cylinders ? 1 : 0;
        var localTrack = track - side * Cylinders;
        return Heads == Commodore1571Geometry.SideCount ? Commodore1571Geometry.ToLogicalBlock(localTrack, sector, Cylinders, side) : Commodore1541Geometry.ToSideLogicalBlock(localTrack, sector, Cylinders);
    }

    /// <summary>Convertit un bloc logique en numéro de piste Commodore et de secteur.</summary>
    public (int Track, int Sector) FromLogicalBlock(int logicalBlock)
    {
        if (FormatId == DiskImageFormatIds.Commodore1581) return Commodore1581Geometry.FromLogicalBlock(logicalBlock);
        var address = Commodore1541Geometry.FromLogicalBlock(logicalBlock, Cylinders, Heads);
        return (address.Track + address.Side * Cylinders, address.Sector);
    }

    /// <summary>Construit l'adresse physique associée au bloc logique.</summary>
    public SectorAddress CreateAddress(int logicalBlock)
    {
        if (FormatId == DiskImageFormatIds.Commodore1581)
        {
            var address = Commodore1581Geometry.FromLogicalBlock(logicalBlock);
            return new(address.Track - 1, 0, address.Sector);
        }
        var address1541 = Commodore1541Geometry.FromLogicalBlock(logicalBlock, Cylinders, Heads);
        return new(address1541.Track - 1, address1541.Side, address1541.Sector);
    }
}
