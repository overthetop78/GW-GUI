namespace GWGUI.MediaFileSystems.FileSystems.Atari.Dos;

/// <summary>Décrit une entrée active du répertoire Atari DOS avant la reconstruction de son contenu.</summary>
public sealed record AtariDosDirectoryEntry(
    int EntryNumber,
    int DirectorySector,
    int DirectorySlot,
    string Name,
    AtariDosDirectoryFlags Flags,
    int DeclaredSectorCount,
    int FirstSector);
