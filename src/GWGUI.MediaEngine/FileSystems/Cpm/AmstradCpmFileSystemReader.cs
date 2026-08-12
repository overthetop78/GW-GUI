using System.Collections.Frozen;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Recognition.Amstrad;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Cpm;

/// <summary>Lit les volumes CP/M des images Amstrad CPC et PCW.</summary>
public sealed class AmstradCpmFileSystemReader : IFileSystemReader
{
    private static readonly IReadOnlySet<string> Formats = new[] { DiskImageFormatIds.AmstradCpc, DiskImageFormatIds.AmstradPcw }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Identifiant technique du lecteur.</summary>
    public string Id => FileSystemIds.AmstradCpm;
    /// <summary>Formats Amstrad pris en charge.</summary>
    public IReadOnlySet<string> CatalogFormatIds => Formats;

    /// <summary>Indique si un répertoire CP/M Amstrad plausible est présent.</summary>
    public bool CanRead(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId)) return false;
        var logical = CpmDirectoryReader.Flatten(image);
        var layout = ResolveLayout(image, logical);
        return layout is not null && CpmDirectoryReader.LooksLikeDirectory(logical, layout, IsPcw(image), rejectLowercase: false);
    }

    /// <summary>Lit le volume CP/M Amstrad et reconstruit ses fichiers.</summary>
    public FileSystemVolume Read(SectorImage image)
    {
        var logical = CpmDirectoryReader.Flatten(image);
        var layout = ResolveLayout(image, logical) ?? throw CpmFileSystemExceptions.MissingLayout(image.FormatId);
        if (!CpmDirectoryReader.LooksLikeDirectory(logical, layout, IsPcw(image), rejectLowercase: false)) throw CpmFileSystemExceptions.UnsupportedDirectory(image.FormatId);
        var directory = CpmDirectoryReader.ReadDirectory(logical, layout, rejectLowercase: false);
        var warnings = new List<string>();
        if (logical.MissingBlocks.Count != 0) warnings.Add($"CP/M image contains {logical.MissingBlocks.Count} missing logical block(s).");
        if (logical.TruncatedBlocks.Count != 0) warnings.Add($"CP/M image contains {logical.TruncatedBlocks.Count} truncated logical block(s).");
        var files = new List<FileSystemEntry>();
        var usedAllocations = new HashSet<int>();
        foreach (var group in CpmDirectoryReader.GroupExtents(directory.Extents))
        {
            var file = CpmDirectoryReader.Reconstruct(logical, layout, group, warnings);
            if (file.Rejected) continue;
            usedAllocations.UnionWith(file.UsedAllocations);
            files.Add(new(group.Key.Name, FileSystemEntryKind.File, file.Content.Count, null, CpmFormat.UserArea(group.Key.User), group.Key.User, CpmFormat.NoStorageReference, file.Valid, [], file.Content));
        }
        var totalAllocations = Math.Max(0, (logical.Bytes.Length - layout.AllocationOrigin) / layout.AllocationBlockSize);
        var freeAllocations = Math.Max(0, totalAllocations - usedAllocations.Count - layout.DirectoryBlocks);
        return new(directory.VolumeName, FileSystemIds.AmstradCpm, image.Capacity, freeAllocations * (long)layout.AllocationBlockSize, null, null, files.OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }

    /// <summary>Résout une disposition CPC depuis l'identifiant du premier secteur ou une disposition PCW depuis sa spécification validée.</summary>
    private static CpmLayout? ResolveLayout(SectorImage image, CpmDirectoryReader.LogicalImage logical)
    {
        if (image.FormatId.Equals(DiskImageFormatIds.AmstradCpc, StringComparison.OrdinalIgnoreCase))
        {
            var first = image.AvailableBlocks.OrderBy(block => block.LogicalBlock).FirstOrDefault();
            if (first is null) return null;
            return first.Address.Number switch
            {
                >= AmstradCpmLayout.SystemFirstSectorId and <= AmstradCpmLayout.SystemLastSectorId => AmstradCpmLayout.CpcSystem,
                >= AmstradCpmLayout.DataFirstSectorId and <= AmstradCpmLayout.DataLastSectorId => AmstradCpmLayout.CpcData,
                _ => CpmDirectoryReader.FindDirectory(logical, AmstradCpmLayout.CpcSystem, AmstradCpmLayout.CpcSectorSize, allowEmpty: false, rejectLowercase: false)
            };
        }
        return AmstradCpmDiskSpecification.TryParse(logical.Bytes, out var specification) ? AmstradCpmLayout.FromPcw(specification, logical.Bytes.Length) : null;
    }

    /// <summary>Indique si l'image utilise le format PCW.</summary>
    private static bool IsPcw(SectorImage image) => image.FormatId.Equals(DiskImageFormatIds.AmstradPcw, StringComparison.OrdinalIgnoreCase);
}
