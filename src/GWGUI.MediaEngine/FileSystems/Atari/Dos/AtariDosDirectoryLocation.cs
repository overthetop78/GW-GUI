namespace GWGUI.MediaEngine.FileSystems.Atari.Dos;

/// <summary>Décrit l'emplacement physique et la capacité d'un catalogue Atari DOS.</summary>
public sealed record AtariDosDirectoryLocation(
    int FirstSector,
    int SectorCount,
    int EntriesPerSector)
{
    /// <summary>Indique que le catalogue occupe l'emplacement Atari DOS habituel.</summary>
    public bool IsCanonical => FirstSector == AtariDosFileSystemLayout.FirstDirectorySector;
}
