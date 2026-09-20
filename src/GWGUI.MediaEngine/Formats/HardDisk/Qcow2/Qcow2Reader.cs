using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Blocks;

namespace GWGUI.MediaEngine.Formats.HardDisk.Qcow2;

/// <summary>Reads autonomous unencrypted and uncompressed QCOW2 version 2 and 3 images.</summary>
public sealed class Qcow2Reader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { HardDiskImageFormatIds.Qcow2 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.HardDisk }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => Qcow2Format.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [Qcow2Format.Signature];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        try
        {
            await ReadDocumentAsync(context, false, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (InvalidDataException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
        => ReadDocumentAsync(context, true, cancellationToken);

    private static async Task<MediaImageDocument> ReadDocumentAsync(
        MediaRecognitionContext context,
        bool includeRanges,
        CancellationToken cancellationToken)
    {
        var header = await ReadHeaderAsync(context, cancellationToken).ConfigureAwait(false);
        var refcounts = await Qcow2RefcountReader.CreateAsync(context, header, cancellationToken).ConfigureAwait(false);
        await refcounts.RequireAllocatedAsync(0, cancellationToken).ConfigureAwait(false);
        var l1Length = checked((int)header.L1Size * sizeof(ulong));
        var l1 = await context.ReadAsync(header.L1TableOffset, l1Length, cancellationToken).ConfigureAwait(false);
        var source = includeRanges ? new FileRandomAccessData(context.Source.PrimaryPath) : null;
        var ranges = new List<MediaDataRange>();
        var virtualClusterCount = checked((header.VirtualSize + header.ClusterSize - 1) / header.ClusterSize);
        var entriesPerL2 = header.ClusterSize / sizeof(ulong);
        var l2Cache = new Dictionary<long, ReadOnlyMemory<byte>>();
        for (long virtualCluster = 0; virtualCluster < virtualClusterCount; virtualCluster++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var l1Index = virtualCluster / entriesPerL2;
            if (l1Index >= header.L1Size) throw new InvalidDataException("The QCOW2 L1 table does not cover the virtual disk.");
            var l1Entry = BinaryPrimitives.ReadUInt64BigEndian(l1.Span.Slice(checked((int)l1Index * 8), 8));
            var address = checked(virtualCluster * header.ClusterSize);
            var length = Math.Min((long)header.ClusterSize, header.VirtualSize - address);
            if (l1Entry == 0)
            {
                AddRange(ranges, address, length, MediaDataRangeKind.Unallocated);
                continue;
            }
            var l2Offset = GetClusterOffset(l1Entry, header.ClusterSize, "L1");
            await refcounts.RequireAllocatedAsync(l2Offset, cancellationToken).ConfigureAwait(false);
            if (!l2Cache.TryGetValue(l2Offset, out var l2))
            {
                l2 = await context.ReadAsync(l2Offset, header.ClusterSize, cancellationToken).ConfigureAwait(false);
                l2Cache.Add(l2Offset, l2);
            }
            var l2Index = virtualCluster % entriesPerL2;
            var l2Entry = BinaryPrimitives.ReadUInt64BigEndian(l2.Span.Slice(checked((int)l2Index * 8), 8));
            if ((l2Entry & HardDiskFormatConstants.Qcow2CompressedFlag) != 0)
                throw new NotSupportedException("Compressed QCOW2 clusters are not supported.");
            if ((l2Entry & HardDiskFormatConstants.Qcow2ZeroFlag) != 0)
            {
                AddRange(ranges, address, length, MediaDataRangeKind.Zero);
                continue;
            }
            var dataOffset = (long)(l2Entry & HardDiskFormatConstants.Qcow2ClusterOffsetMask);
            if (dataOffset == 0)
            {
                AddRange(ranges, address, length, MediaDataRangeKind.Unallocated);
                continue;
            }
            ValidateClusterOffset(dataOffset, header.ClusterSize, context.Length, "L2 data");
            await refcounts.RequireAllocatedAsync(dataOffset, cancellationToken).ConfigureAwait(false);
            AddRange(
                ranges,
                address,
                length,
                includeRanges ? MediaDataRangeKind.Stored : MediaDataRangeKind.Unavailable,
                source,
                includeRanges ? dataOffset : 0);
        }

        return new MediaImageDocument(
            context.Source,
            HardDiskImageFormatIds.Qcow2,
            MediaKind.HardDisk,
            new BlockMediaImageRepresentation(
                header.VirtualSize,
                HardDiskFormatConstants.LegacyLogicalSectorSize,
                ranges),
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["version"] = header.Version.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["variant"] = HardDiskImageVariant.Sparse.ToString(),
                ["clusterSize"] = header.ClusterSize.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static async ValueTask<Qcow2Header> ReadHeaderAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        if (context.Length < HardDiskFormatConstants.Qcow2Version2HeaderSize)
            throw new InvalidDataException("The file is too short to contain a QCOW2 header.");
        var memory = await context.ReadAsync(
            0,
            (int)Math.Min(context.Length, HardDiskFormatConstants.Qcow2Version3HeaderSize),
            cancellationToken).ConfigureAwait(false);
        var header = memory.Span;
        if (BinaryPrimitives.ReadUInt32BigEndian(header[..4]) != HardDiskFormatConstants.Qcow2Magic)
            throw new InvalidDataException("The QCOW2 signature is missing.");
        var version = BinaryPrimitives.ReadUInt32BigEndian(header[4..8]);
        if (!Qcow2Format.Versions.Contains(version)) throw new NotSupportedException($"QCOW2 version {version} is unsupported.");
        var backingOffset = BinaryPrimitives.ReadUInt64BigEndian(header[8..16]);
        var backingSize = BinaryPrimitives.ReadUInt32BigEndian(header[16..20]);
        if (backingOffset != 0 || backingSize != 0)
            throw new NotSupportedException("QCOW2 backing files require a parent resolver that is not registered.");
        var clusterBits = BinaryPrimitives.ReadUInt32BigEndian(header[20..24]);
        if (clusterBits is < HardDiskFormatConstants.Qcow2MinimumClusterBits or > HardDiskFormatConstants.Qcow2MaximumClusterBits)
            throw new InvalidDataException("The QCOW2 cluster size is invalid.");
        var clusterSize = 1 << (int)clusterBits;
        var virtualSizeUnsigned = BinaryPrimitives.ReadUInt64BigEndian(header[24..32]);
        if (virtualSizeUnsigned == 0 || virtualSizeUnsigned > long.MaxValue
            || virtualSizeUnsigned % HardDiskFormatConstants.LegacyLogicalSectorSize != 0)
            throw new InvalidDataException("The QCOW2 virtual disk size is invalid.");
        if (BinaryPrimitives.ReadUInt32BigEndian(header[32..36]) != 0)
            throw new NotSupportedException("Encrypted QCOW2 images are not supported.");
        var l1Size = BinaryPrimitives.ReadUInt32BigEndian(header[36..40]);
        var l1OffsetUnsigned = BinaryPrimitives.ReadUInt64BigEndian(header[40..48]);
        var refcountOffsetUnsigned = BinaryPrimitives.ReadUInt64BigEndian(header[48..56]);
        var refcountClusters = BinaryPrimitives.ReadUInt32BigEndian(header[56..60]);
        var snapshotCount = BinaryPrimitives.ReadUInt32BigEndian(header[60..64]);
        var snapshotsOffset = BinaryPrimitives.ReadUInt64BigEndian(header[64..72]);
        if (snapshotCount != 0 || snapshotsOffset != 0)
            throw new NotSupportedException("QCOW2 snapshots are not supported.");
        if (l1Size == 0 || l1OffsetUnsigned > long.MaxValue || refcountOffsetUnsigned > long.MaxValue || refcountClusters == 0)
            throw new InvalidDataException("The QCOW2 table geometry is invalid.");
        var l1Offset = (long)l1OffsetUnsigned;
        var refcountOffset = (long)refcountOffsetUnsigned;
        ValidateClusterOffset(l1Offset, clusterSize, context.Length, "L1 table");
        ValidateClusterOffset(refcountOffset, clusterSize, context.Length, "refcount table");
        var l1Bytes = checked((long)l1Size * sizeof(ulong));
        if (l1Offset > context.Length - l1Bytes
            || refcountClusters > int.MaxValue / clusterSize
            || refcountOffset > context.Length - (long)refcountClusters * clusterSize)
            throw new InvalidDataException("A QCOW2 table exceeds the file.");

        var refcountOrder = HardDiskFormatConstants.Qcow2DefaultRefcountOrder;
        if (version == HardDiskFormatConstants.Qcow2Version3)
        {
            if (header.Length < HardDiskFormatConstants.Qcow2Version3HeaderSize)
                throw new InvalidDataException("The QCOW2 version 3 header is truncated.");
            var incompatibleFeatures = BinaryPrimitives.ReadUInt64BigEndian(header[72..80]);
            if ((incompatibleFeatures & ~HardDiskFormatConstants.Qcow2KnownIncompatibleFeatures) != 0)
                throw new NotSupportedException($"QCOW2 incompatible feature bits 0x{incompatibleFeatures:X} are unsupported.");
            if (BinaryPrimitives.ReadUInt64BigEndian(header[88..96]) != 0)
                throw new NotSupportedException("QCOW2 autoclear feature bits are unsupported.");
            refcountOrder = checked((int)BinaryPrimitives.ReadUInt32BigEndian(header[96..100]));
            var headerLength = BinaryPrimitives.ReadUInt32BigEndian(header[100..104]);
            if (headerLength < HardDiskFormatConstants.Qcow2Version3HeaderSize || headerLength > clusterSize)
                throw new InvalidDataException("The QCOW2 header length is invalid.");
        }
        if (refcountOrder != HardDiskFormatConstants.Qcow2DefaultRefcountOrder)
            throw new NotSupportedException($"QCOW2 refcount order {refcountOrder} is unsupported.");
        return new Qcow2Header(
            version,
            clusterSize,
            (long)virtualSizeUnsigned,
            l1Size,
            l1Offset,
            refcountOffset,
            refcountClusters);
    }

    private static long GetClusterOffset(ulong entry, int clusterSize, string table)
    {
        if ((entry & HardDiskFormatConstants.Qcow2CompressedFlag) != 0)
            throw new NotSupportedException($"Compressed QCOW2 {table} entries are unsupported.");
        var offset = (long)(entry & HardDiskFormatConstants.Qcow2ClusterOffsetMask);
        if (offset == 0 || offset % clusterSize != 0)
            throw new InvalidDataException($"The QCOW2 {table} cluster offset is invalid.");
        return offset;
    }

    private static void ValidateClusterOffset(long offset, int clusterSize, long fileLength, string structure)
    {
        if (offset < 0 || offset % clusterSize != 0 || offset > fileLength - clusterSize)
            throw new InvalidDataException($"The QCOW2 {structure} cluster is outside the file.");
    }

    private static void AddRange(
        List<MediaDataRange> ranges,
        long address,
        long length,
        MediaDataRangeKind kind,
        FileRandomAccessData? source = null,
        long sourceOffset = 0)
    {
        if (ranges.Count > 0)
        {
            var previous = ranges[^1];
            var sourceContinues = kind != MediaDataRangeKind.Stored
                || ReferenceEquals(previous.Source, source) && previous.SourceOffset + previous.Length == sourceOffset;
            if (previous.Kind == kind && previous.Address + previous.Length == address && sourceContinues)
            {
                ranges[^1] = new MediaDataRange(previous.Address, previous.Length + length, kind, previous.Source, previous.SourceOffset);
                return;
            }
        }
        ranges.Add(new MediaDataRange(address, length, kind, source, sourceOffset));
    }

    private sealed record Qcow2Header(
        uint Version,
        int ClusterSize,
        long VirtualSize,
        uint L1Size,
        long L1TableOffset,
        long RefcountTableOffset,
        uint RefcountTableClusters);

    private sealed class Qcow2RefcountReader
    {
        private readonly MediaRecognitionContext context;
        private readonly Qcow2Header header;
        private readonly ReadOnlyMemory<byte> table;
        private readonly Dictionary<long, ReadOnlyMemory<byte>> blocks = [];

        private Qcow2RefcountReader(MediaRecognitionContext context, Qcow2Header header, ReadOnlyMemory<byte> table)
        {
            this.context = context;
            this.header = header;
            this.table = table;
        }

        public static async Task<Qcow2RefcountReader> CreateAsync(
            MediaRecognitionContext context,
            Qcow2Header header,
            CancellationToken cancellationToken)
        {
            var length = checked((int)header.RefcountTableClusters * header.ClusterSize);
            var table = await context.ReadAsync(header.RefcountTableOffset, length, cancellationToken).ConfigureAwait(false);
            return new Qcow2RefcountReader(context, header, table);
        }

        public async ValueTask RequireAllocatedAsync(long clusterOffset, CancellationToken cancellationToken)
        {
            ValidateClusterOffset(clusterOffset, header.ClusterSize, context.Length, "referenced");
            var clusterIndex = clusterOffset / header.ClusterSize;
            var entriesPerBlock = header.ClusterSize / sizeof(ushort);
            var tableIndex = clusterIndex / entriesPerBlock;
            if (tableIndex >= table.Length / sizeof(ulong))
                throw new InvalidDataException("The QCOW2 refcount table does not cover a referenced cluster.");
            var blockOffset = (long)(BinaryPrimitives.ReadUInt64BigEndian(
                table.Span.Slice(checked((int)tableIndex * 8), 8)) & HardDiskFormatConstants.Qcow2ClusterOffsetMask);
            if (blockOffset == 0) throw new InvalidDataException("A referenced QCOW2 cluster has no refcount block.");
            ValidateClusterOffset(blockOffset, header.ClusterSize, context.Length, "refcount block");
            if (!blocks.TryGetValue(blockOffset, out var block))
            {
                block = await context.ReadAsync(blockOffset, header.ClusterSize, cancellationToken).ConfigureAwait(false);
                blocks.Add(blockOffset, block);
            }
            var entryIndex = clusterIndex % entriesPerBlock;
            if (BinaryPrimitives.ReadUInt16BigEndian(block.Span.Slice(checked((int)entryIndex * 2), 2)) == 0)
                throw new InvalidDataException("A referenced QCOW2 cluster has a zero refcount.");
        }
    }
}
