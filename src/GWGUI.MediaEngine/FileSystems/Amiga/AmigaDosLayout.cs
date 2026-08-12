namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Définit la disposition binaire et les limites d'AmigaDOS.</summary>
public static class AmigaDosLayout
{
    /// <summary>Taille d'un bloc logique.</summary>
    public const int BlockSize = 512;
    /// <summary>Largeur d'un mot ou pointeur AmigaDOS.</summary>
    public const int WordSize = sizeof(uint);
    /// <summary>Numéro du bloc de boot.</summary>
    public const int BootBlock = 0;
    /// <summary>Longueur de la signature DOS.</summary>
    public const int DosSignatureLength = 3;
    /// <summary>Offset de la variante après la signature DOS.</summary>
    public const int DosVariantOffset = DosSignatureLength;
    /// <summary>Nombre d'entrées de la table de hachage racine.</summary>
    public const int RootHashTableEntryCount = 72;
    /// <summary>Offset du type primaire.</summary>
    public const int PrimaryTypeOffset = 0;
    /// <summary>Offset de la taille de la table de hachage.</summary>
    public const int HashTableSizeOffset = 12;
    /// <summary>Offset du premier champ réservé suivant la table de hachage.</summary>
    public const int FirstReservedOffset = 16;
    /// <summary>Offset du premier pointeur de données.</summary>
    public const int DataPointersOffset = 24;
    /// <summary>Offset du premier pointeur de bitmap.</summary>
    public const int BitmapPointersOffset = 316;
    /// <summary>Offset du drapeau de validité du bitmap.</summary>
    public const int BitmapValidityOffset = 312;
    /// <summary>Offset des bits de protection.</summary>
    public const int ProtectionOffset = 320;
    /// <summary>Offset de la taille du fichier.</summary>
    public const int FileSizeOffset = 324;
    /// <summary>Offset du nom long et du commentaire.</summary>
    public const int LongNameOffset = 328;
    /// <summary>Offset de la date.</summary>
    public const int DateOffset = 420;
    /// <summary>Offset de la date de dernière modification du volume.</summary>
    public const int VolumeModifiedDateOffset = 472;
    /// <summary>Offset du nom ordinaire.</summary>
    public const int OrdinaryNameOffset = 432;
    /// <summary>Offset du pointeur de chaîne de hachage.</summary>
    public const int HashChainOffset = 496;
    /// <summary>Offset du pointeur de bloc d'extension.</summary>
    public const int ExtensionBlockOffset = 504;
    /// <summary>Offset du type secondaire.</summary>
    public const int SecondaryTypeOffset = 508;
    /// <summary>Longueur maximale d'un nom ordinaire.</summary>
    public const int OrdinaryNameMaximumLength = 30;
    /// <summary>Longueur maximale d'un nom long.</summary>
    public const int LongNameMaximumLength = 107;
    /// <summary>Longueur maximale du commentaire d'une entrée.</summary>
    public const int CommentMaximumLength = 79;
    /// <summary>Offset du pointeur racine dans le bloc de boot.</summary>
    public const int BootRootPointerOffset = 8;
    /// <summary>Offset de l'indice de séquence d'un en-tête de fichier.</summary>
    public const int HighSequenceOffset = 8;
    /// <summary>Profondeur maximale des répertoires.</summary>
    public const int MaximumDirectoryDepth = 64;
    /// <summary>Nombre de minutes par jour.</summary>
    public const int MinutesPerDay = 24 * 60;
    /// <summary>Nombre de ticks AmigaDOS par seconde.</summary>
    public const int TicksPerSecond = 50;
    /// <summary>Durée d'un tick AmigaDOS en millisecondes.</summary>
    public const double TickDurationMilliseconds = 20d;
    /// <summary>Nombre maximal de pointeurs de bitmap racine.</summary>
    public const int MaximumBitmapPointerCount = 25;
    /// <summary>Offset des mots de bitmap après leur checksum.</summary>
    public const int BitmapDataOffset = WordSize;
    /// <summary>Somme attendue d'un bloc dont le checksum est valide.</summary>
    public const uint ValidChecksumSum = 0;
    /// <summary>Taille de l'en-tête d'un bloc de données OFS.</summary>
    public const int OfsDataHeaderLength = 24;
    /// <summary>Longueur maximale des données d'un bloc OFS.</summary>
    public const int OfsDataMaximumLength = 488;
    /// <summary>Type primaire d'un bloc d'en-tête.</summary>
    public const int HeaderPrimaryType = 2;
    /// <summary>Type primaire d'un bloc d'extension de fichier.</summary>
    public const int FileExtensionPrimaryType = 16;
    /// <summary>Type primaire d'un bloc de données OFS.</summary>
    public const int OfsDataPrimaryType = 8;
    /// <summary>Type secondaire d'un bloc racine.</summary>
    public const int RootSecondaryType = 1;
    /// <summary>Type secondaire d'un répertoire.</summary>
    public const int DirectorySecondaryType = 2;
    /// <summary>Type secondaire d'un fichier.</summary>
    public const int FileSecondaryType = -3;
    /// <summary>Type secondaire d'un lien de fichier.</summary>
    public const int FileLinkSecondaryType = -4;
    /// <summary>Type secondaire d'un lien de répertoire.</summary>
    public const int DirectoryLinkSecondaryType = 4;
    /// <summary>Type secondaire d'un lien dur.</summary>
    public const int HardLinkSecondaryType = 3;
    /// <summary>Premier octet de la signature DOS.</summary>
    public const byte DosSignatureD = (byte)'D';
    /// <summary>Deuxième octet de la signature DOS.</summary>
    public const byte DosSignatureO = (byte)'O';
    /// <summary>Troisième octet de la signature DOS.</summary>
    public const byte DosSignatureS = (byte)'S';
    /// <summary>Première variante AmigaDOS valide.</summary>
    public const AmigaDosVariant MinimumVariant = AmigaDosVariant.Ofs;
    /// <summary>Dernière variante AmigaDOS valide.</summary>
    public const AmigaDosVariant MaximumVariant = AmigaDosVariant.FfsLongNames;
    /// <summary>Époque utilisée par les dates AmigaDOS.</summary>
    public static readonly DateTimeOffset Epoch = new(1978, 1, 1, 0, 0, 0, TimeSpan.Zero);
}
