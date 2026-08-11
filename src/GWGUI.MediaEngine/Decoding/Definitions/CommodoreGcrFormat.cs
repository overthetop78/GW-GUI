namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Commodore GCR.</summary>
internal static class CommodoreGcrFormat
{
    /// <summary>Identifiant technique du codec.</summary>
    public const string CodecId = FluxCodecIds.CommodoreGcr;
    /// <summary>Nom affiché du codec.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.CommodoreGcr;
    /// <summary>Nom utilisé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "Commodore";
    /// <summary>Libellé d'une synchronisation GCR.</summary>
    public const string SyncDescription = "GCR sync";
    /// <summary>Libellé d'un bloc de données.</summary>
    public const string DataBlockDescription = "data block";
    /// <summary>Libellé du checksum d'en-tête.</summary>
    public const string HeaderChecksumDescription = "header checksum";
    /// <summary>Libellé du checksum des données.</summary>
    public const string DataChecksumDescription = "data checksum";
    /// <summary>Marque identifiant un en-tête.</summary>
    public const byte HeaderMark = 0x08;
    /// <summary>Marque identifiant un bloc de données.</summary>
    public const byte DataMark = 0x07;
    /// <summary>Nombre d'octets d'un en-tête complet.</summary>
    public const int HeaderByteCount = 6;
    /// <summary>Position de la marque dans l'en-tête.</summary>
    public const int HeaderMarkOffset = 0;
    /// <summary>Position du checksum dans l'en-tête.</summary>
    public const int HeaderChecksumOffset = 1;
    /// <summary>Position du numéro de secteur dans l'en-tête.</summary>
    public const int HeaderSectorOffset = 2;
    /// <summary>Position du numéro de piste dans l'en-tête.</summary>
    public const int HeaderTrackOffset = 3;
    /// <summary>Position du second identifiant de disque dans l'en-tête.</summary>
    public const int HeaderDiskId2Offset = 4;
    /// <summary>Position du premier identifiant de disque dans l'en-tête.</summary>
    public const int HeaderDiskId1Offset = 5;
    /// <summary>Taille d'un secteur en octets.</summary>
    public const int SectorByteCount = 256;
    /// <summary>Nombre d'octets d'un bloc de données complet.</summary>
    public const int DataRecordByteCount = SectorByteCount + 2;
    /// <summary>Position de la marque dans le bloc de données.</summary>
    public const int DataMarkOffset = 0;
    /// <summary>Position de la charge utile dans le bloc de données.</summary>
    public const int DataPayloadOffset = 1;
    /// <summary>Position du checksum dans le bloc de données.</summary>
    public const int DataChecksumOffset = DataPayloadOffset + SectorByteCount;
    /// <summary>Face logique produite par le décodeur.</summary>
    public const byte LogicalHead = 0;
    /// <summary>Code représentant un secteur de 256 octets.</summary>
    public const byte SectorSizeCode = 1;
    /// <summary>Nombre de bits d'un symbole GCR encodé.</summary>
    public const int EncodedNibbleBitCount = CommodoreGcrCodec.EncodedNibbleBitCount;
    /// <summary>Nombre de bits d'un octet GCR encodé.</summary>
    public const int EncodedByteBitCount = CommodoreGcrCodec.EncodedByteBitCount;
    /// <summary>Nombre de bits d'un en-tête GCR encodé.</summary>
    public const int EncodedHeaderBitCount = HeaderByteCount * EncodedByteBitCount;
    /// <summary>Nombre de bits d'un bloc de données GCR encodé.</summary>
    public const int EncodedDataRecordBitCount = DataRecordByteCount * EncodedByteBitCount;
    /// <summary>Longueur minimale d'une synchronisation.</summary>
    public const int MinimumSyncBitCount = 10;
    /// <summary>Nombre de bits de remplissage précédant un secteur encodé.</summary>
    public const int LeadingGapBitCount = 100;
    /// <summary>Nombre de bits bruts séparant le remplissage de la synchronisation.</summary>
    public const int RawGapBitCount = 3;
    /// <summary>Longueur d'une synchronisation produite par l'encodeur.</summary>
    public const int SyncGapBitCount = 20;
    /// <summary>Longueur de l'intervalle entre l'en-tête et les données.</summary>
    public const int HeaderDataGapBitCount = 6;
    /// <summary>Longueur de l'intervalle suivant les données.</summary>
    public const int TrailingGapBitCount = 32;
    /// <summary>Nom de l'attribut du second identifiant de disque.</summary>
    public const string Id2AttributeName = "id2";
    /// <summary>Nom de l'attribut du premier identifiant de disque.</summary>
    public const string Id1AttributeName = "id1";
    /// <summary>Nom de l'attribut de piste logique.</summary>
    public const string TrackAttributeName = "track";
    /// <summary>Second identifiant de disque utilisé par défaut.</summary>
    public const byte DefaultId2 = 0xa1;
    /// <summary>Premier identifiant de disque utilisé par défaut.</summary>
    public const byte DefaultId1 = 0x1a;
    /// <summary>Nombre de pistes par face utilisé pour calculer la piste logique.</summary>
    public const int TracksPerSide = 35;
    /// <summary>Masque isolant un demi-octet.</summary>
    public const int NibbleMask = CommodoreGcrCodec.NibbleMask;
    /// <summary>Poids d'un secteur dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur du calcul de confiance.</summary>
    public const double ConfidenceDivisor = 42;

    /// <summary>Crée l'exception signalant une taille de secteur incompatible.</summary>
    /// <param name="actualSize">Taille observée en octets.</param>
    /// <returns>Exception contenant les tailles attendue et observée.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Commodore sectors contain {SectorByteCount} bytes; received {actualSize} bytes.");
}
