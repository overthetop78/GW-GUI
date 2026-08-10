using System.Buffers.Binary;
using System.Text;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Readers;

public sealed class CpmFileSystemReader : IFileSystemReader
{
    public string Id => "cpm";
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            DiskImageFormatIds.Commodore1541, DiskImageFormatIds.Commodore1571, DiskImageFormatIds.Commodore1581,
            DiskImageFormatIds.EpsonQx10_320, DiskImageFormatIds.EpsonQx10_396,
            DiskImageFormatIds.EpsonQx10_399, DiskImageFormatIds.EpsonQx10_400, DiskImageFormatIds.EpsonQx10Logo
        };

    public bool CanRead(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId) || image.BlockSize is not (256 or 512 or 1024)) return false;
        var layout = Layout.For(image);
        if (layout is null) return false;
        var bytes = Flatten(image);
        return ScoreDirectory(bytes, ResolveLayout(image, bytes, layout.Value)) >= 4;
    }

    public FileSystemVolume Read(SectorImage image)
    {
        var configuredLayout = Layout.For(image) ?? throw new InvalidDataException("The CP/M disk layout is not supported.");
        var bytes = Flatten(image);
        var layout = ResolveLayout(image, bytes, configuredLayout);
        if (ScoreDirectory(bytes, layout) < 4) throw new InvalidDataException("The image does not contain a supported CP/M directory.");
        var warnings = new List<string>();
        var extents = new List<Extent>();
        string volumeName = string.Empty;
        for (var index = 0; index < layout.DirectoryEntries; index++)
        {
            var offset = layout.DirectoryOffset + index * 32;
            if (offset + 32 > bytes.Length) break;
            var entry = bytes.AsSpan(offset, 32);
            var user = entry[0];
            if (user == 0xe5) continue;
            if (user == 0x20)
            {
                var candidate = DecodePart(entry[1..9]);
                if (IsPlausibleLabel(candidate)) volumeName = candidate;
                continue;
            }
            if (user > 31 || !TryDecodeName(entry, out var name)) continue;
            var allocations = ReadAllocations(entry, layout.WideAllocations);
            var extentNumber = entry[12] + (entry[14] << 5);
            extents.Add(new(user, name, extentNumber, entry[15], allocations));
        }
        var entries = new List<FileSystemEntry>();
        var totalBlocks = Math.Max(0, (bytes.Length - layout.DirectoryOffset) / layout.AllocationBlockSize);
        foreach (var group in extents.GroupBy(extent => (extent.User, extent.Name), new ExtentKeyComparer()))
        {
            var referenced = group.SelectMany(extent => extent.Allocations).Where(block => block != 0).ToArray();
            if (referenced.Length > 0 && referenced.Count(block => block < totalBlocks) * 2 < referenced.Length) continue;
            using var content = new MemoryStream();
            var metadataValid = true;
            foreach (var extent in group.OrderBy(extent => extent.Number))
            {
                var extentBytes = new MemoryStream();
                foreach (var allocation in extent.Allocations)
                {
                    if (allocation == 0) continue;
                    var blockOffset = layout.DirectoryOffset + allocation * layout.AllocationBlockSize;
                    if (blockOffset < 0 || blockOffset + layout.AllocationBlockSize > bytes.Length)
                    {
                        warnings.Add($"{group.Key.Name}: CP/M allocation block {allocation} is outside the image.");
                        metadataValid = false; continue;
                    }
                    extentBytes.Write(bytes, blockOffset, layout.AllocationBlockSize);
                }
                var used = Math.Min(extentBytes.Length, extent.RecordCount * 128L);
                content.Write(extentBytes.GetBuffer(), 0, checked((int)used));
            }
            entries.Add(new(group.Key.Name, FileSystemEntryKind.File, content.Length, null, $"CP/M user {group.Key.User}",
                (uint)group.Key.User, -1, metadataValid, [], content.ToArray()));
        }
        var usedBlocks = extents.SelectMany(extent => extent.Allocations).Where(block => block != 0).Distinct().Count();
        return new(volumeName, "CP/M 3", image.Capacity, Math.Max(0, totalBlocks - usedBlocks - layout.DirectoryBlocks) * (long)layout.AllocationBlockSize,
            null, null, entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }

    private static int ScoreDirectory(byte[] bytes, Layout layout)
    {
        var score = 0;
        for (var index = 0; index < layout.DirectoryEntries && layout.DirectoryOffset + index * 32 + 32 <= bytes.Length; index++)
        {
            var entry = bytes.AsSpan(layout.DirectoryOffset + index * 32, 32);
            if (entry[0] <= 31 && TryDecodeName(entry, out _)) score++;
        }
        return score;
    }

    private static Layout ResolveLayout(SectorImage image, byte[] bytes, Layout configured)
    {
        if (!image.FormatId.StartsWith(DiskImageFormatIds.EpsonQx10Prefix, StringComparison.OrdinalIgnoreCase)) return configured;
        var best = configured;
        var bestScore = ScoreDirectory(bytes, configured);
        var limit = Math.Min(bytes.Length - configured.DirectoryEntries * 32, 64 * 1024);
        // CP/M directory entries are 32-byte records. Epson TPM disks may place
        // the directory on a 128-byte logical boundary inside a physical sector,
        // so restricting the search to physical 256-byte boundaries skips valid
        // directories (notably the mixed 256/512-byte QX-10 layouts).
        for (var offset = 0; offset <= limit; offset += 32)
        {
            var candidate = configured with { DirectoryOffset = offset };
            var score = ScoreDirectory(bytes, candidate);
            if (score > bestScore)
            {
                best = candidate;
                bestScore = score;
            }
        }
        return best;
    }

    private static bool TryDecodeName(ReadOnlySpan<byte> entry, out string name)
    {
        name = string.Empty;
        if (entry.Length < 12) return false;
        for (var index = 1; index <= 11; index++)
        {
            var value = entry[index] & 0x7f;
            if (value != 0x20 && (value < 0x21 || value > 0x7e)) return false;
            if (value is >= (byte)'a' and <= (byte)'z') return false;
        }
        var stem = DecodePart(entry[1..9]); var extension = DecodePart(entry[9..12]);
        if (stem.Length == 0) return false;
        name = extension.Length == 0 ? stem : stem + "." + extension;
        return true;
    }

    private static string DecodePart(ReadOnlySpan<byte> value)
    {
        Span<byte> clean = stackalloc byte[value.Length];
        for (var index = 0; index < value.Length; index++) clean[index] = (byte)(value[index] & 0x7f);
        return System.Text.Encoding.ASCII.GetString(clean).Trim();
    }

    private static bool IsPlausibleLabel(string value) => value.Length > 0
        && value.All(character => char.IsLetterOrDigit(character) || character is ' ' or '-' or '_' or '.');

    private static IReadOnlyList<int> ReadAllocations(ReadOnlySpan<byte> entry, bool wide)
    {
        var result = new List<int>();
        if (wide) for (var index = 16; index < 32; index += 2) result.Add(BinaryPrimitives.ReadUInt16LittleEndian(entry[index..]));
        else for (var index = 16; index < 32; index++) result.Add(entry[index]);
        return result;
    }

    private static byte[] Flatten(SectorImage image)
    {
        using var output = new MemoryStream(image.Capacity <= int.MaxValue ? (int)image.Capacity : 0);
        var missing = new byte[image.BlockSize];
        for (var block = 0; block < image.BlockCount; block++)
        {
            if (image.TryGetBlock(block, out var sector)) output.Write(sector.Data.ToArray());
            else output.Write(missing);
        }
        return output.ToArray();
    }

    private readonly record struct Extent(int User, string Name, int Number, int RecordCount, IReadOnlyList<int> Allocations);
    private readonly record struct Layout(int DirectoryOffset, int DirectoryEntries, int AllocationBlockSize, int DirectoryBlocks, bool WideAllocations)
    {
        public static Layout? For(SectorImage image) => image.FormatId switch
        {
            DiskImageFormatIds.Commodore1541 => new(0x0a00, 64, 1024, 2, false),
            DiskImageFormatIds.Commodore1571 => new(0x0a00, 128, 2048, 2, true),
            DiskImageFormatIds.Commodore1581 => new(0, 128, 2048, 2, true),
            DiskImageFormatIds.EpsonQx10_320 => new(4 * 2 * 16 * 256, 64, 2048, 2, false),
            DiskImageFormatIds.EpsonQx10_396 => new(4 * 16 * 256, 64, 2048, 2, false),
            DiskImageFormatIds.EpsonQx10_399 => new(16 * 256, 64, 2048, 2, false),
            DiskImageFormatIds.EpsonQx10_400 => new(2 * 2 * 5 * 1024, 64, 2048, 2, false),
            DiskImageFormatIds.EpsonQx10Logo => new(4 * 16 * 256, 64, 2048, 2, false),
            _ => null
        };
    }

    private sealed class ExtentKeyComparer : IEqualityComparer<(int User, string Name)>
    {
        public bool Equals((int User, string Name) x, (int User, string Name) y) => x.User == y.User && StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name);
        public int GetHashCode((int User, string Name) obj) => HashCode.Combine(obj.User, StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name));
    }
}
