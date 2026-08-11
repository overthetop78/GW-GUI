namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class MicralNFmFormat
{
    public const byte AddressMark = 0xff;
    public const int SyncZeroCount = 3;
    public const int IdentityByteCount = 2;
    public const int SectorSize = 128;
    public const int ChecksumByteCount = 1;
    public const byte CarryMask = 0x80;
    public const byte ComplementMask = 0xff;
    public const int GapBitCount = 128;
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly(FluxEncoding.EncodeFm(0,0,0,AddressMark));
}
