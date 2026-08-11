namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class AmigaMfmFormat
{
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Amiga sectors contain {SectorByteCount} bytes; received {actualSize} bytes.");
    public static ArgumentException OddEncodedByteCount(int actualCount) => new($"Amiga odd/even encoding requires an even byte count; received {actualCount} bytes.");
    public const ushort SyncWord = 0x4489;
    public const int SyncWordCount = 2;
    public const int EncodedByteBitCount = 16;
    public const int SyncBitCount = SyncWordCount * EncodedByteBitCount;
    public const int EncodedSectorByteCount = 540;
    public const int EncodedHeaderByteCount = 28;
    public const int EncodedDataOffset = EncodedHeaderByteCount;
    public const int SectorByteCount = 512;
    public const byte SectorSizeCode = 2;
    public const byte FormatByte = 0xff;
    public const int InfoByteCount = 4;
    public const int LabelByteCount = 16;
    public const int HeaderParitySourceByteCount = 20;
    public const int HeaderParityHighOffset = 22;
    public const int HeaderParityLowOffset = 23;
    public const int DataParityHighOffset = 26;
    public const int DataParityLowOffset = 27;
    public const int ParityFieldByteCount = 8;
    public const int LeadingGapBitCount = 100;
    public const int TrailingGapBitCount = 128;
    public const int NibbleBitCount = 4;
}
