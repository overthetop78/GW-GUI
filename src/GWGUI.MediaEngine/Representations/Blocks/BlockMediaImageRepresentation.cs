using System.Collections.ObjectModel;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Representations.Blocks;

/// <summary>Describes the 64-bit address ranges available in a block-addressed media image.</summary>
public sealed class BlockMediaImageRepresentation : IMediaImageRepresentation
{
    public BlockMediaImageRepresentation(long logicalLength, IReadOnlyList<(long Address, long Length)> ranges)
        : this(
            logicalLength,
            1,
            ranges?.Select(range => new MediaDataRange(
                range.Address,
                range.Length,
                MediaDataRangeKind.Unavailable)).ToArray()
                ?? throw new ArgumentNullException(nameof(ranges)))
    {
    }

    public BlockMediaImageRepresentation(
        long logicalLength,
        int logicalBlockSize,
        IReadOnlyList<MediaDataRange> ranges,
        HardDiskGeometry? geometry = null)
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
}
