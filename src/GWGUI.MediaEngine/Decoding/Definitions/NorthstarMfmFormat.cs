using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format NorthStar MFM.</summary>
internal static class NorthstarMfmFormat
{
    /// <summary>Identifiant technique du codec.</summary>
    public const string CodecId = FluxCodecIds.NorthstarMfm;
    /// <summary>Nom affiché du codec.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.NorthstarMfm;
    /// <summary>Nom employé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "NorthStar";
    /// <summary>Marque d'adresse du secteur.</summary>
    public const byte AddressMark = 0xfb;
    /// <summary>Nombre d'octets nuls précédant la marque.</summary>
    public const int SyncZeroCount = 7;
    /// <summary>Nombre de bits encodant un octet MFM.</summary>
    public const int EncodedByteBitCount = 16;
    /// <summary>Longueur de la marque complète.</summary>
    public const int MarkBitCount = (SyncZeroCount + 1) * EncodedByteBitCount;
    /// <summary>Nombre d'octets d'identité.</summary>
    public const int IdentityByteCount = 1;
    /// <summary>Longueur encodée de l'identité.</summary>
    public const int IdentityBitCount = IdentityByteCount * EncodedByteBitCount;
    /// <summary>Taille d'un secteur.</summary>
    public const int SectorSize = 512;
    /// <summary>Longueur encodée de la charge utile.</summary>
    public const int PayloadBitCount = SectorSize * EncodedByteBitCount;
    /// <summary>Nombre d'octets du checksum.</summary>
    public const int ChecksumByteCount = 1;
    /// <summary>Longueur d'un bloc complet.</summary>
    public const int FullBlockBitCount = MarkBitCount + IdentityBitCount + PayloadBitCount + ChecksumByteCount * EncodedByteBitCount;
    /// <summary>Décalage du demi-octet de cylindre.</summary>
    public const int CylinderShift = 4;
    /// <summary>Masque du cylindre sur quatre bits.</summary>
    public const byte CylinderMask = 0x0f;
    /// <summary>Masque du secteur sur quatre bits.</summary>
    public const byte SectorMask = 0x0f;
    /// <summary>Face logique des secteurs NorthStar.</summary>
    public const byte LogicalHead = 0;
    /// <summary>Code de taille des secteurs de 512 octets.</summary>
    public const byte SectorSizeCode = 2;
    /// <summary>Gap ajouté après un secteur encodé.</summary>
    public const int GapBitCount = 128;
    /// <summary>Avancement du balayage après une marque.</summary>
    public const int ScanAdvance = MarkBitCount - 1;
    /// <summary>Poids d'un secteur dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur du calcul de confiance.</summary>
    public const double ConfidenceDivisor = 20;
    /// <summary>Marque composée de sept zéros suivis de <c>0xfb</c>.</summary>
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly(TrackBitEncoding.EncodeCompactMfm(0, 0, 0, 0, 0, 0, 0, AddressMark));

    /// <summary>Crée l'exception signalant une taille de secteur invalide.</summary>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"NorthStar sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}

/// <summary>Empaquette et dépaquette l'adresse NorthStar sur deux demi-octets.</summary>
internal static class NorthstarMfmAddress
{
    /// <summary>Empaquette le cylindre et le secteur.</summary>
    public static byte Pack(int cylinder, int sector) => (byte)((cylinder & NorthstarMfmFormat.CylinderMask) << NorthstarMfmFormat.CylinderShift | sector & NorthstarMfmFormat.SectorMask);

    /// <summary>Dépaquette le cylindre et le secteur.</summary>
    public static (byte Cylinder, byte Sector) Unpack(byte value) => ((byte)((value >> NorthstarMfmFormat.CylinderShift) & NorthstarMfmFormat.CylinderMask), (byte)(value & NorthstarMfmFormat.SectorMask));
}

/// <summary>Représente l'identité d'un secteur NorthStar.</summary>
internal sealed record NorthstarMfmIdentity(byte Cylinder, byte Sector, byte PackedValue);

/// <summary>Représente un bloc NorthStar complet.</summary>
internal sealed record NorthstarMfmBlock(NorthstarMfmIdentity Identity, byte[] Data, byte StoredChecksum, bool ChecksumValid);
