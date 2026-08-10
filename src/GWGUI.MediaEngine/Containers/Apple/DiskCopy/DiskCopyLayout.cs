namespace GWGUI.MediaEngine.Containers.Apple.DiskCopy;

/// <summary>
/// Décrit les tailles et offsets, exprimés en octets depuis le début du conteneur, de DiskCopy 4.2.
/// </summary>
public static class DiskCopyLayout
{
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
