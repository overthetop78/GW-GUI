using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Models.Blocks;

namespace GWGUI.MediaEngine.Images.Reading.Blocks;

/// <summary>Reads logical byte ranges from a block media representation.</summary>
internal static class BlockMediaDataReader
{
    public static async ValueTask ReadExactlyAsync(BlockMediaImageRepresentation image, long address, Memory<byte> destination, CancellationToken cancellationToken = default)
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
                    await range.Source!.ReadExactlyAsync(checked(range.SourceOffset + offsetInRange), slice, cancellationToken).ConfigureAwait(false);
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

    internal static async Task CopyToAsync(BlockMediaImageRepresentation blocks, Stream output, CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * DataSizeConstants.BytesPerKibibyte];
        long address = 0;
        while (address < blocks.Capacity)
        {
            var count = (int)Math.Min(buffer.Length, blocks.Capacity - address);
            await BlockMediaDataReader.ReadExactlyAsync(blocks, address, buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            address += count;
        }
    }

    internal static async Task CopyToAsync(MediaDataRange range, Stream output, CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        var completed = 0L;
        while (completed < range.Length)
        {
            var count = (int)Math.Min(buffer.Length, range.Length - completed);
            await range.Source!.ReadExactlyAsync(range.SourceOffset + completed, buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            completed += count;
        }
    }

    internal static Task<byte[]?> ReadRangeAsync(MediaDataRange? range, CancellationToken cancellationToken) =>
        ReadRangeAsync(range, 0, range?.Length ?? 0, cancellationToken);
    internal static async Task<byte[]?> ReadRangeAsync(MediaDataRange? range, long relativeOffset, long length, CancellationToken cancellationToken)
    {
        if (range is not { Source: not null } || length <= 0 || length > int.MaxValue || relativeOffset < 0 || relativeOffset + length > range.Length)
            return null;
        var data = new byte[checked((int)length)];
        await range.Source.ReadExactlyAsync(range.SourceOffset + relativeOffset, data, cancellationToken).ConfigureAwait(false);
        return data;
    }
}
