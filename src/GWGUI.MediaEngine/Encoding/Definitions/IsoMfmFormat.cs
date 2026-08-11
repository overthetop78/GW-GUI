using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding.Definitions;

internal static class IsoMfmFormat
{
    public const byte SyncByte = 0xa1;
    public const int SyncByteCount = 3;
    public const ushort EncodedSyncByte = 0x4489;
    public const string EncodedSyncHex = "448944894489";
    public const byte IdAddressMark = 0xfe;
    public const byte DataAddressMark = 0xfb;
    public const byte DeletedDataAddressMark = 0xf8;
    public const int EncodedByteBitCount = 16;
    public const int SyncBitCount = SyncByteCount * EncodedByteBitCount;
    public const int SyncAndMarkBitCount = SyncBitCount + EncodedByteBitCount;
    public const int HeaderFieldByteCount = 4;
    public const int CrcByteCount = 2;
    public const int HeaderBytesAfterMark = HeaderFieldByteCount + CrcByteCount;
    public const int HeaderBitCount = SyncAndMarkBitCount + HeaderBytesAfterMark * EncodedByteBitCount;
    public const int MaximumSectorSizeCode = 7;
    public const int BaseSectorSize = 128;
    public const int HeaderGapBitCount = 160;
    public const int DataGapBitCount = 256;
    public const ushort CrcPolynomial = Crc16Calculator.CcittPolynomial;
    public const ushort CrcInitialValue = Crc16Calculator.AllBitsSetInitialValue;
}
