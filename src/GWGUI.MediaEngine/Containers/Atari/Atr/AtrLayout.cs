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
    public const int BootSectorSize = 128;
    /// <summary>Taille sectorielle ATR de 128 octets.</summary>
    public const int SingleDensitySectorSize = 128;
    /// <summary>Taille sectorielle ATR de 256 octets.</summary>
    public const int DoubleDensitySectorSize = 256;
    /// <summary>Taille sectorielle ATR étendue de 512 octets.</summary>
    public const int ExtendedSectorSize = 512;

    /// <summary>Indique si une taille sectorielle est prise en charge.</summary>
    /// <param name="sectorSize">Taille sectorielle observée, en octets.</param>
    /// <returns><see langword="true"/> pour une taille prise en charge ; sinon <see langword="false"/>.</returns>
    public static bool IsSupportedSectorSize(int sectorSize) =>
        sectorSize is SingleDensitySectorSize or DoubleDensitySectorSize or ExtendedSectorSize;
}
