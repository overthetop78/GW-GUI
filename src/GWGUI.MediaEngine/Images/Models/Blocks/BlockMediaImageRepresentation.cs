using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces;
using IMediaBlockRepresentation = global::GWGUI.MediaFileSystems.Interfaces.IMediaBlockRepresentation;
using GWGUI.MediaEngine.Images.Reading.Blocks;

namespace GWGUI.MediaEngine.Images.Models.Blocks;

/// <summary>Describes the 64-bit address ranges available in a block-addressed media image.</summary>
public sealed class BlockMediaImageRepresentation : IMediaBlockRepresentation, IMediaImageRepresentation
{
    public BlockMediaImageRepresentation(long logicalLength, IReadOnlyList<(long Address, long Length)> ranges)
        : this(
            logicalLength,
            1,
            ranges?.Select(range => new MediaDataRange(range.Address, range.Length, MediaDataRangeKind.Unavailable)).ToArray()
                ?? throw new ArgumentNullException(nameof(ranges)))
    {
    }

    public BlockMediaImageRepresentation(long logicalLength, int logicalBlockSize, IReadOnlyList<MediaDataRange> ranges, HardDiskGeometry? geometry = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(logicalLength);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(logicalBlockSize);
        ArgumentNullException.ThrowIfNull(ranges);
        if (logicalLength % logicalBlockSize != 0)
            throw new ArgumentException("The logical length must contain complete logical blocks.", nameof(logicalLength));
        if (geometry is not null && geometry.Capacity > logicalLength)
            throw new ArgumentOutOfRangeException(nameof(geometry), "The declared geometry exceeds the logical media capacity.");

        var orderedRanges = ranges.OrderBy(range => range.Address).ToArray();
        long previousEnd = 0;
        foreach (var range in orderedRanges)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(range.Address);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(range.Length);
            var end = checked(range.Address + range.Length);
            if (end > logicalLength) throw new ArgumentOutOfRangeException(nameof(ranges), range, "A block range exceeds the logical media length.");
            if (range.Address < previousEnd) throw new ArgumentException("Block ranges must not overlap.", nameof(ranges));
            previousEnd = end;
        }

        LogicalLength = logicalLength;
        LogicalBlockSize = logicalBlockSize;
        LogicalBlockCount = logicalLength / logicalBlockSize;
        Ranges = new ReadOnlyCollection<MediaDataRange>(orderedRanges);
        Geometry = geometry;
    }

    public MediaRepresentationKind RepresentationKind => MediaRepresentationKind.Blocks;

    public long? LogicalLength { get; }

    public long Capacity => LogicalLength!.Value;

    public int LogicalBlockSize { get; }

    public long LogicalBlockCount { get; }

    public bool SupportsRandomAccess => true;

    public bool SupportsSequentialAccess => true;

    public IReadOnlyList<MediaDataRange> Ranges { get; }

    public HardDiskGeometry? Geometry { get; }

    ValueTask IMediaBlockRepresentation.ReadExactlyAsync(long address, Memory<byte> destination, CancellationToken cancellationToken) => BlockMediaDataReader.ReadExactlyAsync(this, address, destination, cancellationToken);

    internal static void AddRange(List<MediaDataRange> ranges, long address, long length, MediaDataRangeKind kind, GWGUI.MediaFileSystems.Interfaces.IMediaRandomAccessData? source = null, long sourceOffset = 0, bool checkedLength = false)
    {
        if (ranges.Count > 0)
        {
            var previous = ranges[^1];
            var sourcesContinue = kind != MediaDataRangeKind.Stored || ReferenceEquals(previous.Source, source) && previous.SourceOffset + previous.Length == sourceOffset;
            if (previous.Kind == kind && previous.Address + previous.Length == address && sourcesContinue)
            {
                ranges[^1] = new MediaDataRange(previous.Address, (checkedLength ? checked(previous.Length + length) : unchecked(previous.Length + length)), kind, previous.Source, previous.SourceOffset);
                return;
            }
        }

        ranges.Add(new MediaDataRange(address, length, kind, source, sourceOffset));
    }

    internal bool HasCompleteReadableCoverage()
    {
        long cursor = 0;
        foreach (var range in Ranges)
        {
            if (range.Address != cursor || range.Kind == MediaDataRangeKind.Unavailable) return false;
            cursor = checked(cursor + range.Length);
        }
        return cursor == Capacity;
    }
}
