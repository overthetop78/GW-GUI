using System.Collections.Frozen;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Apple.Lisa;

/// <summary>Lit les systèmes de fichiers Lisa Office portés par des pages munies de tags valides.</summary>
public sealed class LisaFileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => FileSystemIds.Lisa;

    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[] { MediaImageFormatIds.AppleLisaOffice }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool CanRead(IMediaSectorImage image) => image.FormatId.Equals(MediaImageFormatIds.AppleLisaOffice, StringComparison.OrdinalIgnoreCase) && image.AvailableBlocks.Any(block => LisaPageTagReader.TryRead(block, out var tag) && tag.FileId == LisaFileSystemLayout.MddfFileId);

    /// <inheritdoc />
    public FileSystemVolume Read(IMediaSectorImage image)
    {
        if (!CanRead(image)) throw LisaFileSystemExceptions.MissingTaggedFileSystem(image.AvailableBlocks.Count);
        var mddf = LisaMddfReader.Read(image);
        var catalog = LisaCatalogReader.Read(image, mddf.Version);
        var warnings = catalog.Warnings.ToList();
        AddInvalidTagWarnings(image, warnings);
        var entries = CreateEntries(image, catalog, warnings);
        var validTags = image.AvailableBlocks.Select(block => LisaPageTagReader.TryRead(block, out var tag) ? tag : (LisaPageTag?)null).Where(tag => tag.HasValue).Select(tag => tag!.Value).ToArray();
        var freeBytes = validTags.Length == 0 ? 0 : (long)validTags.Count(tag => tag.FileId is LisaFileSystemLayout.FreePageFileId or LisaFileSystemLayout.AlternateFreePageFileId) * image.BlockSize;
        if (validTags.Length == 0) warnings.Add(LisaFileSystemExceptions.UnknownFreeSpace());
        return new(mddf.VolumeName, FileSystemIds.Lisa, image.Capacity, freeBytes, null, null, entries, warnings);
    }

    /// <summary>Crée les entrées de fichiers depuis les pages utilisateur valides.</summary>
    private static IReadOnlyList<FileSystemEntry> CreateEntries(IMediaSectorImage image, LisaCatalogResult catalog, List<string> warnings)
    {
        var entries = new List<FileSystemEntry>();
        foreach (var item in LisaFileContentReader.ReadAll(image, warnings))
        {
            if (!catalog.Names.TryGetValue(item.FileId, out var name)) continue;
            entries.Add(new(name, FileSystemEntryKind.File, item.File.Content.Count, null, LisaFileSystemExceptions.FileDescription(item.FileId), 0, item.File.FirstLogicalBlock, item.File.IsValid, [], item.File.Content));
        }
        return entries.AsReadOnly();
    }

    /// <summary>Signale chaque bloc dont le tag est absent ou trop court.</summary>
    private static void AddInvalidTagWarnings(IMediaSectorImage image, ICollection<string> warnings)
    {
        foreach (var block in image.AvailableBlocks)
        {
            if (LisaPageTagReader.TryRead(block, out _)) continue;
            warnings.Add(LisaFileSystemExceptions.InvalidTag(block.LogicalBlock, block.Tag?.Count ?? 0, LisaFileSystemLayout.TagLength));
        }
    }
}
