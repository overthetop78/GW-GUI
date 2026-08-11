namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class AppleIIGcrFormat
{
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Apple II sectors contain {SectorSize} bytes; received {actualSize} bytes.");
    public const byte PrologueFirstByte = 0xd5;
    public const byte PrologueSecondByte = 0xaa;
    public const byte SixAndTwoAddressPrologueLastByte = 0x96;
    public const byte FiveAndThreeAddressPrologueLastByte = 0xb5;
    public const byte DataPrologueLastByte = 0xad;
    public const uint SixAndTwoAddressPrologue = 0xd5aa96;
    public const uint FiveAndThreeAddressPrologue = 0xd5aab5;
    public const uint DataPrologue = 0xd5aaad;
    public const byte EpilogueFirstByte = 0xde;
    public const byte EpilogueSecondByte = 0xaa;
    public const byte EpilogueLastByte = 0xeb;
    public const byte SyncByte = 0xff;
    public const byte FourAndFourMask = 0xaa;
    public const int SyncByteCount = 3;
    public const int PrologueByteCount = 3;
    public const int PrologueBitCount = PrologueByteCount * Primitives.BitPrimitives.BitsPerByte;
    public const int AddressValueCount = 4;
    public const int EncodedBytesPerAddressValue = 2;
    public const int EncodedAddressByteCount = AddressValueCount * EncodedBytesPerAddressValue;
    public const int EncodedAddressBitCount = EncodedAddressByteCount * Primitives.BitPrimitives.BitsPerByte;
    public const int AddressBlockBitCount = PrologueBitCount + EncodedAddressBitCount;
    public const int DataSearchBitCount = 1024;
    public const int CircularTailBitCount = 4096;
    public const int SectorSize = 256;
    public const byte SectorSizeCode = 1;
    public const int SixAndTwoEncodedByteCount = 343;
    public const int SixAndTwoDecodedByteCount = 342;
    public const int SixAndTwoAuxiliaryByteCount = 86;
    public const int SixAndTwoWorkBufferByteCount = 300;
    public const int FiveAndThreeEncodedByteCount = 411;
    public const int FiveAndThreeAuxiliaryByteCount = 154;
    public const int FiveAndThreeChunkByteCount = 51;
    public const int FiveAndThreeSectorsPerTrack = 13;
    public const int LeadingGapBitCount = 100;
    public const int TrailingGapBitCount = 32;
    public const byte DefaultVolume = 254;
    public const string VolumeAttributeName = "volume";
    public const string SectorsPerTrackAttributeName = "sectorsPerTrack";
    public static IReadOnlyList<byte> SixAndTwoTable { get; } = Array.AsReadOnly<byte>([0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff]);
    public static IReadOnlyList<byte> FiveAndThreeTable { get; } = Array.AsReadOnly<byte>([0xab,0xad,0xae,0xaf,0xb5,0xb6,0xb7,0xba,0xbb,0xbd,0xbe,0xbf,0xd6,0xd7,0xda,0xdb,0xdd,0xde,0xdf,0xea,0xeb,0xed,0xee,0xef,0xf5,0xf6,0xf7,0xfa,0xfb,0xfd,0xfe,0xff]);
}
