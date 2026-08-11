namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Apple IWM GCR commun à Macintosh et Lisa FileWare.</summary>
internal static class AppleIwmGcrFormat
{
    public const string StructureDescriptionName = "Apple Macintosh";
    public const string ChecksumLabel = "checksum";
    public const string AddressChecksumLabel = "address checksum";
    public const string DataChecksumLabel = "data checksum";
    public const string UnavailableChecksumVariant = "checksum unavailable";
    public const string UnpairedDataVariant = "data prologue";
    public const int ConfidenceSectorWeight = 2;
    public const double ConfidenceDivisor = 24;
    /// <summary>Identifiant technique de la spécialisation Macintosh.</summary>
    public const string MacintoshCodecId = FluxCodecIds.AppleMacGcr;
    /// <summary>Nom affiché de la spécialisation Macintosh.</summary>
    public const string MacintoshCodecDisplayName = FluxCodecDisplayNames.AppleMacGcr;
    /// <summary>Identifiant technique de la spécialisation Lisa FileWare.</summary>
    public const string LisaCodecId = FluxCodecIds.AppleLisaFileWareGcr;
    /// <summary>Nom affiché de la spécialisation Lisa FileWare.</summary>
    public const string LisaCodecDisplayName = FluxCodecDisplayNames.AppleLisaFileWareGcr;
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"Apple Macintosh sectors contain {SectorByteCount} bytes; received {actualSize} bytes.");
    /// <summary>Définit adresse marque premier octet utilisé par ce format.</summary>
    public const byte AddressMarkFirstByte = 0xd5;
    /// <summary>Définit adresse marque second octet utilisé par ce format.</summary>
    public const byte AddressMarkSecondByte = 0xaa;
    /// <summary>Définit adresse marque last octet utilisé par ce format.</summary>
    public const byte AddressMarkLastByte = 0x96;
    /// <summary>Définit données marque last octet utilisé par ce format.</summary>
    public const byte DataMarkLastByte = 0xad;
    /// <summary>Définit épilogue premier octet utilisé par ce format.</summary>
    public const byte EpilogueFirstByte = 0xde;
    /// <summary>Définit épilogue second octet utilisé par ce format.</summary>
    public const byte EpilogueSecondByte = 0xaa;
    /// <summary>Définit synchronisation octet utilisé par ce format.</summary>
    public const byte SyncByte = 0xff;
    /// <summary>Définit marque octet nombre utilisé par ce format.</summary>
    public const int MarkByteCount = 3;
    /// <summary>Définit marque bit nombre utilisé par ce format.</summary>
    public const int MarkBitCount = MarkByteCount * Primitives.BitPrimitives.BitsPerByte;
    public const int MarkAdvanceBitCount = MarkBitCount - 1;
    /// <summary>Définit en-tête symbol nombre utilisé par ce format.</summary>
    public const int HeaderSymbolCount = 5;
    /// <summary>Définit en-tête valeur nombre utilisé par ce format.</summary>
    public const int HeaderValueCount = 4;
    /// <summary>Définit données symbol nombre utilisé par ce format.</summary>
    public const int DataSymbolCount = 704;
    /// <summary>Définit encodé charge utile symbol nombre utilisé par ce format.</summary>
    public const int EncodedPayloadSymbolCount = 699;
    /// <summary>Position du symbole identifiant le secteur dans le bloc de données.</summary>
    public const int DataSectorSymbolOffset = 0;
    /// <summary>Position du premier symbole de checksum dans le bloc de données.</summary>
    public const int ChecksumSymbolOffset = EncodedPayloadSymbolCount + 1;
    /// <summary>Position du symbole contenant les bits hauts des trois checksums.</summary>
    public const int PackedChecksumSymbolOffset = ChecksumSymbolOffset;
    /// <summary>Position du symbole de checksum associé au troisième accumulateur.</summary>
    public const int ThirdChecksumSymbolOffset = ChecksumSymbolOffset + 1;
    /// <summary>Position du symbole de checksum associé au deuxième accumulateur.</summary>
    public const int SecondChecksumSymbolOffset = ChecksumSymbolOffset + 2;
    /// <summary>Position du symbole de checksum associé au premier accumulateur.</summary>
    public const int FirstChecksumSymbolOffset = ChecksumSymbolOffset + 3;
    /// <summary>Définit somme de contrôle symbol nombre utilisé par ce format.</summary>
    public const int ChecksumSymbolCount = 4;
    /// <summary>Définit tag octet nombre utilisé par ce format.</summary>
    public const int TagByteCount = 12;
    /// <summary>Définit secteur octet nombre utilisé par ce format.</summary>
    public const int SectorByteCount = 512;
    /// <summary>Définit tagged secteur octet nombre utilisé par ce format.</summary>
    public const int TaggedSectorByteCount = TagByteCount + SectorByteCount;
    /// <summary>Définit secteur taille code utilisé par ce format.</summary>
    public const byte SectorSizeCode = 2;
    /// <summary>Définit par défaut format utilisé par ce format.</summary>
    public const byte DefaultFormat = 0x12;
    /// <summary>Définit cylindre haut bit masque utilisé par ce format.</summary>
    public const int CylinderHighBitMask = 0x03;
    /// <summary>Définit cylindre haut bit décalage utilisé par ce format.</summary>
    public const int CylinderHighBitShift = 6;
    /// <summary>Définit face bit décalage utilisé par ce format.</summary>
    public const int HeadBitShift = 5;
    /// <summary>Définit face bit masque utilisé par ce format.</summary>
    public const int HeadBitMask = 0x01;
    /// <summary>Définit group octet nombre utilisé par ce format.</summary>
    public const int GroupByteCount = 175;
    /// <summary>Définit last group index utilisé par ce format.</summary>
    public const int LastGroupIndex = GroupByteCount - 1;
    /// <summary>Définit six bit masque utilisé par ce format.</summary>
    public const byte SixBitMask = 0x3f;
    /// <summary>Définit somme de contrôle octet masque utilisé par ce format.</summary>
    public const uint ChecksumByteMask = 0xff;
    /// <summary>Définit somme de contrôle carry bit utilisé par ce format.</summary>
    public const uint ChecksumCarryBit = 0x100;
    /// <summary>Définit somme de contrôle haut bits masque utilisé par ce format.</summary>
    public const uint ChecksumHighBitsMask = 0xc0;
    /// <summary>Définit encodé haut bits masque utilisé par ce format.</summary>
    public const int EncodedHighBitsMask = 0xc0;
    /// <summary>Définit premier somme de contrôle décalage utilisé par ce format.</summary>
    public const int FirstChecksumShift = 6;
    /// <summary>Définit second somme de contrôle décalage utilisé par ce format.</summary>
    public const int SecondChecksumShift = 4;
    /// <summary>Définit third somme de contrôle décalage utilisé par ce format.</summary>
    public const int ThirdChecksumShift = 2;
    /// <summary>Définit premier packed somme de contrôle masque utilisé par ce format.</summary>
    public const int FirstPackedChecksumMask = 0x30;
    /// <summary>Définit second packed somme de contrôle masque utilisé par ce format.</summary>
    public const int SecondPackedChecksumMask = 0x0c;
    /// <summary>Définit third packed somme de contrôle masque utilisé par ce format.</summary>
    public const int ThirdPackedChecksumMask = 0x03;
    /// <summary>Définit données recherche bit nombre utilisé par ce format.</summary>
    public const int DataSearchBitCount = 512;
    /// <summary>Définit circular tail bit nombre utilisé par ce format.</summary>
    public const int CircularTailBitCount = 8192;
    /// <summary>Définit adresse leading intervalle bit nombre utilisé par ce format.</summary>
    public const int AddressLeadingGapBitCount = 100;
    /// <summary>Définit adresse trailing intervalle bit nombre utilisé par ce format.</summary>
    public const int AddressTrailingGapBitCount = 32;
    /// <summary>Définit données trailing intervalle bit nombre utilisé par ce format.</summary>
    public const int DataTrailingGapBitCount = 64;
    /// <summary>Définit format attribut name utilisé par ce format.</summary>
    public const string FormatAttributeName = "format";
    /// <summary>Définit tag attribut préfixe utilisé par ce format.</summary>
    public const string TagAttributePrefix = "tag";
    /// <summary>Expose la table de conversion GCR 6-et-2 partagée avec le format Apple II.</summary>
    public static IReadOnlyList<byte> SixAndTwoTable => AppleIIGcrFormat.SixAndTwoTable;
    /// <summary>Expose adresse marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> AddressMark { get; } = Array.AsReadOnly<byte>([AddressMarkFirstByte, AddressMarkSecondByte, AddressMarkLastByte]);
    /// <summary>Expose données marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> DataMark { get; } = Array.AsReadOnly<byte>([AddressMarkFirstByte, AddressMarkSecondByte, DataMarkLastByte]);
}
