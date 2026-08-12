using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Data General 2F.</summary>
internal static class DataGeneralFmFormat
{
    /// <summary>Identifiant technique du codec.</summary>
    public const string CodecId = FluxCodecIds.DataGeneralFm;
    /// <summary>Nom affiché du codec.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.DataGeneralFm;
    /// <summary>Nom utilisé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "Data General";
    /// <summary>Libellé du contrôle d'intégrité.</summary>
    public const string ChecksumDescription = "checksum";
    /// <summary>Premier octet de synchronisation.</summary>
    public const byte FirstSyncByte = 0x00;
    /// <summary>Second octet de synchronisation.</summary>
    public const byte SecondSyncByte = 0x01;
    /// <summary>Nombre de bits d'un octet encodé en FM.</summary>
    public const int EncodedByteBitCount = 16;
    /// <summary>Nombre de bits occupés par la synchronisation FM.</summary>
    public const int EncodedSyncBitCount = 2 * EncodedByteBitCount;
    /// <summary>Nombre d'octets décrivant l'identité du secteur.</summary>
    public const int IdentityByteCount = 2;
    /// <summary>Position de l'octet contenant le cylindre et la face.</summary>
    public const int CylinderAndHeadOffset = 0;
    /// <summary>Position de l'octet contenant le secteur.</summary>
    public const int SectorOffset = 1;
    /// <summary>Masque isolant le cylindre.</summary>
    public const byte CylinderMask = 0x7f;
    /// <summary>Masque isolant la face.</summary>
    public const byte HeadMask = 0x80;
    /// <summary>Plus grand cylindre représentable sous le bit de face.</summary>
    public const int MaximumCylinder = CylinderMask;
    /// <summary>Plus grande face représentable par le bit de face.</summary>
    public const int MaximumHead = 1;
    /// <summary>Décalage du bit de face.</summary>
    public const int HeadShift = 7;
    /// <summary>Décalage du numéro de secteur.</summary>
    public const int SectorShift = 2;
    /// <summary>Plus grand numéro de secteur accepté.</summary>
    public const int MaximumSectorNumber = 7;
    /// <summary>Distance minimale entre l'identité et la synchronisation des données.</summary>
    public const int MinimumDataSyncDistanceBits = 32;
    /// <summary>Distance maximale entre l'identité et la synchronisation des données.</summary>
    public const int MaximumDataSyncDistanceBits = 256;
    /// <summary>Taille d'un secteur en octets.</summary>
    public const int SectorSize = 512;
    /// <summary>Nombre d'octets du checksum.</summary>
    public const int ChecksumByteCount = 2;
    /// <summary>Nombre d'octets du bloc de données complet.</summary>
    public const int DataBlockByteCount = SectorSize + ChecksumByteCount;
    /// <summary>Position de l'octet fort du checksum.</summary>
    public const int ChecksumHighByteOffset = SectorSize;
    /// <summary>Position de l'octet faible du checksum.</summary>
    public const int ChecksumLowByteOffset = SectorSize + 1;
    /// <summary>Face logique utilisée dans les données encodées.</summary>
    public const byte LogicalHead = 0;
    /// <summary>Code représentant un secteur de 512 octets.</summary>
    public const byte SectorSizeCode = 2;
    /// <summary>Poids d'un secteur dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur du calcul de confiance.</summary>
    public const double ConfidenceDivisor = 24;
    /// <summary>Longueur du remplissage suivant l'identité.</summary>
    public const int HeaderGapBitCount = 64;
    /// <summary>Longueur du remplissage suivant les données.</summary>
    public const int DataGapBitCount = 128;
    /// <summary>Synchronisation FM encodée utilisée pour l'en-tête et les données.</summary>
    public static IReadOnlyList<byte> Sync { get; } = Array.AsReadOnly(TrackBitEncoding.EncodeCompactFm(FirstSyncByte, SecondSyncByte));

    /// <summary>Crée l'exception signalant une taille de secteur incompatible.</summary>
    /// <param name="actualSize">Taille observée.</param>
    /// <returns>Exception décrivant les tailles attendue et observée.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Data General sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
