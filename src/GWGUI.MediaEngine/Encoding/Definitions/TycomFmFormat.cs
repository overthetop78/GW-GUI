namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class TycomFmFormat
{
    public const byte HeaderAddressMark = 0xfe;
    public const byte DeletedDataMark = 0xf8;
    public const byte DataMarkF9 = 0xf9;
    public const byte DataMarkFa = 0xfa;
    public const byte DataMark = 0xfb;
    public const int HeaderDecodedByteCount = 4;
    public const int SectorSize = 128;
    public const int CrcByteCount = 2;
    public const int GapBitCount = 64;
    public const int DataSearchByteCount = 88 + 16;
    public const ushort CrcPolynomial = Primitives.Crc16Calculator.CcittPolynomial;
    public const ushort CrcInitialValue = Primitives.Crc16Calculator.AllBitsSetInitialValue;
    public static IReadOnlyList<byte> HeaderMark { get; } = Array.AsReadOnly<byte>([0x55,0x11,0x15,0x54]);
    public static IReadOnlyList<(IReadOnlyList<byte> Pattern, byte Mark)> DataMarks { get; } = Array.AsReadOnly<(IReadOnlyList<byte>,byte)>([(Array.AsReadOnly<byte>([0x55,0x11,0x14,0x44]),DeletedDataMark),(Array.AsReadOnly<byte>([0x55,0x11,0x14,0x45]),DataMarkF9),(Array.AsReadOnly<byte>([0x55,0x11,0x14,0x54]),DataMarkFa),(Array.AsReadOnly<byte>([0x55,0x11,0x14,0x55]),DataMark)]);
}
