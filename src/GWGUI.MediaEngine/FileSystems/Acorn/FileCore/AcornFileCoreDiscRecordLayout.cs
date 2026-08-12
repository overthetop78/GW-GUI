namespace GWGUI.MediaEngine.FileSystems.Acorn.FileCore;

/// <summary>Définit les champs binaires d'un DiscRecord FileCore.</summary>
public static class AcornFileCoreDiscRecordLayout
{
    /// <summary>Offset du logarithme de taille sectorielle.</summary>
    public const int Log2SectorSizeOffset = 0;
    /// <summary>Offset de la longueur d'identifiant.</summary>
    public const int IdLengthOffset = 4;
    /// <summary>Offset du logarithme des octets par map-bit.</summary>
    public const int Log2BytesPerMapBitOffset = 5;
    /// <summary>Offset bas du nombre de zones.</summary>
    public const int ZoneCountLowOffset = 9;
    /// <summary>Offset des bits de réserve d'une zone.</summary>
    public const int ZoneSpareBitsOffset = 10;
    /// <summary>Offset de l'adresse racine.</summary>
    public const int RootAddressOffset = 12;
    /// <summary>Offset du mot bas de taille disque.</summary>
    public const int DiscSizeLowOffset = 16;
    /// <summary>Offset du nom du disque.</summary>
    public const int DiscNameOffset = 22;
    /// <summary>Longueur du nom du disque.</summary>
    public const int DiscNameLength = 10;
    /// <summary>Offset du mot haut de taille disque.</summary>
    public const int DiscSizeHighOffset = 36;
    /// <summary>Offset du logarithme de taille de partage.</summary>
    public const int Log2ShareSizeOffset = 40;
    /// <summary>Masque du logarithme de taille de partage.</summary>
    public const byte Log2ShareSizeMask = 0x0f;
    /// <summary>Offset haut du nombre de zones.</summary>
    public const int ZoneCountHighOffset = 42;
    /// <summary>Minimum du logarithme sectoriel.</summary>
    public const int MinimumLog2SectorSize = 8;
    /// <summary>Maximum du logarithme sectoriel.</summary>
    public const int MaximumLog2SectorSize = 10;
    /// <summary>Nombre maximal de bits d'identifiant.</summary>
    public const int MaximumIdLength = 19;
    /// <summary>Nombre de bits supplémentaires minimal d'un identifiant.</summary>
    public const int MinimumIdExtraBits = 3;
    /// <summary>Nombre minimal de zones.</summary>
    public const int MinimumZoneCount = 1;
    /// <summary>Première adresse racine valide.</summary>
    public const int MinimumRootAddress = 1;
    /// <summary>Première taille de disque valide.</summary>
    public const int MinimumDiscSize = 1;
}
