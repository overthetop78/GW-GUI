using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Geometries.Commodore;

namespace GWGUI.MediaEngine.Images;

internal static class CommodoreGeometry
{
    public const int StandardTrackCount = Commodore1541Geometry.StandardTrackCount;
    public const int ExtendedTrackCount = Commodore1541Geometry.ExtendedTrackCount;
    public const int StandardSideCount = 1;
    public const int DoubleSideCount = 2;
    public const int MaximumSectorsPer1541Track = Commodore1541Geometry.MaximumSectorsPerTrack;
    public const int Commodore1581PhysicalSectorSize = Commodore1581Geometry.PhysicalSectorSize;
    public const int Commodore1581LogicalBlockSize = Commodore1581Geometry.LogicalBlockSize;
    public const int Commodore1581SectorsPerTrack = Commodore1581Geometry.PhysicalSectorsPerTrack;
    public const int Commodore1581LogicalBlocksPerTrack = Commodore1581Geometry.LogicalBlocksPerTrack;
    public const int Commodore1581PhysicalSectorsPerLogicalBlock = Commodore1581Geometry.LogicalBlocksPerPhysicalSector;
    public static int SectorsFor1541Track(int track) => Commodore1541Geometry.SectorsPerTrack(track);

    public static int BlocksPer1541Side(int tracks) => Commodore1541Geometry.BlocksPerSide(tracks);

    public static int To1541LogicalBlock(int track, int sector, int tracksPerSide, int side = 0)
    {
        if (track < 1 || track > tracksPerSide) throw new ArgumentOutOfRangeException(nameof(track));
        var sectors = SectorsFor1541Track(track);
        if (sector < 0 || sector >= sectors) throw new ArgumentOutOfRangeException(nameof(sector));
        return side * BlocksPer1541Side(tracksPerSide)
            + Enumerable.Range(1, track - 1).Sum(SectorsFor1541Track)
            + sector;
    }

    public static (int Track, int Sector, int Side) From1541LogicalBlock(int block, int tracksPerSide, int sides) => Commodore1541Geometry.FromLogicalBlock(block, tracksPerSide, sides);

    public static int To1581LogicalBlock(int track, int sector) => Commodore1581Geometry.ToLogicalBlock(track, sector);
}
