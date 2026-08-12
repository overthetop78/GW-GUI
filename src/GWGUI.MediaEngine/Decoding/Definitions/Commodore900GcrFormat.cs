using GWGUI.MediaEngine.Geometries.Commodore;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques communes du format Commodore 900 GCR.</summary>
internal static class Commodore900GcrFormat
{
    /// <summary>Identifiant technique du codec.</summary>
    public const string CodecId = FluxCodecIds.Commodore900Gcr;
    /// <summary>Nom affiché du codec.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.Commodore900Gcr;
    /// <summary>Nom utilisé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "Commodore 900";
    /// <summary>Description d'une synchronisation GCR.</summary>
    public const string SyncDescription = "GCR sync";
    /// <summary>Nom du contrôle d'intégrité.</summary>
    public const string ChecksumDescription = "checksum";
    /// <summary>Description d'un bloc de données non apparié.</summary>
    public const string UnpairedDataDescription = "data block";
    /// <summary>Marque d'un en-tête.</summary>
    public const byte HeaderMark = 0x08;
    /// <summary>Marque d'un bloc de données.</summary>
    public const byte DataMark = 0x07;
    /// <summary>Nombre d'octets composant l'en-tête.</summary>
    public const int HeaderByteCount = 4;
    /// <summary>Position de la marque dans l'en-tête.</summary>
    public const int HeaderMarkOffset = 0;
    /// <summary>Position du cylindre dans l'en-tête.</summary>
    public const int HeaderCylinderOffset = 1;
    /// <summary>Position du secteur dans l'en-tête.</summary>
    public const int HeaderSectorOffset = 2;
    /// <summary>Position du checksum dans l'en-tête.</summary>
    public const int HeaderChecksumOffset = 3;
    /// <summary>Taille d'une charge utile sectorielle.</summary>
    public const int SectorByteCount = Commodore900Geometry.SectorSize;
    /// <summary>Nombre d'octets composant un bloc de données complet.</summary>
    public const int DataRecordByteCount = SectorByteCount + 2;
    /// <summary>Position de la marque dans un bloc de données.</summary>
    public const int DataMarkOffset = 0;
    /// <summary>Position de la charge utile dans un bloc de données.</summary>
    public const int DataPayloadOffset = 1;
    /// <summary>Position du checksum dans un bloc de données.</summary>
    public const int DataChecksumOffset = DataPayloadOffset + SectorByteCount;
    /// <summary>Face logique attribuée aux secteurs.</summary>
    public const byte LogicalHead = 0;
    /// <summary>Code de taille correspondant à 512 octets.</summary>
    public const byte SectorSizeCode = 2;
    /// <summary>Nombre de bits d'un symbole GCR.</summary>
    public const int EncodedNibbleBitCount = 5;
    /// <summary>Nombre de bits encodant un octet.</summary>
    public const int EncodedByteBitCount = EncodedNibbleBitCount * 2;
    /// <summary>Longueur encodée d'un en-tête.</summary>
    public const int EncodedHeaderBitCount = HeaderByteCount * EncodedByteBitCount;
    /// <summary>Longueur encodée d'un bloc de données.</summary>
    public const int EncodedDataRecordBitCount = DataRecordByteCount * EncodedByteBitCount;
    /// <summary>Longueur minimale d'une synchronisation.</summary>
    public const int MinimumSyncBitCount = 10;
    /// <summary>Longueur de synchronisation produite par l'encodeur.</summary>
    public const int SyncGapBitCount = 40;
    /// <summary>Intervalle produit après un enregistrement.</summary>
    public const int RecordGapBitCount = 120;
    /// <summary>Nombre attendu de secteurs utilisé comme diviseur de confiance.</summary>
    public const int ExpectedSectorCount = Commodore900Geometry.MinimumSectorsPerTrack;
    /// <summary>Masque isolant un demi-octet.</summary>
    public const int NibbleMask = 0x0f;
    /// <summary>Plus grand cylindre ou secteur représentable dans l'en-tête.</summary>
    public const int MaximumAddressValue = byte.MaxValue;

    /// <summary>Crée l'exception signalant une taille sectorielle invalide.</summary>
    /// <param name="actualSize">Taille observée.</param><returns>Exception contenant les tailles attendue et observée.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Commodore 900 sectors contain {SectorByteCount} bytes; received {actualSize} bytes.");
}
