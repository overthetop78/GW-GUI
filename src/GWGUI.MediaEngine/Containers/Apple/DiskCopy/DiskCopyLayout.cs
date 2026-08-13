namespace GWGUI.MediaEngine.Containers.Apple.DiskCopy;

/// <summary>
/// Décrit les tailles et offsets, exprimés en octets depuis le début du conteneur, de DiskCopy 4.2.
/// </summary>
public static class DiskCopyLayout
{
    /// <summary>Offset de la longueur du nom Pascal.</summary>
    public const int NameLengthOffset = 0;
    /// <summary>Offset des octets du nom suivant sa longueur.</summary>
    public const int NameOffset = 1;
    /// <summary>Longueur maximale du nom DiskCopy.</summary>
    public const int MaximumNameLength = 63;
    /// <summary>Longueur minimale, en octets, des données sectorielles déclarées.</summary>
    public const int MinimumDataLength = 1;
    /// <summary>Taille, en octets, de l’en-tête DiskCopy 4.2.</summary>
    public const int HeaderSize = 84;

    /// <summary>Offset du champ 32 bits big-endian contenant la longueur des données sectorielles.</summary>
    public const int DataLengthOffset = 64;

    /// <summary>Offset du champ 32 bits big-endian contenant la longueur des tags sectoriels.</summary>
    public const int TagLengthOffset = 68;

    /// <summary>Offset du checksum 32 bits big-endian des données sectorielles.</summary>
    public const int DataChecksumOffset = 72;

    /// <summary>Offset du checksum 32 bits big-endian des tags sectoriels.</summary>
    public const int TagChecksumOffset = 76;

    /// <summary>Offset du type de disquette DiskCopy.</summary>
    public const int DiskFormatOffset = 80;

    /// <summary>Offset de l'octet identifiant le format logique DiskCopy.</summary>
    public const int FormatByteOffset = 81;

    /// <summary>Offset du mot magique big-endian terminant l'en-tête.</summary>
    public const int PrivateWordOffset = 82;

    /// <summary>Taille, en octets, des données d’un bloc logique DiskCopy.</summary>
    public const int DataBlockSize = 512;

    /// <summary>Taille, en octets, du tag associé à chaque bloc logique.</summary>
    public const int TagSizePerBlock = 12;

    /// <summary>Nombre d’octets initiaux exclus du calcul du checksum des tags.</summary>
    public const int TagChecksumExcludedPrefixSize = TagSizePerBlock;

    /// <summary>Index du bloc logique à partir duquel rechercher le marqueur MacWorks.</summary>
    public const int PrebootSearchBlockIndex = 2;

    /// <summary>Longueur, en octets, de la zone dans laquelle rechercher le marqueur MacWorks.</summary>
    public const int PrebootSearchLength = 16;
}
