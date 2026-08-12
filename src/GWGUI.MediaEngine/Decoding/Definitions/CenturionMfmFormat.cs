using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques communes du format Centurion MFM.</summary>
internal static class CenturionMfmFormat
{
    /// <summary>Identifiant technique du codec Centurion MFM.</summary>
    public const string CodecId = FluxCodecIds.CenturionMfm;
    /// <summary>Nom affiché du codec Centurion MFM.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.CenturionMfm;
    /// <summary>Nom du format utilisé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "Centurion";
    /// <summary>Nom du contrôle d'intégrité Centurion.</summary>
    public const string CrcDescription = "CRC";
    /// <summary>Description d'une marque de secteur tronquée.</summary>
    public const string SectorMarkDescription = "sector mark";
    /// <summary>Description d'un bloc de données non apparié.</summary>
    public const string DataBlockDescription = "data block";
    /// <summary>Description d'un CRC indisponible.</summary>
    public const string UnavailableCrcDescription = "CRC unavailable";
    /// <summary>Clé reconnue dans le préfixe d'un bloc de données.</summary>
    public const byte SupportedDataKey = 0;
    /// <summary>Nombre d'octets composant l'en-tête, CRC inclus.</summary>
    public const int HeaderByteCount = 4;
    /// <summary>Position du cylindre dans l'en-tête.</summary>
    public const int HeaderCylinderOffset = 0;
    /// <summary>Position du secteur dans l'en-tête.</summary>
    public const int HeaderSectorOffset = 1;
    /// <summary>Position du CRC dans l'en-tête.</summary>
    public const int HeaderCrcOffset = 2;
    /// <summary>Nombre d'octets composant le préfixe des données.</summary>
    public const int DataPrefixByteCount = 3;
    /// <summary>Position de la clé dans le préfixe des données.</summary>
    public const int DataKeyOffset = 0;
    /// <summary>Position du champ de taille dans le préfixe des données.</summary>
    public const int DataSizeOffset = 1;
    /// <summary>Longueur du champ de taille, en octets.</summary>
    public const int DataSizeByteCount = 2;
    /// <summary>Nombre d'octets précédant la charge utile dans le bloc couvert par le CRC.</summary>
    public const int DataCrcPrefixByteCount = 2;
    /// <summary>Nombre d'octets composant un CRC Centurion.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Taille d'un bloc d'allocation Centurion.</summary>
    public const int AllocationBlockSize = 256;
    /// <summary>Nombre minimal de blocs alloués à une charge utile, même vide.</summary>
    public const int MinimumAllocationBlockCount = 1;
    /// <summary>Plus grand numéro de cylindre ou de secteur écrit sur un octet.</summary>
    public const int MaximumAddressValue = byte.MaxValue;
    /// <summary>Plus grand nombre de blocs écrit dans le champ de taille.</summary>
    public const int MaximumAllocationBlockCount = byte.MaxValue;
    /// <summary>Octet réservé placé avant le nombre de blocs.</summary>
    public const byte ReservedDataPrefixByte = SupportedDataKey;
    /// <summary>Octet de remplissage du dernier bloc.</summary>
    public const byte PaddingByte = 0;
    /// <summary>Distance de recherche entre la fin de l'en-tête et les données.</summary>
    public const int DataSearchDistanceBitCount = 400;
    /// <summary>Nombre de bits séparant l'en-tête et les données produits par l'encodeur.</summary>
    public const int HeaderGapBitCount = DataSearchDistanceBitCount;
    /// <summary>Nombre de bits ajoutés après un bloc de données.</summary>
    public const int DataGapBitCount = 128;
    /// <summary>Nombre de bits encodant un octet MFM.</summary>
    public const int EncodedByteBitCount = 16;
    /// <summary>Face logique portée par les secteurs Centurion.</summary>
    public const byte LogicalHead = 0;
    /// <summary>Polynôme du CRC Centurion.</summary>
    public const ushort CrcPolynomial = Crc16Calculator.CcittPolynomial;
    /// <summary>Valeur initiale du CRC Centurion.</summary>
    public const ushort CrcInitialValue = Crc16Calculator.ZeroInitialValue;
    /// <summary>Poids d'un secteur reconnu dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur propre au calcul de confiance Centurion.</summary>
    public const double ConfidenceDivisor = 20;
    /// <summary>Marque précédant un en-tête de secteur.</summary>
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly<byte>([0x91, 0x22, 0x44, 0x89]);
    /// <summary>Marque précédant un bloc de données.</summary>
    public static IReadOnlyList<byte> DataMark { get; } = Array.AsReadOnly<byte>([0xaa, 0xaa, 0xaa, 0xa9]);
    /// <summary>Longueur de la marque de secteur, en bits.</summary>
    public static int SectorMarkBitCount => SectorMark.Count * BitPrimitives.BitsPerByte;
    /// <summary>Longueur de la marque de données, en bits.</summary>
    public static int DataMarkBitCount => DataMark.Count * BitPrimitives.BitsPerByte;
    /// <summary>Longueur minimale d'un en-tête complet, en bits.</summary>
    public static int HeaderBitCount => SectorMarkBitCount + HeaderByteCount * EncodedByteBitCount;
    /// <summary>Longueur minimale d'un préfixe de données complet, en bits.</summary>
    public static int DataPrefixBitCount => DataMarkBitCount + DataPrefixByteCount * EncodedByteBitCount;
    /// <summary>Valeur d'avancement après la reconnaissance d'une marque de secteur.</summary>
    public static int SectorMarkAdvanceBitCount => SectorMarkBitCount - 1;
    /// <summary>Valeur d'avancement après la reconnaissance d'une marque de données.</summary>
    public static int DataMarkAdvanceBitCount => DataMarkBitCount - 1;

    /// <summary>Décrit une clé de données non prise en charge.</summary>
    /// <param name="key">Clé observée.</param>
    /// <returns>Description contenant la clé.</returns>
    public static string UnsupportedKeyDescription(byte key) => $"unsupported key {key}";
}
