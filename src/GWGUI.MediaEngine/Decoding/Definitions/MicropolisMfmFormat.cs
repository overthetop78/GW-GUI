using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Micropolis MFM.</summary>
internal static class MicropolisMfmFormat
{
    /// <summary>Identifiant technique du codec.</summary>
    public const string CodecId = FluxCodecIds.MicropolisMfm;
    /// <summary>Nom affiché du codec.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.MicropolisMfm;
    /// <summary>Nom employé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "Micropolis";
    /// <summary>Marque d'adresse du record.</summary>
    public const byte AddressMark = 0xff;
    /// <summary>Nombre minimal d'octets nuls précédant la marque.</summary>
    public const int SyncZeroCount = 3;
    /// <summary>Nombre de bits encodant un octet MFM.</summary>
    public const int EncodedByteBitCount = 16;
    /// <summary>Longueur du motif minimal de synchronisation.</summary>
    public const int SyncBitCount = (SyncZeroCount + 1) * EncodedByteBitCount;
    /// <summary>Position de la marque dans le record.</summary>
    public const int AddressMarkOffset = 0;
    /// <summary>Position du cylindre dans le record.</summary>
    public const int CylinderOffset = 1;
    /// <summary>Position du secteur dans le record.</summary>
    public const int SectorOffset = 2;
    /// <summary>Nombre d'octets d'identité du record.</summary>
    public const int RecordIdentityByteCount = 3;
    /// <summary>Nombre d'octets réservés avant la charge utile.</summary>
    public const int HeaderPaddingByteCount = 10;
    /// <summary>Position de la charge utile.</summary>
    public const int DataOffset = RecordIdentityByteCount + HeaderPaddingByteCount;
    /// <summary>Taille de la charge utile.</summary>
    public const int SectorSize = 256;
    /// <summary>Position du checksum.</summary>
    public const int ChecksumOffset = DataOffset + SectorSize;
    /// <summary>Nombre d'octets terminaux.</summary>
    public const int TrailerPaddingByteCount = 5;
    /// <summary>Position du premier octet terminal.</summary>
    public const int TrailerOffset = ChecksumOffset + 1;
    /// <summary>Nombre total d'octets du record.</summary>
    public const int RecordByteCount = TrailerOffset + TrailerPaddingByteCount;
    /// <summary>Premier octet couvert par le checksum.</summary>
    public const int ChecksumDataOffset = CylinderOffset;
    /// <summary>Nombre d'octets couverts par le checksum.</summary>
    public const int ChecksumDataLength = ChecksumOffset - ChecksumDataOffset;
    /// <summary>Face logique des secteurs Micropolis.</summary>
    public const byte LogicalHead = 0;
    /// <summary>Code de taille des secteurs de 256 octets.</summary>
    public const byte SectorSizeCode = 1;
    /// <summary>Nombre d'octets nuls émis avant le record.</summary>
    public const int PreambleByteCount = 40;
    /// <summary>Gap ajouté après le record.</summary>
    public const int GapBitCount = 128;
    /// <summary>Modulo du checksum.</summary>
    public const int ChecksumModulus = 255;
    /// <summary>Poids d'un secteur dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur du calcul de confiance.</summary>
    public const double ConfidenceDivisor = 24;
    /// <summary>Motif minimal composé de trois zéros et de la marque.</summary>
    public static IReadOnlyList<byte> Sync { get; } = Array.AsReadOnly(TrackBitEncoding.EncodeCompactMfm(0, 0, 0, AddressMark));

    /// <summary>Crée l'exception signalant une taille de secteur invalide.</summary>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Micropolis sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}

/// <summary>Calcule le checksum modulo 255 du format Micropolis.</summary>
internal static class MicropolisChecksum
{
    /// <summary>Calcule le checksum de la séquence fournie.</summary>
    public static byte Compute(IEnumerable<byte> data)
    {
        var value = 0;
        foreach (var item in data)
        {
            if (value > MicropolisMfmFormat.ChecksumModulus) value -= MicropolisMfmFormat.ChecksumModulus;
            value += item;
        }
        return (byte)value;
    }
}

/// <summary>Représente les champs d'un record Micropolis complet.</summary>
internal sealed record MicropolisMfmRecord(byte Cylinder, byte Sector, byte[] Data, byte StoredChecksum, byte[] Trailer, byte[] Bytes)
{
    /// <summary>Indique si le checksum stocké correspond au contenu du record.</summary>
    public bool ChecksumValid => MicropolisChecksum.Compute(Bytes.Skip(MicropolisMfmFormat.ChecksumDataOffset).Take(MicropolisMfmFormat.ChecksumDataLength)) == StoredChecksum;

    /// <summary>Analyse les octets d'un record complet.</summary>
    public static MicropolisMfmRecord? Parse(byte[] bytes)
    {
        if (bytes.Length != MicropolisMfmFormat.RecordByteCount || bytes[MicropolisMfmFormat.AddressMarkOffset] != MicropolisMfmFormat.AddressMark) return null;
        return new(bytes[MicropolisMfmFormat.CylinderOffset], bytes[MicropolisMfmFormat.SectorOffset], bytes.AsSpan(MicropolisMfmFormat.DataOffset, MicropolisMfmFormat.SectorSize).ToArray(), bytes[MicropolisMfmFormat.ChecksumOffset], bytes.AsSpan(MicropolisMfmFormat.TrailerOffset, MicropolisMfmFormat.TrailerPaddingByteCount).ToArray(), bytes);
    }

    /// <summary>Crée les octets d'un record à encoder.</summary>
    public static MicropolisMfmRecord Create(byte cylinder, byte sector, IReadOnlyList<byte> data)
    {
        var bytes = new byte[MicropolisMfmFormat.RecordByteCount];
        bytes[MicropolisMfmFormat.AddressMarkOffset] = MicropolisMfmFormat.AddressMark;
        bytes[MicropolisMfmFormat.CylinderOffset] = cylinder;
        bytes[MicropolisMfmFormat.SectorOffset] = sector;
        data.ToArray().CopyTo(bytes, MicropolisMfmFormat.DataOffset);
        bytes[MicropolisMfmFormat.ChecksumOffset] = MicropolisChecksum.Compute(bytes.Skip(MicropolisMfmFormat.ChecksumDataOffset).Take(MicropolisMfmFormat.ChecksumDataLength));
        return Parse(bytes)!;
    }
}
