namespace GWGUI.MediaEngine.Formats.Floppy.Atx;

/// <summary>Disposition binaire des conteneurs Atari ATX.</summary>
public static class AtxLayout
{
    public const int FileHeaderSize = 48;
    public const int VersionOffset = 4;
    public const int TrackDataOffset = 28;
    public const int TrackHeaderSize = 32;
    public const int TrackNextOffset = 0;
    public const int TrackTypeOffset = 4;
    public const int TrackNumberOffset = 8;
    public const int TrackSectorCountOffset = 10;
    public const int TrackDataRelativeOffset = 20;
    public const int SectorListHeaderSize = 8;
    public const int SectorHeaderSize = 8;
    public const int SectorNumberOffset = 0;
    public const int SectorStatusOffset = 1;
    public const int SectorPositionOffset = 2;
    public const int SectorDataRelativeOffset = 4;
    public const int TrackCount = 40;
    public const int SectorsPerTrack = 18;
    public const int SectorSize = 128;
    public const int LogicalSectorCount = TrackCount * SectorsPerTrack;
    public const int MaximumSectorRecordsPerTrack = 64;
}
