namespace GWGUI.MediaFileSystems.FileSystems.Atari.Dos;

/// <summary>Décrit la portion d'une chaîne Atari DOS délimitée par son entrée de répertoire.</summary>
public sealed record AtariDosSectorChain(
    IReadOnlyList<int> SectorNumbers,
    IReadOnlyList<int?> StoredFileNumbers,
    int ContinuationSector,
    bool IsComplete);
