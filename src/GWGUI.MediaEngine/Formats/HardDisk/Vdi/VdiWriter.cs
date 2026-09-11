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

namespace GWGUI.MediaEngine.Formats.HardDisk.Vdi;

/// <summary>Writes autonomous fixed and dynamic VDI 1.1 images.</summary>
public sealed class VdiWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { HardDiskImageFormatIds.Vdi }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public VdiWriter(IAtomicImageFileWriter? files = null)
    {
        this.files = files ?? new AtomicImageFileWriter();
    }

    public string Id => MediaImageWriterIds.HardDiskVdi;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentationKinds;
    public IReadOnlySet<string> ProducedFileExtensions => VdiFormat.Extensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return SupportedFormatIds.Contains(targetFormatId)
            && VdiFormat.Extensions.Contains(targetExtension)
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
            throw new InvalidDataException("The media document cannot be written as a complete VDI image.");
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
        var blockSize = HardDiskFormatConstants.VdiDefaultBlockSize;
        var blockCountLong = checked((blocks.Capacity + blockSize - 1) / blockSize);
        if (blockCountLong > int.MaxValue) throw new InvalidDataException("The VDI block table is too large.");
        var blockCount = (int)blockCountLong;
        var mapStorageLength = Align(checked(blockCount * sizeof(uint)), HardDiskFormatConstants.LegacyLogicalSectorSize);
        var dataOffset = checked(HardDiskFormatConstants.VdiHeaderSize + mapStorageLength);
        var map = new uint[blockCount];
        Array.Fill(map, HardDiskFormatConstants.VdiUnallocatedBlock);
        await output.WriteAsync(new byte[dataOffset], cancellationToken).ConfigureAwait(false);
        var block = new byte[blockSize];
        var allocatedCount = 0;
        for (var logicalBlock = 0; logicalBlock < blockCount; logicalBlock++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            block.AsSpan().Clear();
            var address = checked((long)logicalBlock * blockSize);
            var logicalLength = (int)Math.Min(blockSize, blocks.Capacity - address);
            await BlockMediaDataReader.ReadExactlyAsync(
                blocks,
                address,
                block.AsMemory(0, logicalLength),
                cancellationToken).ConfigureAwait(false);
            if (!fixedImage && block.AsSpan(0, logicalLength).IndexOfAnyExcept((byte)0) < 0)
            {
                map[logicalBlock] = HardDiskFormatConstants.VdiDiscardedBlock;
                continue;
            }
            map[logicalBlock] = checked((uint)allocatedCount++);
            await output.WriteAsync(block, cancellationToken).ConfigureAwait(false);
        }

        var end = output.Position;
        output.Position = 0;
        var header = CreateHeader(blocks, fixedImage, dataOffset, blockCount, allocatedCount, blockSize);
        await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        var mapBytes = new byte[mapStorageLength];
        for (var index = 0; index < map.Length; index++)
            BinaryPrimitives.WriteUInt32LittleEndian(mapBytes.AsSpan(index * 4, 4), map[index]);
        await output.WriteAsync(mapBytes, cancellationToken).ConfigureAwait(false);
        output.Position = end;
    }

    private static byte[] CreateHeader(
        BlockMediaImageRepresentation blocks,
        bool fixedImage,
        int dataOffset,
        int blockCount,
        int allocatedCount,
        int blockSize)
    {
        var header = new byte[HardDiskFormatConstants.VdiHeaderSize];
        System.Text.Encoding.ASCII.GetBytes("<<< GW GUI Virtual Disk Image >>>\n").CopyTo(header, 0);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x40, 4), HardDiskFormatConstants.VdiSignature);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x44, 4), HardDiskFormatConstants.VdiVersion1_1);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x48, 4), 0x180);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x4C, 4),
            fixedImage ? HardDiskFormatConstants.VdiFixedImageType : HardDiskFormatConstants.VdiDynamicImageType);
        System.Text.Encoding.ASCII.GetBytes("GW GUI MediaEngine").CopyTo(header, 0x54);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x154, 4), HardDiskFormatConstants.VdiHeaderSize);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x158, 4), (uint)dataOffset);
        if (blocks.Geometry is { } geometry)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x15C, 4), checked((uint)geometry.Cylinders));
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x160, 4), checked((uint)geometry.Heads));
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x164, 4), checked((uint)geometry.SectorsPerTrack));
        }
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x168, 4), HardDiskFormatConstants.LegacyLogicalSectorSize);
        BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(0x170, 8), (ulong)blocks.Capacity);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x178, 4), (uint)blockSize);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x180, 4), (uint)blockCount);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0x184, 4), (uint)allocatedCount);
        Guid.NewGuid().TryWriteBytes(header.AsSpan(0x188, 16));
        Guid.NewGuid().TryWriteBytes(header.AsSpan(0x198, 16));
        return header;
    }

    private static HardDiskImageVariant SelectVariant(MediaImageDocument document, BlockMediaImageRepresentation blocks)
    {
        if (document.FormatId.Equals(HardDiskImageFormatIds.Vdi, StringComparison.OrdinalIgnoreCase)
            && document.Metadata.TryGetValue("variant", out var value)
            && Enum.TryParse<HardDiskImageVariant>(value, true, out var variant)
            && variant is HardDiskImageVariant.Fixed or HardDiskImageVariant.Dynamic)
            return variant;
        return blocks.Ranges.Any(range => range.Kind is MediaDataRangeKind.Zero or MediaDataRangeKind.Unallocated)
            ? HardDiskImageVariant.Dynamic
            : HardDiskImageVariant.Fixed;
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

    private static int Align(int value, int alignment) => checked((value + alignment - 1) / alignment * alignment);
}
