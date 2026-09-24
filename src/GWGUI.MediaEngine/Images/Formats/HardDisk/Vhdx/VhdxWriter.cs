using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Images.Reading.Blocks;
using GWGUI.MediaEngine.Images.Models.Blocks;

namespace GWGUI.MediaEngine.Images.Formats.HardDisk.Vhdx;

/// <summary>Writes autonomous fixed and dynamic VHDX images.</summary>
public sealed class VhdxWriter : IMediaImageWriter
{
    private const long MetadataRegionOffset = HardDiskFormatConstants.VhdxOneMebibyte;
    private const long BatRegionOffset = 2L * HardDiskFormatConstants.VhdxOneMebibyte;
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { HardDiskImageFormatIds.Vhdx }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public VhdxWriter(IAtomicImageFileWriter? files = null)
    {
        this.files = files ?? new AtomicImageFileWriter();
    }

    public string Id => MediaImageWriterIds.HardDiskVhdx;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentationKinds;
    public IReadOnlySet<string> ProducedFileExtensions => VhdxFormat.Extensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return SupportedFormatIds.Contains(targetFormatId)
            && VhdxFormat.Extensions.Contains(targetExtension)
            && document.MediaKind == MediaKind.HardDisk
            && document.Representation is BlockMediaImageRepresentation blocks
            && blocks.Capacity > 0
            && VhdxFormat.LogicalSectorSizes.Contains(blocks.LogicalBlockSize)
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
            throw new InvalidDataException("The media document cannot be written as a complete VHDX image.");

        var fixedImage = SelectVariant(document, blocks) == HardDiskImageVariant.Fixed;
        await files.WriteAsync(
            outputPath,
            (output, token) => WriteContainerAsync(blocks, fixedImage, output, token),
            cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static async Task WriteContainerAsync(
        BlockMediaImageRepresentation blocks,
        bool fixedImage,
        Stream output,
        CancellationToken cancellationToken)
    {
        var blockSize = SelectBlockSize(blocks.Capacity);
        var payloadBlockCount = checked((blocks.Capacity + blockSize - 1) / blockSize);
        var chunkRatio = checked((1L << 23) * blocks.LogicalBlockSize / blockSize);
        var batEntryCount = checked(payloadBlockCount + (payloadBlockCount - 1) / chunkRatio);
        var batByteLength = checked(batEntryCount * sizeof(ulong));
        var batRegionLength = Align(batByteLength, HardDiskFormatConstants.VhdxOneMebibyte);
        if (batRegionLength > uint.MaxValue || batByteLength > int.MaxValue)
            throw new InvalidDataException("The VHDX BAT exceeds the supported in-memory table size.");
        var logOffset = checked(BatRegionOffset + batRegionLength);
        var payloadOffset = checked(logOffset + HardDiskFormatConstants.VhdxOneMebibyte);

        await WriteAtAsync(output, 0, CreateFileIdentifier(), cancellationToken).ConfigureAwait(false);
        var fileWriteGuid = Guid.NewGuid();
        var dataWriteGuid = Guid.NewGuid();
        await WriteAtAsync(output, HardDiskFormatConstants.VhdxHeader1Offset,
            CreateHeader(1, fileWriteGuid, dataWriteGuid, logOffset), cancellationToken).ConfigureAwait(false);
        await WriteAtAsync(output, HardDiskFormatConstants.VhdxHeader2Offset,
            CreateHeader(2, fileWriteGuid, dataWriteGuid, logOffset), cancellationToken).ConfigureAwait(false);
        var regionTable = CreateRegionTable((uint)batRegionLength);
        await WriteAtAsync(output, HardDiskFormatConstants.VhdxRegionTable1Offset, regionTable, cancellationToken).ConfigureAwait(false);
        await WriteAtAsync(output, HardDiskFormatConstants.VhdxRegionTable2Offset, regionTable, cancellationToken).ConfigureAwait(false);
        await WriteAtAsync(output, MetadataRegionOffset,
            CreateMetadata(blocks, blockSize, fixedImage), cancellationToken).ConfigureAwait(false);

        var bat = new byte[(int)batByteLength];
        var data = new byte[blockSize];
        for (long payloadIndex = 0; payloadIndex < payloadBlockCount; payloadIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            data.AsSpan().Clear();
            var address = checked(payloadIndex * blockSize);
            var logicalLength = (int)Math.Min(blockSize, blocks.Capacity - address);
            await BlockMediaDataReader.ReadExactlyAsync(
                blocks,
                address,
                data.AsMemory(0, logicalLength),
                cancellationToken).ConfigureAwait(false);
            var writePayload = fixedImage || data.AsSpan(0, logicalLength).IndexOfAnyExcept((byte)0) >= 0;
            var batIndex = checked(payloadIndex + payloadIndex / chunkRatio);
            if (!writePayload)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(
                    bat.AsSpan(checked((int)(batIndex * 8)), 8),
                    HardDiskFormatConstants.VhdxBatZero);
                continue;
            }

            var fileOffset = checked(payloadOffset + payloadIndex * blockSize);
            var entry = checked((ulong)fileOffset) | HardDiskFormatConstants.VhdxBatFullyPresent;
            BinaryPrimitives.WriteUInt64LittleEndian(bat.AsSpan(checked((int)(batIndex * 8)), 8), entry);
            await WriteAtAsync(output, fileOffset, data, cancellationToken).ConfigureAwait(false);
        }

        await WriteAtAsync(output, BatRegionOffset, bat, cancellationToken).ConfigureAwait(false);
        var minimumLength = checked(payloadOffset + (fixedImage ? payloadBlockCount * blockSize : 0));
        if (output.Length < minimumLength) output.SetLength(minimumLength);
    }

