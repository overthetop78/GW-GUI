using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Iso Mfm.</summary>
internal static class IsoMfmFormat
{
    /// <summary>Définit synchronisation octet utilisé par ce format.</summary>
    public const byte SyncByte = 0xa1;
    /// <summary>Définit synchronisation octet nombre utilisé par ce format.</summary>
    public const int SyncByteCount = 3;
    /// <summary>Définit encodé synchronisation octet utilisé par ce format.</summary>
    public const ushort EncodedSyncByte = 0x4489;
    /// <summary>Définit encodé synchronisation hex utilisé par ce format.</summary>
    public const string EncodedSyncHex = "448944894489";
    /// <summary>Définit id adresse marque utilisé par ce format.</summary>
    public const byte IdAddressMark = 0xfe;
    /// <summary>Définit données adresse marque utilisé par ce format.</summary>
    public const byte DataAddressMark = 0xfb;
    /// <summary>Définit supprimées données adresse marque utilisé par ce format.</summary>
    public const byte DeletedDataAddressMark = 0xf8;
    /// <summary>Définit encodé octet bit nombre utilisé par ce format.</summary>
    public const int EncodedByteBitCount = 16;
    /// <summary>Définit synchronisation bit nombre utilisé par ce format.</summary>
    public const int SyncBitCount = SyncByteCount * EncodedByteBitCount;
    /// <summary>Définit synchronisation and marque bit nombre utilisé par ce format.</summary>
    public const int SyncAndMarkBitCount = SyncBitCount + EncodedByteBitCount;
    /// <summary>Définit en-tête field octet nombre utilisé par ce format.</summary>
    public const int HeaderFieldByteCount = 4;
    /// <summary>Définit crc octet nombre utilisé par ce format.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Définit en-tête octets after marque utilisé par ce format.</summary>
    public const int HeaderBytesAfterMark = HeaderFieldByteCount + CrcByteCount;
    /// <summary>Définit en-tête bit nombre utilisé par ce format.</summary>
    public const int HeaderBitCount = SyncAndMarkBitCount + HeaderBytesAfterMark * EncodedByteBitCount;
    /// <summary>Définit maximal secteur taille code utilisé par ce format.</summary>
    public const int MaximumSectorSizeCode = 7;
    /// <summary>Définit base secteur taille utilisé par ce format.</summary>
    public const int BaseSectorSize = 128;
    /// <summary>Définit en-tête intervalle bit nombre utilisé par ce format.</summary>
    public const int HeaderGapBitCount = 160;
    /// <summary>Définit données intervalle bit nombre utilisé par ce format.</summary>
    public const int DataGapBitCount = 256;
    /// <summary>Définit crc polynôme utilisé par ce format.</summary>
    public const ushort CrcPolynomial = Crc16Calculator.CcittPolynomial;
    /// <summary>Définit crc initiale valeur utilisé par ce format.</summary>
    public const ushort CrcInitialValue = Crc16Calculator.AllBitsSetInitialValue;
}
