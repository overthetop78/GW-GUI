namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class MembrainMfmFormat
{
    public const byte SyncByte = 0xa1;
    public const byte HeaderAddressMark = 0xfe;
    public const byte DataAddressMark = 0xf8;
    public const byte LastDataAddressMark = 0xfb;
    public const int SectorSize = 512;
    public const int CrcByteCount = 2;
    public const int CylinderLowBitCount = 3;
    public const int CylinderLowShift = 5;
    public const int HeadShift = 4;
    public const byte CylinderHighMask = 0x1f;
    public const byte CylinderLowValueMask = 0x07;
    public const byte CylinderLowMask = 0xe0;
    public const byte HeadMask = 1;
    public const byte SectorMask = 0x0f;
    public const int HeaderGapBitCount = 64;
    public const int DataGapBitCount = 128;
    public const ushort CrcPolynomial = Primitives.Crc16Calculator.IbmPolynomial;
    public const ushort CrcInitialValue = Primitives.Crc16Calculator.ZeroInitialValue;
    public static IReadOnlyList<byte> SectorHeader { get; } = Array.AsReadOnly<byte>([0x44,0x89,0x55,0x54]);
    public static IReadOnlyList<byte> SectorData { get; } = Array.AsReadOnly<byte>([0x44,0x89,0x55,0x4a]);
}
