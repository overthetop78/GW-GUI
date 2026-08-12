using GWGUI.MediaEngine.Definitions;
using System.Buffers.Binary;
using System.Text;
using GWGUI.MediaEngine.FileSystems.Cpm;
using GWGUI.MediaEngine.Recognition.Amstrad;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Read-only CP/M directory reader for Amstrad CPC and PCW media.</summary>
public sealed class AmstradCpmFileSystemReader : IFileSystemReader
{
    public string Id => Definitions.FileSystemIds.AmstradCpm;
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.AmstradCpc, DiskImageFormatIds.AmstradPcw };

    public bool CanRead(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId)) return false;
        var bytes = Flatten(image);
        var layout = GetLayout(image, bytes);
        return layout is not null && CpmDirectoryProbe.LooksLikeDirectory(bytes, layout.Value,
            allowEmpty: image.FormatId.Equals(DiskImageFormatIds.AmstradPcw, StringComparison.OrdinalIgnoreCase));
    }

    public FileSystemVolume Read(SectorImage image)
    {
        var bytes = Flatten(image);
        var layout = GetLayout(image, bytes) ?? throw new InvalidDataException("The Amstrad CP/M disk layout is not supported.");
        if (!CpmDirectoryProbe.LooksLikeDirectory(bytes, layout,
                allowEmpty: image.FormatId.Equals(DiskImageFormatIds.AmstradPcw, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException("The image does not contain a supported Amstrad CP/M directory.");
        var extents = new List<Extent>();
        var warnings = new List<string>();
        string volumeName = string.Empty;
        for (var index = 0; index < layout.DirectoryEntries; index++)
        {
            var offset = layout.DirectoryOffset + index * 32;
            var entry = bytes.AsSpan(offset, 32);
            var user = entry[0];
            if (user == 0xe5) continue;
            if (user is 0x20 or 0x21)
            {
                if (user == 0x20) volumeName = DecodePart(entry[1..9]);
                continue;
            }
            if (user > 31 || !TryDecodeName(entry, out var name)) continue;
            extents.Add(new(user, name, entry[12] + (entry[14] << 5), entry[15], ReadAllocations(entry, layout.WideAllocations)));
        }

        var files = new List<FileSystemEntry>();
        foreach (var group in extents.GroupBy(extent => (extent.User, extent.Name), new ExtentKeyComparer()))
        {
            using var content = new MemoryStream();
            var valid = true;
            foreach (var extent in group.OrderBy(extent => extent.Number))
            {
                using var extentBytes = new MemoryStream();
                foreach (var allocation in extent.Allocations)
                {
                    if (allocation == 0) continue;
                    var blockOffset = layout.AllocationOrigin + allocation * layout.AllocationBlockSize;
                    if (blockOffset < 0 || blockOffset + layout.AllocationBlockSize > bytes.Length)
                    {
                        warnings.Add($"{group.Key.Name}: CP/M allocation block {allocation} is outside the image.");
                        valid = false;
                        continue;
                    }
                    extentBytes.Write(bytes, blockOffset, layout.AllocationBlockSize);
                }
                var used = Math.Min(extentBytes.Length, extent.RecordCount * 128L);
                content.Write(extentBytes.GetBuffer(), 0, checked((int)used));
            }
            files.Add(new(group.Key.Name, FileSystemEntryKind.File, content.Length, null, $"CP/M user {group.Key.User}",
                (uint)group.Key.User, -1, valid, [], content.ToArray()));
        }
        var usedBlocks = extents.SelectMany(extent => extent.Allocations).Where(block => block != 0).Distinct().Count();
        var totalBlocks = Math.Max(0, (bytes.Length - layout.AllocationOrigin) / layout.AllocationBlockSize);
        var freeBlocks = Math.Max(0, totalBlocks - usedBlocks - layout.DirectoryBlocks);
        var system = Definitions.FileSystemDisplayNames.AmstradCpm(image.FormatId);
        return new(volumeName, system, image.Capacity, freeBlocks * (long)layout.AllocationBlockSize, null, null,
            files.OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }

    private static CpmDirectoryLayout? GetLayout(SectorImage image, byte[] bytes)
    {
        if (image.FormatId.Equals(DiskImageFormatIds.AmstradCpc, StringComparison.OrdinalIgnoreCase))
        {
            var first = image.AvailableBlocks.OrderBy(block => block.LogicalBlock).FirstOrDefault();
            if (first is null) return null;
            var firstId = first.Address.Number;
            return firstId switch
            {
                >= 0xc1 and <= 0xc9 => new(0, 0, 64, 1024, 2, false),
                >= 0x41 and <= 0x49 => new(2 * 9 * 512, 2 * 9 * 512, 64, 1024, 2, false),
                _ => CpmDirectoryProbe.FindCpcRawDirectory(bytes)
            };
        }
        if (!PcwDiskSpecificationProbe.LooksLikePcwDiskSpecification(bytes)) return null;
        var sectorsPerTrack = bytes[3];
        var sectorSize = 128 << (bytes[4] & 7);
        var reservedTracks = bytes[5];
        var allocationSize = 128 << (bytes[6] & 7);
        var directoryBlocks = bytes[7];
        if (sectorsPerTrack is 0 or > 64 || sectorSize is < 128 or > 4096 || allocationSize is < 512 or > 16384 || directoryBlocks is 0 or > 16)
            return null;
        var origin = checked(reservedTracks * sectorsPerTrack * sectorSize);
        return new(origin, origin, directoryBlocks * allocationSize / 32, allocationSize, directoryBlocks,
            (bytes.Length - origin) / allocationSize > 255);
    }


    private static bool TryDecodeName(ReadOnlySpan<byte> entry, out string name)
    {
        name = string.Empty;
        for (var index = 1; index <= 11; index++)
        {
            var value = entry[index] & 0x7f;
            if (value != 0x20 && (value < 0x21 || value > 0x7e)) return false;
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

    private static IReadOnlyList<int> ReadAllocations(ReadOnlySpan<byte> entry, bool wide)
    {
        var result = new List<int>();
        if (wide) for (var index = 16; index < 32; index += 2) result.Add(BinaryPrimitives.ReadUInt16LittleEndian(entry[index..]));
        else for (var index = 16; index < 32; index++) result.Add(entry[index]);
        return result;
    }

    private static byte[] Flatten(SectorImage image)
    {
        using var output = new MemoryStream();
        foreach (var block in image.AvailableBlocks.OrderBy(block => block.LogicalBlock)) output.Write(block.Data.ToArray());
        return output.ToArray();
    }

    private readonly record struct Extent(int User, string Name, int Number, int RecordCount, IReadOnlyList<int> Allocations);
    private sealed class ExtentKeyComparer : IEqualityComparer<(int User, string Name)>
    {
        public bool Equals((int User, string Name) x, (int User, string Name) y) => x.User == y.User && StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name);
        public int GetHashCode((int User, string Name) obj) => HashCode.Combine(obj.User, StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name));
    }
}
