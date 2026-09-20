using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Blocks;

namespace GWGUI.MediaEngine.Formats.HardDisk.Vhd;

/// <summary>Reads autonomous fixed and dynamic VHD images and exposes their logical block ranges.</summary>
public sealed class VhdReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { HardDiskImageFormatIds.Vhd }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.HardDisk }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> SupportedSignatures =
        [VhdFormat.FooterSignature];

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;

    IReadOnlySet<string> IMediaImageReader.Extensions => VhdFormat.Extensions;

    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => SupportedSignatures;

    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;

    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;

    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;

    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId))
            return false;
        try
        {
            var footer = await ReadFooterAsync(context, cancellationToken).ConfigureAwait(false);
            if (footer.DiskType == HardDiskFormatConstants.VhdDifferencingDiskType) return false;
            return footer.DiskType is HardDiskFormatConstants.VhdFixedDiskType
                or HardDiskFormatConstants.VhdDynamicDiskType;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        var footer = await ReadFooterAsync(context, cancellationToken).ConfigureAwait(false);
        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        if (source.Length != context.Length)
            throw new InvalidDataException("The VHD length changed after recognition.");

        IReadOnlyList<MediaDataRange> ranges;
        HardDiskImageVariant variant;
        switch (footer.DiskType)
        {
            case HardDiskFormatConstants.VhdFixedDiskType:
                if (footer.CurrentSize != context.Length - HardDiskFormatConstants.VhdFooterSize)
                    throw new InvalidDataException("The fixed VHD payload length does not match its footer capacity.");
                ranges = [new MediaDataRange(0, footer.CurrentSize, MediaDataRangeKind.Stored, source)];
                variant = HardDiskImageVariant.Fixed;
                break;
            case HardDiskFormatConstants.VhdDynamicDiskType:
                ranges = await ReadDynamicRangesAsync(context, source, footer, cancellationToken).ConfigureAwait(false);
                variant = HardDiskImageVariant.Dynamic;
                break;
            case HardDiskFormatConstants.VhdDifferencingDiskType:
                throw new NotSupportedException("Differencing VHD images require a parent resolver that is not registered.");
            default:
                throw new InvalidDataException($"Unsupported VHD disk type {footer.DiskType}.");
        }

        var geometry = TryCreateGeometry(footer);
        var representation = new BlockMediaImageRepresentation(
            footer.CurrentSize,
            HardDiskFormatConstants.LegacyLogicalSectorSize,
            ranges,
            geometry);
        return new MediaImageDocument(
            context.Source,
            HardDiskImageFormatIds.Vhd,
            MediaKind.HardDisk,
            representation,
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["variant"] = variant.ToString(),
                ["uniqueId"] = footer.UniqueId.ToString("D")
            });
    }

    private static async ValueTask<VhdFooter> ReadFooterAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        if (context.Length < HardDiskFormatConstants.VhdFooterSize)
            throw new InvalidDataException("The file is too short to contain a VHD footer.");
        var memory = await context.ReadAsync(
            context.Length - HardDiskFormatConstants.VhdFooterSize,
            HardDiskFormatConstants.VhdFooterSize,
            cancellationToken).ConfigureAwait(false);
        var footer = memory.Span;
        if (!footer[..8].SequenceEqual(VhdFormat.FooterSignature.Span))
            throw new InvalidDataException("The VHD footer cookie is missing.");
        ValidateChecksum(footer, 64, "VHD footer");

        var formatVersion = BinaryPrimitives.ReadUInt32BigEndian(footer[12..16]);
        if (formatVersion != 0x00010000)
            throw new InvalidDataException($"Unsupported VHD format version 0x{formatVersion:X8}.");
        var currentSizeUnsigned = BinaryPrimitives.ReadUInt64BigEndian(footer[48..56]);
        if (currentSizeUnsigned == 0 || currentSizeUnsigned > long.MaxValue)
            throw new InvalidDataException("The VHD current size is outside the supported 64-bit range.");
        var currentSize = (long)currentSizeUnsigned;
        if (currentSize % HardDiskFormatConstants.LegacyLogicalSectorSize != 0)
            throw new InvalidDataException("The VHD current size is not aligned to 512-byte sectors.");

        return new VhdFooter(
            BinaryPrimitives.ReadUInt64BigEndian(footer[16..24]),
            currentSize,
            BinaryPrimitives.ReadUInt16BigEndian(footer[56..58]),
            footer[58],
            footer[59],
            BinaryPrimitives.ReadUInt32BigEndian(footer[60..64]),
            new Guid(footer[68..84], bigEndian: true));
    }

    private static async Task<IReadOnlyList<MediaDataRange>> ReadDynamicRangesAsync(
        MediaRecognitionContext context,
        FileRandomAccessData source,
        VhdFooter footer,
        CancellationToken cancellationToken)
    {
        if (footer.DataOffset > (ulong)(context.Length - HardDiskFormatConstants.VhdDynamicHeaderSize))
            throw new InvalidDataException("The VHD dynamic header offset is outside the file.");
        var headerMemory = await context.ReadAsync(
            (long)footer.DataOffset,
            HardDiskFormatConstants.VhdDynamicHeaderSize,
            cancellationToken).ConfigureAwait(false);
        var header = headerMemory.Span;
        if (!header[..8].SequenceEqual(VhdFormat.DynamicHeaderSignature.Span))
            throw new InvalidDataException("The VHD dynamic header cookie is missing.");
        ValidateChecksum(header, 36, "VHD dynamic header");

        var tableOffsetUnsigned = BinaryPrimitives.ReadUInt64BigEndian(header[16..24]);
        if (tableOffsetUnsigned > long.MaxValue)
            throw new InvalidDataException("The VHD BAT offset exceeds the supported range.");
        var tableOffset = (long)tableOffsetUnsigned;
        var maximumEntries = BinaryPrimitives.ReadUInt32BigEndian(header[28..32]);
        var blockSize = BinaryPrimitives.ReadUInt32BigEndian(header[32..36]);
        if (blockSize == 0 || blockSize % HardDiskFormatConstants.LegacyLogicalSectorSize != 0)
            throw new InvalidDataException("The VHD dynamic block size is invalid.");
        var requiredEntries = checked((footer.CurrentSize + blockSize - 1) / blockSize);
        if (maximumEntries < requiredEntries || requiredEntries > int.MaxValue)
            throw new InvalidDataException("The VHD BAT does not cover the declared capacity.");
        var batByteLength = checked((int)requiredEntries * sizeof(uint));
        if (tableOffset < 0 || tableOffset > context.Length - batByteLength)
            throw new InvalidDataException("The VHD BAT exceeds the file.");
        var bat = await context.ReadAsync(tableOffset, batByteLength, cancellationToken).ConfigureAwait(false);

        var sectorsPerBlock = checked((int)(blockSize / HardDiskFormatConstants.LegacyLogicalSectorSize));
        var bitmapByteLength = checked((sectorsPerBlock + 7) / 8);
        var bitmapStorageLength = Align(bitmapByteLength, HardDiskFormatConstants.LegacyLogicalSectorSize);
        var ranges = new List<MediaDataRange>();
        for (var blockIndex = 0; blockIndex < requiredEntries; blockIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var logicalBlockStart = checked(blockIndex * (long)blockSize);
            var logicalBlockLength = Math.Min((long)blockSize, footer.CurrentSize - logicalBlockStart);
            var batEntry = BinaryPrimitives.ReadUInt32BigEndian(bat.Span.Slice(blockIndex * sizeof(uint), sizeof(uint)));
            if (batEntry == HardDiskFormatConstants.VhdUnusedBatEntry)
            {
                AddRange(ranges, logicalBlockStart, logicalBlockLength, MediaDataRangeKind.Unallocated);
                continue;
            }

            var storedBlockStart = checked((long)batEntry * HardDiskFormatConstants.LegacyLogicalSectorSize);
            var dataStart = checked(storedBlockStart + bitmapStorageLength);
            if (storedBlockStart < 0 || dataStart > context.Length - logicalBlockLength)
                throw new InvalidDataException($"VHD BAT entry {blockIndex} points outside the file.");
            var bitmap = await context.ReadAsync(storedBlockStart, bitmapByteLength, cancellationToken).ConfigureAwait(false);
            var sectorCount = checked((int)((logicalBlockLength + HardDiskFormatConstants.LegacyLogicalSectorSize - 1)
                / HardDiskFormatConstants.LegacyLogicalSectorSize));
            for (var sectorIndex = 0; sectorIndex < sectorCount; sectorIndex++)
            {
                var logicalAddress = checked(logicalBlockStart + sectorIndex * (long)HardDiskFormatConstants.LegacyLogicalSectorSize);
                var length = Math.Min(
                    HardDiskFormatConstants.LegacyLogicalSectorSize,
                    footer.CurrentSize - logicalAddress);
                var allocated = (bitmap.Span[sectorIndex / 8] & (0x80 >> (sectorIndex % 8))) != 0;
                if (allocated)
                {
                    var sourceOffset = checked(dataStart + sectorIndex * (long)HardDiskFormatConstants.LegacyLogicalSectorSize);
                    AddRange(ranges, logicalAddress, length, MediaDataRangeKind.Stored, source, sourceOffset);
                }
                else
                {
                    AddRange(ranges, logicalAddress, length, MediaDataRangeKind.Zero);
                }
            }
        }

        return ranges;
    }

    private static HardDiskGeometry? TryCreateGeometry(VhdFooter footer)
    {
        if (footer.Cylinders == 0 || footer.Heads == 0 || footer.SectorsPerTrack == 0) return null;
        var geometry = new HardDiskGeometry(
            footer.Cylinders,
            footer.Heads,
            footer.SectorsPerTrack,
            HardDiskFormatConstants.LegacyLogicalSectorSize);
        return geometry.Capacity <= footer.CurrentSize ? geometry : null;
    }

    private static void ValidateChecksum(ReadOnlySpan<byte> data, int checksumOffset, string structure)
    {
        uint sum = 0;
        for (var index = 0; index < data.Length; index++)
            if (index < checksumOffset || index >= checksumOffset + sizeof(uint)) sum += data[index];
        var expected = ~sum;
        var observed = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(checksumOffset, sizeof(uint)));
        if (observed != expected)
            throw new InvalidDataException($"The {structure} checksum is invalid.");
    }

    private static int Align(int value, int alignment) => checked((value + alignment - 1) / alignment * alignment);

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
            var sourcesContinue = kind != MediaDataRangeKind.Stored
                || ReferenceEquals(previous.Source, source) && previous.SourceOffset + previous.Length == sourceOffset;
            if (previous.Kind == kind && previous.Address + previous.Length == address && sourcesContinue)
            {
                ranges[^1] = new MediaDataRange(
                    previous.Address,
                    checked(previous.Length + length),
                    kind,
                    previous.Source,
                    previous.SourceOffset);
                return;
            }
        }

        ranges.Add(new MediaDataRange(address, length, kind, source, sourceOffset));
    }

    private sealed record VhdFooter(
        ulong DataOffset,
        long CurrentSize,
        ushort Cylinders,
        byte Heads,
        byte SectorsPerTrack,
        uint DiskType,
        Guid UniqueId);
}
