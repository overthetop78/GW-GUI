using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Décrit une marque du format ISO FM.</summary>
internal sealed record IsoFmMarkDefinition(ushort Pattern, byte Mark, FluxStructureKind Kind, bool Deleted);

/// <summary>Regroupe les définitions techniques du format ISO FM.</summary>
internal static class IsoFmFormat
{
    /// <summary>Identifiant technique.</summary>
    public const string CodecId = FluxCodecIds.IsoFm;
    /// <summary>Nom affiché.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.IsoFm;
    /// <summary>Nom utilisé dans les descriptions.</summary>
    public const string StructureDescriptionName = "ISO FM";
    /// <summary>Marque d'identité.</summary>
    public const byte IdAddressMark = 0xfe;
    /// <summary>Marque de données normale.</summary>
    public const byte DataAddressMark = 0xfb;
    /// <summary>Marque de données supprimée.</summary>
    public const byte DeletedDataAddressMark = 0xf8;
    /// <summary>Motif encodé de la marque d'identité.</summary>
    public const ushort EncodedIdAddressMark = 0xf57e;
    /// <summary>Motif encodé des données normales.</summary>
    public const ushort EncodedDataAddressMark = 0xf56f;
    /// <summary>Motif encodé des données supprimées.</summary>
    public const ushort EncodedDeletedDataAddressMark = 0xf56a;
    /// <summary>Longueur d'une marque.</summary>
    public const int EncodedMarkBitCount = 16;
    /// <summary>Longueur d'un octet FM.</summary>
    public const int EncodedByteBitCount = 16;
    /// <summary>Nombre de champs CHRN.</summary>
    public const int HeaderFieldByteCount = 4;
    /// <summary>Position du cylindre.</summary>
    public const int HeaderCylinderOffset = 0;
    /// <summary>Position de la face.</summary>
    public const int HeaderHeadOffset = 1;
    /// <summary>Position du secteur.</summary>
    public const int HeaderSectorOffset = 2;
    /// <summary>Position du code de taille.</summary>
    public const int HeaderSizeCodeOffset = 3;
    /// <summary>Nombre d'octets du CRC.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Nombre d'octets suivant la marque d'identité.</summary>
    public const int HeaderBytesAfterMark = HeaderFieldByteCount + CrcByteCount;
    /// <summary>Longueur totale de l'en-tête.</summary>
    public const int HeaderBitCount = EncodedMarkBitCount + HeaderBytesAfterMark * EncodedByteBitCount;
    /// <summary>Code de taille maximal.</summary>
    public const int MaximumSectorSizeCode = 7;
    /// <summary>Taille sectorielle de base.</summary>
    public const int BaseSectorSize = 128;
    /// <summary>Plus grande valeur représentable par les champs CHRN sur un octet.</summary>
    public const int MaximumAddressValue = byte.MaxValue;
    /// <summary>Avancement après une marque.</summary>
    public const int MarkScanAdvance = EncodedMarkBitCount - 1;
    /// <summary>Avancement après un en-tête.</summary>
    public const int HeaderScanAdvance = HeaderBitCount - 1;
    /// <summary>Remplissage suivant l'en-tête.</summary>
    public const int HeaderGapBitCount = 160;
    /// <summary>Remplissage suivant les données.</summary>
    public const int DataGapBitCount = 256;
    /// <summary>Polynôme CRC.</summary>
    public const ushort CrcPolynomial = Crc16Calculator.CcittPolynomial;
    /// <summary>Valeur initiale du CRC.</summary>
    public const ushort CrcInitialValue = Crc16Calculator.AllBitsSetInitialValue;
    /// <summary>Poids d'un secteur dans la confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur du calcul de confiance.</summary>
    public const double ConfidenceDivisor = 18;
    /// <summary>Définitions fermées des trois marques.</summary>
    public static IReadOnlyList<IsoFmMarkDefinition> Marks { get; } = Array.AsReadOnly<IsoFmMarkDefinition>(
    [
        new(EncodedIdAddressMark, IdAddressMark, FluxStructureKind.IdAddressMark, false),
        new(EncodedDataAddressMark, DataAddressMark, FluxStructureKind.DataAddressMark, false),
        new(EncodedDeletedDataAddressMark, DeletedDataAddressMark, FluxStructureKind.DeletedDataAddressMark, true)
    ]);

    /// <summary>Calcule la taille sectorielle associée au code fourni.</summary>
    public static int SectorSize(byte sizeCode) => sizeCode <= MaximumSectorSizeCode ? BaseSectorSize << sizeCode : 0;

    /// <summary>Obtient la définition fermée de la marque de données normale ou supprimée.</summary>
    public static IsoFmMarkDefinition DataMark(bool deleted) => Marks.Single(mark => mark.Deleted == deleted && mark.Mark != IdAddressMark);

    /// <summary>Crée l'erreur signalant un code de taille incompatible avec les données.</summary>
    public static ArgumentException InvalidSizeCode(byte sizeCode, int actualSize) => new($"ISO FM size code {sizeCode} describes {SectorSize(sizeCode)} bytes; received {actualSize} bytes.");
}
