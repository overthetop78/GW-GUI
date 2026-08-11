namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Victor9k Gcr.</summary>
internal static class Victor9kGcrFormat
{
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Victor 9000 sectors contain {SectorByteCount} bytes; received {actualSize} bytes.");
    /// <summary>Définit en-tête marque hex utilisé par ce format.</summary>
    public const string HeaderMarkHex = "5555555555551111";
    /// <summary>Définit données marque hex utilisé par ce format.</summary>
    public const string DataMarkHex = "5555555555551104";
    /// <summary>Définit marque octet nombre utilisé par ce format.</summary>
    public const int MarkByteCount = 8;
    /// <summary>Définit marque bit nombre utilisé par ce format.</summary>
    public const int MarkBitCount = 64;
    /// <summary>Définit encodé données début bit offset utilisé par ce format.</summary>
    public const int EncodedDataStartBitOffset = 49;
    /// <summary>Définit encodé cellule stride utilisé par ce format.</summary>
    public const int EncodedCellStride = 2;
    /// <summary>Définit encodé nibble bit nombre utilisé par ce format.</summary>
    public const int EncodedNibbleBitCount = Decoding.Definitions.CommodoreGcrCodec.EncodedNibbleBitCount;
    /// <summary>Définit en-tête octet nombre utilisé par ce format.</summary>
    public const int HeaderByteCount = 6;
    /// <summary>Définit secteur octet nombre utilisé par ce format.</summary>
    public const int SectorByteCount = 512;
    /// <summary>Définit décodé données octet nombre utilisé par ce format.</summary>
    public const int DecodedDataByteCount = SectorByteCount + 3;
    /// <summary>Définit secteur taille code utilisé par ce format.</summary>
    public const byte SectorSizeCode = 2;
    /// <summary>Définit données recherche encodé octet nombre utilisé par ce format.</summary>
    public const int DataSearchEncodedByteCount = 98;
    /// <summary>Définit en-tête intervalle bit nombre utilisé par ce format.</summary>
    public const int HeaderGapBitCount = 20;
    /// <summary>Définit données intervalle bit nombre utilisé par ce format.</summary>
    public const int DataGapBitCount = 64;
    /// <summary>Définit en-tête type utilisé par ce format.</summary>
    public const byte HeaderType = 0x06;
    /// <summary>Définit en-tête id2 utilisé par ce format.</summary>
    public const byte HeaderId2 = 0xa1;
    /// <summary>Définit en-tête id1 utilisé par ce format.</summary>
    public const byte HeaderId1 = 0x1a;
    /// <summary>Définit nibble masque utilisé par ce format.</summary>
    public const int NibbleMask = Decoding.Definitions.CommodoreGcrCodec.NibbleMask;
    /// <summary>Expose en-tête marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> HeaderMark { get; } = Array.AsReadOnly(Convert.FromHexString(HeaderMarkHex));
    /// <summary>Expose données marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> DataMark { get; } = Array.AsReadOnly(Convert.FromHexString(DataMarkHex));
}
