namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class MicropolisMfmFormat
{
    public const byte AddressMark = 0xff;
    public const int SyncZeroCount = 3;
    public const int RecordIdentityByteCount = 3;
    public const int SectorSize = 256;
    public const int HeaderPaddingByteCount = 10;
    public const int TrailerPaddingByteCount = 5;
    public const int RecordByteCount = 275;
    public const int PreambleByteCount = 40;
    public const int GapBitCount = 128;
    public const int ChecksumModulus = 255;
    public static IReadOnlyList<byte> Sync { get; } = Array.AsReadOnly(FluxEncoding.EncodeMfm(0,0,0,AddressMark));
}
