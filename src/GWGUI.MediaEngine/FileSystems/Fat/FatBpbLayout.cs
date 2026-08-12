namespace GWGUI.MediaEngine.FileSystems.Fat;

/// <summary>Définit les offsets des champs géométriques du BIOS Parameter Block FAT.</summary>
internal static class FatBpbLayout
{
    /// <summary>Longueur minimale requise pour tous les champs utilisés.</summary>
    public const int MinimumLength = 36;
    /// <summary>Taille sectorielle attendue en octets.</summary>
    public const int SectorSize = 512;
    /// <summary>Offset de l'identifiant OEM.</summary>
    public const int OemOffset = 3;
    /// <summary>Longueur de l'identifiant OEM.</summary>
    public const int OemLength = 8;
    /// <summary>Caractère de remplissage nul.</summary>
    public const char NullPadding = '\0';
    /// <summary>Caractère de remplissage espace.</summary>
    public const char SpacePadding = ' ';
    /// <summary>Premier numéro de secteur logique.</summary>
    public const int FirstSectorNumber = 1;
    /// <summary>Maximum admis de secteurs par piste.</summary>
    public const int MaximumSectorsPerTrack = 63;
    /// <summary>Maximum admis de têtes.</summary>
    public const int MaximumHeadCount = 2;
    /// <summary>Maximum admis de cylindres.</summary>
    public const int MaximumCylinderCount = 255;
    /// <summary>Décalage du nombre d'octets par secteur.</summary>
    public const int BytesPerSectorOffset = 11;
    /// <summary>Décalage du nombre total de secteurs sur 16 bits.</summary>
    public const int TotalSectors16Offset = 19;
    /// <summary>Décalage du nombre de secteurs par piste.</summary>
    public const int SectorsPerTrackOffset = 24;
    /// <summary>Décalage du nombre de faces.</summary>
    public const int HeadCountOffset = 26;
    /// <summary>Décalage du nombre total de secteurs sur 32 bits.</summary>
    public const int TotalSectors32Offset = 32;
}