    private static byte[] CreateFileIdentifier()
    {
        var identifier = new byte[HardDiskFormatConstants.VhdxFileIdentifierSize];
        VhdxFormat.FileSignature.Span.CopyTo(identifier);
        System.Text.Encoding.Unicode.GetBytes("GW GUI MediaEngine").CopyTo(identifier, 8);
        return identifier;
    }

    private static byte[] CreateHeader(ulong sequence, Guid fileWriteGuid, Guid dataWriteGuid, long logOffset)
    {
        var header = new byte[HardDiskFormatConstants.VhdxHeaderSize];
        VhdxFormat.HeaderSignature.Span.CopyTo(header);
        BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(8, 8), sequence);
        fileWriteGuid.TryWriteBytes(header.AsSpan(16, 16));
        dataWriteGuid.TryWriteBytes(header.AsSpan(32, 16));
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(66, 2), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(68, 4), HardDiskFormatConstants.VhdxOneMebibyte);
        BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(72, 8), (ulong)logOffset);
        WriteCrc(header, 4);
        return header;
    }

    private static byte[] CreateRegionTable(uint batRegionLength)
    {
        var table = new byte[HardDiskFormatConstants.VhdxRegionTableSize];
        VhdxFormat.RegionTableSignature.Span.CopyTo(table);
        BinaryPrimitives.WriteUInt32LittleEndian(table.AsSpan(8, 4), 2);
        WriteRegionEntry(table.AsSpan(16, 32), HardDiskFormatConstants.VhdxBatRegionGuid, BatRegionOffset, batRegionLength, true);
        WriteRegionEntry(table.AsSpan(48, 32), HardDiskFormatConstants.VhdxMetadataRegionGuid,
            MetadataRegionOffset, HardDiskFormatConstants.VhdxOneMebibyte, true);
        WriteCrc(table, 4);
        return table;
    }

    private static byte[] CreateMetadata(BlockMediaImageRepresentation blocks, int blockSize, bool fixedImage)
    {
        var metadata = new byte[HardDiskFormatConstants.VhdxOneMebibyte];
        VhdxFormat.MetadataTableSignature.Span.CopyTo(metadata);
        BinaryPrimitives.WriteUInt16LittleEndian(metadata.AsSpan(10, 2), 5);
        const int valueOffset = 64 * 1_024;
        var cursor = valueOffset;
        WriteMetadataEntry(metadata.AsSpan(32, 32), HardDiskFormatConstants.VhdxFileParametersGuid, cursor, 8, 4);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata.AsSpan(cursor, 4), (uint)blockSize);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata.AsSpan(cursor + 4, 4),
            fixedImage ? HardDiskFormatConstants.VhdxFileParametersLeaveBlocksAllocated : 0);
        cursor += 8;
        WriteMetadataEntry(metadata.AsSpan(64, 32), HardDiskFormatConstants.VhdxVirtualDiskSizeGuid, cursor, 8, 6);
        BinaryPrimitives.WriteUInt64LittleEndian(metadata.AsSpan(cursor, 8), (ulong)blocks.Capacity);
        cursor += 8;
        WriteMetadataEntry(metadata.AsSpan(96, 32), HardDiskFormatConstants.VhdxPage83DataGuid, cursor, 16, 6);
        Guid.NewGuid().TryWriteBytes(metadata.AsSpan(cursor, 16));
        cursor += 16;
        WriteMetadataEntry(metadata.AsSpan(128, 32), HardDiskFormatConstants.VhdxLogicalSectorSizeGuid, cursor, 4, 6);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata.AsSpan(cursor, 4), (uint)blocks.LogicalBlockSize);
        cursor += 4;
        WriteMetadataEntry(metadata.AsSpan(160, 32), HardDiskFormatConstants.VhdxPhysicalSectorSizeGuid, cursor, 4, 6);
        BinaryPrimitives.WriteUInt32LittleEndian(metadata.AsSpan(cursor, 4), (uint)blocks.LogicalBlockSize);
        return metadata;
    }

    private static void WriteRegionEntry(Span<byte> entry, Guid id, long offset, uint length, bool required)
    {
        id.TryWriteBytes(entry[..16]);
        BinaryPrimitives.WriteUInt64LittleEndian(entry[16..24], (ulong)offset);
        BinaryPrimitives.WriteUInt32LittleEndian(entry[24..28], length);
        BinaryPrimitives.WriteUInt32LittleEndian(entry[28..32], required ? 1u : 0u);
    }

    private static void WriteMetadataEntry(Span<byte> entry, Guid id, int offset, int length, uint flags)
    {
        id.TryWriteBytes(entry[..16]);
        BinaryPrimitives.WriteUInt32LittleEndian(entry[16..20], (uint)offset);
        BinaryPrimitives.WriteUInt32LittleEndian(entry[20..24], (uint)length);
        BinaryPrimitives.WriteUInt32LittleEndian(entry[24..28], flags);
    }

    private static void WriteCrc(Span<byte> structure, int checksumOffset)
    {
        structure.Slice(checksumOffset, 4).Clear();
        BinaryPrimitives.WriteUInt32LittleEndian(structure.Slice(checksumOffset, 4), Crc32CFunctions.Compute(structure));
    }

    private static async Task WriteAtAsync(
        Stream output,
        long offset,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken)
    {
        output.Position = offset;
        await output.WriteAsync(data, cancellationToken).ConfigureAwait(false);
    }

    private static HardDiskImageVariant SelectVariant(MediaImageDocument document, BlockMediaImageRepresentation blocks)
    {
        if (document.FormatId.Equals(HardDiskImageFormatIds.Vhdx, StringComparison.OrdinalIgnoreCase)
            && document.Metadata.TryGetValue("variant", out var value)
            && Enum.TryParse<HardDiskImageVariant>(value, true, out var variant)
            && variant is HardDiskImageVariant.Fixed or HardDiskImageVariant.Dynamic)
            return variant;
        return blocks.Ranges.Any(range => range.Kind is MediaDataRangeKind.Zero or MediaDataRangeKind.Unallocated)
            ? HardDiskImageVariant.Dynamic
            : HardDiskImageVariant.Fixed;
    }

    private static int SelectBlockSize(long capacity)
    {
        var blockSize = HardDiskFormatConstants.VhdxDefaultBlockSize;
        while (capacity > (long)blockSize * uint.MaxValue && blockSize < 256 * HardDiskFormatConstants.VhdxOneMebibyte)
            blockSize *= 2;
        return blockSize;
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

    private static long Align(long value, int alignment) => checked((value + alignment - 1) / alignment * alignment);
}
