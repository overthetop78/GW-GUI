using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Iso Fm.</summary>
internal static class IsoFmFormat
{
    /// <summary>Définit id adresse marque utilisé par ce format.</summary>
    public const byte IdAddressMark = 0xfe;
    /// <summary>Définit données adresse marque utilisé par ce format.</summary>
    public const byte DataAddressMark = 0xfb;
    /// <summary>Définit supprimées données adresse marque utilisé par ce format.</summary>
    public const byte DeletedDataAddressMark = 0xf8;
    /// <summary>Définit encodé id adresse marque utilisé par ce format.</summary>
    public const ushort EncodedIdAddressMark = 0xf57e;
    /// <summary>Définit encodé données adresse marque utilisé par ce format.</summary>
    public const ushort EncodedDataAddressMark = 0xf56f;
    /// <summary>Définit encodé supprimées données adresse marque utilisé par ce format.</summary>
    public const ushort EncodedDeletedDataAddressMark = 0xf56a;
    /// <summary>Définit encodé marque bit nombre utilisé par ce format.</summary>
    public const int EncodedMarkBitCount = 16;
    /// <summary>Définit encodé octet bit nombre utilisé par ce format.</summary>
    public const int EncodedByteBitCount = 16;
    /// <summary>Définit en-tête field octet nombre utilisé par ce format.</summary>
    public const int HeaderFieldByteCount = 4;
    /// <summary>Définit crc octet nombre utilisé par ce format.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Définit en-tête octets after marque utilisé par ce format.</summary>
    public const int HeaderBytesAfterMark = HeaderFieldByteCount + CrcByteCount;
    /// <summary>Définit en-tête bit nombre utilisé par ce format.</summary>
    public const int HeaderBitCount = EncodedMarkBitCount + HeaderBytesAfterMark * EncodedByteBitCount;
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
