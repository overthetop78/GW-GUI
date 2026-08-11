namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Commodore Gcr.</summary>
internal static class CommodoreGcrFormat
{
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Commodore sectors contain {SectorByteCount} bytes; received {actualSize} bytes.");
    /// <summary>Définit en-tête marque utilisé par ce format.</summary>
    public const byte HeaderMark = 0x08;
    /// <summary>Définit données marque utilisé par ce format.</summary>
    public const byte DataMark = 0x07;
    /// <summary>Définit en-tête octet nombre utilisé par ce format.</summary>
    public const int HeaderByteCount = 6;
    /// <summary>Définit secteur octet nombre utilisé par ce format.</summary>
    public const int SectorByteCount = 256;
    /// <summary>Définit données enregistrement octet nombre utilisé par ce format.</summary>
    public const int DataRecordByteCount = SectorByteCount + 2;
    /// <summary>Définit secteur taille code utilisé par ce format.</summary>
    public const byte SectorSizeCode = 1;
    /// <summary>Définit encodé nibble bit nombre utilisé par ce format.</summary>
    public const int EncodedNibbleBitCount = 5;
    /// <summary>Définit encodé octet bit nombre utilisé par ce format.</summary>
    public const int EncodedByteBitCount = EncodedNibbleBitCount * 2;
    /// <summary>Définit minimal synchronisation bit nombre utilisé par ce format.</summary>
    public const int MinimumSyncBitCount = 10;
    /// <summary>Définit leading intervalle bit nombre utilisé par ce format.</summary>
    public const int LeadingGapBitCount = 100;
    /// <summary>Définit brut intervalle bit nombre utilisé par ce format.</summary>
    public const int RawGapBitCount = 3;
    /// <summary>Définit synchronisation intervalle bit nombre utilisé par ce format.</summary>
    public const int SyncGapBitCount = 20;
    /// <summary>Définit en-tête données intervalle bit nombre utilisé par ce format.</summary>
    public const int HeaderDataGapBitCount = 6;
    /// <summary>Définit trailing intervalle bit nombre utilisé par ce format.</summary>
    public const int TrailingGapBitCount = 32;
    /// <summary>Définit id2 attribut name utilisé par ce format.</summary>
    public const string Id2AttributeName = "id2";
    /// <summary>Définit id1 attribut name utilisé par ce format.</summary>
    public const string Id1AttributeName = "id1";
    /// <summary>Définit piste attribut name utilisé par ce format.</summary>
    public const string TrackAttributeName = "track";
    /// <summary>Définit par défaut id2 utilisé par ce format.</summary>
    public const byte DefaultId2 = 0xa1;
    /// <summary>Définit par défaut id1 utilisé par ce format.</summary>
    public const byte DefaultId1 = 0x1a;
    /// <summary>Définit tracks per side utilisé par ce format.</summary>
    public const int TracksPerSide = 35;
    /// <summary>Définit nibble masque utilisé par ce format.</summary>
    public const int NibbleMask = 0x0f;
    /// <summary>Expose encodage table utilisé par ce format.</summary>
    public static IReadOnlyList<int> EncodingTable { get; } = Array.AsReadOnly<int>([0x0a,0x0b,0x12,0x13,0x0e,0x0f,0x16,0x17,0x09,0x19,0x1a,0x1b,0x0d,0x1d,0x1e,0x15]);
    /// <summary>Expose decoding table utilisé par ce format.</summary>
    public static IReadOnlyDictionary<int, int> DecodingTable { get; } = EncodingTable.Select((value, index) => (value, index)).ToDictionary(item => item.value, item => item.index);
}
