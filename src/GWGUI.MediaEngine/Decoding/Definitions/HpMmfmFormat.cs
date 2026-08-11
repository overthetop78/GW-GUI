namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format HP MMFM.</summary>
internal static class HpMmfmFormat
{
    /// <summary>Identifiant technique du codec.</summary>
    public const string CodecId = FluxCodecIds.HpMmfm;
    /// <summary>Nom affiché du codec.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.HpMmfm;
    /// <summary>Nom utilisé dans les descriptions.</summary>
    public const string StructureDescriptionName = "HP MMFM";
    /// <summary>Nombre d'octets d'une synchronisation.</summary>
    public const int SyncByteCount = 4;
    /// <summary>Nombre de bits d'une synchronisation.</summary>
    public const int SyncBitCount = 32;
    /// <summary>Nombre d'octets d'identité.</summary>
    public const int IdentityByteCount = 2;
    /// <summary>Nombre d'octets du CRC.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Nombre d'octets de l'en-tête.</summary>
    public const int HeaderByteCount = IdentityByteCount + CrcByteCount;
    /// <summary>Nombre de bits d'un octet encodé.</summary>
    public const int EncodedByteBitCount = 16;
    /// <summary>Longueur totale de l'en-tête.</summary>
    public const int HeaderBitCount = SyncBitCount + HeaderByteCount * EncodedByteBitCount;
    /// <summary>Position du cylindre dans l'identité.</summary>
    public const int HeaderCylinderOffset = 0;
    /// <summary>Position du secteur et de la face.</summary>
    public const int HeaderSectorOffset = 1;
    /// <summary>Décalage du bit de face.</summary>
    public const int HeadShift = 7;
    /// <summary>Masque isolant le secteur.</summary>
    public const byte SectorMask = 0x7f;
    /// <summary>Taille sectorielle.</summary>
    public const int SectorSize = 256;
    /// <summary>Nombre d'octets du bloc encodé.</summary>
    public const int EncodedDataByteCount = SectorSize + CrcByteCount;
    /// <summary>Code de taille produit.</summary>
    public const byte SectorSizeCode = 1;
    /// <summary>Borne minimale de recherche des données.</summary>
    public const int MinimumDataSearchOffsetBits = 8 * EncodedByteBitCount;
    /// <summary>Borne maximale de recherche des données.</summary>
    public const int MaximumDataSearchOffsetBits = 58 * EncodedByteBitCount;
    /// <summary>Remplissage suivant l'en-tête.</summary>
    public const int HeaderGapBitCount = 128;
    /// <summary>Remplissage suivant les données.</summary>
    public const int DataGapBitCount = 256;
    /// <summary>Poids d'un secteur dans la confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur du calcul de confiance.</summary>
    public const double ConfidenceDivisor = 20;
    /// <summary>Synchronisation d'en-tête.</summary>
    public static IReadOnlyList<byte> SectorSync { get; } = Array.AsReadOnly<byte>([0x55, 0x55, 0x2a, 0x54]);
    /// <summary>Synchronisation de données.</summary>
    public static IReadOnlyList<byte> DataSync { get; } = Array.AsReadOnly<byte>([0x55, 0x55, 0x2a, 0x44]);
    /// <summary>Crée l'exception signalant une taille incompatible.</summary>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"HP MMFM sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
