namespace GWGUI.MediaEngine.FileSystems.AppleDos;

/// <summary>Définit et valide les champs du VTOC Apple DOS utilisés par le lecteur et le sondage brut.</summary>
internal static class AppleDosVtoc
{
    /// <summary>Piste contenant le VTOC.</summary>
    public const int Track = 17;
    /// <summary>Décalage de la piste du premier secteur de catalogue.</summary>
    public const int CatalogTrackOffset = 1;
    /// <summary>Décalage du secteur du premier secteur de catalogue.</summary>
    public const int CatalogSectorOffset = 2;
    /// <summary>Décalage du nombre de secteurs par piste.</summary>
    public const int SectorsPerTrackOffset = 0x35;
    /// <summary>Décalage de la taille de secteur sur deux octets.</summary>
    public const int SectorSizeOffset = 0x36;
    /// <summary>Longueur minimale nécessaire à la validation.</summary>
    public const int MinimumLength = SectorSizeOffset + sizeof(ushort);

    /// <summary>Vérifie les bornes du catalogue et les caractéristiques sectorielles déclarées par le VTOC.</summary>
    public static bool IsValid(ReadOnlySpan<byte> vtoc, int trackCount, int sectorsPerTrack, int sectorSize)
    {
        if (vtoc.Length < MinimumLength) return false;
        var declaredSectorSize = vtoc[SectorSizeOffset] | vtoc[SectorSizeOffset + 1] << 8;
        return vtoc[CatalogTrackOffset] is > 0 && vtoc[CatalogTrackOffset] < trackCount && vtoc[CatalogSectorOffset] < sectorsPerTrack && vtoc[SectorsPerTrackOffset] == sectorsPerTrack && declaredSectorSize == sectorSize;
    }
}
