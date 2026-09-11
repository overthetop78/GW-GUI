using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Blocks;

namespace GWGUI.MediaEngine.Formats.HardDisk.Vhdx;

/// <summary>Reads autonomous VHDX images after validating their active structures.</summary>
public sealed class VhdxReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { HardDiskImageFormatIds.Vhdx }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.HardDisk }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> SupportedSignatures = [VhdxFormat.FileSignature];

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => VhdxFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => SupportedSignatures;
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        try
        {
            await ReadStructuresAsync(context, cancellationToken).ConfigureAwait(false);
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

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        var structures = await ReadStructuresAsync(context, cancellationToken).ConfigureAwait(false);
        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        if (source.Length != context.Length) throw new InvalidDataException("The VHDX length changed after recognition.");
        var ranges = await ReadBatRangesAsync(context, source, structures, cancellationToken).ConfigureAwait(false);
        var representation = new BlockMediaImageRepresentation(
            structures.VirtualDiskSize,
            structures.LogicalSectorSize,
            ranges);
        var variant = (structures.FileParametersFlags & HardDiskFormatConstants.VhdxFileParametersLeaveBlocksAllocated) != 0
            ? HardDiskImageVariant.Fixed
            : HardDiskImageVariant.Dynamic;
        return new MediaImageDocument(
            context.Source,
            HardDiskImageFormatIds.Vhdx,
            MediaKind.HardDisk,
            representation,
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["variant"] = variant.ToString(),
                ["page83Id"] = structures.Page83Id.ToString("D"),
                ["physicalSectorSize"] = structures.PhysicalSectorSize.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static async Task<VhdxStructures> ReadStructuresAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        if (context.Length < HardDiskFormatConstants.VhdxRegionTable2Offset + HardDiskFormatConstants.VhdxRegionTableSize)
            throw new InvalidDataException("The file is too short to contain mandatory VHDX structures.");
        var identifier = await context.ReadAsync(0, VhdxFormat.FileSignature.Length, cancellationToken).ConfigureAwait(false);
        if (!identifier.Span.SequenceEqual(VhdxFormat.FileSignature.Span))
            throw new InvalidDataException("The VHDX file signature is missing.");

        var header1 = await TryReadHeaderAsync(context, HardDiskFormatConstants.VhdxHeader1Offset, cancellationToken).ConfigureAwait(false);
        var header2 = await TryReadHeaderAsync(context, HardDiskFormatConstants.VhdxHeader2Offset, cancellationToken).ConfigureAwait(false);
        var header = SelectHeader(header1, header2);
        if (header.LogGuid != Guid.Empty)
            throw new NotSupportedException("A VHDX image with an active log must be replayed before it can be read.");

        var region1 = await TryReadRegionTableAsync(context, HardDiskFormatConstants.VhdxRegionTable1Offset, cancellationToken).ConfigureAwait(false);
        var region2 = await TryReadRegionTableAsync(context, HardDiskFormatConstants.VhdxRegionTable2Offset, cancellationToken).ConfigureAwait(false);
        var regions = region1 ?? region2 ?? throw new InvalidDataException("Both VHDX region tables are invalid.");
        if (!regions.TryGetValue(HardDiskFormatConstants.VhdxBatRegionGuid, out var batRegion)
            || !regions.TryGetValue(HardDiskFormatConstants.VhdxMetadataRegionGuid, out var metadataRegion))
            throw new InvalidDataException("The VHDX BAT or metadata region is missing.");

        var metadata = await ReadMetadataAsync(context, metadataRegion, cancellationToken).ConfigureAwait(false);
        if ((metadata.FileParametersFlags & HardDiskFormatConstants.VhdxFileParametersHasParent) != 0)
            throw new NotSupportedException("Differencing VHDX images require a parent resolver that is not registered.");
        return new VhdxStructures(
            batRegion,
            metadata.BlockSize,
            metadata.FileParametersFlags,
            metadata.VirtualDiskSize,
            metadata.LogicalSectorSize,
            metadata.PhysicalSectorSize,
            metadata.Page83Id);
    }

    private static async ValueTask<VhdxHeader?> TryReadHeaderAsync(
        MediaRecognitionContext context,
        long offset,
        CancellationToken cancellationToken)
    {
        var memory = await context.ReadAsync(offset, HardDiskFormatConstants.VhdxHeaderSize, cancellationToken).ConfigureAwait(false);
        var header = memory.Span;
        if (!header[..4].SequenceEqual(VhdxFormat.HeaderSignature.Span)) return null;
        if (!HasValidCrc(header, 4)) return null;
        if (BinaryPrimitives.ReadUInt16LittleEndian(header[66..68]) != 1) return null;
        return new VhdxHeader(
            BinaryPrimitives.ReadUInt64LittleEndian(header[8..16]),
            new Guid(header[48..64]));
    }

    private static VhdxHeader SelectHeader(VhdxHeader? first, VhdxHeader? second)
    {
        if (first is null && second is null) throw new InvalidDataException("Both VHDX headers are invalid.");
        if (first is null) return second!;
        if (second is null) return first;
        return first.SequenceNumber >= second.SequenceNumber ? first : second;
    }

    private static async ValueTask<Dictionary<Guid, VhdxRegion>> TryReadRegionTableAsync(
        MediaRecognitionContext context,
        long offset,
        CancellationToken cancellationToken)
    {
        var memory = await context.ReadAsync(offset, HardDiskFormatConstants.VhdxRegionTableSize, cancellationToken).ConfigureAwait(false);
        var table = memory.Span;
        if (!table[..4].SequenceEqual(VhdxFormat.RegionTableSignature.Span) || !HasValidCrc(table, 4)) return null!;
        var entryCount = BinaryPrimitives.ReadUInt32LittleEndian(table[8..12]);
        if (entryCount > HardDiskFormatConstants.VhdxMaximumRegionEntries) return null!;
        var regions = new Dictionary<Guid, VhdxRegion>();
        for (var index = 0; index < entryCount; index++)
        {
            var entry = table.Slice(16 + index * 32, 32);
            var id = new Guid(entry[..16]);
            var fileOffsetUnsigned = BinaryPrimitives.ReadUInt64LittleEndian(entry[16..24]);
            var length = BinaryPrimitives.ReadUInt32LittleEndian(entry[24..28]);
            var required = (BinaryPrimitives.ReadUInt32LittleEndian(entry[28..32]) & 1) != 0;
            if (fileOffsetUnsigned > long.MaxValue || length == 0) return null!;
            var fileOffset = (long)fileOffsetUnsigned;
            if (fileOffset % HardDiskFormatConstants.VhdxOneMebibyte != 0 || fileOffset > context.Length - length) return null!;
            if (id != HardDiskFormatConstants.VhdxBatRegionGuid
                && id != HardDiskFormatConstants.VhdxMetadataRegionGuid)
            {
                if (required) throw new NotSupportedException($"Required VHDX region {id:D} is not supported.");
                continue;
            }
            if (!regions.TryAdd(id, new VhdxRegion(fileOffset, length))) return null!;
        }
        return regions;
    }

    private static async Task<VhdxMetadata> ReadMetadataAsync(
        MediaRecognitionContext context,
        VhdxRegion region,
        CancellationToken cancellationToken)
    {
        var header = await context.ReadAsync(region.Offset, 64 * 1_024, cancellationToken).ConfigureAwait(false);
        if (!header.Span[..8].SequenceEqual(VhdxFormat.MetadataTableSignature.Span))
            throw new InvalidDataException("The VHDX metadata table signature is missing.");
        var entryCount = BinaryPrimitives.ReadUInt16LittleEndian(header.Span[10..12]);
        if (entryCount > 2_047 || 32 + entryCount * 32 > header.Length)
            throw new InvalidDataException("The VHDX metadata entry count is invalid.");

        uint? blockSize = null;
        uint? fileFlags = null;
        ulong? virtualSize = null;
        uint? logicalSectorSize = null;
        uint? physicalSectorSize = null;
        Guid? page83Id = null;
        var known = new HashSet<Guid>
        {
            HardDiskFormatConstants.VhdxFileParametersGuid,
            HardDiskFormatConstants.VhdxVirtualDiskSizeGuid,
            HardDiskFormatConstants.VhdxPage83DataGuid,
            HardDiskFormatConstants.VhdxLogicalSectorSizeGuid,
            HardDiskFormatConstants.VhdxPhysicalSectorSizeGuid
        };
        for (var index = 0; index < entryCount; index++)
        {
            var entry = header.Span.Slice(32 + index * 32, 32);
            var id = new Guid(entry[..16]);
            var itemOffset = BinaryPrimitives.ReadUInt32LittleEndian(entry[16..20]);
            var itemLength = BinaryPrimitives.ReadUInt32LittleEndian(entry[20..24]);
            var flags = BinaryPrimitives.ReadUInt32LittleEndian(entry[24..28]);
            if (itemOffset > region.Length || itemLength > region.Length - itemOffset)
                throw new InvalidDataException($"VHDX metadata item {id:D} exceeds its region.");
            if (!known.Contains(id))
            {
                if ((flags & 4) != 0) throw new NotSupportedException($"Required VHDX metadata {id:D} is not supported.");
                continue;
            }
            var item = await context.ReadAsync(region.Offset + itemOffset, checked((int)itemLength), cancellationToken).ConfigureAwait(false);
            if (id == HardDiskFormatConstants.VhdxFileParametersGuid && item.Length >= 8)
            {
                blockSize = BinaryPrimitives.ReadUInt32LittleEndian(item.Span[..4]);
                fileFlags = BinaryPrimitives.ReadUInt32LittleEndian(item.Span[4..8]);
            }
            else if (id == HardDiskFormatConstants.VhdxVirtualDiskSizeGuid && item.Length == 8)
                virtualSize = BinaryPrimitives.ReadUInt64LittleEndian(item.Span);
            else if (id == HardDiskFormatConstants.VhdxLogicalSectorSizeGuid && item.Length == 4)
                logicalSectorSize = BinaryPrimitives.ReadUInt32LittleEndian(item.Span);
            else if (id == HardDiskFormatConstants.VhdxPhysicalSectorSizeGuid && item.Length == 4)
                physicalSectorSize = BinaryPrimitives.ReadUInt32LittleEndian(item.Span);
            else if (id == HardDiskFormatConstants.VhdxPage83DataGuid && item.Length == 16)
                page83Id = new Guid(item.Span);
        }

        if (blockSize is null || fileFlags is null || virtualSize is null || logicalSectorSize is null
            || physicalSectorSize is null || page83Id is null)
            throw new InvalidDataException("Mandatory VHDX metadata is missing.");
        if (blockSize < HardDiskFormatConstants.VhdxOneMebibyte
            || blockSize > 256 * HardDiskFormatConstants.VhdxOneMebibyte
            || !IsPowerOfTwo(blockSize.Value))
            throw new InvalidDataException("The VHDX payload block size is invalid.");
        if (!VhdxFormat.LogicalSectorSizes.Contains(checked((int)logicalSectorSize.Value))
            || !VhdxFormat.LogicalSectorSizes.Contains(checked((int)physicalSectorSize.Value)))
            throw new InvalidDataException("The VHDX sector size is unsupported.");
        if (virtualSize == 0 || virtualSize > long.MaxValue || virtualSize % logicalSectorSize != 0)
            throw new InvalidDataException("The VHDX virtual disk size is invalid.");
        return new VhdxMetadata(
            checked((int)blockSize.Value),
            fileFlags.Value,
            (long)virtualSize.Value,
            checked((int)logicalSectorSize.Value),
            checked((int)physicalSectorSize.Value),
            page83Id.Value);
    }

    private static async Task<IReadOnlyList<MediaDataRange>> ReadBatRangesAsync(
        MediaRecognitionContext context,
        FileRandomAccessData source,
        VhdxStructures structures,
        CancellationToken cancellationToken)
    {
        var payloadBlockCount = checked((structures.VirtualDiskSize + structures.BlockSize - 1) / structures.BlockSize);
        var chunkRatio = checked((1L << 23) * structures.LogicalSectorSize / structures.BlockSize);
        if (chunkRatio <= 0) throw new InvalidDataException("The VHDX chunk ratio is invalid.");
        var batEntryCount = checked(payloadBlockCount + (payloadBlockCount - 1) / chunkRatio);
        var batByteLength = checked(batEntryCount * sizeof(ulong));
        if (batByteLength > structures.BatRegion.Length || batByteLength > int.MaxValue)
            throw new InvalidDataException("The VHDX BAT region is too short.");
        var bat = await context.ReadAsync(structures.BatRegion.Offset, (int)batByteLength, cancellationToken).ConfigureAwait(false);
        var ranges = new List<MediaDataRange>();
        for (long payloadIndex = 0; payloadIndex < payloadBlockCount; payloadIndex++)
        {
            var batIndex = checked(payloadIndex + payloadIndex / chunkRatio);
            var entry = BinaryPrimitives.ReadUInt64LittleEndian(bat.Span.Slice(checked((int)(batIndex * 8)), 8));
            var state = entry & HardDiskFormatConstants.VhdxBatStateMask;
            var address = checked(payloadIndex * structures.BlockSize);
            var length = Math.Min((long)structures.BlockSize, structures.VirtualDiskSize - address);
            switch (state)
            {
                case HardDiskFormatConstants.VhdxBatZero:
                    AddRange(ranges, address, length, MediaDataRangeKind.Zero);
                    break;
                case HardDiskFormatConstants.VhdxBatNotPresent:
                case HardDiskFormatConstants.VhdxBatUndefined:
                case HardDiskFormatConstants.VhdxBatUnmapped:
                    AddRange(ranges, address, length, MediaDataRangeKind.Unallocated);
                    break;
                case HardDiskFormatConstants.VhdxBatFullyPresent:
                    var fileOffset = checked((long)(entry & HardDiskFormatConstants.VhdxBatFileOffsetMask));
                    if (fileOffset > context.Length - length)
                        throw new InvalidDataException($"VHDX BAT entry {batIndex} points outside the file.");
                    AddRange(ranges, address, length, MediaDataRangeKind.Stored, source, fileOffset);
                    break;
                case HardDiskFormatConstants.VhdxBatPartiallyPresent:
                    throw new NotSupportedException("Partially present VHDX payload blocks require sector bitmap resolution.");
                default:
                    throw new InvalidDataException($"VHDX BAT entry {batIndex} uses invalid state {state}.");
            }
        }
        return ranges;
    }

    private static bool HasValidCrc(ReadOnlySpan<byte> structure, int checksumOffset)
    {
        var expected = BinaryPrimitives.ReadUInt32LittleEndian(structure.Slice(checksumOffset, 4));
        var copy = structure.ToArray();
        copy.AsSpan(checksumOffset, 4).Clear();
        return Crc32CFunctions.Compute(copy) == expected;
    }

    private static bool IsPowerOfTwo(uint value) => value != 0 && (value & (value - 1)) == 0;

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

    private sealed record VhdxHeader(ulong SequenceNumber, Guid LogGuid);
    private sealed record VhdxRegion(long Offset, uint Length);
    private sealed record VhdxMetadata(
        int BlockSize,
        uint FileParametersFlags,
        long VirtualDiskSize,
        int LogicalSectorSize,
        int PhysicalSectorSize,
        Guid Page83Id);
    private sealed record VhdxStructures(
        VhdxRegion BatRegion,
        int BlockSize,
        uint FileParametersFlags,
        long VirtualDiskSize,
        int LogicalSectorSize,
        int PhysicalSectorSize,
        Guid Page83Id);
}
