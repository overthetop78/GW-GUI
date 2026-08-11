using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format E-mu FM.</summary>
internal static class EmuFmFormat
{
    /// <summary>Identifiant technique du codec.</summary>
    public const string CodecId = FluxCodecIds.EmuFm;
    /// <summary>Nom affiché du codec.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.EmuFm;
    /// <summary>Nom utilisé dans les descriptions.</summary>
    public const string StructureDescriptionName = "E-mu";
    /// <summary>Nom utilisé pour une marque non classée.</summary>
    public const string UnclassifiedStructureName = "E-mu Emulator";
    /// <summary>Variante de la marque commune.</summary>
    public const string MarkDescription = "header/data mark";
    /// <summary>Libellé du contrôle CRC.</summary>
    public const string CrcDescription = "CRC";
    /// <summary>Taille de la marque commune encodée, en octets.</summary>
    public const int MarkByteCount = 8;
    /// <summary>Taille de la marque commune, en bits.</summary>
    public const int MarkBitCount = MarkByteCount * BitPrimitives.BitsPerByte;
    /// <summary>Nombre de bits d'un octet FM double largeur.</summary>
    public const int EncodedFmByteBitCount = 32;
    /// <summary>Nombre d'octets décodés de l'en-tête.</summary>
    public const int HeaderDecodedByteCount = 3;
    /// <summary>Position de la piste brute.</summary>
    public const int HeaderRawTrackOffset = 0;
    /// <summary>Position de l'octet fort du CRC d'en-tête.</summary>
    public const int HeaderCrcHighOffset = 1;
    /// <summary>Position de l'octet faible du CRC d'en-tête.</summary>
    public const int HeaderCrcLowOffset = 2;
    /// <summary>Longueur totale de l'en-tête encodé.</summary>
    public const int HeaderBitCount = MarkBitCount + HeaderDecodedByteCount * EncodedFmByteBitCount;
    /// <summary>Taille du secteur E-mu.</summary>
    public const int SectorSize = 0xe00;
    /// <summary>Nombre d'octets utiles du secteur.</summary>
    public const int PayloadByteCount = 3584;
    /// <summary>Nombre d'octets du CRC.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Nombre d'octets du bloc de données complet.</summary>
    public const int DataBlockByteCount = PayloadByteCount + CrcByteCount;
    /// <summary>Numéro logique du secteur unique.</summary>
    public const byte SectorNumber = 1;
    /// <summary>Code de taille produit.</summary>
    public const byte SectorSizeCode = 0;
    /// <summary>Décalage séparant cylindre et face.</summary>
    public const int TrackShift = 1;
    /// <summary>Masque isolant la face.</summary>
    public const byte HeadMask = 1;
    /// <summary>Longueur du remplissage produit par l'encodeur.</summary>
    public const int GapBitCount = 64;
    /// <summary>Distance maximale de recherche des données.</summary>
    public const int MaximumDataSearchDistanceBits = (88 + 16) * BitPrimitives.BitsPerByte * 2;
    /// <summary>Polynôme CRC E-mu.</summary>
    public const ushort CrcPolynomial = Crc16Calculator.IbmPolynomial;
    /// <summary>Valeur initiale du CRC E-mu.</summary>
    public const ushort CrcInitialValue = Crc16Calculator.ZeroInitialValue;
    /// <summary>Poids d'un secteur dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur du calcul de confiance.</summary>
    public const double ConfidenceDivisor = 20;
    /// <summary>Marque binaire commune aux en-têtes et aux données.</summary>
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly<byte>([0x45, 0x45, 0x55, 0x55, 0x45, 0x54, 0x54, 0x45]);

    /// <summary>Crée l'exception signalant une taille de secteur incompatible.</summary>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"E-mu sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
