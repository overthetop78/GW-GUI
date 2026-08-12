using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;


using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Geometries.Acorn;

namespace GWGUI.MediaEngine.FileSystems.Readers;

public sealed class BbcDfsFileSystemReader : IFileSystemReader
{
    public string Id => "acorn-dfs";
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.AcornDfsSingleSided, DiskImageFormatIds.AcornDfsSingleSided80,
            DiskImageFormatIds.AcornDfsDoubleSided, DiskImageFormatIds.AcornDfsDoubleSided80 };

    public bool CanRead(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId) || image.BlockSize != BbcDfsGeometry.SectorSize || !image.TryGetBlock(0, out var names)
            || !image.TryGetBlock(1, out var metadata) || names.Data.Count != BbcDfsGeometry.SectorSize || metadata.Data.Count != BbcDfsGeometry.SectorSize) return false;
        var count = metadata.Data[5];
        return count % 8 == 0 && count <= 31 * 8 && ((metadata.Data[6] & 3) << BitPrimitives.BitsPerByte | metadata.Data[7]) <= image.BlockCount;
    }

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw new InvalidDataException("The image does not contain a valid BBC DFS catalogue.");
        var names = image.GetBlock(0).Span;
        var metadata = image.GetBlock(1).Span;
        var title = Decode(names[..8]) + Decode(metadata[..4]);
        var totalSectors = (metadata[6] & 3) << BitPrimitives.BitsPerByte | metadata[7];
        var fileCount = metadata[5] / 8;
        var entries = new List<FileSystemEntry>(fileCount);
        var warnings = new List<string>();
        var usedSectors = 2;
        for (var index = 0; index < fileCount; index++)
        {
            var nameOffset = 8 + index * 8;
            var metaOffset = 8 + index * 8;
            var leaf = Decode(names.Slice(nameOffset, 7));
            var directoryByte = names[nameOffset + 7];
            var directory = (char)(directoryByte & 0x7f);
            var name = directory == '$' ? leaf : $"{directory}.{leaf}";
            var packed = metadata[metaOffset + 6];
            var length = metadata[metaOffset + 4] | metadata[metaOffset + 5] << BitPrimitives.BitsPerByte | (packed & 0x30) << 12;
            var start = metadata[metaOffset + 7] | (packed & 3) << BitPrimitives.BitsPerByte;
            var load = metadata[metaOffset] | metadata[metaOffset + 1] << BitPrimitives.BitsPerByte | (packed & 0x0c) << 14;
            var execute = metadata[metaOffset + 2] | metadata[metaOffset + 3] << BitPrimitives.BitsPerByte | (packed & 0xc0) << 10;
            var sectorCount = (length + BbcDfsGeometry.SectorSize - 1) / BbcDfsGeometry.SectorSize;
            usedSectors += sectorCount;
            var content = new byte[length];
            var copied = 0;
            var valid = true;
            for (var sector = 0; sector < sectorCount; sector++)
            {
                if (!image.TryGetBlock(start + sector, out var block))
                {
                    warnings.Add($"{name}: data sector {start + sector} is missing.");
                    valid = false;
                    continue;
                }
                var count = Math.Min(BbcDfsGeometry.SectorSize, length - copied);
                block.Data.Take(count).ToArray().CopyTo(content, copied);
                copied += count;
            }
            var comment = $"DFS load &{load:X6}, execute &{execute:X6}";
            entries.Add(new(name, FileSystemEntryKind.File, length, null, comment,
                (uint)((directoryByte & 0x80) != 0 ? 1 : 0), start, valid, [], content));
        }
        var capacity = (long)totalSectors * BbcDfsGeometry.SectorSize;
        return new(title.Trim(), "Acorn DFS", capacity, Math.Max(0, totalSectors - usedSectors) * (long)BbcDfsGeometry.SectorSize,
            null, null, entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }

    private static string Decode(ReadOnlySpan<byte> bytes)
    {
        Span<byte> clean = stackalloc byte[bytes.Length];
        for (var index = 0; index < bytes.Length; index++) clean[index] = (byte)(bytes[index] & 0x7f);
        return System.Text.Encoding.ASCII.GetString(clean).TrimEnd('\0', ' ');
    }
}
