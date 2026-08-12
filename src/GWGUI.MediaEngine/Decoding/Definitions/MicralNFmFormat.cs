using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Micral N FM.</summary>
internal static class MicralNFmFormat
{
    /// <summary>Identifiant technique du codec.</summary>
    public const string CodecId = FluxCodecIds.MicralNFm;
    /// <summary>Nom affiché du codec.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.MicralNFm;
    /// <summary>Nom employé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "Micral N";
    /// <summary>Nombre d'octets nuls précédant la marque.</summary>
    public const int SyncZeroCount = 3;
    /// <summary>Marque d'adresse du bloc.</summary>
    public const byte AddressMark = 0xff;
    /// <summary>Nombre de bits encodant un octet FM.</summary>
    public const int EncodedByteBitCount = 16;
    /// <summary>Longueur de la marque FM en bits.</summary>
    public const int MarkBitCount = (SyncZeroCount + 1) * EncodedByteBitCount;
    /// <summary>Position du numéro de secteur après la marque.</summary>
    public const int SectorOffset = 0;
    /// <summary>Position du cylindre après la marque.</summary>
    public const int CylinderOffset = 1;
    /// <summary>Position de la charge utile après la marque.</summary>
    public const int DataOffset = 2;
    /// <summary>Nombre d'octets d'identité.</summary>
    public const int IdentityByteCount = 2;
    /// <summary>Taille d'un secteur.</summary>
    public const int SectorSize = 128;
    /// <summary>Nombre d'octets du checksum.</summary>
    public const int ChecksumByteCount = 1;
    /// <summary>Position du checksum après la marque.</summary>
    public const int ChecksumOffset = DataOffset + SectorSize;
    /// <summary>Nombre d'octets lus après la marque.</summary>
    public const int BytesAfterMark = IdentityByteCount + SectorSize + ChecksumByteCount;
    /// <summary>Nombre total d'octets du bloc, synchronisation comprise.</summary>
    public const int BlockByteCount = SyncZeroCount + 1 + BytesAfterMark;
    /// <summary>Longueur totale du bloc en bits.</summary>
    public const int BlockBitCount = BlockByteCount * EncodedByteBitCount;
    /// <summary>Face logique des secteurs Micral N.</summary>
    public const byte LogicalHead = 0;
    /// <summary>Code de taille des secteurs de 128 octets.</summary>
    public const byte SectorSizeCode = 0;
    /// <summary>Masque de complément du checksum.</summary>
    public const byte ComplementMask = 0xff;
    /// <summary>Masque du bit de retenue.</summary>
    public const byte CarryMask = 0x80;
    /// <summary>Gap ajouté après chaque secteur encodé.</summary>
    public const int GapBitCount = 128;
    /// <summary>Avancement du balayage après une marque.</summary>
    public const int ScanAdvance = MarkBitCount - 1;
    /// <summary>Poids d'un secteur dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur du calcul de confiance.</summary>
    public const double ConfidenceDivisor = 20;
    /// <summary>Marque FM composée de trois zéros et de <c>0xff</c>.</summary>
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly(TrackBitEncoding.EncodeFm(0, 0, 0, AddressMark));

    /// <summary>Crée l'exception signalant une taille de secteur invalide.</summary>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Micral N sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}

/// <summary>Calcule le checksum propre au format Micral N.</summary>
internal static class MicralNChecksum
{
    /// <summary>Met à jour le checksum avec un octet supplémentaire.</summary>
    public static byte Update(byte checksum, byte data)
    {
        var carrySource = ((data ^ checksum) ^ MicralNFmFormat.ComplementMask) & ((data + checksum) ^ data);
        var carry = (carrySource & MicralNFmFormat.CarryMask) != 0 ? 1 : 0;
        return (byte)(checksum + data + carry);
    }

    /// <summary>Calcule le checksum de la séquence fournie.</summary>
    public static byte Compute(IEnumerable<byte> data)
    {
        byte checksum = 0;
        foreach (var value in data) checksum = Update(checksum, value);
        return checksum;
    }
}

/// <summary>Représente un bloc Micral N décodé.</summary>
internal sealed record MicralNFmBlock(byte Sector, byte Cylinder, byte[] Data, byte StoredChecksum, bool ChecksumValid);
