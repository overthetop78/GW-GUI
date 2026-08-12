using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Lisa;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Lit les systèmes Lisa Office à pages taguées.</summary>
public sealed class LisaFileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => Definitions.FileSystemIds.Lisa;

    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { DiskImageFormatIds.AppleLisaOffice, DiskImageFormatIds.Mac400 };

    /// <inheritdoc />
    public bool CanRead(SectorImage image) => image.FormatId.Equals(DiskImageFormatIds.AppleLisaOffice, StringComparison.OrdinalIgnoreCase) && image.AvailableBlocks.Any(block => TagFileId(block) == LisaFileSystemLayout.MddfFileId);

    /// <inheritdoc />
    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw LisaFileSystemExceptions.MissingTaggedFileSystem(image.AvailableBlocks.Count);
        var mddfBytes = image.AvailableBlocks.Where(block => TagFileId(block) == LisaFileSystemLayout.MddfFileId).Last().Data.ToArray();
        var mddf = mddfBytes.AsSpan();
        var version = BinaryPrimitives.ReadUInt16BigEndian(mddf[LisaVolumeHeader.VersionOffset..]);
        var volumeNameLength = mddf.Length > LisaVolumeHeader.NameLengthOffset ? Math.Min(mddf[LisaVolumeHeader.NameLengthOffset], (byte)LisaVolumeHeader.MaximumNameLength) : 0;
        var volumeName = volumeNameLength > 0 && mddf.Length >= LisaVolumeHeader.NameOffset + volumeNameLength ? LisaVolumeHeader.DecodeName(mddf.Slice(LisaVolumeHeader.NameOffset, volumeNameLength)) : string.Empty;
        if (string.IsNullOrWhiteSpace(volumeName)) volumeName = "Lisa";
        var catalog = LisaCatalogReader.Read(image, version);
        var warnings = catalog.Warnings.ToList();
        var entries = new List<FileSystemEntry>();
        foreach (var group in image.AvailableBlocks.Select(block => (Block: block, FileId: TagFileId(block))).Where(item => IsUserFile(item.FileId)).GroupBy(item => item.FileId).OrderBy(group => group.Key))
        {
            var ordered = group.OrderBy(item => TagPageNumber(item.Block)).ToArray();
            using var content = new MemoryStream();
            var expectedPage = ordered.Length == 0 ? 0 : TagPageNumber(ordered[0].Block);
            foreach (var item in ordered)
            {
                var page = TagPageNumber(item.Block);
                while (expectedPage < page) { warnings.Add(LisaFileSystemExceptions.MissingPage(group.Key, expectedPage)); expectedPage++; }
                content.Write(item.Block.Data.ToArray());
                expectedPage = page + 1;
            }
            var name = catalog.Names.TryGetValue(group.Key, out var catalogName) ? catalogName : LisaCatalogReader.FallbackName(group.Key);
            entries.Add(new(name, FileSystemEntryKind.File, content.Length, null, $"Lisa file ${group.Key:X4}", 0, ordered[0].Block.LogicalBlock, catalog.Names.ContainsKey(group.Key), [], content.ToArray()));
        }
        var freePages = image.AvailableBlocks.Count(block => TagFileId(block) is LisaFileSystemLayout.FreePageFileId or LisaFileSystemLayout.AlternateFreePageFileId);
        return new(volumeName, Definitions.FileSystemDisplayNames.Lisa(version), image.Capacity, (long)freePages * image.BlockSize, null, null, entries, warnings);
    }

    /// <summary>Lit l'identifiant de fichier conservé dans le tag d'un bloc.</summary>
    internal static ushort TagFileId(SectorBlock block)
    {
        if (block.Tag is null || block.Tag.Count <= LisaFileSystemLayout.TagFileIdLowOffset) return LisaFileSystemLayout.FreePageFileId;
        return (ushort)((block.Tag[LisaFileSystemLayout.TagFileIdHighOffset] << BitPrimitives.BitsPerByte) | block.Tag[LisaFileSystemLayout.TagFileIdLowOffset]);
    }

    /// <summary>Lit le numéro de page conservé dans le tag d'un bloc.</summary>
    internal static int TagPageNumber(SectorBlock block)
    {
        if (block.Tag is null || block.Tag.Count < LisaFileSystemLayout.MinimumTagLength) return block.LogicalBlock;
        return ((block.Tag[LisaFileSystemLayout.TagPageHighOffset] << BitPrimitives.BitsPerByte) | block.Tag[LisaFileSystemLayout.TagPageLowOffset]) & LisaFileSystemLayout.PageNumberMask;
    }

    /// <summary>Indique si un identifiant représente un fichier utilisateur.</summary>
    internal static bool IsUserFile(ushort fileId) => fileId is >= LisaFileSystemLayout.FirstUserFileId and <= LisaFileSystemLayout.LastUserFileId && !LisaFileSystemLayout.ReservedFileIds.Contains(fileId);
}
