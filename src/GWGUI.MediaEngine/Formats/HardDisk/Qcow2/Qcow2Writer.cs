using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Reading.Blocks;
using GWGUI.MediaEngine.Representations.Blocks;

namespace GWGUI.MediaEngine.Formats.HardDisk.Qcow2;

/// <summary>Writes autonomous QCOW2 version 2 and 3 images without compression or parents.</summary>
public sealed class Qcow2Writer : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { HardDiskImageFormatIds.Qcow2 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public Qcow2Writer(IAtomicImageFileWriter? files = null)
    {
        this.files = files ?? new AtomicImageFileWriter();
    }

    public string Id => MediaImageWriterIds.HardDiskQcow2;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentationKinds;
    public IReadOnlySet<string> ProducedFileExtensions => Qcow2Format.Extensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return SupportedFormatIds.Contains(targetFormatId)
            && Qcow2Format.Extensions.Contains(targetExtension)
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
            throw new InvalidDataException("The media document cannot be written as a supported QCOW2 image.");
        var version = SelectVersion(document);
        await files.WriteAsync(
            outputPath,
            (output, token) => WriteContainerAsync(blocks, version, output, token),
            cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static async Task WriteContainerAsync(
        BlockMediaImageRepresentation blocks,
        uint version,
        Stream output,
        CancellationToken cancellationToken)
    {
        var clusterBits = HardDiskFormatConstants.Qcow2DefaultClusterBits;
        var clusterSize = 1 << clusterBits;
        var virtualClusterCount = checked((blocks.Capacity + clusterSize - 1) / clusterSize);
        var entriesPerL2 = clusterSize / sizeof(ulong);
        var l2TableCount = checked((virtualClusterCount + entriesPerL2 - 1) / entriesPerL2);
        if (l2TableCount > uint.MaxValue) throw new InvalidDataException("The QCOW2 L1 table is too large.");
        var l1Size = checked((uint)l2TableCount);
        var l1Clusters = checked((l2TableCount * sizeof(ulong) + clusterSize - 1) / clusterSize);
        var entriesPerRefcountBlock = clusterSize / sizeof(ushort);
        var refcountBlockCount = 1L;
        var refcountTableClusters = 1L;
        while (true)
        {
            var maximumClusters = checked(1 + refcountTableClusters + refcountBlockCount + l1Clusters + l2TableCount + virtualClusterCount);
            var requiredBlocks = checked((maximumClusters + entriesPerRefcountBlock - 1) / entriesPerRefcountBlock);
            var requiredTableClusters = checked((requiredBlocks * sizeof(ulong) + clusterSize - 1) / clusterSize);
            if (requiredBlocks == refcountBlockCount && requiredTableClusters == refcountTableClusters) break;
            refcountBlockCount = requiredBlocks;
            refcountTableClusters = requiredTableClusters;
        }
        if (refcountTableClusters > uint.MaxValue
            || refcountTableClusters * clusterSize > int.MaxValue
            || refcountBlockCount * clusterSize > int.MaxValue)
            throw new InvalidDataException("The QCOW2 refcount structures are too large.");

        var refcountTableCluster = 1L;
        var refcountBlocksCluster = refcountTableCluster + refcountTableClusters;
        var l1Cluster = refcountBlocksCluster + refcountBlockCount;
        var l2Cluster = l1Cluster + l1Clusters;
        var dataCluster = l2Cluster + l2TableCount;
        output.SetLength(checked(dataCluster * clusterSize));

        var l1 = new byte[checked((int)(l1Clusters * clusterSize))];
        for (long tableIndex = 0; tableIndex < l2TableCount; tableIndex++)
        {
            var tableOffset = checked((l2Cluster + tableIndex) * clusterSize);
            BinaryPrimitives.WriteUInt64BigEndian(
                l1.AsSpan(checked((int)tableIndex * 8), 8),
                (ulong)tableOffset | HardDiskFormatConstants.Qcow2CopiedFlag);
        }
        await WriteAtAsync(output, checked(l1Cluster * clusterSize), l1, cancellationToken).ConfigureAwait(false);

        var data = new byte[clusterSize];
        long allocatedDataClusters = 0;
        for (long tableIndex = 0; tableIndex < l2TableCount; tableIndex++)
        {
            var l2 = new byte[clusterSize];
            for (var entryIndex = 0; entryIndex < entriesPerL2; entryIndex++)
            {
                var virtualCluster = checked(tableIndex * entriesPerL2 + entryIndex);
                if (virtualCluster >= virtualClusterCount) break;
                data.AsSpan().Clear();
                var address = checked(virtualCluster * clusterSize);
                var logicalLength = (int)Math.Min(clusterSize, blocks.Capacity - address);
                await BlockMediaDataReader.ReadExactlyAsync(
                    blocks,
                    address,
                    data.AsMemory(0, logicalLength),
                    cancellationToken).ConfigureAwait(false);
                ulong entry;
                if (data.AsSpan(0, logicalLength).IndexOfAnyExcept((byte)0) < 0)
                {
                    entry = HardDiskFormatConstants.Qcow2ZeroFlag;
                }
                else
                {
                    var fileCluster = checked(dataCluster + allocatedDataClusters++);
                    var fileOffset = checked(fileCluster * clusterSize);
                    entry = (ulong)fileOffset | HardDiskFormatConstants.Qcow2CopiedFlag;
                    await WriteAtAsync(output, fileOffset, data, cancellationToken).ConfigureAwait(false);
                }
                BinaryPrimitives.WriteUInt64BigEndian(l2.AsSpan(entryIndex * 8, 8), entry);
            }
            await WriteAtAsync(output, checked((l2Cluster + tableIndex) * clusterSize), l2, cancellationToken).ConfigureAwait(false);
        }

        var usedClusterCount = checked(dataCluster + allocatedDataClusters);
        output.SetLength(checked(usedClusterCount * clusterSize));
        var refcountTable = new byte[checked((int)(refcountTableClusters * clusterSize))];
        var refcountBlocks = new byte[checked((int)(refcountBlockCount * clusterSize))];
        for (long blockIndex = 0; blockIndex < refcountBlockCount; blockIndex++)
        {
            var blockOffset = checked((refcountBlocksCluster + blockIndex) * clusterSize);
            BinaryPrimitives.WriteUInt64BigEndian(
                refcountTable.AsSpan(checked((int)blockIndex * 8), 8),
                (ulong)blockOffset);
        }
        for (long clusterIndex = 0; clusterIndex < usedClusterCount; clusterIndex++)
            BinaryPrimitives.WriteUInt16BigEndian(refcountBlocks.AsSpan(checked((int)clusterIndex * 2), 2), 1);
        await WriteAtAsync(output, checked(refcountTableCluster * clusterSize), refcountTable, cancellationToken).ConfigureAwait(false);
        await WriteAtAsync(output, checked(refcountBlocksCluster * clusterSize), refcountBlocks, cancellationToken).ConfigureAwait(false);

        var header = CreateHeader(
            version,
            clusterBits,
            blocks.Capacity,
            l1Size,
            checked(l1Cluster * clusterSize),
            checked(refcountTableCluster * clusterSize),
            checked((uint)refcountTableClusters));
        await WriteAtAsync(output, 0, header, cancellationToken).ConfigureAwait(false);
    }

    private static byte[] CreateHeader(
        uint version,
        int clusterBits,
        long virtualSize,
        uint l1Size,
        long l1Offset,
        long refcountTableOffset,
        uint refcountTableClusters)
    {
        var headerSize = version == HardDiskFormatConstants.Qcow2Version3
            ? HardDiskFormatConstants.Qcow2Version3HeaderSize
            : HardDiskFormatConstants.Qcow2Version2HeaderSize;
        var header = new byte[headerSize];
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(0, 4), HardDiskFormatConstants.Qcow2Magic);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4, 4), version);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(20, 4), (uint)clusterBits);
        BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(24, 8), (ulong)virtualSize);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(36, 4), l1Size);
        BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(40, 8), (ulong)l1Offset);
        BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(48, 8), (ulong)refcountTableOffset);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(56, 4), refcountTableClusters);
        if (version == HardDiskFormatConstants.Qcow2Version3)
        {
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(96, 4), HardDiskFormatConstants.Qcow2DefaultRefcountOrder);
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(100, 4), (uint)headerSize);
        }
        return header;
    }

    private static uint SelectVersion(MediaImageDocument document)
    {
        if (document.FormatId.Equals(HardDiskImageFormatIds.Qcow2, StringComparison.OrdinalIgnoreCase)
            && document.Metadata.TryGetValue("version", out var value)
            && uint.TryParse(value, out var version)
            && Qcow2Format.Versions.Contains(version))
            return version;
        return HardDiskFormatConstants.Qcow2Version3;
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
}
