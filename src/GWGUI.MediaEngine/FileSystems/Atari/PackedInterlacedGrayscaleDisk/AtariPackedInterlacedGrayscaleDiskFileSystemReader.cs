using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems.Atari.PackedInterlacedGrayscaleDisk;

/// <summary>Reads Atari disks containing fixed records of packed five-bit, two-frame grayscale images.</summary>
public sealed class AtariPackedInterlacedGrayscaleDiskFileSystemReader : IFileSystemReader
{
    private const int SectorSize = 128;
    private const int SectorCount = 1040;
    private const int SectorsPerImage = 79;
    private const int ImageLength = 10_000;
    private const int RecordLength = SectorsPerImage * SectorSize;
    private const int PaddingLength = RecordLength - ImageLength;
    private const int ImagesPerSide = 12;
    private const int LoaderFirstSector = 2;
    private const int LoaderLastSector = 36;
    private const ushort LoaderTableAddress = 0x0f5e;
    private static readonly byte[] SideMarker = [0xd3, 0xcc, 0xc9, 0xc4, 0xc5, 0xd3, 0xc8, 0xd7];
    private static readonly int[] SideMarkerOffsets = [119, 120];
    private static readonly int[] SideBStartSectors = [2, 81, 160, 239, 362, 441, 520, 599, 678, 757, 836, 915];

    public string Id => FileSystemIds.AtariPackedInterlacedGrayscaleDisk;

    public IReadOnlySet<string> CatalogFormatIds { get; } = new[]
    {
        DiskImageFormatIds.Atari130,
        DiskImageFormatIds.AtariXfd130
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public bool CanRead(SectorImage image) => TryReadLayout(image, out _);

    public FileSystemVolume Read(SectorImage image)
    {
        if (!TryReadLayout(image, out var layout))
            throw new InvalidDataException("The sector image is not an Atari packed interlaced grayscale disk.");

        var entries = new List<FileSystemEntry>();
        if (layout.Loader is not null)
        {
            var boot = image.GetBlock(0).ToArray();
            entries.Add(new FileSystemEntry(
                "BOOT.BIN",
                FileSystemEntryKind.File,
                boot.Length,
                null,
                "Atari 8-bit boot stream",
                0,
                1,
                true,
                [],
                boot,
                nativeTypeId: "atari-boot-stream",
                occupiedSize: boot.Length,
                attributes: ["bootable"],
                dataValid: true,
                syntheticName: true,
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["container"] = FileSystemIds.AtariPackedInterlacedGrayscaleDisk,
                    ["loadAddress"] = "$0480",
                    ["bootSectorCount"] = "1"
                }));

            entries.Add(new FileSystemEntry(
                "LOADER.XEX",
                FileSystemEntryKind.File,
                layout.Loader.Length,
                null,
                "Atari executable loader",
                0,
                LoaderFirstSector,
                true,
                [],
                layout.Loader,
                nativeTypeId: "atari-xex",
                occupiedSize: (LoaderLastSector - LoaderFirstSector + 1L) * SectorSize,
                attributes: ["bootable"],
                dataValid: true,
                syntheticName: true,
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["container"] = FileSystemIds.AtariPackedInterlacedGrayscaleDisk,
                    ["firstSector"] = LoaderFirstSector.ToString(CultureInfo.InvariantCulture),
                    ["lastSector"] = LoaderLastSector.ToString(CultureInfo.InvariantCulture),
                    ["entryPoint"] = "$05F3"
                }));
        }

