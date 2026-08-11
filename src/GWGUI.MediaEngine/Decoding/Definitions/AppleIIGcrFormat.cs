using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Apple IIGcr.</summary>
internal static class AppleIIGcrFormat
{
    /// <summary>Nom du format utilisé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "Apple II";
    /// <summary>Nom de la variante Apple II à treize secteurs.</summary>
    public const string ThirteenSectorVariant = "13-sector";
    /// <summary>Nom du contrôle de checksum des données.</summary>
    public const string ChecksumLabel = "checksum";
    /// <summary>Nom du contrôle de checksum des adresses.</summary>
    public const string AddressChecksumLabel = "address checksum";
    /// <summary>Nom du contrôle de checksum associé aux données.</summary>
    public const string DataChecksumLabel = "data checksum";
    /// <summary>Description utilisée lorsqu'un checksum ne peut pas être lu.</summary>
    public const string UnavailableChecksumVariant = "checksum unavailable";
    /// <summary>Description du prologue de données Apple II non apparié.</summary>
    public const string UnpairedDataVariant = "data prologue D5 AA AD";
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Apple II sectors contain {SectorSize} bytes; received {actualSize} bytes.");
    /// <summary>Définit prologue premier octet utilisé par ce format.</summary>
    public const byte PrologueFirstByte = 0xd5;
    /// <summary>Définit prologue second octet utilisé par ce format.</summary>
    public const byte PrologueSecondByte = 0xaa;
    /// <summary>Définit six and two adresse prologue last octet utilisé par ce format.</summary>
    public const byte SixAndTwoAddressPrologueLastByte = 0x96;
    /// <summary>Définit five and three adresse prologue last octet utilisé par ce format.</summary>
    public const byte FiveAndThreeAddressPrologueLastByte = 0xb5;
    /// <summary>Définit données prologue last octet utilisé par ce format.</summary>
    public const byte DataPrologueLastByte = 0xad;
    /// <summary>Définit six and two adresse prologue utilisé par ce format.</summary>
    public const uint SixAndTwoAddressPrologue = 0xd5aa96;
    /// <summary>Définit five and three adresse prologue utilisé par ce format.</summary>
    public const uint FiveAndThreeAddressPrologue = 0xd5aab5;
    /// <summary>Définit données prologue utilisé par ce format.</summary>
    public const uint DataPrologue = 0xd5aaad;
    /// <summary>Définit épilogue premier octet utilisé par ce format.</summary>
    public const byte EpilogueFirstByte = 0xde;
    /// <summary>Définit épilogue second octet utilisé par ce format.</summary>
    public const byte EpilogueSecondByte = 0xaa;
    /// <summary>Définit épilogue last octet utilisé par ce format.</summary>
    public const byte EpilogueLastByte = 0xeb;
    /// <summary>Définit synchronisation octet utilisé par ce format.</summary>
    public const byte SyncByte = 0xff;
    /// <summary>Définit four and four masque utilisé par ce format.</summary>
    public const byte FourAndFourMask = 0xaa;
    /// <summary>Définit synchronisation octet nombre utilisé par ce format.</summary>
    public const int SyncByteCount = 3;
    /// <summary>Définit prologue octet nombre utilisé par ce format.</summary>
    public const int PrologueByteCount = 3;
    /// <summary>Définit prologue bit nombre utilisé par ce format.</summary>
    public const int PrologueBitCount = PrologueByteCount * Primitives.BitPrimitives.BitsPerByte;
    /// <summary>Nombre de bits à avancer après une marque tout en laissant la boucle atteindre le bit suivant.</summary>
    public const int PrologueAdvanceBitCount = PrologueBitCount - 1;
    /// <summary>Définit adresse valeur nombre utilisé par ce format.</summary>
    public const int AddressValueCount = 4;
    /// <summary>Définit encodé octets per adresse valeur utilisé par ce format.</summary>
    public const int EncodedBytesPerAddressValue = 2;
    /// <summary>Définit encodé adresse octet nombre utilisé par ce format.</summary>
    public const int EncodedAddressByteCount = AddressValueCount * EncodedBytesPerAddressValue;
    /// <summary>Définit encodé adresse bit nombre utilisé par ce format.</summary>
    public const int EncodedAddressBitCount = EncodedAddressByteCount * Primitives.BitPrimitives.BitsPerByte;
    /// <summary>Définit adresse bloc bit nombre utilisé par ce format.</summary>
    public const int AddressBlockBitCount = PrologueBitCount + EncodedAddressBitCount;
    /// <summary>Définit données recherche bit nombre utilisé par ce format.</summary>
    public const int DataSearchBitCount = 1024;
    /// <summary>Définit circular tail bit nombre utilisé par ce format.</summary>
    public const int CircularTailBitCount = 4096;
    /// <summary>Définit secteur taille utilisé par ce format.</summary>
    public const int SectorSize = 256;
    /// <summary>Définit secteur taille code utilisé par ce format.</summary>
    public const byte SectorSizeCode = 1;
    /// <summary>Face logique portée par les secteurs Apple II décodés.</summary>
    public const byte LogicalHead = 0;
    /// <summary>Poids d'un secteur reconnu dans le calcul de confiance.</summary>
    public const int ConfidenceSectorWeight = 2;
    /// <summary>Diviseur propre au codec Apple II dans le calcul de confiance.</summary>
    public const double ConfidenceDivisor = 32;
    /// <summary>Définit six and two encodé octet nombre utilisé par ce format.</summary>
    public const int SixAndTwoEncodedByteCount = 343;
    /// <summary>Définit six and two décodé octet nombre utilisé par ce format.</summary>
    public const int SixAndTwoDecodedByteCount = 342;
    /// <summary>Définit six and two auxiliary octet nombre utilisé par ce format.</summary>
    public const int SixAndTwoAuxiliaryByteCount = 86;
    /// <summary>Définit six and two work buffer octet nombre utilisé par ce format.</summary>
    public const int SixAndTwoWorkBufferByteCount = 300;
    /// <summary>Définit five and three encodé octet nombre utilisé par ce format.</summary>
    public const int FiveAndThreeEncodedByteCount = 411;
    /// <summary>Définit five and three auxiliary octet nombre utilisé par ce format.</summary>
    public const int FiveAndThreeAuxiliaryByteCount = 154;
    /// <summary>Définit five and three chunk octet nombre utilisé par ce format.</summary>
    public const int FiveAndThreeChunkByteCount = 51;
    /// <summary>Définit five and three sectors per piste utilisé par ce format.</summary>
    public const int FiveAndThreeSectorsPerTrack = 13;
    /// <summary>Définit leading intervalle bit nombre utilisé par ce format.</summary>
    public const int LeadingGapBitCount = 100;
    /// <summary>Définit trailing intervalle bit nombre utilisé par ce format.</summary>
    public const int TrailingGapBitCount = 32;
    /// <summary>Définit par défaut volume utilisé par ce format.</summary>
    public const byte DefaultVolume = 254;
    /// <summary>Définit volume attribut name utilisé par ce format.</summary>
    public const string VolumeAttributeName = "volume";
    /// <summary>Définit sectors per piste attribut name utilisé par ce format.</summary>
    public const string SectorsPerTrackAttributeName = "sectorsPerTrack";
    /// <summary>Expose six and two table utilisé par ce format.</summary>
    public static IReadOnlyList<byte> SixAndTwoTable { get; } = Array.AsReadOnly<byte>(
    [
        0x96, 0x97, 0x9a, 0x9b, 0x9d, 0x9e, 0x9f, 0xa6, 0xa7, 0xab, 0xac, 0xad, 0xae, 0xaf, 0xb2, 0xb3,
        0xb4, 0xb5, 0xb6, 0xb7, 0xb9, 0xba, 0xbb, 0xbc, 0xbd, 0xbe, 0xbf, 0xcb, 0xcd, 0xce, 0xcf, 0xd3,
        0xd6, 0xd7, 0xd9, 0xda, 0xdb, 0xdc, 0xdd, 0xde, 0xdf, 0xe5, 0xe6, 0xe7, 0xe9, 0xea, 0xeb, 0xec,
        0xed, 0xee, 0xef, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf9, 0xfa, 0xfb, 0xfc, 0xfd, 0xfe, 0xff
    ]);
    /// <summary>Expose five and three table utilisé par ce format.</summary>
    public static IReadOnlyList<byte> FiveAndThreeTable { get; } = Array.AsReadOnly<byte>(
    [
        0xab, 0xad, 0xae, 0xaf, 0xb5, 0xb6, 0xb7, 0xba, 0xbb, 0xbd, 0xbe, 0xbf, 0xd6, 0xd7, 0xda, 0xdb,
        0xdd, 0xde, 0xdf, 0xea, 0xeb, 0xed, 0xee, 0xef, 0xf5, 0xf6, 0xf7, 0xfa, 0xfb, 0xfd, 0xfe, 0xff
    ]);
    /// <summary>Expose la table inverse 6-and-2 construite depuis la table commune.</summary>
    public static IReadOnlyDictionary<byte, byte> InverseSixAndTwoTable { get; } = new ReadOnlyDictionary<byte, byte>(SixAndTwoTable.Select((value, index) => (value, index)).ToDictionary(item => item.value, item => (byte)item.index));
    /// <summary>Expose la table inverse 5-and-3 construite depuis la table commune.</summary>
    public static IReadOnlyDictionary<byte, byte> InverseFiveAndThreeTable { get; } = new ReadOnlyDictionary<byte, byte>(FiveAndThreeTable.Select((value, index) => (value, index)).ToDictionary(item => item.value, item => (byte)item.index));
}
