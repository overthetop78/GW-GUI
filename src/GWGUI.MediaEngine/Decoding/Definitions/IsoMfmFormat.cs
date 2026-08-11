using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Décrit une marque ISO MFM.</summary>
internal sealed record IsoMfmMarkDefinition(byte Mark, FluxStructureKind Kind, bool Deleted);

/// <summary>Regroupe les définitions techniques du format ISO MFM.</summary>
internal static class IsoMfmFormat
{
    /// <summary>Identifiant technique.</summary>
    public const string CodecId = FluxCodecIds.IsoMfm;
    /// <summary>Nom affiché.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.IsoMfm;
    /// <summary>Nom utilisé dans les descriptions.</summary>
    public const string StructureDescriptionName = "ISO MFM";
    /// <summary>Octet de synchronisation couvert par le CRC.</summary>
    public const byte SyncByte = 0xa1;
    /// <summary>Nombre d'octets de synchronisation.</summary>
    public const int SyncByteCount = 3;
    /// <summary>Motif encodé d'une synchronisation.</summary>
    public const ushort EncodedSyncByte = 0x4489;
    /// <summary>Trois motifs de synchronisation encodés.</summary>
    public const string EncodedSyncHex = "448944894489";
    /// <summary>Marque d'identité.</summary>
    public const byte IdAddressMark = 0xfe;
    /// <summary>Marque de données normale.</summary>
    public const byte DataAddressMark = 0xfb;
    /// <summary>Marque de données supprimée.</summary>
    public const byte DeletedDataAddressMark = 0xf8;
    /// <summary>Nombre de bits d'un octet MFM.</summary>
    public const int EncodedByteBitCount = 16;
    /// <summary>Longueur des trois synchronisations.</summary>
    public const int SyncBitCount = SyncByteCount * EncodedByteBitCount;
    /// <summary>Longueur des synchronisations et de la marque.</summary>
    public const int SyncAndMarkBitCount = SyncBitCount + EncodedByteBitCount;
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
    /// <summary>Octets suivant la marque d'identité.</summary>
    public const int HeaderBytesAfterMark = HeaderFieldByteCount + CrcByteCount;
    /// <summary>Longueur totale de l'en-tête.</summary>
    public const int HeaderBitCount = SyncAndMarkBitCount + HeaderBytesAfterMark * EncodedByteBitCount;
    /// <summary>Code de taille maximal.</summary>
    public const int MaximumSectorSizeCode = 7;
    /// <summary>Taille sectorielle de base.</summary>
    public const int BaseSectorSize = 128;
    /// <summary>Avancement après une marque.</summary>
    public const int MarkScanAdvance = SyncBitCount - 1;
    /// <summary>Avancement après un en-tête.</summary>
    public const int HeaderScanAdvance = HeaderBitCount - 1;
    /// <summary>Longueur maximale de la queue circulaire.</summary>
    public const int CircularTailBitCount = 20_000;
    /// <summary>Remplissage suivant l'en-tête.</summary>
    public const int HeaderGapBitCount = 160;
    /// <summary>Remplissage suivant les données.</summary>
    public const int DataGapBitCount = 256;
    /// <summary>Polynôme CRC.</summary>
    public const ushort CrcPolynomial = Crc16Calculator.CcittPolynomial;
    /// <summary>Valeur initiale du CRC.</summary>
    public const ushort CrcInitialValue = Crc16Calculator.AllBitsSetInitialValue;
    /// <summary>Poids d'un secteur valide dans le score.</summary>
    public const int ValidSectorScoreWeight = 1000;
    /// <summary>Poids d'un secteur contenant des données.</summary>
    public const int DataSectorScoreWeight = 10;
    /// <summary>Poids du nombre total de secteurs.</summary>
    public const int SectorScoreWeight = 1;
    /// <summary>Poids d'un secteur dans la confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur du calcul de confiance.</summary>
    public const double ConfidenceDivisor = 12;
    /// <summary>Facteurs PLL essayés dans l'ordre.</summary>
    public static IReadOnlyList<double> PllFactors { get; } = Array.AsReadOnly([1d, .98, 1.02, .96, 1.04, .94, 1.06]);
    /// <summary>Définitions fermées des marques.</summary>
    public static IReadOnlyList<IsoMfmMarkDefinition> Marks { get; } = Array.AsReadOnly<IsoMfmMarkDefinition>(
    [
        new(IdAddressMark, FluxStructureKind.IdAddressMark, false),
        new(DataAddressMark, FluxStructureKind.DataAddressMark, false),
        new(DeletedDataAddressMark, FluxStructureKind.DeletedDataAddressMark, true)
    ]);

    /// <summary>Calcule la taille associée au code fourni.</summary>
    public static int SectorSize(byte sizeCode) => sizeCode <= MaximumSectorSizeCode ? BaseSectorSize << sizeCode : 0;
}
