namespace GWGUI.Scp.Containers.Apple.TwoImg;

/// <summary>
/// Décrit les tailles et offsets, exprimés en octets depuis le début du conteneur, de l’en-tête 2IMG.
/// </summary>
public static class TwoImgLayout
{
    /// <summary>Taille minimale, en octets, d’un en-tête 2IMG complet.</summary>
    public const int MinimumHeaderSize = 64;

    /// <summary>Offset de la signature ASCII 2IMG.</summary>
    public const int SignatureOffset = 0;

    /// <summary>Longueur, en octets, de la signature ASCII 2IMG.</summary>
    public const int SignatureLength = 4;

    /// <summary>Offset du champ 16 bits little-endian contenant la taille de l’en-tête.</summary>
    public const int HeaderSizeOffset = 8;

    /// <summary>Offset du champ 16 bits little-endian contenant la version du conteneur.</summary>
    public const int VersionOffset = 10;

    /// <summary>Offset du champ 32 bits little-endian contenant le format de la charge utile.</summary>
    public const int ImageFormatOffset = 12;

    /// <summary>Offset du champ 32 bits little-endian contenant la position de la charge utile.</summary>
    public const int DataOffsetOffset = 24;

    /// <summary>Offset du champ 32 bits little-endian contenant la longueur de la charge utile.</summary>
    public const int DataLengthOffset = 28;
}
