using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems.Atari.KFile;

/// <summary>Recognizes KBoot images and restores the single Atari executable stored after the loader.</summary>
public sealed class AtariKFileFileSystemReader : IFileSystemReader
{
    private const int BootSectorCount = 3;
    private const int PayloadLengthOffset = 9;
    private const int PayloadLengthByteCount = 3;
    private const int AtariSectorSize = 128;

    public string Id => FileSystemIds.AtariKFile;

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

    public bool CanRead(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId) || image.BlockCount <= BootSectorCount ||
            !image.TryGetBlock(0, out var boot) || boot.Data.Count < AtariSectorSize)
            return false;

        var header = boot.Data;
        if (header[0] != 0 || header[1] != BootSectorCount ||
            BinaryPrimitives.ReadUInt16LittleEndian(header.Skip(2).Take(2).ToArray()) != 0x0700 ||
            BinaryPrimitives.ReadUInt16LittleEndian(header.Skip(4).Take(2).ToArray()) != 0x0714 ||
            header[6] != 0x4c || header[7] != 0x14 || header[8] != 0x07)
            return false;

        var payloadLength = PayloadLength(header);
        if (payloadLength <= 0 || payloadLength > (long)(image.BlockCount - BootSectorCount) * image.BlockSize)
            return false;

        return TryReadPayload(image, payloadLength, out var payload) && IsAtariExecutable(payload);
    }

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw new InvalidDataException("The sector image is not a valid Atari K-file boot disk.");
        var boot = image.GetBlock(0).ToArray();
        var payloadLength = PayloadLength(boot);
        if (!TryReadPayload(image, payloadLength, out var payload))
            throw new InvalidDataException("The Atari K-file payload is incomplete.");

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["container"] = FileSystemIds.AtariKFile,
            ["payloadLength"] = payloadLength.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["bootSectorCount"] = BootSectorCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["loadAddress"] = "$0700",
            ["initAddress"] = "$0714"
        };
        var occupiedSize = (long)Math.Ceiling(payloadLength / (double)image.BlockSize) * image.BlockSize;
        var entry = new FileSystemEntry(
            "RUN.XEX",
            FileSystemEntryKind.File,
            payloadLength,
            null,
            string.Empty,
            0,
            BootSectorCount + 1,
            true,
            [],
            payload,
            nativeTypeId: "atari-xex",
            occupiedSize: occupiedSize,
            attributes: ["bootable"],
            dataValid: true,
            syntheticName: true,
            metadata: metadata);
        return new FileSystemVolume(
            FileSystemDisplayNames.AtariKFile,
            FileSystemIds.AtariKFile,
            image.Capacity,
            0,
            null,
            null,
            [entry],
            [],
            freeSpaceKnown: false,
            attributes: ["bootable", "single-file"],
            bootable: true);
    }

    private static int PayloadLength(IReadOnlyList<byte> header) =>
        header[PayloadLengthOffset] |
        header[PayloadLengthOffset + 1] << 8 |
        header[PayloadLengthOffset + 2] << 16;

    private static bool TryReadPayload(SectorImage image, int payloadLength, out byte[] payload)
    {
        payload = new byte[payloadLength];
        var written = 0;
        for (var logicalBlock = BootSectorCount; written < payloadLength; logicalBlock++)
        {
            if (logicalBlock >= image.BlockCount || !image.TryGetBlock(logicalBlock, out var block)) return false;
            var count = Math.Min(block.Data.Count, payloadLength - written);
            block.Data.Take(count).ToArray().CopyTo(payload, written);
            written += count;
        }
        return true;
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
}
