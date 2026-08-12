using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Décrit une marque de données TYCOM FM.</summary>
internal sealed record TycomFmMarkDefinition(byte Mark, IReadOnlyList<byte> Pattern, bool Deleted);

/// <summary>Regroupe les définitions techniques du format TYCOM FM.</summary>
internal static class TycomFmFormat
{
    /// <summary>Identifiant technique du codec.</summary>
    public const string CodecId = FluxCodecIds.TycomFm;
    /// <summary>Nom affiché du codec.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.TycomFm;
    /// <summary>Nom employé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "TYCOM";
    /// <summary>Marque d'adresse d'en-tête.</summary>
    public const byte HeaderAddressMark = 0xfe;
    /// <summary>Première marque de données acceptée.</summary>
    public const byte FirstDataMark = 0xf8;
    /// <summary>Marque de données supprimées.</summary>
    public const byte DeletedDataMark = FirstDataMark;
    /// <summary>Dernière marque de données acceptée.</summary>
    public const byte DataMark = 0xfb;
    /// <summary>Plus petit cylindre encodable.</summary>
    public const int MinimumCylinder = byte.MinValue;
    /// <summary>Plus grand cylindre encodable.</summary>
    public const int MaximumCylinder = byte.MaxValue;
    /// <summary>Plus petit numéro de secteur encodable.</summary>
    public const int MinimumSector = byte.MinValue;
    /// <summary>Plus grand numéro de secteur encodable.</summary>
    public const int MaximumSector = byte.MaxValue;
    /// <summary>Nombre d'octets physiques d'une marque.</summary>
    public const int MarkByteCount = 4;
    /// <summary>Longueur d'une marque en bits.</summary>
    public const int MarkBitCount = MarkByteCount * BitPrimitives.BitsPerByte;
    /// <summary>Nombre d'octets décodés suivant la marque d'en-tête.</summary>
    public const int HeaderDecodedByteCount = 4;
    /// <summary>Position du cylindre dans l'en-tête décodé.</summary>
    public const int HeaderCylinderOffset = 0;
    /// <summary>Position du secteur dans l'en-tête décodé.</summary>
    public const int HeaderSectorOffset = 1;
    /// <summary>Position du CRC fort dans l'en-tête décodé.</summary>
    public const int HeaderCrcHighOffset = 2;
    /// <summary>Position du CRC faible dans l'en-tête décodé.</summary>
    public const int HeaderCrcLowOffset = 3;
    /// <summary>Nombre de bits d'un octet FM double largeur.</summary>
    public const int EncodedByteBitCount = 32;
    /// <summary>Longueur totale de l'en-tête.</summary>
    public const int HeaderBitCount = MarkBitCount + HeaderDecodedByteCount * EncodedByteBitCount;
    /// <summary>Taille d'un secteur.</summary>
    public const int SectorSize = 128;
    /// <summary>Nombre d'octets du CRC.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Nombre d'octets du bloc de données, marque comprise.</summary>
    public const int DataBlockByteCount = 1 + SectorSize + CrcByteCount;
    /// <summary>Longueur du bloc de données.</summary>
    public const int DataBlockBitCount = DataBlockByteCount * EncodedByteBitCount;
    /// <summary>Face logique des secteurs TYCOM.</summary>
    public const byte LogicalHead = 0;
    /// <summary>Code de taille des secteurs de 128 octets.</summary>
    public const byte SectorSizeCode = 0;
    /// <summary>Distance maximale de recherche des données.</summary>
    public const int MaximumDataSearchDistanceBits = (88 + 16) * BitPrimitives.BitsPerByte * 2;
    /// <summary>Polynôme du CRC.</summary>
    public const ushort CrcPolynomial = Crc16Calculator.CcittPolynomial;
    /// <summary>Valeur initiale du CRC.</summary>
    public const ushort CrcInitialValue = Crc16Calculator.AllBitsSetInitialValue;
    /// <summary>Gap ajouté par l'encodeur.</summary>
    public const int GapBitCount = 64;
    /// <summary>Poids d'un secteur dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur du calcul de confiance.</summary>
    public const double ConfidenceDivisor = 20;
    /// <summary>Motif physique de la marque d'en-tête.</summary>
    public static IReadOnlyList<byte> HeaderMark { get; } = FmAddressMarkPatterns.For(HeaderAddressMark);
    /// <summary>Définitions fermées des quatre marques de données.</summary>
    public static IReadOnlyList<TycomFmMarkDefinition> DataMarks { get; } = Array.AsReadOnly(Enumerable.Range(FirstDataMark, DataMark - FirstDataMark + 1).Select(value => new TycomFmMarkDefinition((byte)value, FmAddressMarkPatterns.For((byte)value), value == DeletedDataMark)).ToArray());

    /// <summary>Sélectionne ensemble la marque et son motif selon l'état supprimé du secteur.</summary>
    public static TycomFmMarkDefinition SelectDataMark(bool deleted) => DataMarks.Single(definition => definition.Mark == (deleted ? DeletedDataMark : DataMark));

    /// <summary>Crée l'exception signalant une taille de secteur invalide.</summary>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"TYCOM sectors contain {SectorSize} bytes; received {actualSize} bytes.");
    /// <summary>Crée l'exception signalant un cylindre impossible à encoder.</summary>
    public static ArgumentOutOfRangeException InvalidCylinder(int cylinder) => TrackEncodingExceptions.FormatValueOutOfRange(StructureDescriptionName, "cylinder", cylinder, MaximumCylinder);
    /// <summary>Crée l'exception signalant un numéro de secteur impossible à encoder.</summary>
    public static ArgumentOutOfRangeException InvalidSector(int sector) => TrackEncodingExceptions.FormatValueOutOfRange(StructureDescriptionName, "sector number", sector, MaximumSector);
}

/// <summary>Calcule et sérialise les CRC TYCOM partagés par le décodeur et l'encodeur.</summary>
internal static class TycomFmCrc
{
    /// <summary>Calcule le CRC de la marque d'adresse, du cylindre et du secteur.</summary>
    public static ushort ComputeHeader(byte cylinder, byte sector) => Crc16Calculator.Compute([TycomFmFormat.HeaderAddressMark, cylinder, sector], TycomFmFormat.CrcPolynomial, TycomFmFormat.CrcInitialValue);
    /// <summary>Calcule le CRC de la marque de données et de la charge utile.</summary>
    public static ushort ComputeData(byte mark, IEnumerable<byte> data) => Crc16Calculator.Compute(new[] { mark }.Concat(data), TycomFmFormat.CrcPolynomial, TycomFmFormat.CrcInitialValue);
    /// <summary>Sérialise un CRC avec l'octet fort en premier.</summary>
    public static byte[] ToBigEndianBytes(ushort crc) => [(byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc];
    /// <summary>Vérifie un champ incluant ses deux octets de CRC.</summary>
    public static bool IsValid(IEnumerable<byte> bytes) => Crc16Calculator.Compute(bytes, TycomFmFormat.CrcPolynomial, TycomFmFormat.CrcInitialValue) == 0;
}

/// <summary>Représente un en-tête TYCOM décodé.</summary>
internal sealed record TycomFmHeader(int Offset, byte Cylinder, byte Sector, bool CrcValid, byte[] Bytes);

/// <summary>Représente une marque de données TYCOM trouvée.</summary>
internal sealed record TycomFmDataMark(int Offset, TycomFmMarkDefinition Definition);

/// <summary>Représente un bloc de données TYCOM décodé.</summary>
internal sealed record TycomFmData(TycomFmDataMark Mark, byte[] Payload, byte[] Bytes, bool CrcValid);
