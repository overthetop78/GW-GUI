using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Apple Rwts18.</summary>
internal static class AppleRwts18Format
{
    /// <summary>Crée l'exception signalant invalide piste layout.</summary>
    /// <param name="actualSectorCount">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidTrackLayout(int actualSectorCount) => new($"RWTS18 tracks contain {SectorCount} sectors of {SectorByteCount} bytes; received {actualSectorCount} sectors.");
    /// <summary>Définit encodé adresse marque utilisé par ce format.</summary>
    public const ushort EncodedAddressMark = 0xd59d;
    /// <summary>Définit adresse marque bit nombre utilisé par ce format.</summary>
    public const int AddressMarkBitCount = 16;
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
    /// <summary>Définit charge utile with somme de contrôle symbol nombre utilisé par ce format.</summary>
    public const int PayloadWithChecksumSymbolCount = PayloadSymbolCount + 1;
    /// <summary>Définit données enregistrement octet nombre utilisé par ce format.</summary>
    public const int DataRecordByteCount = PayloadWithChecksumSymbolCount + 2;
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
    /// <summary>Expose la table de conversion des nibbles partagée avec le format Apple II.</summary>
    public static IReadOnlyList<byte> NibbleTable => AppleIIGcrFormat.SixAndTwoTable;
}
