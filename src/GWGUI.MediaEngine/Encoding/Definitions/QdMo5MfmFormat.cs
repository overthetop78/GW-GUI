namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Qd Mo5 Mfm.</summary>
internal static class QdMo5MfmFormat
{
    /// <summary>Définit secteur taille utilisé par ce format.</summary>
    public const int SectorSize = 128;
    /// <summary>Définit secteur number octet nombre utilisé par ce format.</summary>
    public const int SectorNumberByteCount = 2;
    /// <summary>Définit en-tête remplissage octet nombre utilisé par ce format.</summary>
    public const int HeaderPaddingByteCount = 13;
    /// <summary>Définit données préfixe octet nombre utilisé par ce format.</summary>
    public const int DataPrefixByteCount = 1;
    /// <summary>Définit somme de contrôle octet nombre utilisé par ce format.</summary>
    public const int ChecksumByteCount = 1;
    /// <summary>Définit préfixe attribut utilisé par ce format.</summary>
    public const string PrefixAttribute = "prefix";
    /// <summary>Définit par défaut préfixe utilisé par ce format.</summary>
    public const byte DefaultPrefix = 0x5a;
    /// <summary>Définit en-tête intervalle bit nombre utilisé par ce format.</summary>
    public const int HeaderGapBitCount = 160;
    /// <summary>Définit données intervalle bit nombre utilisé par ce format.</summary>
    public const int DataGapBitCount = 128;
    /// <summary>Définit données recherche octet nombre utilisé par ce format.</summary>
    public const int DataSearchByteCount = 88 + 16;
    /// <summary>Expose en-tête marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> HeaderMark { get; } = Array.AsReadOnly<byte>([0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0x44,0x91]);
    /// <summary>Expose données marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> DataMark { get; } = Array.AsReadOnly<byte>([0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0x91,0x44]);
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"QD MO5 sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
