using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format QD MO5 MFM.</summary>
internal static class QdMo5MfmFormat
{
    /// <summary>Identifiant technique du codec.</summary>
    public const string CodecId = FluxCodecIds.QdMo5Mfm;
    /// <summary>Nom affiché du codec.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.QdMo5Mfm;
    /// <summary>Nom employé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "QD MO5";
    /// <summary>Nombre d'octets physiques du préambule commun.</summary>
    public const int PreambleByteCount = 10;
    /// <summary>Longueur du préambule commun en bits.</summary>
    public const int PreambleBitCount = PreambleByteCount * BitPrimitives.BitsPerByte;
    /// <summary>Nombre d'octets physiques de chaque motif complet.</summary>
    public const int PhysicalMarkByteCount = 12;
    /// <summary>Longueur physique de chaque motif complet.</summary>
    public const int PhysicalMarkBitCount = PhysicalMarkByteCount * BitPrimitives.BitsPerByte;
    /// <summary>Nombre de bits encodant un octet MFM.</summary>
    public const int EncodedByteBitCount = 16;
    /// <summary>Marque d'en-tête décodée.</summary>
    public const byte HeaderAddressMark = 0xfb;
    /// <summary>Nombre d'octets du numéro de secteur.</summary>
    public const int SectorNumberByteCount = 2;
    /// <summary>Nombre d'octets réservés de l'en-tête.</summary>
    public const int HeaderPaddingByteCount = 13;
    /// <summary>Nombre d'octets MFM après le préambule de l'en-tête.</summary>
    public const int HeaderBytesAfterPreamble = 1 + SectorNumberByteCount + HeaderPaddingByteCount;
    /// <summary>Longueur complète de l'en-tête.</summary>
    public const int HeaderBitCount = PreambleBitCount + HeaderBytesAfterPreamble * EncodedByteBitCount;
    /// <summary>Préfixe de données par défaut.</summary>
    public const byte DefaultPrefix = 0x5a;
    /// <summary>Nom de l'attribut portant le préfixe.</summary>
    public const string PrefixAttribute = "prefix";
    /// <summary>Nombre d'octets de préfixe.</summary>
    public const int DataPrefixByteCount = 1;
    /// <summary>Taille de la charge utile.</summary>
    public const int SectorSize = 128;
    /// <summary>Nombre d'octets du checksum.</summary>
    public const int ChecksumByteCount = 1;
    /// <summary>Nombre d'octets MFM après le préambule des données.</summary>
    public const int DataBytesAfterPreamble = DataPrefixByteCount + SectorSize + ChecksumByteCount;
    /// <summary>Longueur complète du bloc de données.</summary>
    public const int DataBlockBitCount = PreambleBitCount + DataBytesAfterPreamble * EncodedByteBitCount;
    /// <summary>Distance maximale de recherche des données.</summary>
    public const int DataSearchBitCount = (88 + 16) * BitPrimitives.BitsPerByte;
    /// <summary>Cylindre logique du format.</summary>
    public const byte LogicalCylinder = 0;
    /// <summary>Face logique du format.</summary>
    public const byte LogicalHead = 0;
    /// <summary>Code de taille des secteurs de 128 octets.</summary>
    public const byte SectorSizeCode = 0;
    /// <summary>Gap suivant l'en-tête.</summary>
    public const int HeaderGapBitCount = 160;
    /// <summary>Gap suivant les données.</summary>
    public const int DataGapBitCount = 128;
    /// <summary>Poids d'un secteur dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur du calcul de confiance.</summary>
    public const double ConfidenceDivisor = 20;
    /// <summary>Préambule physique commun aux deux motifs.</summary>
    public static IReadOnlyList<byte> Preamble { get; } = Array.AsReadOnly<byte>([0xa9, 0x14, 0xa9, 0x14, 0xa9, 0x14, 0xa9, 0x14, 0xa9, 0x14]);
    /// <summary>Deux octets physiques encodant la marque d'en-tête.</summary>
    public static IReadOnlyList<byte> EncodedHeaderMark { get; } = Array.AsReadOnly<byte>([0x44, 0x91]);
    /// <summary>Deux octets physiques encodant le préfixe par défaut.</summary>
    public static IReadOnlyList<byte> EncodedDefaultDataPrefix { get; } = Array.AsReadOnly<byte>([0x91, 0x44]);
    /// <summary>Motif physique complet de l'en-tête.</summary>
    public static IReadOnlyList<byte> HeaderMark { get; } = Array.AsReadOnly(Preamble.Concat(EncodedHeaderMark).ToArray());
    /// <summary>Motif physique complet des données avec le préfixe par défaut.</summary>
    public static IReadOnlyList<byte> DataMark { get; } = Array.AsReadOnly(Preamble.Concat(EncodedDefaultDataPrefix).ToArray());

    /// <summary>Crée l'exception signalant une taille de secteur invalide.</summary>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"QD MO5 sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}

/// <summary>Calcule le checksum additif du format QD MO5.</summary>
internal static class QdMo5Checksum
{
    /// <summary>Calcule la somme du préfixe et de la charge utile.</summary>
    public static byte Compute(byte prefix, IEnumerable<byte> data) => (byte)(prefix + data.Sum(value => value));
}

/// <summary>Représente un en-tête QD MO5 décodé.</summary>
internal sealed record QdMo5MfmHeader(int Offset, int Sector, byte[] ReservedBytes);

/// <summary>Représente un bloc de données QD MO5 décodé.</summary>
internal sealed record QdMo5MfmData(int Offset, byte Prefix, byte[] Payload, byte StoredChecksum, bool ChecksumValid);
