using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using System.Text.RegularExpressions;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Blocks;

namespace GWGUI.MediaEngine.Formats.HardDisk.Chd;

/// <summary>Reads autonomous, uncompressed CHD V5 hard disk images with GDDD geometry metadata.</summary>
public sealed partial class ChdHardDiskReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { HardDiskImageFormatIds.Chd }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.HardDisk }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => ChdHardDiskFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [ChdHardDiskFormat.Signature];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        try
        {
            await ReadLayoutAsync(context, cancellationToken).ConfigureAwait(false);
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
        var layout = await ReadLayoutAsync(context, cancellationToken).ConfigureAwait(false);
        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        var ranges = new List<MediaDataRange>();
        for (long hunkIndex = 0; hunkIndex < layout.HunkCount; hunkIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entryOffset = checked((int)hunkIndex * ChdConstants.Version5MapEntrySize);
            var storedHunk = BinaryPrimitives.ReadUInt32BigEndian(layout.Map.Span.Slice(entryOffset, 4));
            var logicalOffset = checked(hunkIndex * layout.HunkBytes);
            var length = Math.Min((long)layout.HunkBytes, layout.LogicalBytes - logicalOffset);
            if (storedHunk == 0)
            {
                AddRange(ranges, logicalOffset, length, MediaDataRangeKind.Zero);
                continue;
            }

            var sourceOffset = checked((long)storedHunk * layout.HunkBytes);
            if (sourceOffset > context.Length - layout.HunkBytes)
                throw new InvalidDataException($"CHD hunk {hunkIndex} points outside the file.");
            AddRange(ranges, logicalOffset, length, MediaDataRangeKind.Stored, source, sourceOffset);
        }

        return new MediaImageDocument(
            context.Source,
            HardDiskImageFormatIds.Chd,
            MediaKind.HardDisk,
            new BlockMediaImageRepresentation(
                layout.LogicalBytes,
                layout.UnitBytes,
                ranges,
                layout.Geometry),
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["version"] = ChdConstants.Version5.ToString(CultureInfo.InvariantCulture),
                ["variant"] = HardDiskImageVariant.Fixed.ToString(),
                ["hunkSize"] = layout.HunkBytes.ToString(CultureInfo.InvariantCulture)
            });
    }

    private static async ValueTask<ChdLayout> ReadLayoutAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        if (context.Length < ChdConstants.Version5HeaderSize)
            throw new InvalidDataException("The file is too short to contain a CHD V5 header.");
        var headerMemory = await context.ReadAsync(
            0,
            ChdConstants.Version5HeaderSize,
            cancellationToken).ConfigureAwait(false);
        var header = headerMemory.Span;
        if (!header[..ChdHardDiskFormat.Signature.Length].SequenceEqual(ChdHardDiskFormat.Signature.Span))
            throw new InvalidDataException("The CHD signature is missing.");
        if (BinaryPrimitives.ReadUInt32BigEndian(header[8..12]) != ChdConstants.Version5HeaderSize
            || BinaryPrimitives.ReadUInt32BigEndian(header[12..16]) != ChdConstants.Version5)
            throw new NotSupportedException("Only CHD V5 images are supported.");
        for (var offset = 16; offset < 32; offset += sizeof(uint))
        {
            if (BinaryPrimitives.ReadUInt32BigEndian(header[offset..(offset + 4)]) != ChdConstants.UncompressedCodec)
                throw new NotSupportedException("Compressed CHD images are not supported by this profile.");
        }
        if (!header[104..124].SequenceEqual(new byte[ChdConstants.Sha1Length]))
            throw new NotSupportedException("CHD images with a parent are not supported.");

        var logicalBytesUnsigned = BinaryPrimitives.ReadUInt64BigEndian(header[32..40]);
        var mapOffsetUnsigned = BinaryPrimitives.ReadUInt64BigEndian(header[40..48]);
        var metadataOffsetUnsigned = BinaryPrimitives.ReadUInt64BigEndian(header[48..56]);
        var hunkBytesUnsigned = BinaryPrimitives.ReadUInt32BigEndian(header[56..60]);
        var unitBytesUnsigned = BinaryPrimitives.ReadUInt32BigEndian(header[60..64]);
        if (logicalBytesUnsigned == 0 || logicalBytesUnsigned > long.MaxValue
            || mapOffsetUnsigned > long.MaxValue || metadataOffsetUnsigned > long.MaxValue
            || hunkBytesUnsigned == 0 || hunkBytesUnsigned > int.MaxValue
            || unitBytesUnsigned == 0 || unitBytesUnsigned > int.MaxValue)
            throw new InvalidDataException("The CHD V5 dimensions are invalid.");
        var logicalBytes = (long)logicalBytesUnsigned;
        var mapOffset = (long)mapOffsetUnsigned;
        var metadataOffset = (long)metadataOffsetUnsigned;
        var hunkBytes = (int)hunkBytesUnsigned;
        var unitBytes = (int)unitBytesUnsigned;
        if (logicalBytes % unitBytes != 0 || hunkBytes % unitBytes != 0)
            throw new InvalidDataException("The CHD logical and hunk sizes are not aligned to complete units.");
        var hunkCount = checked((logicalBytes + hunkBytes - 1) / hunkBytes);
        var mapLengthLong = checked(hunkCount * ChdConstants.Version5MapEntrySize);
        if (mapOffset < ChdConstants.Version5HeaderSize
            || mapLengthLong > int.MaxValue
            || mapOffset > context.Length - mapLengthLong)
            throw new InvalidDataException("The CHD V5 map exceeds the file.");
        var map = await context.ReadAsync(mapOffset, (int)mapLengthLong, cancellationToken).ConfigureAwait(false);
        var geometry = await ReadHardDiskMetadataAsync(context, metadataOffset, cancellationToken).ConfigureAwait(false);
        if (geometry.SectorSize != unitBytes || geometry.Capacity != logicalBytes)
            throw new InvalidDataException("The CHD GDDD geometry does not match the logical image dimensions.");
        return new ChdLayout(logicalBytes, hunkBytes, unitBytes, hunkCount, map, geometry);
    }

    private static async ValueTask<HardDiskGeometry> ReadHardDiskMetadataAsync(
        MediaRecognitionContext context,
        long firstOffset,
        CancellationToken cancellationToken)
    {
        if (firstOffset <= 0) throw new InvalidDataException("The CHD has no hard disk metadata.");
        var visited = new HashSet<long>();
        var offset = firstOffset;
        while (offset != 0)
        {
            if (!visited.Add(offset)) throw new InvalidDataException("The CHD metadata chain contains a loop.");
            if (offset < ChdConstants.Version5HeaderSize
                || offset > context.Length - ChdConstants.MetadataHeaderSize)
                throw new InvalidDataException("A CHD metadata header is outside the file.");
            var headerMemory = await context.ReadAsync(
                offset,
                ChdConstants.MetadataHeaderSize,
                cancellationToken).ConfigureAwait(false);
            var header = headerMemory.Span;
            var tag = BinaryPrimitives.ReadUInt32BigEndian(header[..4]);
            var lengthAndFlags = BinaryPrimitives.ReadUInt32BigEndian(header[4..8]);
            var length = checked((int)(lengthAndFlags & ChdConstants.MetadataLengthMask));
            var nextUnsigned = BinaryPrimitives.ReadUInt64BigEndian(header[8..16]);
            if (nextUnsigned > long.MaxValue) throw new InvalidDataException("A CHD metadata link is invalid.");
            var dataOffset = checked(offset + ChdConstants.MetadataHeaderSize);
            if (length <= 0 || dataOffset > context.Length - length)
                throw new InvalidDataException("A CHD metadata record exceeds the file.");
            if (tag == ChdHardDiskFormat.RequiredMetadataTag)
            {
                var data = await context.ReadAsync(dataOffset, length, cancellationToken).ConfigureAwait(false);
                var text = System.Text.Encoding.ASCII.GetString(data.Span).TrimEnd('\0');
                var match = HardDiskMetadataRegex().Match(text);
                if (!match.Success) throw new InvalidDataException("The CHD GDDD metadata is invalid.");
                return new HardDiskGeometry(
                    long.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                    int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
                    int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture),
                    int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture));
            }
            offset = (long)nextUnsigned;
        }
        throw new InvalidDataException("The CHD metadata does not classify the image as a hard disk.");
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

    [GeneratedRegex(@"^CYLS:(\d+),HEADS:(\d+),SECS:(\d+),BPS:(\d+)$", RegexOptions.CultureInvariant)]
    private static partial Regex HardDiskMetadataRegex();

    private sealed record ChdLayout(
        long LogicalBytes,
        int HunkBytes,
        int UnitBytes,
        long HunkCount,
        ReadOnlyMemory<byte> Map,
        HardDiskGeometry Geometry);
}
