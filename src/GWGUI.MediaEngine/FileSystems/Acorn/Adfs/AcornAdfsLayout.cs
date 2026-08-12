namespace GWGUI.MediaEngine.FileSystems.Acorn.Adfs;

/// <summary>Définit la disposition des catalogues ADFS au format Hugo ou Nick.</summary>
public static class AcornAdfsLayout
{
    /// <summary>Taille d'un bloc logique.</summary>
    public const int BlockSize = 1024;
    /// <summary>Taille de l'unité d'adressage FileCore.</summary>
    public const int FileCoreUnitSize = 256;
    /// <summary>Taille d'un répertoire.</summary>
    public const int DirectorySize = 2048;
    /// <summary>Nombre attendu de blocs de l'image ADFS prise en charge.</summary>
    public const int ImageBlockCount = 800;
    /// <summary>Nombre maximal d'entrées.</summary>
    public const int EntryCount = 77;
    /// <summary>Taille d'une entrée.</summary>
    public const int EntrySize = 26;
    /// <summary>Offset de la première entrée.</summary>
    public const int EntriesOffset = 5;
    /// <summary>Offset du nom dans une entrée.</summary>
    public const int EntryNameOffset = 0;
    /// <summary>Longueur du nom dans une entrée.</summary>
    public const int EntryNameLength = 10;
    /// <summary>Offset de l'adresse de chargement.</summary>
    public const int EntryLoadOffset = 10;
    /// <summary>Offset de l'adresse d'exécution.</summary>
    public const int EntryExecuteOffset = 14;
    /// <summary>Offset de la longueur.</summary>
    public const int EntryLengthOffset = 18;
    /// <summary>Offset de l'adresse indirecte.</summary>
    public const int EntryIndirectAddressOffset = 22;
    /// <summary>Offset des attributs.</summary>
    public const int EntryAttributesOffset = 25;
    /// <summary>Bit identifiant un répertoire.</summary>
    public const byte DirectoryAttribute = 0x08;
    /// <summary>Valeur terminant la liste d'entrées.</summary>
    public const byte EndOfEntries = 0;
    /// <summary>Offset de la queue du répertoire.</summary>
    public const int TailOffset = EntriesOffset + EntryCount * EntrySize;
    /// <summary>Offset du titre dans la queue.</summary>
    public const int TitleOffset = TailOffset + 6;
    /// <summary>Longueur du titre.</summary>
    public const int TitleLength = 19;
    /// <summary>Offset du nom du répertoire.</summary>
    public const int DirectoryNameOffset = TailOffset + 25;
    /// <summary>Longueur du nom du répertoire.</summary>
    public const int DirectoryNameLength = 10;
    /// <summary>Offset de la copie finale du numéro de séquence.</summary>
    public const int TailSequenceOffset = DirectorySize - 6;
    /// <summary>Offset de la signature finale.</summary>
    public const int FooterSignatureOffset = DirectorySize - 5;
    /// <summary>Offset de la signature initiale.</summary>
    public const int HeaderSignatureOffset = 1;
    /// <summary>Longueur des signatures.</summary>
    public const int SignatureLength = 4;
    /// <summary>Signature Hugo.</summary>
    public static ReadOnlySpan<byte> HugoSignature => "Hugo"u8;
    /// <summary>Signature Nick.</summary>
    public static ReadOnlySpan<byte> NickSignature => "Nick"u8;
    /// <summary>Profondeur maximale d'une arborescence.</summary>
    public const int MaximumDepth = 64;
}
