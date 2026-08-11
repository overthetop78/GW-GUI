namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class HeathkitFmFormat
{
    public const byte AddressMark = 0xbf;
    public const int SyncZeroCount = 3;
    public const int HeaderByteCount = 4;
    public const int SectorSize = 256;
    public const int HeaderGapBitCount = 160;
    public const int DataGapBitCount = 128;
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly(FluxEncoding.EncodeFm(0,0,0,AddressMark));
}
