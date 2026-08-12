using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Heathkit FM.</summary>
internal static class HeathkitFmFormat
{
    /// <summary>Identifiant technique du codec.</summary>
    public const string CodecId = FluxCodecIds.HeathkitFm;
    /// <summary>Nom affiché du codec.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.HeathkitFm;
    /// <summary>Nom utilisé dans les descriptions.</summary>
    public const string StructureDescriptionName = "Heathkit";
    /// <summary>Marque d'adresse.</summary>
    public const byte AddressMark = 0xbf;
    /// <summary>Nombre d'octets nuls précédant la marque.</summary>
    public const int SyncZeroCount = 3;
    /// <summary>Nombre d'octets de la marque FM.</summary>
    public const int MarkByteCount = SyncZeroCount + 1;
    /// <summary>Nombre de bits de la marque encodée.</summary>
    public const int MarkBitCount = MarkByteCount * EncodedFmByteBitCount;
    /// <summary>Nombre d'octets suivant la marque d'en-tête.</summary>
    public const int HeaderByteCount = 4;
    /// <summary>Position du volume.</summary>
    public const int HeaderVolumeOffset = 0;
    /// <summary>Position du cylindre.</summary>
    public const int HeaderCylinderOffset = 1;
    /// <summary>Position du secteur.</summary>
    public const int HeaderSectorOffset = 2;
    /// <summary>Position du checksum.</summary>
    public const int HeaderChecksumOffset = 3;
    /// <summary>Nombre de bits d'un octet FM.</summary>
    public const int EncodedFmByteBitCount = 16;
    /// <summary>Longueur totale de l'en-tête.</summary>
    public const int HeaderBitCount = MarkBitCount + HeaderByteCount * EncodedFmByteBitCount;
    /// <summary>Taille sectorielle.</summary>
    public const int SectorSize = 256;
    /// <summary>Nombre d'octets du checksum des données.</summary>
    public const int DataChecksumByteCount = 1;
    /// <summary>Nombre d'octets du bloc de données.</summary>
    public const int DataBlockByteCount = SectorSize + DataChecksumByteCount;
    /// <summary>Face logique produite.</summary>
    public const byte LogicalHead = 0;
    /// <summary>Code de taille produit.</summary>
    public const byte SectorSizeCode = 1;
    /// <summary>Distance maximale de recherche de la marque suivante.</summary>
    public const int MaximumDataSearchDistanceBits = (88 + 16) * BitPrimitives.BitsPerByte;
    /// <summary>Longueur du remplissage suivant l'en-tête.</summary>
    public const int HeaderGapBitCount = 160;
    /// <summary>Longueur du remplissage suivant les données.</summary>
    public const int DataGapBitCount = 128;
    /// <summary>Poids d'un secteur dans la confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur de confiance.</summary>
    public const double ConfidenceDivisor = 20;
    /// <summary>Nom de l'attribut de volume.</summary>
    public const string VolumeAttributeName = "volume";
    /// <summary>Volume utilisé par défaut.</summary>
    public const byte DefaultVolume = 0;
    /// <summary>Marque FM commune aux en-têtes et aux données.</summary>
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly(TrackBitEncoding.EncodeFm(0, 0, 0, AddressMark));

    /// <summary>Crée l'exception signalant une taille sectorielle incompatible.</summary>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Heathkit sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}
