namespace GWGUI.MediaEngine.Containers.TeleDisk;

internal static class Td0Layout
{
    public const int HeaderSize = 12;
    public const int SignatureOffset = 0;
    public const int ByteFieldSize = 1;
    public const int VersionOffset = 4;
    public const int DataRateOffset = 5;
    public const int SteppingOffset = 7;
    public const int HeaderCrcOffset = 10;
    public const int CommentHeaderSize = 10;
    public const int CommentLengthOffset = 2;
    public const int CommentCrcOffset = 0;
    public const int TrackHeaderSize = 4;
    public const int TrackSectorCountOffset = 0;
    public const int TrackCylinderOffset = 1;
    public const int TrackHeadOffset = 2;
    public const int TrackCrcOffset = 3;
    public const int SectorHeaderSize = 6;
    public const int SectorCylinderOffset = 0;
    public const int SectorHeadOffset = 1;
    public const int SectorNumberOffset = 2;
    public const int SectorSizeCodeOffset = 3;
    public const int SectorFlagsOffset = 4;
    public const int SectorCrcOffset = 5;
    public const int SectorDataHeaderSize = 3;
    public const int EncodedLengthOffset = 0;
    public const int EncodingOffset = 2;
    public const int EncodingFieldSize = 1;
    public const int WordSize = 2;
    public const int BaseSectorSize = 128;
    public const int MaximumSectorSizeCode = 6;
    public const int HeadMask = 0x01;
    public const byte CommentPresentMask = 0x80;
    public const byte EndOfTracks = 0xFF;
    public const int RepeatedSectorPayloadSize = 4;
    public const int RepeatedSectorCountOffset = 0;
    public const int RepeatedSectorPatternOffset = 2;
    public const int RepeatedSectorSecondPatternByteOffset = 3;
    public const int RleControlSize = 2;
    public const int PatternWordSize = 2;
}
