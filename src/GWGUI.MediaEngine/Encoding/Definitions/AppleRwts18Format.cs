namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class AppleRwts18Format
{
    public const ushort EncodedAddressMark = 0xd59d;
    public const int AddressMarkBitCount = 16;
    public const int AddressByteCount = 4;
    public const byte AddressTrailer = 0xaa;
    public const byte DataEpilogue = 0xd4;
    public const byte SyncByte = 0xff;
    public const int SectorCount = 6;
    public const int LastSectorNumber = SectorCount - 1;
    public const int SectorByteCount = 768;
    public const byte SectorSizeCode = 3;
    public const int PageByteCount = 256;
    public const int PayloadSymbolCount = 1024;
    public const int PayloadWithChecksumSymbolCount = PayloadSymbolCount + 1;
    public const int DataRecordByteCount = PayloadWithChecksumSymbolCount + 2;
    public const int DataReadWindowByteCount = 1100;
    public const int CircularTailBitCount = 16_384;
    public const int FirstSectorGapBitCount = 200;
    public const int OtherSectorGapBitCount = 4;
    public const string IdentifierAttributeName = "id";
    public const byte DefaultIdentifier = 0xa4;
    public const byte SixBitMask = 0x3f;
    public static IReadOnlyList<byte> NibbleTable => AppleIIGcrFormat.SixAndTwoTable;
}
