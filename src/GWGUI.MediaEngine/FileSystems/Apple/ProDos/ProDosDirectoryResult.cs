namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Contient les entrées d'un répertoire ProDOS et la validité de sa chaîne.</summary>
internal sealed record ProDosDirectoryResult
{
    /// <summary>Crée un résultat de lecture de répertoire.</summary>
    public ProDosDirectoryResult(IReadOnlyList<FileSystemEntry> entries, bool isValid)
    {
        Entries = entries;
        IsValid = isValid;
    }

    /// <summary>Entrées triées du répertoire.</summary>
    public IReadOnlyList<FileSystemEntry> Entries { get; }
    /// <summary>Indique si la chaîne du répertoire est valide.</summary>
    public bool IsValid { get; }
}
