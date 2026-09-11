using System.Collections.ObjectModel;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Representations.Blocks;

/// <summary>Describes the 64-bit address ranges available in a block-addressed media image.</summary>
public sealed class BlockMediaImageRepresentation : IMediaImageRepresentation
{
    public BlockMediaImageRepresentation(long logicalLength, IReadOnlyList<(long Address, long Length)> ranges)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(logicalLength);
        ArgumentNullException.ThrowIfNull(ranges);

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
        Ranges = new ReadOnlyCollection<(long Address, long Length)>(orderedRanges);
    }

    public MediaRepresentationKind RepresentationKind => MediaRepresentationKind.Blocks;

    public long? LogicalLength { get; }

    public bool SupportsRandomAccess => true;

    public bool SupportsSequentialAccess => true;

    public IReadOnlyList<(long Address, long Length)> Ranges { get; }
}
