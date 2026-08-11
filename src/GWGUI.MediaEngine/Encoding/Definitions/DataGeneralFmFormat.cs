namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class DataGeneralFmFormat
{
    public const int SectorSize = 512;
    public const int ChecksumByteCount = 2;
    public const int HeaderGapBitCount = 64;
    public const int DataGapBitCount = 128;
    public const byte CylinderMask = 0x7f;
    public const byte HeadMask = 0x80;
    public const int HeadShift = 7;
    public const int SectorShift = 2;
    public static IReadOnlyList<byte> Sync { get; } = Array.AsReadOnly(FluxEncoding.EncodeFm(0x00,0x01));
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Data General sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
