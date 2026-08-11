namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class CenturionMfmFormat
{
    public const byte SupportedDataKey = 0;
    public const int HeaderByteCount = 4;
    public const int DataPrefixByteCount = 3;
    public const int CrcByteCount = 2;
    public const int AllocationBlockSize = 256;
    public const int HeaderGapBitCount = 400;
    public const int DataGapBitCount = 128;
    public const ushort CrcPolynomial = Primitives.Crc16Calculator.CcittPolynomial;
    public const ushort CrcInitialValue = Primitives.Crc16Calculator.ZeroInitialValue;
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly<byte>([0x91,0x22,0x44,0x89]);
    public static IReadOnlyList<byte> DataMark { get; } = Array.AsReadOnly<byte>([0xaa,0xaa,0xaa,0xa9]);
}
