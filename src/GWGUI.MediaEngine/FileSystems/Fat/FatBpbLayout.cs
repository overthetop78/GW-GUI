namespace GWGUI.MediaEngine.FileSystems.Fat;

/// <summary>Définit les offsets des champs géométriques du BIOS Parameter Block FAT.</summary>
internal static class FatBpbLayout
{
    /// <summary>Longueur minimale requise pour tous les champs utilisés.</summary>
    public const int MinimumLength = 36;
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
