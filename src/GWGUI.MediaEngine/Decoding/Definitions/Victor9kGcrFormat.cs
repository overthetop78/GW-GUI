using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Victor 9000 GCR.</summary>
internal static class Victor9kGcrFormat
{
    /// <summary>Identifiant technique du codec.</summary>
    public const string CodecId = FluxCodecIds.Victor9kGcr;
    /// <summary>Nom affiché du codec.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.Victor9kGcr;
    /// <summary>Nom employé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "Victor 9000";
    /// <summary>Motif hexadécimal de l'en-tête.</summary>
    public const string HeaderMarkHex = "5555555555551111";
    /// <summary>Motif hexadécimal des données.</summary>
    public const string DataMarkHex = "5555555555551104";
    /// <summary>Nombre d'octets d'une marque.</summary>
    public const int MarkByteCount = 8;
    /// <summary>Longueur d'une marque en bits.</summary>
    public const int MarkBitCount = MarkByteCount * BitPrimitives.BitsPerByte;
    /// <summary>Position de départ du flux GCR entrelacé.</summary>
    public const int EncodedDataStartBitOffset = 49;
    /// <summary>Pas entre deux cellules GCR utiles.</summary>
    public const int EncodedCellStride = 2;
    /// <summary>Nombre d'octets de l'en-tête.</summary>
    public const int HeaderByteCount = 6;
    /// <summary>Position du type fixe d'en-tête.</summary>
    public const int HeaderTypeOffset = 0;
    /// <summary>Position du cylindre.</summary>
    public const int HeaderCylinderOffset = 1;
    /// <summary>Position du secteur.</summary>
    public const int HeaderSectorOffset = 2;
    /// <summary>Position de la somme cylindre/secteur.</summary>
    public const int HeaderSumOffset = 3;
    /// <summary>Position du premier identifiant fixe.</summary>
    public const int HeaderId2Offset = 4;
    /// <summary>Position du second identifiant fixe.</summary>
    public const int HeaderId1Offset = 5;
    /// <summary>Type fixe d'en-tête.</summary>
    public const byte HeaderType = 0x06;
    /// <summary>Premier identifiant fixe.</summary>
    public const byte HeaderId2 = 0xa1;
    /// <summary>Second identifiant fixe.</summary>
    public const byte HeaderId1 = 0x1a;
    /// <summary>Préfixe des données.</summary>
    public const byte DataPrefix = 0x00;
    /// <summary>Position du préfixe de données.</summary>
    public const int DataPrefixOffset = 0;
    /// <summary>Position de la charge utile.</summary>
    public const int DataOffset = 1;
    /// <summary>Taille d'un secteur.</summary>
    public const int SectorByteCount = 512;
    /// <summary>Position de l'octet faible du checksum.</summary>
    public const int ChecksumLowOffset = DataOffset + SectorByteCount;
    /// <summary>Position de l'octet fort du checksum.</summary>
    public const int ChecksumHighOffset = ChecksumLowOffset + 1;
    /// <summary>Nombre total d'octets du bloc de données.</summary>
    public const int DecodedDataByteCount = ChecksumHighOffset + 1;
    /// <summary>Face logique.</summary>
    public const byte LogicalHead = 0;
    /// <summary>Code de taille.</summary>
    public const byte SectorSizeCode = 2;
    /// <summary>Distance maximale de recherche des données.</summary>
    public const int MaximumDataSearchDistanceBits = 98 * 16;
    /// <summary>Gap suivant l'en-tête.</summary>
    public const int HeaderGapBitCount = 20;
    /// <summary>Gap suivant les données.</summary>
    public const int DataGapBitCount = 64;
    /// <summary>Poids d'un secteur dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur du calcul de confiance.</summary>
    public const double ConfidenceDivisor = 24;
    /// <summary>Motif physique de l'en-tête.</summary>
    public static IReadOnlyList<byte> HeaderMark { get; } = Array.AsReadOnly(Convert.FromHexString(HeaderMarkHex));
    /// <summary>Motif physique des données.</summary>
    public static IReadOnlyList<byte> DataMark { get; } = Array.AsReadOnly(Convert.FromHexString(DataMarkHex));

    /// <summary>Crée l'exception signalant une taille de secteur invalide.</summary>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Victor 9000 sectors contain {SectorByteCount} bytes; received {actualSize} bytes.");
}

/// <summary>Calcule le checksum additif 16 bits Victor 9000.</summary>
internal static class Victor9kChecksum
{
    /// <summary>Calcule la somme des octets fournis.</summary>
    public static ushort Compute(IEnumerable<byte> data)
    {
        ushort checksum = 0;
        foreach (var value in data) checksum += value;
        return checksum;
    }
}

/// <summary>Représente un en-tête Victor 9000 décodé.</summary>
internal sealed record Victor9kHeader(byte Cylinder, byte Sector, byte[] Bytes, bool Valid);

/// <summary>Représente un bloc de données Victor 9000 décodé.</summary>
internal sealed record Victor9kData(byte Prefix, byte[] Payload, ushort StoredChecksum, bool ChecksumValid, int EndOffset);
