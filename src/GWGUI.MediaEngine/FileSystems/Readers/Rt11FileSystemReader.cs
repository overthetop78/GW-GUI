using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Rt11;
using GWGUI.MediaEngine.SectorImages;


namespace GWGUI.MediaEngine.FileSystems.Readers;

public sealed class Rt11FileSystemReader : IFileSystemReader
{
    private const ushort Tentative = 0x0100;
    private const ushort Empty = 0x0200;
    private const ushort Permanent = 0x0400;
    private const ushort EndOfSegment = 0x0800;
    private const ushort Protected = 0x8000;
    private const string Radix50 = " ABCDEFGHIJKLMNOPQRSTUVWXYZ$.%0123456789";

    public string Id => "rt11";
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.DecRx02 };

    public bool CanRead(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId) || image.BlockSize != 512 || !image.TryGetBlock(1, out var home)) return false;
        var span = home.Data is byte[] data ? data.AsSpan() : home.Data.ToArray().AsSpan();
        return Rt11HomeBlockProbe.LooksLikeRt11(span);
    }

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw new InvalidDataException("The image does not contain an RT-11 file system.");
        var home = image.GetBlock(1).Span;
        var volumeName = DecodeAscii(home.Slice(472, 12));
        var directoryBlock = ReadUInt16(home, 468);
        var entries = new List<FileSystemEntry>();
        var warnings = new List<string>();
        var freeBlocks = 0L;
        var seenSegments = new HashSet<int>();
        var segment = 1;

        while (segment != 0 && segment <= 31 && seenSegments.Add(segment))
        {
            var firstBlock = directoryBlock + (segment - 1) * 2;
            if (!TryReadPair(image, firstBlock, out var bytes))
            {
                warnings.Add($"RT-11 directory segment {segment} is missing.");
                break;
            }
            var nextSegment = ReadUInt16(bytes, 2);
            var extraBytes = ReadUInt16(bytes, 6);
            var dataBlock = ReadUInt16(bytes, 8);
            var entrySize = 14 + extraBytes;
            if (entrySize < 14 || entrySize > 128)
            {
                warnings.Add($"RT-11 directory segment {segment} has an invalid entry size.");
                break;
            }

            for (var offset = 10; offset + 2 <= bytes.Length; offset += entrySize)
            {
                var status = ReadUInt16(bytes, offset);
                if ((status & EndOfSegment) != 0) break;
                if (offset + entrySize > bytes.Length) break;
                var blockLength = ReadUInt16(bytes, offset + 8);
                if ((status & Empty) != 0)
                {
                    freeBlocks += blockLength;
                    dataBlock += blockLength;
                    continue;
                }
                if ((status & (Permanent | Tentative)) == 0)
                {
                    dataBlock += blockLength;
                    continue;
                }

                var name = DecodeRadix50(ReadUInt16(bytes, offset + 2)) + DecodeRadix50(ReadUInt16(bytes, offset + 4));
                var extension = DecodeRadix50(ReadUInt16(bytes, offset + 6));
                name = name.TrimEnd(); extension = extension.TrimEnd();
                if (extension.Length != 0) name += "." + extension;
                if (string.IsNullOrWhiteSpace(name))
                {
                    warnings.Add($"RT-11 entry at block {dataBlock} has an empty name.");
                    dataBlock += blockLength;
                    continue;
                }

                var valid = TryReadContent(image, dataBlock, blockLength, out var content);
                if (!valid) warnings.Add($"{name}: one or more data blocks are missing.");
                var date = DecodeDate(ReadUInt16(bytes, offset + 12));
                var comment = (status & Tentative) != 0 ? "RT-11 tentative file" : "RT-11 file";
                entries.Add(new(name, FileSystemEntryKind.File, blockLength * 512L, date, comment,
                    (uint)((status & Protected) != 0 ? 1 : 0), dataBlock, valid, [], content));
                dataBlock += blockLength;
            }
            segment = nextSegment;
        }

        var commentParts = Array.Empty<string>();
        if (commentParts.Any()) warnings.Insert(0, string.Join(" · ", commentParts!));
        return new FileSystemVolume(volumeName, "DEC RT-11", image.Capacity, freeBlocks * 512,
            null, entries.Select(entry => entry.Modified).Where(date => date.HasValue).Max(),
            entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }

    private static bool TryReadPair(SectorImage image, int firstBlock, out byte[] bytes)
    {
        bytes = new byte[1024];
        if (!image.TryGetBlock(firstBlock, out var first) || !image.TryGetBlock(firstBlock + 1, out var second)) return false;
        first.Data.Take(512).ToArray().CopyTo(bytes, 0);
        second.Data.Take(512).ToArray().CopyTo(bytes, 512);
        return first.Data.Count >= 512 && second.Data.Count >= 512;
    }

    private static bool TryReadContent(SectorImage image, int start, int count, out byte[] content)
    {
        content = new byte[count * 512];
        var valid = true;
        for (var index = 0; index < count; index++)
        {
            if (!image.TryGetBlock(start + index, out var block) || block.Data.Count < 512) { valid = false; continue; }
            block.Data.Take(512).ToArray().CopyTo(content, index * 512);
        }
        return valid;
    }

    private static string DecodeRadix50(ushort word)
    {
        Span<char> result = stackalloc char[3];
        result[0] = Radix50[word / 1600 % 40];
        result[1] = Radix50[word / 40 % 40];
        result[2] = Radix50[word % 40];
        return new string(result);
    }

    private static DateTimeOffset? DecodeDate(ushort word)
    {
        if (word == 0) return null;
        var day = word & 0x1f;
        var month = word >> 5 & 0x0f;
        var year = 1972 + (word >> 9 & 0x1f) + (word >> 14 & 3) * 32;
        try { return new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero); }
        catch (ArgumentOutOfRangeException) { return null; }
    }

    private static string DecodeAscii(ReadOnlySpan<byte> bytes) => System.Text.Encoding.ASCII.GetString(bytes).TrimEnd('\0', ' ');
    private static ushort ReadUInt16(ReadOnlySpan<byte> source, int offset) => (ushort)(source[offset] | source[offset + 1] << 8);
}
