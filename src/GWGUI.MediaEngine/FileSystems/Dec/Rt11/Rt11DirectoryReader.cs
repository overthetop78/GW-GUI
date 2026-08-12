using GWGUI.MediaEngine.FileSystems.Readers;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Parcourt les segments de répertoire RT-11 et reconstruit leurs entrées.</summary>
public static class Rt11DirectoryReader
{
    /// <summary>Lit tous les segments accessibles depuis le premier bloc.</summary>
    public static Rt11DirectoryResult Read(SectorImage image, int directoryBlock)
    {
        var entries = new List<FileSystemEntry>();
        var warnings = new List<string>();
        var freeBlocks = 0L;
        var seenSegments = new HashSet<int>();
        var segment = 1;
        while (segment != 0 && segment <= Rt11FileSystemLayout.MaximumSegmentCount && seenSegments.Add(segment))
        {
            var firstBlock = directoryBlock + (segment - 1) * Rt11FileSystemLayout.SegmentBlockCount;
            if (!TryReadPair(image, firstBlock, out var bytes)) { warnings.Add(Rt11FileSystemExceptions.MissingBlockPair(firstBlock)); break; }
            var nextSegment = ReadUInt16(bytes, Rt11FileSystemLayout.NextSegmentOffset);
            var entrySize = Rt11FileSystemLayout.MinimumEntrySize + ReadUInt16(bytes, Rt11FileSystemLayout.ExtraBytesOffset);
            var dataBlock = ReadUInt16(bytes, Rt11FileSystemLayout.DataBlockOffset);
            if (entrySize is < Rt11FileSystemLayout.MinimumEntrySize or > Rt11FileSystemLayout.MaximumEntrySize) { warnings.Add(Rt11FileSystemExceptions.InvalidEntrySize(segment, entrySize)); break; }
            for (var offset = Rt11FileSystemLayout.EntriesOffset; offset + sizeof(ushort) <= bytes.Length; offset += entrySize)
            {
                var status = (Rt11DirectoryEntryStatus)ReadUInt16(bytes, offset + Rt11FileSystemLayout.StatusOffset);
                if (status.HasFlag(Rt11DirectoryEntryStatus.EndOfSegment)) break;
                if (offset + entrySize > bytes.Length) break;
                var blockLength = ReadUInt16(bytes, offset + Rt11FileSystemLayout.BlockLengthOffset);
                if (status.HasFlag(Rt11DirectoryEntryStatus.Empty)) { freeBlocks += blockLength; dataBlock += blockLength; continue; }
                if ((status & (Rt11DirectoryEntryStatus.Permanent | Rt11DirectoryEntryStatus.Tentative)) == 0) { dataBlock += blockLength; continue; }
                var name = DecodeRadix50(ReadUInt16(bytes, offset + Rt11FileSystemLayout.NameOffset)) + DecodeRadix50(ReadUInt16(bytes, offset + Rt11FileSystemLayout.NameOffset + sizeof(ushort)));
                var extension = DecodeRadix50(ReadUInt16(bytes, offset + Rt11FileSystemLayout.ExtensionOffset));
                name = name.TrimEnd();
                extension = extension.TrimEnd();
                if (extension.Length != 0) name += "." + extension;
                if (string.IsNullOrWhiteSpace(name)) { warnings.Add(Rt11FileSystemExceptions.EmptyName(dataBlock)); dataBlock += blockLength; continue; }
                var valid = TryReadContent(image, dataBlock, blockLength, out var content);
                if (!valid) warnings.Add(Rt11FileSystemExceptions.TruncatedContent(dataBlock, blockLength));
                var comment = status.HasFlag(Rt11DirectoryEntryStatus.Tentative) ? "RT-11 tentative file" : "RT-11 file";
                entries.Add(new(name, FileSystemEntryKind.File, blockLength * (long)Rt11FileSystemLayout.BlockSize, DecodeDate(ReadUInt16(bytes, offset + Rt11FileSystemLayout.DateOffset)), comment, status.HasFlag(Rt11DirectoryEntryStatus.Protected) ? 1u : 0u, dataBlock, valid, [], content));
                dataBlock += blockLength;
            }
            segment = nextSegment;
        }
        return new(entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), freeBlocks, warnings);
    }

    /// <summary>Décode un mot RADIX-50 en trois caractères.</summary>
    public static string DecodeRadix50(ushort word)
    {
        Span<char> result = stackalloc char[3];
        result[0] = Rt11FileSystemLayout.Radix50[word / 1600 % 40];
        result[1] = Rt11FileSystemLayout.Radix50[word / 40 % 40];
        result[2] = Rt11FileSystemLayout.Radix50[word % 40];
        return new string(result);
    }

    /// <summary>Décode une date RT-11.</summary>
    public static DateTimeOffset? DecodeDate(ushort word)
    {
        if (word == 0) return null;
        var day = word & 0x1f;
        var month = word >> 5 & 0x0f;
        var year = Rt11FileSystemLayout.EpochYear + (word >> 9 & 0x1f) + (word >> 14 & 3) * 32;
        try { return new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero); }
        catch (ArgumentOutOfRangeException) { return null; }
    }

    private static bool TryReadPair(SectorImage image, int firstBlock, out byte[] bytes)
    {
        bytes = new byte[Rt11FileSystemLayout.BlockSize * Rt11FileSystemLayout.SegmentBlockCount];
        if (!image.TryGetBlock(firstBlock, out var first) || !image.TryGetBlock(firstBlock + 1, out var second)) return false;
        first.Data.Take(Rt11FileSystemLayout.BlockSize).ToArray().CopyTo(bytes, 0);
        second.Data.Take(Rt11FileSystemLayout.BlockSize).ToArray().CopyTo(bytes, Rt11FileSystemLayout.BlockSize);
        return first.Data.Count >= Rt11FileSystemLayout.BlockSize && second.Data.Count >= Rt11FileSystemLayout.BlockSize;
    }

    private static bool TryReadContent(SectorImage image, int start, int count, out byte[] content)
    {
        content = new byte[count * Rt11FileSystemLayout.BlockSize];
        var valid = true;
        for (var index = 0; index < count; index++)
        {
            if (!image.TryGetBlock(start + index, out var block) || block.Data.Count < Rt11FileSystemLayout.BlockSize) { valid = false; continue; }
            block.Data.Take(Rt11FileSystemLayout.BlockSize).ToArray().CopyTo(content, index * Rt11FileSystemLayout.BlockSize);
        }
        return valid;
    }

    private static ushort ReadUInt16(ReadOnlySpan<byte> source, int offset) => (ushort)(source[offset] | source[offset + 1] << BitPrimitives.BitsPerByte);
}

/// <summary>Résultat de lecture du répertoire RT-11.</summary>
public sealed record Rt11DirectoryResult(IReadOnlyList<FileSystemEntry> Entries, long FreeBlocks, IReadOnlyList<string> Warnings);
