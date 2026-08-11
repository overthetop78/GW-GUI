namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Décrit les positions et tailles binaires d'un conteneur ImageDisk.</summary>
internal static class ImdLayout
{
    /// <summary>Longueur de l'en-tête d'une piste, en octets.</summary>
    public const int TrackHeaderSize = 5;
    /// <summary>Position relative du mode.</summary>
    public const int ModeOffset = 0;
    /// <summary>Position relative du cylindre.</summary>
    public const int CylinderOffset = 1;
    /// <summary>Position relative des drapeaux de face.</summary>
    public const int HeadFlagsOffset = 2;
    /// <summary>Position relative du nombre de secteurs.</summary>
    public const int SectorCountOffset = 3;
    /// <summary>Position relative du code de taille sectorielle.</summary>
    public const int SectorSizeCodeOffset = 4;
    /// <summary>Longueur d'une entrée de carte d'octets.</summary>
    public const int MapEntrySize = 1;
    /// <summary>Longueur d'une entrée de carte de tailles, en octets.</summary>
    public const int SectorSizeMapEntrySize = 2;
    /// <summary>Taille sectorielle correspondant au code exponentiel zéro, en octets.</summary>
    public const int BaseSectorSize = 128;
    /// <summary>Code indiquant la présence d'une carte de tailles explicites.</summary>
    public const byte ExplicitSectorSizeCode = 0xFF;
    /// <summary>Code de taille exponentiel maximal accepté.</summary>
    public const byte MaximumExponentialSizeCode = 6;
}
