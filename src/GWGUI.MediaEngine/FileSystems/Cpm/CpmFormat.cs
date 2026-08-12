namespace GWGUI.MediaEngine.FileSystems.Cpm;

/// <summary>Définit la structure commune des entrées de répertoire CP/M.</summary>
internal static class CpmFormat
{
    /// <summary>Taille d'une entrée de répertoire.</summary>
    public const int DirectoryEntrySize = 32;
    /// <summary>Taille d'un enregistrement CP/M.</summary>
    public const int RecordSize = 128;
    /// <summary>Offset du nom de fichier.</summary>
    public const int FileNameOffset = 1;
    /// <summary>Longueur du nom de fichier.</summary>
    public const int FileNameLength = 8;
    /// <summary>Offset de l'extension.</summary>
    public const int FileExtensionOffset = 9;
    /// <summary>Longueur de l'extension.</summary>
    public const int FileExtensionLength = 3;
    /// <summary>Offset de la partie basse du numéro d'extent.</summary>
    public const int ExtentLowOffset = 12;
    /// <summary>Offset de la partie haute du numéro d'extent.</summary>
    public const int ExtentHighOffset = 14;
    /// <summary>Offset du compteur d'enregistrements.</summary>
    public const int RecordCountOffset = 15;
    /// <summary>Offset des allocations.</summary>
    public const int AllocationOffset = 16;
    /// <summary>Marqueur d'une entrée inutilisée.</summary>
    public const byte UnusedEntryMarker = 0xe5;
    /// <summary>Zone utilisateur réservée au label de volume.</summary>
    public const byte VolumeLabelUser = 0x20;
    /// <summary>Zone utilisateur réservée au mot de passe.</summary>
    public const byte PasswordLabelUser = 0x21;
    /// <summary>Plus grand numéro de zone utilisateur ordinaire.</summary>
    public const byte MaximumUserNumber = 31;
    /// <summary>Masque retirant les bits d'attribut des caractères.</summary>
    public const byte AttributeBitMask = 0x7f;
    /// <summary>Décalage de la partie haute du numéro d'extent.</summary>
    public const int ExtentHighShift = 5;
    /// <summary>Nombre d'allocations codées sur un octet.</summary>
    public const int NarrowAllocationCount = 16;
    /// <summary>Nombre d'allocations codées sur deux octets.</summary>
    public const int WideAllocationCount = 8;
    /// <summary>Taille d'une allocation large.</summary>
    public const int WideAllocationSize = 2;
    /// <summary>Valeur indiquant l'absence de référence de stockage.</summary>
    public const int NoStorageReference = -1;
    /// <summary>Score minimal d'un répertoire CP/M générique.</summary>
    public const int MinimumDirectoryScore = 4;
    /// <summary>Longueur maximale de recherche d'un répertoire Epson.</summary>
    public const int MaximumEpsonDirectorySearchLength = 64 * 1024;

    /// <summary>Construit la description d'une zone utilisateur CP/M.</summary>
    public static string UserArea(byte user) => $"CP/M user {user}";
}
