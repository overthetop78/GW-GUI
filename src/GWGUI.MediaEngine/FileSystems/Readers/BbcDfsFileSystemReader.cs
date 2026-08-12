using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Acorn.BbcDfs;
using GWGUI.MediaEngine.SectorImages;


using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Geometries.Acorn;

namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Lit les catalogues BBC DFS simple et double face.</summary>
public sealed class BbcDfsFileSystemReader : IFileSystemReader
{
    /// <summary>Identifiant technique du lecteur.</summary>
    public string Id => Definitions.FileSystemIds.AcornDfs;
    /// <summary>Formats BBC DFS pris en charge.</summary>
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.AcornDfsSingleSided, DiskImageFormatIds.AcornDfsSingleSided80,
            DiskImageFormatIds.AcornDfsDoubleSided, DiskImageFormatIds.AcornDfsDoubleSided80 };

    public bool CanRead(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId) || image.BlockSize != BbcDfsFileSystemLayout.SectorSize || !image.TryGetBlock(BbcDfsFileSystemLayout.NamesSector, out var names) || !image.TryGetBlock(BbcDfsFileSystemLayout.MetadataSector, out var metadata) || names.Data.Count != BbcDfsFileSystemLayout.SectorSize || metadata.Data.Count != BbcDfsFileSystemLayout.SectorSize) return false;
        var count = metadata.Data[BbcDfsFileSystemLayout.EntryCountOffset];
        return count % BbcDfsFileSystemLayout.EntryPartSize == 0 && count <= BbcDfsFileSystemLayout.MaximumEntryCount * BbcDfsFileSystemLayout.EntryPartSize && ((metadata.Data[BbcDfsFileSystemLayout.TotalSectorsHighOffset] & BbcDfsFileSystemLayout.TotalSectorsHighMask) << BitPrimitives.BitsPerByte | metadata.Data[BbcDfsFileSystemLayout.TotalSectorsLowOffset]) <= image.BlockCount;
    }

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) { var count = image.TryGetBlock(BbcDfsFileSystemLayout.MetadataSector, out var block) && block.Data.Count > BbcDfsFileSystemLayout.EntryCountOffset ? block.Data[BbcDfsFileSystemLayout.EntryCountOffset] / BbcDfsFileSystemLayout.EntryPartSize : -1; throw BbcDfsFileSystemExceptions.InvalidCatalog(count, image.BlockCount); }
        var names = image.GetBlock(BbcDfsFileSystemLayout.NamesSector).Span;
        var metadata = image.GetBlock(BbcDfsFileSystemLayout.MetadataSector).Span;
        var title = Decode(names[..BbcDfsFileSystemLayout.TitleFirstLength]) + Decode(metadata[..BbcDfsFileSystemLayout.TitleSecondLength]);
        var totalSectors = (metadata[BbcDfsFileSystemLayout.TotalSectorsHighOffset] & BbcDfsFileSystemLayout.TotalSectorsHighMask) << BitPrimitives.BitsPerByte | metadata[BbcDfsFileSystemLayout.TotalSectorsLowOffset];
        var fileCount = metadata[BbcDfsFileSystemLayout.EntryCountOffset] / BbcDfsFileSystemLayout.EntryPartSize;
        var entries = new List<FileSystemEntry>(fileCount);
        var warnings = new List<string>();
        var usedSectors = 2;
        for (var index = 0; index < fileCount; index++)
        {
            var nameOffset = BbcDfsFileSystemLayout.FirstEntryOffset + index * BbcDfsFileSystemLayout.EntryPartSize;
            var metaOffset = nameOffset;
            var leaf = Decode(names.Slice(nameOffset, BbcDfsFileSystemLayout.LeafNameLength));
            var directoryByte = names[nameOffset + BbcDfsFileSystemLayout.DirectoryOffset];
            var directory = (char)(directoryByte & BbcDfsFileSystemLayout.CharacterMask);
            var name = directory == '$' ? leaf : $"{directory}.{leaf}";
            var packed = metadata[metaOffset + BbcDfsFileSystemLayout.PackedOffset];
            var length = metadata[metaOffset + BbcDfsFileSystemLayout.LengthOffset] | metadata[metaOffset + BbcDfsFileSystemLayout.LengthOffset + 1] << BitPrimitives.BitsPerByte | (packed & BbcDfsFileSystemLayout.LengthHighMask) << BbcDfsFileSystemLayout.LengthHighShift;
            var start = metadata[metaOffset + BbcDfsFileSystemLayout.StartSectorOffset] | (packed & BbcDfsFileSystemLayout.StartSectorHighMask) << BitPrimitives.BitsPerByte;
            var load = metadata[metaOffset] | metadata[metaOffset + 1] << BitPrimitives.BitsPerByte | (packed & BbcDfsFileSystemLayout.LoadHighMask) << BbcDfsFileSystemLayout.LoadHighShift;
            var execute = metadata[metaOffset + 2] | metadata[metaOffset + 3] << BitPrimitives.BitsPerByte | (packed & BbcDfsFileSystemLayout.ExecuteHighMask) << BbcDfsFileSystemLayout.ExecuteHighShift;
            var sectorCount = (length + BbcDfsFileSystemLayout.SectorSize - 1) / BbcDfsFileSystemLayout.SectorSize;
            usedSectors += sectorCount;
            var content = new byte[length];
            var copied = 0;
            var valid = true;
            for (var sector = 0; sector < sectorCount; sector++)
            {
                if (!image.TryGetBlock(start + sector, out var block))
                {
                    warnings.Add(BbcDfsFileSystemExceptions.FileOutsideImage(name, start, length));
                    valid = false;
                    continue;
                }
                var count = Math.Min(BbcDfsFileSystemLayout.SectorSize, length - copied);
                block.Data.Take(count).ToArray().CopyTo(content, copied);
                copied += count;
            }
            var comment = $"DFS load &{load:X6}, execute &{execute:X6}";
            entries.Add(new(name, FileSystemEntryKind.File, length, null, comment,
                (uint)((directoryByte & BbcDfsFileSystemLayout.LockedBit) != 0 ? 1 : 0), start, valid, [], content));
        }
        var capacity = (long)totalSectors * BbcDfsFileSystemLayout.SectorSize;
        return new(title.Trim(), Definitions.FileSystemIds.AcornDfs, capacity, Math.Max(0, totalSectors - usedSectors) * (long)BbcDfsFileSystemLayout.SectorSize,
            null, null, entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }

    private static string Decode(ReadOnlySpan<byte> bytes)
    {
        Span<byte> clean = stackalloc byte[bytes.Length];
        for (var index = 0; index < bytes.Length; index++) clean[index] = (byte)(bytes[index] & BbcDfsFileSystemLayout.CharacterMask);
        return System.Text.Encoding.ASCII.GetString(clean).TrimEnd('\0', ' ');
    }
}
