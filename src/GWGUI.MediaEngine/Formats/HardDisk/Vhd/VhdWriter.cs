using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Reading.Blocks;
using GWGUI.MediaEngine.Representations.Blocks;

namespace GWGUI.MediaEngine.Formats.HardDisk.Vhd;

/// <summary>Writes autonomous fixed and dynamic VHD images from complete block representations.</summary>
public sealed class VhdWriter : IMediaImageWriter
{
    private const uint FooterFeatures = 2;
    private const uint FormatVersion = 0x00010000;
    private const uint CreatorVersion = 0x00010000;
    private const uint CreatorHostOs = 0x5769326B;
    private const int DynamicTableOffset = HardDiskFormatConstants.VhdFooterSize + HardDiskFormatConstants.VhdDynamicHeaderSize;
    private static readonly DateTime VhdEpoch = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { HardDiskImageFormatIds.Vhd }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public VhdWriter(IAtomicImageFileWriter? files = null)
    {
        this.files = files ?? new AtomicImageFileWriter();
    }

    public string Id => MediaImageWriterIds.HardDiskVhd;

    public IReadOnlySet<string> FormatIds => SupportedFormatIds;

    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentationKinds;

    public IReadOnlySet<string> ProducedFileExtensions => VhdFormat.Extensions;

    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return SupportedFormatIds.Contains(targetFormatId)
            && VhdFormat.Extensions.Contains(targetExtension)
            && document.MediaKind == MediaKind.HardDisk
            && document.Representation is BlockMediaImageRepresentation blocks
            && blocks.Capacity > 0
            && blocks.LogicalBlockSize == HardDiskFormatConstants.LegacyLogicalSectorSize
            && HasCompleteReadableCoverage(blocks);
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        if (!CanWrite(document, targetFormatId, extension)
            || document.Representation is not BlockMediaImageRepresentation blocks)
            throw new InvalidDataException("The media document cannot be written as a complete VHD image.");

