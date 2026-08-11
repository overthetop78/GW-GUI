namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class Aed6200pMfmFormat
{
    public const byte HeaderAddressMark = 0xc6;
    public const byte DeletedDataMark = 0xc0;
    public const byte DataMark = 0xc3;
    public const int HeaderByteCount = 7;
    public const int DataMarkByteCount = 1;
    public const int CrcByteCount = 2;
    public const int FirstGapBitCount = 64;
    public const int SecondGapBitCount = 128;
    public static IReadOnlyList<byte> HeaderPattern { get; } = Array.AsReadOnly<byte>([0x50,0x94]);
    public static IReadOnlyList<IReadOnlyList<byte>> DataPatterns { get; } = Array.AsReadOnly<IReadOnlyList<byte>>([Array.AsReadOnly<byte>([0x50,0x8a]),Array.AsReadOnly<byte>([0x50,0x89]),Array.AsReadOnly<byte>([0x50,0x84]),Array.AsReadOnly<byte>([0x50,0x85])]);
}
