namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class QdMo5MfmFormat
{
    public const int SectorSize = 128;
    public const int SectorNumberByteCount = 2;
    public const int HeaderPaddingByteCount = 13;
    public const int DataPrefixByteCount = 1;
    public const int ChecksumByteCount = 1;
    public const string PrefixAttribute = "prefix";
    public const byte DefaultPrefix = 0x5a;
    public const int HeaderGapBitCount = 160;
    public const int DataGapBitCount = 128;
    public const int DataSearchByteCount = 88 + 16;
    public static IReadOnlyList<byte> HeaderMark { get; } = Array.AsReadOnly<byte>([0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0x44,0x91]);
    public static IReadOnlyList<byte> DataMark { get; } = Array.AsReadOnly<byte>([0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0x91,0x44]);
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"QD MO5 sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