        var variant = SelectVariant(document, blocks);
        await files.WriteAsync(
            outputPath,
            variant == HardDiskImageVariant.Dynamic
                ? (output, token) => WriteDynamicAsync(blocks, output, token)
                : (output, token) => WriteFixedAsync(blocks, output, token),
            cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static HardDiskImageVariant SelectVariant(
        MediaImageDocument document,
        BlockMediaImageRepresentation blocks)
    {
        if (document.FormatId.Equals(HardDiskImageFormatIds.Vhd, StringComparison.OrdinalIgnoreCase)
            && document.Metadata.TryGetValue("variant", out var value)
            && Enum.TryParse<HardDiskImageVariant>(value, true, out var variant)
            && variant is HardDiskImageVariant.Fixed or HardDiskImageVariant.Dynamic)
            return variant;

        return blocks.Ranges.Any(range => range.Kind is MediaDataRangeKind.Zero or MediaDataRangeKind.Unallocated)
            ? HardDiskImageVariant.Dynamic
            : HardDiskImageVariant.Fixed;
    }

    private static async Task WriteFixedAsync(
        BlockMediaImageRepresentation blocks,
        Stream output,
        CancellationToken cancellationToken)
    {
        await WriteLogicalDataAsync(blocks, output, cancellationToken).ConfigureAwait(false);
        var footer = CreateFooter(blocks, HardDiskFormatConstants.VhdFixedDiskType, ulong.MaxValue, Guid.NewGuid());
        await output.WriteAsync(footer, cancellationToken).ConfigureAwait(false);
    }

    private static async Task WriteDynamicAsync(
        BlockMediaImageRepresentation blocks,
        Stream output,
        CancellationToken cancellationToken)
    {
        var blockSize = HardDiskFormatConstants.VhdDefaultDynamicBlockSize;
        var maximumEntriesLong = checked((blocks.Capacity + blockSize - 1) / blockSize);
        if (maximumEntriesLong > uint.MaxValue)
            throw new InvalidDataException("The hard disk is too large for the VHD dynamic BAT.");
        var maximumEntries = (uint)maximumEntriesLong;
        var batLength = checked((int)maximumEntries * sizeof(uint));
        var paddedBatLength = Align(batLength, HardDiskFormatConstants.LegacyLogicalSectorSize);
        var uniqueId = Guid.NewGuid();
        var footer = CreateFooter(
            blocks,
            HardDiskFormatConstants.VhdDynamicDiskType,
            HardDiskFormatConstants.VhdFooterSize,
            uniqueId);
        var header = CreateDynamicHeader(maximumEntries, blockSize);

        await output.WriteAsync(footer, cancellationToken).ConfigureAwait(false);
        await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        var batOffset = output.Position;
        await output.WriteAsync(new byte[paddedBatLength], cancellationToken).ConfigureAwait(false);

        var bat = new uint[maximumEntries];
        Array.Fill(bat, HardDiskFormatConstants.VhdUnusedBatEntry);
        var data = new byte[blockSize];
        var sectorsPerBlock = blockSize / HardDiskFormatConstants.LegacyLogicalSectorSize;
        var bitmapLength = (sectorsPerBlock + 7) / 8;
        var bitmapStorageLength = Align(bitmapLength, HardDiskFormatConstants.LegacyLogicalSectorSize);
        var bitmap = new byte[bitmapStorageLength];
        for (var blockIndex = 0; blockIndex < maximumEntries; blockIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            data.AsSpan().Clear();
            var address = checked((long)blockIndex * blockSize);
            var logicalLength = (int)Math.Min(blockSize, blocks.Capacity - address);
            await BlockMediaDataReader.ReadExactlyAsync(
                blocks,
                address,
                data.AsMemory(0, logicalLength),
                cancellationToken).ConfigureAwait(false);
            if (data.AsSpan(0, logicalLength).IndexOfAnyExcept((byte)0) < 0) continue;

            if (output.Position % HardDiskFormatConstants.LegacyLogicalSectorSize != 0)
                throw new InvalidDataException("The dynamic VHD block is not sector aligned.");
            var sectorOffset = output.Position / HardDiskFormatConstants.LegacyLogicalSectorSize;
            if (sectorOffset > uint.MaxValue)
                throw new InvalidDataException("The dynamic VHD block offset exceeds the BAT range.");
            bat[blockIndex] = (uint)sectorOffset;

            bitmap.AsSpan().Clear();
            var logicalSectorCount = (logicalLength + HardDiskFormatConstants.LegacyLogicalSectorSize - 1)
                / HardDiskFormatConstants.LegacyLogicalSectorSize;
            for (var sectorIndex = 0; sectorIndex < logicalSectorCount; sectorIndex++)
                bitmap[sectorIndex / 8] |= (byte)(0x80 >> (sectorIndex % 8));
            await output.WriteAsync(bitmap, cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(data, cancellationToken).ConfigureAwait(false);
        }

        await output.WriteAsync(footer, cancellationToken).ConfigureAwait(false);
        var end = output.Position;
        output.Position = batOffset;
        var batBytes = new byte[paddedBatLength];
        for (var index = 0; index < bat.Length; index++)
            BinaryPrimitives.WriteUInt32BigEndian(batBytes.AsSpan(index * sizeof(uint), sizeof(uint)), bat[index]);
        await output.WriteAsync(batBytes, cancellationToken).ConfigureAwait(false);
        output.Position = end;
    }

    private static async Task WriteLogicalDataAsync(
        BlockMediaImageRepresentation blocks,
        Stream output,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * DataSizeConstants.BytesPerKibibyte];
        long address = 0;
        while (address < blocks.Capacity)
        {
            var count = (int)Math.Min(buffer.Length, blocks.Capacity - address);
            await BlockMediaDataReader.ReadExactlyAsync(
                blocks,
                address,
                buffer.AsMemory(0, count),
                cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            address += count;
        }
    }

    private static byte[] CreateFooter(
        BlockMediaImageRepresentation blocks,
        uint diskType,
        ulong dataOffset,
        Guid uniqueId)
    {
        var footer = new byte[HardDiskFormatConstants.VhdFooterSize];
        VhdFormat.FooterSignature.Span.CopyTo(footer);
        BinaryPrimitives.WriteUInt32BigEndian(footer.AsSpan(8, 4), FooterFeatures);
        BinaryPrimitives.WriteUInt32BigEndian(footer.AsSpan(12, 4), FormatVersion);
        BinaryPrimitives.WriteUInt64BigEndian(footer.AsSpan(16, 8), dataOffset);
        var seconds = Math.Clamp((long)(DateTime.UtcNow - VhdEpoch).TotalSeconds, 0, uint.MaxValue);
        BinaryPrimitives.WriteUInt32BigEndian(footer.AsSpan(24, 4), (uint)seconds);
        System.Text.Encoding.ASCII.GetBytes("GWGU").CopyTo(footer, 28);
        BinaryPrimitives.WriteUInt32BigEndian(footer.AsSpan(32, 4), CreatorVersion);
        BinaryPrimitives.WriteUInt32BigEndian(footer.AsSpan(36, 4), CreatorHostOs);
        BinaryPrimitives.WriteUInt64BigEndian(footer.AsSpan(40, 8), (ulong)blocks.Capacity);
        BinaryPrimitives.WriteUInt64BigEndian(footer.AsSpan(48, 8), (ulong)blocks.Capacity);
        var geometry = CalculateGeometry(blocks.Capacity / HardDiskFormatConstants.LegacyLogicalSectorSize);
        BinaryPrimitives.WriteUInt16BigEndian(footer.AsSpan(56, 2), geometry.Cylinders);
        footer[58] = geometry.Heads;
        footer[59] = geometry.SectorsPerTrack;
        BinaryPrimitives.WriteUInt32BigEndian(footer.AsSpan(60, 4), diskType);
        uniqueId.TryWriteBytes(footer.AsSpan(68, 16), bigEndian: true, out _);
        WriteChecksum(footer, 64);
        return footer;
    }

    private static byte[] CreateDynamicHeader(uint maximumEntries, int blockSize)
    {
        var header = new byte[HardDiskFormatConstants.VhdDynamicHeaderSize];
        VhdFormat.DynamicHeaderSignature.Span.CopyTo(header);
        BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(8, 8), ulong.MaxValue);
        BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(16, 8), DynamicTableOffset);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(24, 4), FormatVersion);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(28, 4), maximumEntries);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(32, 4), (uint)blockSize);
        WriteChecksum(header, 36);
        return header;
    }

    private static (ushort Cylinders, byte Heads, byte SectorsPerTrack) CalculateGeometry(long totalSectors)
    {
        var sectors = Math.Min(totalSectors, 65535L * 16 * 255);
        int sectorsPerTrack;
        int heads;
        long cylindersTimesHeads;
        if (sectors >= 65535L * 16 * 63)
        {
            sectorsPerTrack = 255;
            heads = 16;
            cylindersTimesHeads = sectors / sectorsPerTrack;
        }
        else
        {
            sectorsPerTrack = 17;
            cylindersTimesHeads = sectors / sectorsPerTrack;
            heads = (int)((cylindersTimesHeads + 1023) / 1024);
            if (heads < 4) heads = 4;
            if (cylindersTimesHeads >= heads * 1024 || heads > 16)
            {
                sectorsPerTrack = 31;
                heads = 16;
                cylindersTimesHeads = sectors / sectorsPerTrack;
            }
            if (cylindersTimesHeads >= heads * 1024)
            {
                sectorsPerTrack = 63;
                heads = 16;
                cylindersTimesHeads = sectors / sectorsPerTrack;
            }
        }

        var cylinders = Math.Min(cylindersTimesHeads / heads, ushort.MaxValue);
        return ((ushort)cylinders, (byte)heads, (byte)sectorsPerTrack);
    }

    private static bool HasCompleteReadableCoverage(BlockMediaImageRepresentation blocks)
    {
        long cursor = 0;
        foreach (var range in blocks.Ranges)
        {
            if (range.Address != cursor || range.Kind == MediaDataRangeKind.Unavailable) return false;
            cursor = checked(cursor + range.Length);
        }
        return cursor == blocks.Capacity;
    }

    private static void WriteChecksum(Span<byte> data, int checksumOffset)
    {
        data.Slice(checksumOffset, sizeof(uint)).Clear();
        uint sum = 0;
        foreach (var value in data) sum += value;
        BinaryPrimitives.WriteUInt32BigEndian(data.Slice(checksumOffset, sizeof(uint)), ~sum);
    }

    private static int Align(int value, int alignment) => checked((value + alignment - 1) / alignment * alignment);
}
