namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class Commodore900GcrFormat
{
    public const byte HeaderMark = 0x08;
    public const byte DataMark = 0x07;
    public const int HeaderByteCount = 4;
    public const int SectorByteCount = 512;
    public const int DataRecordByteCount = SectorByteCount + 2;
    public const byte SectorSizeCode = 2;
    public const int EncodedNibbleBitCount = 5;
    public const int EncodedByteBitCount = EncodedNibbleBitCount * 2;
    public const int MinimumSyncBitCount = 10;
    public const int SyncGapBitCount = 40;
    public const int RecordGapBitCount = 120;
    public const int ExpectedSectorCount = 13;
    public const int NibbleMask = 0x0f;
    public static IReadOnlyList<int> EncodingTable => CommodoreGcrFormat.EncodingTable;
    public static IReadOnlyDictionary<int, int> DecodingTable => CommodoreGcrFormat.DecodingTable;
}
