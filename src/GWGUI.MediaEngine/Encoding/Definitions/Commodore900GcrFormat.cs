namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Commodore900 Gcr.</summary>
internal static class Commodore900GcrFormat
{
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Commodore 900 sectors contain {SectorByteCount} bytes; received {actualSize} bytes.");
    /// <summary>Définit en-tête marque utilisé par ce format.</summary>
    public const byte HeaderMark = 0x08;
    /// <summary>Définit données marque utilisé par ce format.</summary>
    public const byte DataMark = 0x07;
    /// <summary>Définit en-tête octet nombre utilisé par ce format.</summary>
    public const int HeaderByteCount = 4;
    /// <summary>Définit secteur octet nombre utilisé par ce format.</summary>
    public const int SectorByteCount = 512;
    /// <summary>Définit données enregistrement octet nombre utilisé par ce format.</summary>
    public const int DataRecordByteCount = SectorByteCount + 2;
    /// <summary>Définit secteur taille code utilisé par ce format.</summary>
    public const byte SectorSizeCode = 2;
    /// <summary>Définit encodé nibble bit nombre utilisé par ce format.</summary>
    public const int EncodedNibbleBitCount = 5;
    /// <summary>Définit encodé octet bit nombre utilisé par ce format.</summary>
    public const int EncodedByteBitCount = EncodedNibbleBitCount * 2;
    /// <summary>Définit minimal synchronisation bit nombre utilisé par ce format.</summary>
    public const int MinimumSyncBitCount = 10;
    /// <summary>Définit synchronisation intervalle bit nombre utilisé par ce format.</summary>
    public const int SyncGapBitCount = 40;
    /// <summary>Définit enregistrement intervalle bit nombre utilisé par ce format.</summary>
    public const int RecordGapBitCount = 120;
    /// <summary>Définit expected secteur nombre utilisé par ce format.</summary>
    public const int ExpectedSectorCount = 13;
    /// <summary>Définit nibble masque utilisé par ce format.</summary>
    public const int NibbleMask = 0x0f;
    /// <summary>Expose la table d'encodage GCR partagée avec les formats Commodore.</summary>
    public static IReadOnlyList<int> EncodingTable => CommodoreGcrFormat.EncodingTable;
    /// <summary>Expose la table de décodage GCR partagée avec les formats Commodore.</summary>
    public static IReadOnlyDictionary<int, int> DecodingTable => CommodoreGcrFormat.DecodingTable;
}
