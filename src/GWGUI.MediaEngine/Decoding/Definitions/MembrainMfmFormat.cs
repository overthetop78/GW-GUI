using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Membrain MFM.</summary>
internal static class MembrainMfmFormat
{
    /// <summary>Identifiant technique du codec.</summary>
    public const string CodecId = FluxCodecIds.MembrainMfm;
    /// <summary>Nom affiché du codec.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.MembrainMfm;
    /// <summary>Nom employé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "Membrain";
    /// <summary>Mot encodé de synchronisation A1.</summary>
    public const ushort EncodedSyncByte = 0x4489;
    /// <summary>Octet de synchronisation décodé.</summary>
    public const byte SyncByte = 0xa1;
    /// <summary>Marque d'adresse d'en-tête.</summary>
    public const byte HeaderAddressMark = 0xfe;
    /// <summary>Première marque de données acceptée.</summary>
    public const byte FirstDataAddressMark = 0xf8;
    /// <summary>Marque de données émise par l'encodeur.</summary>
    public const byte DataAddressMark = FirstDataAddressMark;
    /// <summary>Dernière marque de données acceptée.</summary>
    public const byte LastDataAddressMark = 0xfb;
    /// <summary>Nombre de bits encodant un octet MFM.</summary>
    public const int EncodedByteBitCount = 16;
    /// <summary>Longueur du motif d'en-tête.</summary>
    public const int HeaderPatternBitCount = 32;
    /// <summary>Longueur du motif de données.</summary>
    public const int DataPatternBitCount = 32;
    /// <summary>Nombre d'octets décodés de l'en-tête, CRC compris.</summary>
    public const int HeaderByteCount = 6;
    /// <summary>Longueur encodée de l'en-tête.</summary>
    public const int HeaderBitCount = HeaderByteCount * EncodedByteBitCount;
    /// <summary>Position de la marque dans l'en-tête décodé.</summary>
    public const int HeaderMarkOffset = 1;
    /// <summary>Position des bits hauts du cylindre.</summary>
    public const int HeaderCylinderHighOffset = 2;
    /// <summary>Position de l'adresse compactée.</summary>
    public const int HeaderPackedAddressOffset = 3;
    /// <summary>Nombre d'octets précédant la charge utile.</summary>
    public const int DataPrefixByteCount = 2;
    /// <summary>Nombre d'octets du CRC.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Taille d'un secteur Membrain.</summary>
    public const int SectorSize = 512;
    /// <summary>Code correspondant à un secteur de 512 octets.</summary>
    public const byte SectorSizeCode = 2;
    /// <summary>Longueur totale du bloc de données décodé.</summary>
    public const int DataBlockByteCount = DataPrefixByteCount + SectorSize + CrcByteCount;
    /// <summary>Nombre de bits bas du cylindre placés dans l'adresse compactée.</summary>
    public const int CylinderLowBitCount = 3;
    /// <summary>Décalage des bits bas du cylindre.</summary>
    public const int CylinderLowShift = 5;
    /// <summary>Décalage du bit de face.</summary>
    public const int HeadShift = 4;
    /// <summary>Masque des bits hauts du cylindre.</summary>
    public const byte CylinderHighMask = 0x1f;
    /// <summary>Masque de la valeur des bits bas du cylindre.</summary>
    public const byte CylinderLowValueMask = 0x07;
    /// <summary>Masque des bits bas du cylindre compactés.</summary>
    public const byte CylinderLowMask = 0xe0;
    /// <summary>Masque du bit de face.</summary>
    public const byte HeadMask = 1;
    /// <summary>Masque du numéro de secteur.</summary>
    public const byte SectorMask = 0x0f;
    /// <summary>Plus grand cylindre représentable par les cinq bits hauts et trois bits bas.</summary>
    public const int MaximumCylinder = (CylinderHighMask << CylinderLowBitCount) | CylinderLowValueMask;
    /// <summary>Plus grande face représentable dans le bit réservé.</summary>
    public const int MaximumHead = HeadMask;
    /// <summary>Plus grand secteur représentable sur quatre bits.</summary>
    public const int MaximumSector = SectorMask;
    /// <summary>Décalage initial de la recherche de données.</summary>
    public const int DataSearchInitialBitOffset = 1;
    /// <summary>Longueur maximale de la recherche de données.</summary>
    public const int DataSearchBitCount = 104 * BitPrimitives.BitsPerByte;
    /// <summary>Gap suivant l'en-tête.</summary>
    public const int HeaderGapBitCount = 64;
    /// <summary>Gap suivant les données.</summary>
    public const int DataGapBitCount = 128;
    /// <summary>Polynôme du CRC Membrain.</summary>
    public const ushort CrcPolynomial = Crc16Calculator.IbmPolynomial;
    /// <summary>Valeur initiale du CRC Membrain.</summary>
    public const ushort CrcInitialValue = Crc16Calculator.ZeroInitialValue;
    /// <summary>Poids d'un secteur dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur du calcul de confiance.</summary>
    public const double ConfidenceDivisor = 20;
    /// <summary>Motif binaire marquant un en-tête.</summary>
    public static IReadOnlyList<byte> HeaderPattern { get; } = Array.AsReadOnly<byte>([0x44, 0x89, 0x55, 0x54]);
    /// <summary>Motif binaire marquant les données émises par l'encodeur.</summary>
    public static IReadOnlyList<byte> DataPattern { get; } = Array.AsReadOnly<byte>([0x44, 0x89, 0x55, 0x4a]);

    /// <summary>Indique si l'octet est une marque de données Membrain.</summary>
    public static bool IsDataAddressMark(byte value) => value is >= FirstDataAddressMark and <= LastDataAddressMark;

    /// <summary>Crée l'exception signalant une taille de secteur invalide.</summary>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Membrain sectors contain {SectorSize} bytes; received {actualSize} bytes.");
}

/// <summary>Regroupe les opérations d'empaquetage de l'adresse Membrain.</summary>
internal static class MembrainMfmAddress
{
    /// <summary>Empaquette le cylindre, la face et le secteur dans les deux octets Membrain.</summary>
    public static (byte CylinderHigh, byte PackedAddress) Pack(int cylinder, int head, int sector) => ((byte)(cylinder >> MembrainMfmFormat.CylinderLowBitCount), (byte)(((cylinder & MembrainMfmFormat.CylinderLowValueMask) << MembrainMfmFormat.CylinderLowShift) | (head << MembrainMfmFormat.HeadShift) | (sector & MembrainMfmFormat.SectorMask)));

    /// <summary>Dépaquette les deux octets d'adresse Membrain.</summary>
    public static (byte Cylinder, byte Head, byte Sector) Unpack(byte cylinderHigh, byte packedAddress) => ((byte)(((cylinderHigh & MembrainMfmFormat.CylinderHighMask) << MembrainMfmFormat.CylinderLowBitCount) | ((packedAddress & MembrainMfmFormat.CylinderLowMask) >> MembrainMfmFormat.CylinderLowShift)), (byte)((packedAddress >> MembrainMfmFormat.HeadShift) & MembrainMfmFormat.HeadMask), (byte)(packedAddress & MembrainMfmFormat.SectorMask));
}

/// <summary>Représente un en-tête Membrain décodé.</summary>
internal sealed record MembrainMfmHeader(int Offset, byte Cylinder, byte Head, byte Sector, bool CrcValid, byte[] Bytes);

/// <summary>Représente un bloc de données Membrain décodé.</summary>
internal sealed record MembrainMfmData(int Offset, byte Mark, byte[] Payload, byte[] Bytes, bool CrcValid);
