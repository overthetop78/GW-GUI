namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class ArburgFormat
{
    public const string SystemAttribute = "system";
    public const int DataBlockSize = 0xa00;
    public const int DataUsefulSize = 0x9fe;
    public const int SystemBlockSize = 0xf00;
    public const int SystemUsefulSize = 0xefe;
    public const int ChecksumByteCount = 2;
    public const int GapBitCount = 64;
    public static IReadOnlyList<byte> DataMark { get; } = Array.AsReadOnly<byte>([0x44,0x44,0x44,0x44,0x55,0x55,0x55,0x55]);
    public static IReadOnlyList<byte> SystemMark { get; } = Array.AsReadOnly<byte>([0x55,0x55,0x55,0x55,0x55,0x24,0x92,0x49]);
}
