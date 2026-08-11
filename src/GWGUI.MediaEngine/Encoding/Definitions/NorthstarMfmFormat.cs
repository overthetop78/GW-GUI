namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class NorthstarMfmFormat
{
    public const byte AddressMark = 0xfb;
    public const int SyncZeroCount = 7;
    public const int SectorSize = 512;
    public const int CylinderShift = 4;
    public const byte SectorMask = 0x0f;
    public const int GapBitCount = 128;
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly(FluxEncoding.EncodeMfm(0,0,0,0,0,0,0,AddressMark));
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"NorthStar sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
