namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class Victor9kGcrFormat
{
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Victor 9000 sectors contain {SectorByteCount} bytes; received {actualSize} bytes.");
    public const string HeaderMarkHex = "5555555555551111";
    public const string DataMarkHex = "5555555555551104";
    public const int MarkByteCount = 8;
    public const int MarkBitCount = 64;
    public const int EncodedDataStartBitOffset = 49;
    public const int EncodedCellStride = 2;
    public const int EncodedNibbleBitCount = 5;
    public const int HeaderByteCount = 6;
    public const int SectorByteCount = 512;
    public const int DecodedDataByteCount = SectorByteCount + 3;
    public const byte SectorSizeCode = 2;
    public const int DataSearchEncodedByteCount = 98;
    public const int HeaderGapBitCount = 20;
    public const int DataGapBitCount = 64;
    public const byte HeaderType = 0x06;
    public const byte HeaderId2 = 0xa1;
    public const byte HeaderId1 = 0x1a;
    public const int NibbleMask = 0x0f;
    public static IReadOnlyList<int> EncodingTable => CommodoreGcrFormat.EncodingTable;
    public static IReadOnlyDictionary<int, int> DecodingTable => CommodoreGcrFormat.DecodingTable;
    public static IReadOnlyList<byte> HeaderMark { get; } = Array.AsReadOnly(Convert.FromHexString(HeaderMarkHex));
    public static IReadOnlyList<byte> DataMark { get; } = Array.AsReadOnly(Convert.FromHexString(DataMarkHex));
}
