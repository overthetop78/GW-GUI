using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class IsoFmFormat
{
    public const byte IdAddressMark = 0xfe;
    public const byte DataAddressMark = 0xfb;
    public const byte DeletedDataAddressMark = 0xf8;
    public const ushort EncodedIdAddressMark = 0xf57e;
    public const ushort EncodedDataAddressMark = 0xf56f;
    public const ushort EncodedDeletedDataAddressMark = 0xf56a;
    public const int EncodedMarkBitCount = 16;
    public const int EncodedByteBitCount = 16;
    public const int HeaderFieldByteCount = 4;
    public const int CrcByteCount = 2;
    public const int HeaderBytesAfterMark = HeaderFieldByteCount + CrcByteCount;
    public const int HeaderBitCount = EncodedMarkBitCount + HeaderBytesAfterMark * EncodedByteBitCount;
    public const int MaximumSectorSizeCode = 7;
    public const int BaseSectorSize = 128;
    public const int HeaderGapBitCount = 160;
    public const int DataGapBitCount = 256;
    public const ushort CrcPolynomial = Crc16Calculator.CcittPolynomial;
    public const ushort CrcInitialValue = Crc16Calculator.AllBitsSetInitialValue;
}
