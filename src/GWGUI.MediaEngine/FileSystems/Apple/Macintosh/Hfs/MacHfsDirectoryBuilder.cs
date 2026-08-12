namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;

/// <summary>Construit l'arborescence HFS depuis les relations parent-enfant du catalogue.</summary>
internal static class MacHfsDirectoryBuilder
{
    /// <summary>Construit les entrées racines HFS.</summary>
    public static IReadOnlyList<FileSystemEntry> Build(IReadOnlyList<MacHfsCatalogRecord> records, List<string> warnings) => BuildChildren(MacHfsFileSystemLayout.RootDirectoryId, records, new HashSet<uint>(), warnings);

    /// <summary>Construit les enfants d'un dossier en détectant les cycles sur le chemin courant.</summary>
    private static IReadOnlyList<FileSystemEntry> BuildChildren(uint parentId, IReadOnlyList<MacHfsCatalogRecord> records, HashSet<uint> path, ICollection<string> warnings)
    {
        if (!path.Add(parentId))
        {
            warnings.Add(MacFileSystemExceptions.DirectoryCycle(parentId));
            return [];
        }
        return records.Where(record => record.ParentId == parentId).Select(record => CreateEntry(record, records, path, warnings)).OrderBy(entry => entry.Kind != FileSystemEntryKind.Directory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>Convertit un record HFS en entrée commune.</summary>
    private static FileSystemEntry CreateEntry(MacHfsCatalogRecord record, IReadOnlyList<MacHfsCatalogRecord> records, HashSet<uint> path, ICollection<string> warnings)
    {
        var children = record.IsDirectory ? BuildChildren(record.Id, records, new HashSet<uint>(path), warnings) : [];
        var content = record.IsDirectory ? null : record.DataFork.Count > 0 ? record.DataFork : record.ResourceFork;
        return new(record.Name, record.IsDirectory ? FileSystemEntryKind.Directory : FileSystemEntryKind.File, record.Size, record.Modified, record.Type, 0, ToStorageReference(record.Id), record.IsValid, children, content);
    }

    /// <summary>Convertit sans dépassement un identifiant HFS vers la référence de stockage commune.</summary>
    private static int ToStorageReference(uint id) => checked((int)Math.Min(id, int.MaxValue));
}
