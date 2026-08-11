namespace GWGUI.MediaEngine.Containers.Atari.Atr;

/// <summary>Décrit la disposition binaire et les tailles prises en charge d'un conteneur ATR.</summary>
internal static class AtrLayout
{
    /// <summary>Taille de l'en-tête ATR, en octets.</summary>
    public const int HeaderSize = 16;
    /// <summary>Offset de la signature, en octets depuis le début du fichier.</summary>
    public const int SignatureOffset = 0;
    /// <summary>Offset du mot bas du nombre de paragraphes de données.</summary>
    public const int ParagraphCountLowOffset = 2;
    /// <summary>Offset de la taille nominale des secteurs.</summary>
    public const int SectorSizeOffset = 4;
    /// <summary>Offset du mot haut du nombre de paragraphes de données.</summary>
    public const int ParagraphCountHighOffset = 6;
    /// <summary>Taille d'un paragraphe ATR, en octets.</summary>
    public const int ParagraphSize = 16;
    /// <summary>Nombre de secteurs d'amorçage placés au début de la charge utile.</summary>
    public const int BootSectorCount = 3;
    /// <summary>Taille d'un secteur d'amorçage, en octets.</summary>
    public const int BootSectorSize = SingleDensitySectorSize;
    /// <summary>Taille sectorielle ATR de 128 octets.</summary>
    public const int SingleDensitySectorSize = 128;
    /// <summary>Taille sectorielle ATR de 256 octets.</summary>
    public const int DoubleDensitySectorSize = 256;
    /// <summary>Taille sectorielle ATR étendue de 512 octets.</summary>
    public const int ExtendedSectorSize = 512;
    /// <summary>Nombre de secteurs d'une image ATR standard de 90 ou 180 Kio.</summary>
    public const int StandardSectorCount = 720;
    /// <summary>Nombre de secteurs d'une image ATR à densité améliorée de 130 Kio.</summary>
    public const int EnhancedDensitySectorCount = 1040;
    /// <summary>Décalage binaire appliqué au mot haut du nombre de paragraphes.</summary>
    public const int ParagraphCountHighWordShift = 16;

    /// <summary>Indique si une taille sectorielle est prise en charge.</summary>
    /// <param name="sectorSize">Taille sectorielle observée, en octets.</param>
    /// <returns><see langword="true"/> pour une taille prise en charge ; sinon <see langword="false"/>.</returns>
    public static bool IsSupportedSectorSize(int sectorSize) => sectorSize is SingleDensitySectorSize or DoubleDensitySectorSize or ExtendedSectorSize;

    /// <summary>Calcule la longueur de la zone d'amorçage précédant les secteurs de taille nominale.</summary>
    /// <param name="sectorSize">Taille nominale des secteurs, en octets.</param>
    /// <returns>Zéro pour les secteurs de 128 octets ; sinon la longueur des trois secteurs d'amorçage.</returns>
    public static int GetBootAreaLength(int sectorSize) => sectorSize == SingleDensitySectorSize ? 0 : BootSectorCount * BootSectorSize;

    /// <summary>Calcule le nombre total de secteurs contenus dans une charge utile ATR valide.</summary>
    /// <param name="payloadLength">Longueur de la charge utile ATR, en octets.</param>
    /// <param name="sectorSize">Taille nominale des secteurs suivant la zone d'amorçage.</param>
    /// <returns>Nombre total de secteurs, secteurs d'amorçage compris.</returns>
    public static int GetSectorCount(int payloadLength, int sectorSize)
    {
        var bootAreaLength = GetBootAreaLength(sectorSize);
        return (sectorSize == SingleDensitySectorSize ? 0 : BootSectorCount) + (payloadLength - bootAreaLength) / sectorSize;
    }
}
