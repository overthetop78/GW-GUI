namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Commodore Gcr.</summary>
internal static class CommodoreGcrFormat
{
    public const string CodecId = FluxCodecIds.CommodoreGcr;
    public const string CodecDisplayName = FluxCodecDisplayNames.CommodoreGcr;
    public const string StructureDescriptionName = "Commodore";
    public const string SyncDescription = "GCR sync";
    public const string DataBlockDescription = "data block";
    public const string HeaderChecksumDescription = "header checksum";
    public const string DataChecksumDescription = "data checksum";
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
    public const int HeaderMarkOffset = 0;
    public const int HeaderChecksumOffset = 1;
    public const int HeaderSectorOffset = 2;
    public const int HeaderTrackOffset = 3;
    public const int HeaderDiskId2Offset = 4;
    public const int HeaderDiskId1Offset = 5;
    /// <summary>Définit secteur octet nombre utilisé par ce format.</summary>
    public const int SectorByteCount = 256;
    /// <summary>Définit données enregistrement octet nombre utilisé par ce format.</summary>
    public const int DataRecordByteCount = SectorByteCount + 2;
    public const int DataMarkOffset = 0;
    public const int DataPayloadOffset = 1;
    public const int DataChecksumOffset = DataPayloadOffset + SectorByteCount;
    public const byte LogicalHead = 0;
    public const int EncodedHeaderBitCount = HeaderByteCount * EncodedByteBitCount;
    public const int EncodedDataRecordBitCount = DataRecordByteCount * EncodedByteBitCount;
    /// <summary>Définit secteur taille code utilisé par ce format.</summary>
    public const byte SectorSizeCode = 1;
    /// <summary>Définit encodé nibble bit nombre utilisé par ce format.</summary>
    public const int EncodedNibbleBitCount = CommodoreGcrCodec.EncodedNibbleBitCount;
    /// <summary>Définit encodé octet bit nombre utilisé par ce format.</summary>
    public const int EncodedByteBitCount = CommodoreGcrCodec.EncodedByteBitCount;
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
    public const int NibbleMask = CommodoreGcrCodec.NibbleMask;
    public const int ConfidenceSectorWeight = 2;
    public const double ConfidenceDivisor = 42;
}
