namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class EmuFmFormat
{
    public const int SectorSize = 0xe00;
    public const int HeaderDecodedByteCount = 3;
    public const int CrcByteCount = 2;
    public const int TrackShift = 1;
    public const byte HeadMask = 1;
    public const int GapBitCount = 64;
    public const ushort CrcPolynomial = Primitives.Crc16Calculator.IbmPolynomial;
    public const ushort CrcInitialValue = Primitives.Crc16Calculator.ZeroInitialValue;
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly<byte>([0x45,0x45,0x55,0x55,0x45,0x54,0x54,0x45]);
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"E-mu sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
