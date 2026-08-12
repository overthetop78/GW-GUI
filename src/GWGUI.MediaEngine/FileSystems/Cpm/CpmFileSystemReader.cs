using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Cpm;

/// <summary>Lit les volumes CP/M des formats Commodore et Epson catalogués.</summary>
public sealed class CpmFileSystemReader : IFileSystemReader
{
    /// <summary>Identifiant technique du lecteur.</summary>
    public string Id => FileSystemIds.Cpm;
    /// <summary>Formats catalogués, exposés par une collection immuable.</summary>
    public IReadOnlySet<string> CatalogFormatIds => CpmLayoutCatalog.FormatIds;

    /// <summary>Indique si un répertoire CP/M plausible est présent.</summary>
    public bool CanRead(SectorImage image)
    {
        var configured = CpmLayoutCatalog.Resolve(image.FormatId);
        if (configured is null || !CpmLayoutCatalog.SupportsBlockSize(image.FormatId, image.BlockSize)) return false;
        var logical = CpmDirectoryReader.Flatten(image);
        var layout = CpmEpsonLayoutDetector.Resolve(image.FormatId, logical, configured);
        return CpmDirectoryReader.ScoreDirectory(logical, layout, rejectLowercase: true) >= CpmFormat.MinimumDirectoryScore;
    }

    /// <summary>Lit le volume CP/M et reconstruit ses fichiers.</summary>
    public FileSystemVolume Read(SectorImage image)
    {
        var configured = CpmLayoutCatalog.Resolve(image.FormatId) ?? throw CpmFileSystemExceptions.MissingLayout(image.FormatId);
        var logical = CpmDirectoryReader.Flatten(image);
        var layout = CpmEpsonLayoutDetector.Resolve(image.FormatId, logical, configured);
        if (CpmDirectoryReader.ScoreDirectory(logical, layout, rejectLowercase: true) < CpmFormat.MinimumDirectoryScore) throw CpmFileSystemExceptions.UnsupportedDirectory(image.FormatId);
        var directory = CpmDirectoryReader.ReadDirectory(logical, layout, rejectLowercase: true);
        return BuildVolume(image, logical, layout, directory);
    }

    /// <summary>Construit le volume depuis les extents communs validés.</summary>
    private static FileSystemVolume BuildVolume(SectorImage image, CpmDirectoryReader.LogicalImage logical, CpmLayout layout, CpmDirectoryReader.DirectoryResult directory)
    {
        var warnings = new List<string>();
        if (logical.MissingBlocks.Count != 0) warnings.Add($"CP/M image contains {logical.MissingBlocks.Count} missing logical block(s).");
        if (logical.TruncatedBlocks.Count != 0) warnings.Add($"CP/M image contains {logical.TruncatedBlocks.Count} truncated logical block(s).");
        var entries = new List<FileSystemEntry>();
        var usedAllocations = new HashSet<int>();
        foreach (var group in CpmDirectoryReader.GroupExtents(directory.Extents))
        {
            var file = CpmDirectoryReader.Reconstruct(logical, layout, group, warnings);
            if (file.Rejected) continue;
            usedAllocations.UnionWith(file.UsedAllocations);
            entries.Add(new(group.Key.Name, FileSystemEntryKind.File, file.Content.Count, null, CpmFormat.UserArea(group.Key.User), group.Key.User, CpmFormat.NoStorageReference, file.Valid, [], file.Content));
        }
        var totalAllocations = Math.Max(0, (logical.Bytes.Length - layout.AllocationOrigin) / layout.AllocationBlockSize);
        var freeAllocations = Math.Max(0, totalAllocations - usedAllocations.Count - layout.DirectoryBlocks);
        return new(directory.VolumeName, FileSystemIds.Cpm, image.Capacity, freeAllocations * (long)layout.AllocationBlockSize, null, null, entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }
}
