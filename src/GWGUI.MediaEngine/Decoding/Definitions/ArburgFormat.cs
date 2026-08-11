using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques communes du format Arburg.</summary>
internal static class ArburgFormat
{
    /// <summary>Identifiant technique du codec Arburg.</summary>
    public const string CodecId = FluxCodecIds.Arburg;
    /// <summary>Nom affiché du codec Arburg.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.Arburg;
    /// <summary>Nom du format utilisé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "Arburg";
    /// <summary>Nom de la variante contenant les données ordinaires.</summary>
    public const string DataBlockDescription = "data block";
    /// <summary>Nom de la variante contenant les données système.</summary>
    public const string SystemBlockDescription = "system block";
    /// <summary>Nom du contrôle d'intégrité utilisé par les deux types de blocs.</summary>
    public const string ChecksumDescription = "checksum";
    /// <summary>Nom de l'attribut sélectionnant un bloc système.</summary>
    public const string SystemAttribute = "system";
    /// <summary>Taille physique d'un bloc de données, checksum et remplissage inclus.</summary>
    public const int DataBlockSize = 0xa00;
    /// <summary>Nombre d'octets utiles dans un bloc de données.</summary>
    public const int DataUsefulSize = 0x9fe;
    /// <summary>Taille physique d'un bloc système, checksum et remplissage inclus.</summary>
    public const int SystemBlockSize = 0xf00;
    /// <summary>Nombre d'octets utiles dans un bloc système.</summary>
    public const int SystemUsefulSize = 0xefe;
    /// <summary>Nombre d'octets composant le checksum additif.</summary>
    public const int ChecksumByteCount = 2;
    /// <summary>Nombre de bits encodant un octet FM Arburg.</summary>
    public const int FmEncodedByteBitCount = 32;
    /// <summary>Nombre de bits encodant un bit système à zéro.</summary>
    public const int SystemZeroEncodedBitCount = 2;
    /// <summary>Nombre de bits encodant un bit système à un.</summary>
    public const int SystemOneEncodedBitCount = 3;
    /// <summary>Premier bit commun aux représentations système de zéro et de un.</summary>
    public const bool SystemPrefixBit = false;
    /// <summary>Second bit représentant une valeur système à zéro.</summary>
    public const bool SystemZeroSecondBit = true;
    /// <summary>Second bit représentant une valeur système à un.</summary>
    public const bool SystemOneSecondBit = false;
    /// <summary>Troisième bit terminant une valeur système à un.</summary>
    public const bool SystemOneThirdBit = true;
    /// <summary>Nombre de bits séparant deux blocs produits par l'encodeur.</summary>
    public const int GapBitCount = 64;
    /// <summary>Cylindre logique attribué aux blocs Arburg.</summary>
    public const byte LogicalCylinder = 0;
    /// <summary>Face logique attribuée aux blocs Arburg.</summary>
    public const byte LogicalHead = 0;
    /// <summary>Numéro logique attribué aux blocs Arburg.</summary>
    public const int LogicalSector = 1;
    /// <summary>Code de taille associé aux blocs Arburg non standard.</summary>
    public const byte SectorSizeCode = 0;
    /// <summary>Poids d'un secteur reconnu dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur propre au calcul de confiance Arburg.</summary>
    public const double ConfidenceDivisor = 8;
    /// <summary>Marque précédant un bloc de données.</summary>
    public static IReadOnlyList<byte> DataMark { get; } = Array.AsReadOnly<byte>([0x44, 0x44, 0x44, 0x44, 0x55, 0x55, 0x55, 0x55]);
    /// <summary>Marque précédant un bloc système.</summary>
    public static IReadOnlyList<byte> SystemMark { get; } = Array.AsReadOnly<byte>([0x55, 0x55, 0x55, 0x55, 0x55, 0x24, 0x92, 0x49]);
    /// <summary>Longueur en bits de la marque d'un bloc de données.</summary>
    public static int DataMarkBitCount => DataMark.Count * BitPrimitives.BitsPerByte;
    /// <summary>Longueur en bits de la marque d'un bloc système.</summary>
    public static int SystemMarkBitCount => SystemMark.Count * BitPrimitives.BitsPerByte;
    /// <summary>Valeur d'avancement après la reconnaissance d'une marque de données.</summary>
    public static int DataMarkAdvanceBitCount => DataMarkBitCount - 1;
    /// <summary>Valeur d'avancement après la reconnaissance d'une marque système.</summary>
    public static int SystemMarkAdvanceBitCount => SystemMarkBitCount - 1;

    /// <summary>Crée l'exception signalant une taille de charge utile incompatible.</summary>
    /// <param name="system">Indique si le bloc demandé est un bloc système.</param>
    /// <param name="actualSize">Taille observée.</param>
    /// <returns>Exception contenant les tailles attendues et observées.</returns>
    public static ArgumentException InvalidPayloadSize(bool system, int actualSize) => new($"Arburg {(system ? "system" : "data")} payload must contain {(system ? SystemUsefulSize : DataUsefulSize)} useful bytes or {(system ? SystemBlockSize : DataBlockSize)} complete bytes; received {actualSize} bytes.");
}
