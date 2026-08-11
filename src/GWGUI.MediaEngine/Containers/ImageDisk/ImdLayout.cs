namespace GWGUI.MediaEngine.Containers.ImageDisk;

internal static class ImdLayout
{
    public const int TrackHeaderSize = 5;
    public const int ModeOffset = 0;
    public const int CylinderOffset = 1;
    public const int HeadFlagsOffset = 2;
    public const int SectorCountOffset = 3;
    public const int SectorSizeCodeOffset = 4;
    public const int MapEntrySize = 1;
    public const int SectorSizeMapEntrySize = 2;
    public const int BaseSectorSize = 128;
    public const byte ExplicitSectorSizeCode = 0xFF;
    public const byte MaximumExponentialSizeCode = 6;
}
