namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Contient les entrées UCSD validées et l'ensemble de leurs blocs utilisés.</summary>
/// <param name="Entries">Entrées de répertoire décodées.</param>
/// <param name="UsedBlocks">Blocs réservés ou attribués aux fichiers.</param>
/// <param name="IsValid">Indique si le répertoire et ses entrées sont cohérents.</param>
internal sealed record UcsdDirectoryEntriesResult(IReadOnlyList<FileSystemEntry> Entries, IReadOnlySet<int> UsedBlocks, bool IsValid);
