namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class CommodoreGcrFormat
{
    public const byte HeaderMark = 0x08;
    public const byte DataMark = 0x07;
    public const int HeaderByteCount = 6;
    public const int SectorByteCount = 256;
    public const int DataRecordByteCount = SectorByteCount + 2;
    public const byte SectorSizeCode = 1;
    public const int EncodedNibbleBitCount = 5;
    public const int EncodedByteBitCount = EncodedNibbleBitCount * 2;
    public const int MinimumSyncBitCount = 10;
    public const int LeadingGapBitCount = 100;
    public const int RawGapBitCount = 3;
    public const int SyncGapBitCount = 20;
    public const int HeaderDataGapBitCount = 6;
    public const int TrailingGapBitCount = 32;
    public const string Id2AttributeName = "id2";
    public const string Id1AttributeName = "id1";
    public const string TrackAttributeName = "track";
    public const byte DefaultId2 = 0xa1;
    public const byte DefaultId1 = 0x1a;
    public const int TracksPerSide = 35;
    public const int NibbleMask = 0x0f;
    public static IReadOnlyList<int> EncodingTable { get; } = Array.AsReadOnly<int>([0x0a,0x0b,0x12,0x13,0x0e,0x0f,0x16,0x17,0x09,0x19,0x1a,0x1b,0x0d,0x1d,0x1e,0x15]);
    public static IReadOnlyDictionary<int, int> DecodingTable { get; } = EncodingTable.Select((value, index) => (value, index)).ToDictionary(item => item.value, item => item.index);
}
