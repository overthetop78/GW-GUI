using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems.Atari.Boot;

/// <summary>Recognizes an Atari 8-bit boot stream stored in consecutive disk sectors.</summary>
public sealed class AtariBootFileSystemReader : IFileSystemReader
{
    private const int BootHeaderLength = 6;

    public string Id => FileSystemIds.AtariBootDisk;

    public IReadOnlySet<string> CatalogFormatIds { get; } = new[]
    {
        DiskImageFormatIds.Atari90,
        DiskImageFormatIds.Atari130,
        DiskImageFormatIds.Atari140,
        DiskImageFormatIds.Atari180,
        DiskImageFormatIds.AtariAtx,
        DiskImageFormatIds.AtariXfd90,
        DiskImageFormatIds.AtariXfd130,
        DiskImageFormatIds.AtariXfd140,
        DiskImageFormatIds.AtariXfd180
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public bool CanRead(SectorImage image) => TryReadBootStream(image, out _, out _, out _);

    public FileSystemVolume Read(SectorImage image)
    {
        if (!TryReadBootStream(image, out var content, out var loadAddress, out var initAddress))
            throw new InvalidDataException("The sector image does not contain a valid Atari boot stream.");

        var sectorCount = image.GetBlock(0).Span[1];
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["container"] = FileSystemIds.AtariBootDisk,
            ["bootFlags"] = $"${image.GetBlock(0).Span[0]:X2}",
            ["bootSectorCount"] = sectorCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["loadAddress"] = $"${loadAddress:X4}",
            ["initAddress"] = $"${initAddress:X4}"
        };
        var entries = new List<FileSystemEntry>();
        entries.Add(new FileSystemEntry(
            "BOOT.BIN",
            FileSystemEntryKind.File,
            content.Length,
            null,
            "Atari 8-bit boot stream",
            0,
            1,
            true,
            [],
            content,
            nativeTypeId: "atari-boot-stream",
            occupiedSize: content.Length,
            attributes: ["bootable"],
            dataValid: true,
            syntheticName: true,
            metadata: metadata));

        var ordinal = 1;
        foreach (var executable in FindLinkedExecutables(image, sectorCount))
        {
            var executableMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["container"] = FileSystemIds.AtariBootDisk,
                ["firstSector"] = executable.FirstSector.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["sectorCount"] = executable.SectorCount.ToString(System.Globalization.CultureInfo.InvariantCulture)
            };
            entries.Add(new FileSystemEntry(
                $"RUN-{ordinal:D4}.XEX",
                FileSystemEntryKind.File,
                executable.Content.Length,
                null,
                string.Empty,
                0,
                executable.FirstSector,
                true,
                [],
                executable.Content,
                nativeTypeId: "atari-xex",
                occupiedSize: (long)executable.SectorCount * image.BlockSize,
                attributes: ["bootable"],
                dataValid: true,
                syntheticName: true,
                metadata: executableMetadata));
            ordinal++;
        }
        return new FileSystemVolume(
            FileSystemDisplayNames.AtariBootDisk,
            FileSystemIds.AtariBootDisk,
            image.Capacity,
            0,
            null,
            null,
            entries,
            [],
            freeSpaceKnown: false,
            attributes: entries.Count == 1 ? ["bootable", "single-file"] : ["bootable"],
            bootable: true);
    }

    private static bool TryReadBootStream(
        SectorImage image,
        out byte[] content,
        out ushort loadAddress,
        out ushort initAddress)
    {
        content = [];
        loadAddress = 0;
        initAddress = 0;
        if (!image.FormatId.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) ||
            !image.TryGetBlock(0, out var firstBlock) || firstBlock.Data.Count < BootHeaderLength)
            return false;

        var header = firstBlock.Data;
        var sectorCount = header[1];
        if (sectorCount == 0 || sectorCount > image.BlockCount) return false;

        loadAddress = BinaryPrimitives.ReadUInt16LittleEndian(header.Skip(2).Take(2).ToArray());
        initAddress = BinaryPrimitives.ReadUInt16LittleEndian(header.Skip(4).Take(2).ToArray());
        if (loadAddress < 0x0200) return false;

        var blocks = new List<byte[]>(sectorCount);
        var byteCount = 0;
        for (var logicalBlock = 0; logicalBlock < sectorCount; logicalBlock++)
        {
            if (!image.TryGetBlock(logicalBlock, out var block) || block.Data.Count == 0) return false;
            var bytes = block.Data.ToArray();
            blocks.Add(bytes);
            byteCount = checked(byteCount + bytes.Length);
        }
        if ((long)loadAddress + byteCount > 0x10000 ||
            !blocks.SelectMany(block => block).Skip(BootHeaderLength).Any(value => value != 0)) return false;

        content = new byte[byteCount];
        var offset = 0;
        foreach (var block in blocks)
        {
            block.CopyTo(content, offset);
            offset += block.Length;
        }
        return true;
    }

    private static IReadOnlyList<LinkedExecutable> FindLinkedExecutables(SectorImage image, int bootSectorCount)
    {
        var executables = new List<LinkedExecutable>();
        var claimedSectors = new HashSet<int>();
        for (var logicalBlock = bootSectorCount; logicalBlock < image.BlockCount; logicalBlock++)
        {
            var firstSector = logicalBlock + 1;
            if (claimedSectors.Contains(firstSector)
                || !image.TryGetBlock(logicalBlock, out var block)
                || block.Data.Count < 2
                || block.Data[0] != 0xff
                || block.Data[1] != 0xff
                || !TryReadLinkedExecutable(image, firstSector, out var executable, out var sectors))
                continue;

            foreach (var sector in sectors) claimedSectors.Add(sector);
            executables.Add(new LinkedExecutable(executable, firstSector, sectors.Count));
        }
        return executables;
    }

    private static bool TryReadLinkedExecutable(
        SectorImage image,
        int firstSector,
        out byte[] content,
        out IReadOnlyCollection<int> sectors)
    {
        var bytes = new List<byte>();
        var visited = new HashSet<int>();
        var current = firstSector;
        while (current != 0 && visited.Count < image.BlockCount)
        {
            if (!visited.Add(current)
                || !image.TryGetBlock(current - 1, out var block)
                || block.Data.Count < 3)
            {
                content = [];
                sectors = [];
                return false;
            }

            var linkOffset = block.Data.Count - 3;
            var used = block.Data[linkOffset + 2];
            if (used > linkOffset)
            {
                content = [];
                sectors = [];
                return false;
            }
            bytes.AddRange(block.Data.Take(used));
            current = (block.Data[linkOffset] & 3) << 8 | block.Data[linkOffset + 1];
            if (current > image.BlockCount)
            {
                content = [];
                sectors = [];
                return false;
            }
        }

        content = bytes.ToArray();
        sectors = visited;
        return current == 0 && IsAtariExecutable(content);
    }

    private static bool IsAtariExecutable(IReadOnlyList<byte> data)
    {
        if (data.Count < 8 || ReadUInt16(data, 0) != 0xffff) return false;
        var offset = 2;
        var segmentCount = 0;
        while (offset < data.Count)
        {
            while (offset + 1 < data.Count && ReadUInt16(data, offset) == 0xffff) offset += 2;
            if (offset + 4 > data.Count) return false;
            var start = ReadUInt16(data, offset);
            var end = ReadUInt16(data, offset + 2);
            if (end < start) return false;
            offset += 4;
            var length = end - start + 1;
            if (offset + length > data.Count) return false;
            offset += length;
            segmentCount++;
        }
        return segmentCount > 0;
    }

    private static int ReadUInt16(IReadOnlyList<byte> data, int offset) => data[offset] | data[offset + 1] << 8;

    private sealed record LinkedExecutable(byte[] Content, int FirstSector, int SectorCount);
}