        for (var index = 0; index < layout.Images.Count; index++)
        {
            var imageRecord = layout.Images[index];
            entries.Add(new FileSystemEntry(
                $"IMAGE-{index + 1:D4}.BIN",
                FileSystemEntryKind.File,
                imageRecord.Content.Length,
                null,
                "Packed 5-bit interlaced grayscale image",
                0,
                imageRecord.FirstSector,
                true,
                [],
                imageRecord.Content,
                nativeTypeId: "atari-packed-5bit-interlaced-grayscale-image",
                occupiedSize: RecordLength,
                attributes: ["image", "packed-5-bit", "two-frame"],
                dataValid: true,
                syntheticName: true,
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["container"] = FileSystemIds.AtariPackedInterlacedGrayscaleDisk,
                    ["firstSector"] = imageRecord.FirstSector.ToString(CultureInfo.InvariantCulture),
                    ["sectorCount"] = SectorsPerImage.ToString(CultureInfo.InvariantCulture),
                    ["packedLength"] = ImageLength.ToString(CultureInfo.InvariantCulture),
                    ["paddingLength"] = PaddingLength.ToString(CultureInfo.InvariantCulture),
                    ["packedBitsPerPixel"] = "5",
                    ["lineCount"] = "200",
                    ["packedBytesPerLine"] = "50",
                    ["frameCount"] = "2",
                    ["decodedBytesPerLinePerFrame"] = "40"
                }));
        }

        return new FileSystemVolume(
            FileSystemDisplayNames.AtariPackedInterlacedGrayscaleDisk,
            FileSystemIds.AtariPackedInterlacedGrayscaleDisk,
            image.Capacity,
            0,
            null,
            null,
            entries,
            [],
            freeSpaceKnown: false,
            attributes: ["raw-data", "packed-images", layout.Loader is null ? "data-side" : "bootable-side"],
            bootable: layout.Loader is not null);
    }

    private static bool TryReadLayout(SectorImage image, out DiskLayout layout)
    {
        layout = new DiskLayout(null, []);
        if (!CatalogMatches(image) || image.BlockCount != SectorCount || image.BlockSize != SectorSize)
            return false;

        if (TryReadSideA(image, out var loader, out var sideAStarts) &&
            TryReadImages(image, sideAStarts, out var sideAImages))
        {
            layout = new DiskLayout(loader, sideAImages);
            return true;
        }

        if (HasSideBMarker(image) && TryReadImages(image, SideBStartSectors, out var sideBImages))
        {
            layout = new DiskLayout(null, sideBImages);
            return true;
        }

        return false;
    }

    private static bool TryReadSideA(SectorImage image, out byte[] loader, out IReadOnlyList<int> startSectors)
    {
        loader = [];
        startSectors = [];
        var boot = image.GetBlock(0).Span;
        if (boot.Length != SectorSize || boot[1] != 1 ||
            BinaryPrimitives.ReadUInt16LittleEndian(boot[2..4]) != 0x0480)
            return false;

        var stream = new List<byte>((LoaderLastSector - LoaderFirstSector + 1) * SectorSize);
        for (var sector = LoaderFirstSector; sector <= LoaderLastSector; sector++)
        {
            if (!image.TryGetBlock(sector - 1, out var block) || block.Data.Count != SectorSize)
                return false;
            stream.AddRange(block.Data);
        }

        if (!TryParseExecutable(stream, out loader, out var memory) || !HasPackedImageLoaderStructure(memory))
            return false;

        var starts = ReadSectorTable(memory, LoaderTableAddress);
        if (starts.Count != ImagesPerSide * 2 + 2 || starts[ImagesPerSide] != 0 || starts[^1] != 0)
            return false;
        var sideA = starts.Take(ImagesPerSide).ToArray();
        var sideB = starts.Skip(ImagesPerSide + 1).Take(ImagesPerSide).ToArray();
        if (!sideB.SequenceEqual(SideBStartSectors)) return false;
        startSectors = sideA;
        return true;
    }

    private static bool TryParseExecutable(
        IReadOnlyList<byte> stream,
        out byte[] executable,
        out byte[] memory)
    {
        executable = [];
        memory = new byte[ushort.MaxValue + 1];
        if (stream.Count < 8 || ReadUInt16(stream, 0) != 0xffff) return false;

        var offset = 2;
        var segmentCount = 0;
        while (offset + 1 < stream.Count)
        {
            while (offset + 1 < stream.Count && ReadUInt16(stream, offset) == 0xffff) offset += 2;
            if (offset + 1 >= stream.Count) return false;
            if (ReadUInt16(stream, offset) == 0)
            {
                executable = stream.Take(offset).ToArray();
                return segmentCount >= 2;
            }
            if (offset + 4 > stream.Count) return false;
            var start = ReadUInt16(stream, offset);
            var end = ReadUInt16(stream, offset + 2);
            if (end < start) return false;
            offset += 4;
            var length = end - start + 1;
            if (offset + length > stream.Count) return false;
            for (var index = 0; index < length; index++) memory[start + index] = stream[offset + index];
            offset += length;
            segmentCount++;
        }
        return false;
    }

    private static bool HasPackedImageLoaderStructure(IReadOnlyList<byte> memory) =>
        Matches(memory, 0x05f3, [0x20, 0xc8, 0x0a]) &&
        Matches(memory, 0x05fc, [0x20, 0x52, 0x09]) &&
        Matches(memory, 0x081a, [0xa9, 0x00, 0x85, 0x84, 0x85, 0x86, 0x85, 0x82, 0xa9, 0x90, 0x85, 0x85, 0xa9, 0xb0, 0x85, 0x87, 0xa9, 0xd8, 0x85, 0x83, 0xa9, 0xc8, 0x85, 0x90]) &&
        Matches(memory, 0x0881, [0xc0, 0x28]) &&
        Matches(memory, 0x089a, [0x18, 0xa5, 0x82, 0x69, 0x32, 0x85, 0x82]) &&
        Matches(memory, 0x0952, [0xa5, 0x96, 0xe6, 0x96, 0x0a, 0xaa, 0xbd, 0x5e, 0x0f, 0x85, 0x52, 0x1d, 0x5f, 0x0f]) &&
        Matches(memory, 0x097f, [0xa9, 0x4f, 0x85, 0x90]) &&
        Matches(memory, 0x094a, SideMarker);

    private static IReadOnlyList<int> ReadSectorTable(IReadOnlyList<byte> memory, int offset)
    {
        var values = new List<int>();
        for (var index = 0; index < ImagesPerSide * 2 + 2; index++)
            values.Add(ReadUInt16(memory, offset + index * 2));
        return values;
    }

    private static bool HasSideBMarker(SectorImage image)
    {
        var sector = image.GetBlock(0).Span;
        if (sector.Length != SectorSize) return false;
        foreach (var offset in SideMarkerOffsets)
            if (sector.Slice(offset, SideMarker.Length).SequenceEqual(SideMarker)) return true;
        return false;
    }

    private static bool TryReadImages(
        SectorImage image,
        IReadOnlyList<int> startSectors,
        out IReadOnlyList<ImageRecord> images)
    {
        images = [];
        if (startSectors.Count != ImagesPerSide) return false;
        var result = new List<ImageRecord>(ImagesPerSide);
        foreach (var firstSector in startSectors)
        {
            if (firstSector < 1 || firstSector + SectorsPerImage - 1 > image.BlockCount) return false;
            var record = new byte[RecordLength];
            for (var index = 0; index < SectorsPerImage; index++)
            {
                if (!image.TryGetBlock(firstSector + index - 1, out var block) || block.Data.Count != SectorSize)
                    return false;
                block.Data.ToArray().CopyTo(record, index * SectorSize);
            }
            if (record.AsSpan(ImageLength, PaddingLength).ContainsAnyExcept((byte)0) ||
                !record.AsSpan(0, ImageLength).ContainsAnyExcept((byte)0))
                return false;
            result.Add(new ImageRecord(firstSector, record.AsSpan(0, ImageLength).ToArray()));
        }
        images = result;
        return true;
    }

    private static bool CatalogMatches(SectorImage image) =>
        image.FormatId.Equals(DiskImageFormatIds.Atari130, StringComparison.OrdinalIgnoreCase) ||
        image.FormatId.Equals(DiskImageFormatIds.AtariXfd130, StringComparison.OrdinalIgnoreCase);

    private static bool Matches(IReadOnlyList<byte> data, int offset, IReadOnlyList<byte> expected)
    {
        if (offset < 0 || offset + expected.Count > data.Count) return false;
        for (var index = 0; index < expected.Count; index++)
            if (data[offset + index] != expected[index]) return false;
        return true;
    }

    private static ushort ReadUInt16(IReadOnlyList<byte> data, int offset) =>
        (ushort)(data[offset] | data[offset + 1] << 8);

    private sealed record ImageRecord(int FirstSector, byte[] Content);
    private sealed record DiskLayout(byte[]? Loader, IReadOnlyList<ImageRecord> Images);
}
