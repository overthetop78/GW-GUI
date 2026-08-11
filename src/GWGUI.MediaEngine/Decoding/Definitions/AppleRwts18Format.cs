namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Apple Rwts18.</summary>
internal static class AppleRwts18Format
{
    public const string CodecId = FluxCodecIds.AppleRwts18;
    public const string CodecDisplayName = FluxCodecDisplayNames.AppleRwts18;
    public const string StructureDescriptionName = "Apple II RWTS18";
    /// <summary>Crée l'exception signalant invalide piste layout.</summary>
    /// <param name="actualSectorCount">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidTrackLayout(int actualSectorCount) => new($"RWTS18 tracks contain {SectorCount} sectors of {SectorByteCount} bytes; received {actualSectorCount} sectors.");
    /// <summary>Définit encodé adresse marque utilisé par ce format.</summary>
    public const ushort EncodedAddressMark = 0xd59d;
    /// <summary>Définit adresse marque bit nombre utilisé par ce format.</summary>
    public const int AddressMarkBitCount = 16;
    public const int AddressMarkAdvanceBitCount = AddressMarkBitCount - 1;
    /// <summary>Définit adresse octet nombre utilisé par ce format.</summary>
    public const int AddressByteCount = 4;
    /// <summary>Définit adresse fin utilisé par ce format.</summary>
    public const byte AddressTrailer = 0xaa;
    /// <summary>Définit données épilogue utilisé par ce format.</summary>
    public const byte DataEpilogue = 0xd4;
    /// <summary>Définit synchronisation octet utilisé par ce format.</summary>
    public const byte SyncByte = 0xff;
    /// <summary>Définit secteur nombre utilisé par ce format.</summary>
    public const int SectorCount = 6;
    public const int PageCountPerSector = 3;
    public const int SecondPageIndex = 1;
    public const int ThirdPageIndex = 2;
    /// <summary>Définit last secteur number utilisé par ce format.</summary>
    public const int LastSectorNumber = SectorCount - 1;
    /// <summary>Définit secteur octet nombre utilisé par ce format.</summary>
    public const int SectorByteCount = 768;
    /// <summary>Définit secteur taille code utilisé par ce format.</summary>
    public const byte SectorSizeCode = 3;
    /// <summary>Définit page octet nombre utilisé par ce format.</summary>
    public const int PageByteCount = 256;
    /// <summary>Définit charge utile symbol nombre utilisé par ce format.</summary>
    public const int PayloadSymbolCount = 1024;
    public const int SymbolsPerPageGroup = 4;
    public const int IdentifierOffset = 0;
    public const int PayloadOffset = IdentifierOffset + 1;
    public const int PayloadChecksumOffset = PayloadSymbolCount;
    /// <summary>Définit charge utile with somme de contrôle symbol nombre utilisé par ce format.</summary>
    public const int PayloadWithChecksumSymbolCount = PayloadSymbolCount + 1;
    /// <summary>Définit données enregistrement octet nombre utilisé par ce format.</summary>
    public const int DataRecordByteCount = PayloadWithChecksumSymbolCount + 2;
    public const int DataEpilogueOffset = DataRecordByteCount - 1;
    /// <summary>Définit données read window octet nombre utilisé par ce format.</summary>
    public const int DataReadWindowByteCount = 1100;
    /// <summary>Définit circular tail bit nombre utilisé par ce format.</summary>
    public const int CircularTailBitCount = 16_384;
    /// <summary>Définit premier secteur intervalle bit nombre utilisé par ce format.</summary>
    public const int FirstSectorGapBitCount = 200;
    /// <summary>Définit other secteur intervalle bit nombre utilisé par ce format.</summary>
    public const int OtherSectorGapBitCount = 4;
    /// <summary>Définit identifier attribut name utilisé par ce format.</summary>
    public const string IdentifierAttributeName = "id";
    /// <summary>Définit par défaut identifier utilisé par ce format.</summary>
    public const byte DefaultIdentifier = 0xa4;
    /// <summary>Définit six bit masque utilisé par ce format.</summary>
    public const byte SixBitMask = 0x3f;
    public const byte HighBitMask = 0xc0;
    public const int FirstPageHighBitShift = 2;
    public const int SecondPageHighBitShift = 4;
    public const int ThirdPageHighBitShift = 6;
    public const int SourceHighBitShift = 6;
    public const int FirstPagePackedShift = 4;
    public const int SecondPagePackedShift = 2;
    public const int ConfidenceCompleteSectorDivisor = SectorCount;
    public const double ConfidenceDetectedSectorDivisor = 24;
}
