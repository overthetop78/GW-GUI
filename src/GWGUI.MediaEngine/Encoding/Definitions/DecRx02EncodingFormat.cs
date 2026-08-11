using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class DecRx02EncodingFormat
{
    public const string HeaderMarkHex = "55111554";
    public const byte HeaderAddressMark = 0xfe;
    public const byte FmDeletedDataMark = 0xf8;
    public const byte M2FmDataMark = 0xf9;
    public const byte DataMarkFa = 0xfa;
    public const byte FmDataMark = 0xfb;
    public const byte DataMarkFc = 0xfc;
    public const byte M2FmDeletedDataMark = 0xfd;
    public const int MarkByteCount = 4;
    public const int MarkBitCount = MarkByteCount * BitPrimitives.BitsPerByte;
    public const int DataMarkByteCount = 1;
    public const int HeaderDecodedByteCount = 6;
    public const int CrcByteCount = 2;
    public const int FmSectorByteCount = 128;
    public const int M2FmSectorByteCount = DecRx02Geometry.PhysicalSectorSize;
    public const byte FmSectorSizeCode = 0;
    public const byte M2FmSectorSizeCode = 1;
    public const int EncodedMfmByteBitCount = 16;
    public const int EncodedFmByteBitCount = 32;
    public const int M2FmPhaseBitCount = 1;
    public const int HeaderBitCount = MarkBitCount + HeaderDecodedByteCount * EncodedFmByteBitCount;
    public const int GapBitCount = 64;
    public const int DataSearchByteCount = 88 + 16;
    public const ushort CrcPolynomial = Crc16Calculator.CcittPolynomial;
    public const ushort CrcInitialValue = Crc16Calculator.AllBitsSetInitialValue;
    public static IReadOnlyList<bool> NormalM2FmRule { get; } = Array.AsReadOnly([false,false,true,false,true,false,true,false,true,false,false]);
    public static IReadOnlyList<bool> EncodedM2FmRule { get; } = Array.AsReadOnly([false,true,false,false,false,true,false,false,false,true,false]);
    public static IReadOnlyList<byte> HeaderMark { get; } = Array.AsReadOnly(Convert.FromHexString(HeaderMarkHex));
    public static IReadOnlyList<(IReadOnlyList<byte> Pattern, byte Mark)> DataMarks { get; } = Array.AsReadOnly<(IReadOnlyList<byte>, byte)>([
        (Array.AsReadOnly(Convert.FromHexString("55111444")), FmDeletedDataMark),
        (Array.AsReadOnly(Convert.FromHexString("55111445")), M2FmDataMark),
        (Array.AsReadOnly(Convert.FromHexString("55111454")), DataMarkFa),
        (Array.AsReadOnly(Convert.FromHexString("55111455")), FmDataMark),
        (Array.AsReadOnly(Convert.FromHexString("55111544")), DataMarkFc),
        (Array.AsReadOnly(Convert.FromHexString("55111545")), M2FmDeletedDataMark)]);
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"DEC RX sectors contain {FmSectorByteCount} or {M2FmSectorByteCount} bytes; received {actualSize} bytes.");
}
