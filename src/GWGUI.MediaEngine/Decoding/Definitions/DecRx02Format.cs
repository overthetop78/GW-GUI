using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Indique l'encodage d'un bloc de données RX02.</summary>
internal enum DecRx02DataEncoding
{
    /// <summary>Encodage FM double largeur.</summary>
    Fm,
    /// <summary>Encodage M²FM.</summary>
    M2Fm
}

/// <summary>Décrit une marque de données RX02.</summary>
/// <param name="Mark">Valeur décodée.</param>
/// <param name="Pattern">Motif binaire.</param>
/// <param name="Encoding">Encodage des données.</param>
/// <param name="Deleted">Indique un secteur supprimé.</param>
/// <param name="SectorSize">Taille physique.</param>
/// <param name="SizeCode">Code de taille attendu dans l'en-tête.</param>
internal sealed record DecRx02DataMarkDefinition(byte Mark, IReadOnlyList<byte> Pattern, DecRx02DataEncoding Encoding, bool Deleted, int SectorSize, byte SizeCode);

/// <summary>Regroupe les définitions techniques du format DEC RX02.</summary>
internal static class DecRx02Format
{
    /// <summary>Identifiant technique du codec.</summary>
    public const string CodecId = FluxCodecIds.DecRx02;
    /// <summary>Nom affiché du codec.</summary>
    public const string CodecDisplayName = "DEC RX02 M²FM";
    /// <summary>Nom utilisé dans les descriptions.</summary>
    public const string StructureDescriptionName = "DEC RX02";
    /// <summary>Nom de l'encodage FM.</summary>
    public const string FmEncodingName = "FM";
    /// <summary>Nom de l'encodage M²FM.</summary>
    public const string M2FmEncodingName = "M²FM";
    /// <summary>Libellé du CRC d'en-tête.</summary>
    public const string HeaderCrcDescription = "header CRC";
    /// <summary>Libellé du CRC des données.</summary>
    public const string DataCrcDescription = "data CRC";
    /// <summary>Variante d'un en-tête sectoriel.</summary>
    public const string SectorHeaderDescription = "sector header";
    /// <summary>Variante d'une marque de données non appariée.</summary>
    public const string UnpairedDataDescription = "data";
    /// <summary>Motif binaire de la marque d'en-tête.</summary>
    /// <summary>Valeur décodée de la marque d'en-tête.</summary>
    public const byte HeaderAddressMark = 0xfe;
    /// <summary>Marque FM supprimée.</summary>
    public const byte FmDeletedDataMark = 0xf8;
    /// <summary>Marque M²FM normale.</summary>
    public const byte M2FmDataMark = 0xf9;
    /// <summary>Marque FM alternative FA.</summary>
    public const byte DataMarkFa = 0xfa;
    /// <summary>Marque FM normale.</summary>
    public const byte FmDataMark = 0xfb;
    /// <summary>Marque FM alternative FC.</summary>
    public const byte DataMarkFc = 0xfc;
    /// <summary>Marque M²FM supprimée.</summary>
    public const byte M2FmDeletedDataMark = 0xfd;
    /// <summary>Nombre d'octets du motif d'une marque.</summary>
    public const int MarkByteCount = 4;
    /// <summary>Nombre de bits du motif d'une marque.</summary>
    public const int MarkBitCount = MarkByteCount * BitPrimitives.BitsPerByte;
    /// <summary>Nombre d'octets physiques de l'en-tête, marque comprise.</summary>
    public const int PhysicalHeaderByteCount = 7;
    /// <summary>Nombre d'octets décodés suivant la marque d'en-tête.</summary>
    public const int HeaderDecodedByteCount = PhysicalHeaderByteCount - 1;
    /// <summary>Position du cylindre.</summary>
    public const int HeaderCylinderOffset = 0;
    /// <summary>Position de la face.</summary>
    public const int HeaderHeadOffset = 1;
    /// <summary>Position du secteur.</summary>
    public const int HeaderSectorOffset = 2;
    /// <summary>Position du code de taille.</summary>
    public const int HeaderSizeCodeOffset = 3;
    /// <summary>Position de l'octet fort du CRC.</summary>
    public const int HeaderCrcHighOffset = 4;
    /// <summary>Position de l'octet faible du CRC.</summary>
    public const int HeaderCrcLowOffset = 5;
    /// <summary>Nombre d'octets du CRC.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Taille d'un secteur FM.</summary>
    public const int FmSectorByteCount = 128;
    /// <summary>Taille d'un secteur M²FM.</summary>
    public const int M2FmSectorByteCount = DecRx02Geometry.PhysicalSectorSize;
    /// <summary>Code de taille FM.</summary>
    public const byte FmSectorSizeCode = 0;
    /// <summary>Code de taille M²FM.</summary>
    public const byte M2FmSectorSizeCode = 1;
    /// <summary>Nombre de bits d'un octet M²FM.</summary>
    public const int EncodedMfmByteBitCount = 16;
    /// <summary>Nombre de bits d'un octet FM double largeur.</summary>
    public const int EncodedFmByteBitCount = 32;
    /// <summary>Décalage de phase M²FM.</summary>
    public const int M2FmPhaseBitCount = 1;
    /// <summary>Longueur totale d'un en-tête encodé.</summary>
    public const int HeaderBitCount = MarkBitCount + HeaderDecodedByteCount * EncodedFmByteBitCount;
    /// <summary>Longueur du remplissage produit par l'encodeur.</summary>
    public const int GapBitCount = 64;
    /// <summary>Distance maximale de recherche d'une marque de données.</summary>
    public const int MaximumDataSearchDistanceBits = (88 + 16) * BitPrimitives.BitsPerByte * 2;
    /// <summary>Polynôme CRC RX02.</summary>
    public const ushort CrcPolynomial = Crc16Calculator.CcittPolynomial;
    /// <summary>Valeur initiale du CRC RX02.</summary>
    public const ushort CrcInitialValue = Crc16Calculator.AllBitsSetInitialValue;
    /// <summary>Poids d'un secteur dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur du calcul de confiance.</summary>
    public const double ConfidenceDivisor = 20;
    /// <summary>Motif normal remplacé lors de la conversion M²FM.</summary>
    public static IReadOnlyList<bool> NormalM2FmRule { get; } = Array.AsReadOnly([false, false, true, false, true, false, true, false, true, false, false]);
    /// <summary>Motif encodé utilisé par la conversion M²FM.</summary>
    public static IReadOnlyList<bool> EncodedM2FmRule { get; } = Array.AsReadOnly([false, true, false, false, false, true, false, false, false, true, false]);
    /// <summary>Motif binaire de la marque d'en-tête.</summary>
    public static IReadOnlyList<byte> HeaderMark { get; } = FmAddressMarkPatterns.For(HeaderAddressMark);
    /// <summary>Définitions fermées des six marques de données.</summary>
    public static IReadOnlyList<DecRx02DataMarkDefinition> DataMarks { get; } = Array.AsReadOnly<DecRx02DataMarkDefinition>(
    [
        Mark(FmDeletedDataMark, DecRx02DataEncoding.Fm, true),
        Mark(M2FmDataMark, DecRx02DataEncoding.M2Fm, false),
        Mark(DataMarkFa, DecRx02DataEncoding.Fm, false),
        Mark(FmDataMark, DecRx02DataEncoding.Fm, false),
        Mark(DataMarkFc, DecRx02DataEncoding.Fm, false),
        Mark(M2FmDeletedDataMark, DecRx02DataEncoding.M2Fm, true)
    ]);

    /// <summary>Crée l'exception signalant une taille de secteur incompatible.</summary>
    /// <param name="actualSize">Taille observée.</param>
    /// <returns>Exception décrivant les tailles admises.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"DEC RX sectors contain {FmSectorByteCount} or {M2FmSectorByteCount} bytes; received {actualSize} bytes.");

    /// <summary>Construit une définition de marque depuis son motif et son encodage.</summary>
    private static DecRx02DataMarkDefinition Mark(byte mark, DecRx02DataEncoding encoding, bool deleted) => new(mark, FmAddressMarkPatterns.For(mark), encoding, deleted, encoding == DecRx02DataEncoding.M2Fm ? M2FmSectorByteCount : FmSectorByteCount, encoding == DecRx02DataEncoding.M2Fm ? M2FmSectorSizeCode : FmSectorSizeCode);
}
