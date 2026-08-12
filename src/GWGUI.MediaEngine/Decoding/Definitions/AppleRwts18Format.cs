using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Apple Rwts18.</summary>
internal static class AppleRwts18Format
{
    /// <summary>Identifiant technique du codec RWTS18.</summary>
    public const string CodecId = FluxCodecIds.AppleRwts18;
    /// <summary>Nom affiché du codec RWTS18.</summary>
    public const string CodecDisplayName = FluxCodecDisplayNames.AppleRwts18;
    /// <summary>Nom utilisé dans les descriptions de structures.</summary>
    public const string StructureDescriptionName = "Apple II RWTS18";
    /// <summary>Nom du contrôle de checksum des adresses.</summary>
    public const string AddressChecksumLabel = "address checksum";
    /// <summary>Nom du contrôle de checksum des données.</summary>
    public const string DataChecksumLabel = "checksum";
    /// <summary>Face logique portée par les secteurs RWTS18.</summary>
    public const byte LogicalHead = 0;
    /// <summary>Crée l'exception signalant invalide piste layout.</summary>
    /// <param name="actualSectorCount">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidTrackLayout(int actualSectorCount) => new($"RWTS18 tracks contain {SectorCount} sectors of {SectorByteCount} bytes; received {actualSectorCount} sectors.");
    /// <summary>Crée l'erreur signalant une taille sectorielle RWTS18 invalide.</summary>
    public static ArgumentException InvalidSectorSize(int sector, int actualSize) => new($"RWTS18 sector {sector} contains {actualSize} bytes; expected {SectorByteCount} bytes.");
    /// <summary>Définit encodé adresse marque utilisé par ce format.</summary>
    public const ushort EncodedAddressMark = 0xd59d;
    /// <summary>Définit adresse marque bit nombre utilisé par ce format.</summary>
    public const int AddressMarkBitCount = 16;
    /// <summary>Nombre de bits à avancer après une marque tout en laissant la boucle atteindre le bit suivant.</summary>
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
    /// <summary>Nombre de pages composant un secteur physique.</summary>
    public const int PageCountPerSector = 3;
    /// <summary>Indice de la deuxième page.</summary>
    public const int SecondPageIndex = 1;
    /// <summary>Indice de la troisième page.</summary>
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
    /// <summary>Nombre de symboles décrivant les trois octets situés à la même position dans les pages.</summary>
    public const int SymbolsPerPageGroup = 4;
    /// <summary>Position de l'identifiant modifiable dans l'enregistrement.</summary>
    public const int IdentifierOffset = 0;
    /// <summary>Position du premier symbole de données dans l'enregistrement.</summary>
    public const int PayloadOffset = IdentifierOffset + 1;
    /// <summary>Position du symbole de checksum dans les valeurs décodées.</summary>
    public const int PayloadChecksumOffset = PayloadSymbolCount;
    /// <summary>Définit charge utile with somme de contrôle symbol nombre utilisé par ce format.</summary>
    public const int PayloadWithChecksumSymbolCount = PayloadSymbolCount + 1;
    /// <summary>Définit données enregistrement octet nombre utilisé par ce format.</summary>
    public const int DataRecordByteCount = PayloadWithChecksumSymbolCount + 2;
    /// <summary>Position de l'épilogue dans l'enregistrement complet.</summary>
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
    /// <summary>Plus grand cylindre représentable dans une adresse RWTS18.</summary>
    public const int MaximumCylinder = SixBitMask;
    /// <summary>Plus grande valeur d'identification représentable sur un octet.</summary>
    public const int MaximumIdentifier = byte.MaxValue;
    /// <summary>Masque les deux bits hauts reconstitués d'un octet de page.</summary>
    public const byte HighBitMask = 0xc0;
    /// <summary>Décalage replaçant les deux bits hauts de la première page.</summary>
    public const int FirstPageHighBitShift = 2;
    /// <summary>Décalage replaçant les deux bits hauts de la deuxième page.</summary>
    public const int SecondPageHighBitShift = 4;
    /// <summary>Décalage replaçant les deux bits hauts de la troisième page.</summary>
    public const int ThirdPageHighBitShift = 6;
    /// <summary>Décalage extrayant les deux bits hauts d'un octet source.</summary>
    public const int SourceHighBitShift = 6;
    /// <summary>Décalage des bits hauts de la première page dans le symbole groupé.</summary>
    public const int FirstPagePackedShift = 4;
    /// <summary>Décalage des bits hauts de la deuxième page dans le symbole groupé.</summary>
    public const int SecondPagePackedShift = 2;
    /// <summary>Diviseur du nombre de secteurs valides dans le calcul de confiance.</summary>
    public const int ConfidenceCompleteSectorDivisor = SectorCount;
    /// <summary>Diviseur du nombre de secteurs détectés dans le calcul de confiance.</summary>
    public const double ConfidenceDetectedSectorDivisor = 24;
    /// <summary>Ordre physique décroissant des six secteurs sur une piste RWTS18.</summary>
    public static IReadOnlyList<int> EncodingSectorOrder { get; } = Array.AsReadOnly(Enumerable.Range(0, SectorCount).Reverse().ToArray());

    /// <summary>Ordonne les secteurs selon leur position physique RWTS18.</summary>
    public static IEnumerable<TrackSector> OrderForEncoding(IEnumerable<TrackSector> sectors) => EncodingSectorOrder.Select(number => sectors.Single(sector => sector.Number == number));
}
