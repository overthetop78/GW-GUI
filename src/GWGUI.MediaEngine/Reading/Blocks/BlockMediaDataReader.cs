using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Representations.Blocks;

namespace GWGUI.MediaEngine.Reading.Blocks;

/// <summary>Reads logical byte ranges from a block media representation.</summary>
internal static class BlockMediaDataReader
{
    public static async ValueTask ReadExactlyAsync(
        BlockMediaImageRepresentation image,
        long address,
        Memory<byte> destination,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentOutOfRangeException.ThrowIfNegative(address);
        if (address > image.Capacity || destination.Length > image.Capacity - address)
            throw new ArgumentOutOfRangeException(nameof(destination), "The requested logical range exceeds the media capacity.");
        if (destination.IsEmpty) return;

        var ranges = image.Ranges;
        var rangeIndex = FindContainingRange(ranges, address);
        var logicalAddress = address;
        var destinationOffset = 0;
        while (destinationOffset < destination.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (rangeIndex >= ranges.Count)
                throw new InvalidDataException($"No media data range covers logical address {logicalAddress}.");

            var range = ranges[rangeIndex];
            if (logicalAddress < range.Address || logicalAddress >= checked(range.Address + range.Length))
                throw new InvalidDataException($"No media data range covers logical address {logicalAddress}.");

            var offsetInRange = logicalAddress - range.Address;
            var count = (int)Math.Min(destination.Length - destinationOffset, range.Length - offsetInRange);
            var slice = destination.Slice(destinationOffset, count);
            switch (range.Kind)
            {
                case MediaDataRangeKind.Stored:
                    await range.Source!.ReadExactlyAsync(
                        checked(range.SourceOffset + offsetInRange),
                        slice,
                        cancellationToken).ConfigureAwait(false);
                    break;
                case MediaDataRangeKind.Zero:
                case MediaDataRangeKind.Unallocated:
                    slice.Span.Clear();
                    break;
                case MediaDataRangeKind.Unavailable:
                    throw new InvalidDataException($"Logical address {logicalAddress} is unavailable in the source image.");
                default:
                    throw new InvalidDataException($"Unsupported media data range kind {range.Kind}.");
            }

            logicalAddress += count;
            destinationOffset += count;
            if (offsetInRange + count == range.Length) rangeIndex++;
        }
    }

    private static int FindContainingRange(IReadOnlyList<MediaDataRange> ranges, long address)
    {
        var low = 0;
        var high = ranges.Count - 1;
        while (low <= high)
        {
            var middle = low + (high - low) / 2;
            var range = ranges[middle];
            if (address < range.Address)
            {
                high = middle - 1;
                continue;
            }

            if (address >= checked(range.Address + range.Length))
            {
                low = middle + 1;
                continue;
            }

            return middle;
        }

        return low;
    }
}
