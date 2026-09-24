using System.Collections.Frozen;


using GWGUI.MediaFileSystems.Constants;


using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Acorn.BbcDfs;

/// <summary>Lit les catalogues BBC DFS simple et double face.</summary>
public sealed class BbcDfsFileSystemReader : IFileSystemReader
{
    /// <summary>Identifiant technique du lecteur.</summary>
    public string Id => Definitions.FileSystemIds.AcornDfs;
    /// <summary>Formats BBC DFS pris en charge.</summary>
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[] { MediaImageFormatIds.AcornAtomDos, MediaImageFormatIds.AcornDfsSingleSided, MediaImageFormatIds.AcornDfsSingleSided80, MediaImageFormatIds.AcornDfsDoubleSided, MediaImageFormatIds.AcornDfsDoubleSided80 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public bool CanRead(IMediaSectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId) || image.BlockSize != BbcDfsFileSystemLayout.SectorSize || !image.TryGetBlock(BbcDfsFileSystemLayout.NamesSector, out var names) || !image.TryGetBlock(BbcDfsFileSystemLayout.MetadataSector, out var metadata) || names.Data.Count != BbcDfsFileSystemLayout.SectorSize || metadata.Data.Count != BbcDfsFileSystemLayout.SectorSize) return false;
        var count = metadata.Data[BbcDfsFileSystemLayout.EntryCountOffset];
        return count % BbcDfsFileSystemLayout.EntryPartSize == 0 && count <= BbcDfsFileSystemLayout.MaximumEntryCount * BbcDfsFileSystemLayout.EntryPartSize && ((metadata.Data[BbcDfsFileSystemLayout.TotalSectorsHighOffset] & BbcDfsFileSystemLayout.TotalSectorsHighMask) << BitConstants.BitsPerByte | metadata.Data[BbcDfsFileSystemLayout.TotalSectorsLowOffset]) <= image.BlockCount;
    }

    public FileSystemVolume Read(IMediaSectorImage image)
    {
        if (!CanRead(image)) { var count = image.TryGetBlock(BbcDfsFileSystemLayout.MetadataSector, out var block) && block.Data.Count > BbcDfsFileSystemLayout.EntryCountOffset ? block.Data[BbcDfsFileSystemLayout.EntryCountOffset] / BbcDfsFileSystemLayout.EntryPartSize : -1; throw BbcDfsFileSystemExceptions.InvalidCatalog(count, image.BlockCount); }
        image.TryGetBlock(BbcDfsFileSystemLayout.NamesSector, out var namesBlock);
        var names = namesBlock.Data.ToArray().AsSpan();
        image.TryGetBlock(BbcDfsFileSystemLayout.MetadataSector, out var metadataBlock);
        var metadata = metadataBlock.Data.ToArray().AsSpan();
        var title = BbcDfsNameCodec.Decode(names[..BbcDfsFileSystemLayout.TitleFirstLength]) + BbcDfsNameCodec.Decode(metadata[..BbcDfsFileSystemLayout.TitleSecondLength]);
        var totalSectors = (metadata[BbcDfsFileSystemLayout.TotalSectorsHighOffset] & BbcDfsFileSystemLayout.TotalSectorsHighMask) << BitConstants.BitsPerByte | metadata[BbcDfsFileSystemLayout.TotalSectorsLowOffset];
        if (totalSectors == 0 && image.FormatId.Equals(MediaImageFormatIds.AcornAtomDos, StringComparison.OrdinalIgnoreCase)) totalSectors = image.BlockCount;
        var fileCount = metadata[BbcDfsFileSystemLayout.EntryCountOffset] / BbcDfsFileSystemLayout.EntryPartSize;
        var entries = new List<FileSystemEntry>(fileCount);
        var warnings = new List<string>();
        var usedSectors = 2;
        for (var index = 0; index < fileCount; index++)
        {
            var nameOffset = BbcDfsFileSystemLayout.FirstEntryOffset + index * BbcDfsFileSystemLayout.EntryPartSize;
            var metaOffset = nameOffset;
            var leaf = BbcDfsNameCodec.Decode(names.Slice(nameOffset, BbcDfsFileSystemLayout.LeafNameLength));
            var directoryByte = names[nameOffset + BbcDfsFileSystemLayout.DirectoryOffset];
            var directory = (char)(directoryByte & BbcDfsFileSystemLayout.CharacterMask);
            var atomDefaultQualifier = image.FormatId.Equals(MediaImageFormatIds.AcornAtomDos, StringComparison.OrdinalIgnoreCase) && directory == ' ';
            var name = directory == BbcDfsFileSystemLayout.RootDirectory || atomDefaultQualifier ? leaf : $"{directory}.{leaf}";
            var length = BbcDfsEntryDecoder.Length(metadata, metaOffset);
            var start = BbcDfsEntryDecoder.StartSector(metadata, metaOffset);
            var load = BbcDfsEntryDecoder.Load(metadata, metaOffset);
            var execute = BbcDfsEntryDecoder.Execute(metadata, metaOffset);
            var sectorCount = (length + BbcDfsFileSystemLayout.SectorSize - 1) / BbcDfsFileSystemLayout.SectorSize;
            usedSectors += sectorCount;
            var content = new byte[length];
            var copied = 0;
            var valid = true;
            for (var sector = 0; sector < sectorCount; sector++)
            {
                var targetSector = start + sector;
                if (targetSector >= totalSectors || !image.TryGetBlock(targetSector, out var block))
                {
                    warnings.Add(BbcDfsFileSystemExceptions.FileOutsideImage(name, start, length));
                    valid = false;
                    continue;
                }
                var targetOffset = sector * BbcDfsFileSystemLayout.SectorSize;
                var count = Math.Min(BbcDfsFileSystemLayout.SectorSize, length - targetOffset);
                if (block.Data.Count < count) { valid = false; continue; }
                for (var byteIndex = 0; byteIndex < count; byteIndex++) content[targetOffset + byteIndex] = block.Data[byteIndex];
                copied += count;
            }
            var comment = BbcDfsNameCodec.Description(load, execute);
            entries.Add(new(name, FileSystemEntryKind.File, length, null, comment,
                (uint)((directoryByte & BbcDfsFileSystemLayout.LockedBit) != 0 ? 1 : 0), start, valid, [], content));
        }
        var capacity = (long)totalSectors * BbcDfsFileSystemLayout.SectorSize;
        return new(title.Trim(), Definitions.FileSystemIds.AcornDfs, capacity, Math.Max(0, totalSectors - usedSectors) * (long)BbcDfsFileSystemLayout.SectorSize,
            null, null, entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }
}
