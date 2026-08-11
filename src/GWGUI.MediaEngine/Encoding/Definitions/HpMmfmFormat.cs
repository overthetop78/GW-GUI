namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class HpMmfmFormat
{
    public const int SectorSize = 256;
    public const int CrcByteCount = 2;
    public const int EncodedDataByteCount = SectorSize + CrcByteCount;
    public const int HeadShift = 7;
    public const byte SectorMask = 0x7f;
    public const int HeaderGapBitCount = 128;
    public const int DataGapBitCount = 256;
    public static IReadOnlyList<byte> SectorSync { get; } = Array.AsReadOnly<byte>([0x55,0x55,0x2a,0x54]);
    public static IReadOnlyList<byte> DataSync { get; } = Array.AsReadOnly<byte>([0x55,0x55,0x2a,0x44]);
}
