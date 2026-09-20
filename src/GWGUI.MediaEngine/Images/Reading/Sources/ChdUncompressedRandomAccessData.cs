using IMediaRandomAccessData = global::GWGUI.MediaFileSystems.Interfaces.IMediaRandomAccessData;

namespace GWGUI.MediaEngine.Images.Reading.Sources;

/// <summary>Maps logical reads through an autonomous CHD V5 uncompressed hunk map.</summary>
internal sealed class ChdUncompressedRandomAccessData : IMediaRandomAccessData
{
    private readonly IMediaRandomAccessData source;
    private readonly int hunkSize;
    private readonly IReadOnlyList<uint> storedHunks;

    public ChdUncompressedRandomAccessData(
        IMediaRandomAccessData source,
        long logicalLength,
        int hunkSize,
        IReadOnlyList<uint> storedHunks)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(storedHunks);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(logicalLength);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hunkSize);
        var expectedHunks = checked((logicalLength + hunkSize - 1) / hunkSize);
        if (storedHunks.Count != expectedHunks)
            throw new ArgumentException("The CHD map does not cover the logical image.", nameof(storedHunks));
        foreach (var storedHunk in storedHunks)
        {
            if (storedHunk == 0) continue;
            var sourceOffset = checked((long)storedHunk * hunkSize);
            if (sourceOffset > source.Length - hunkSize)
                throw new ArgumentException("A CHD hunk points outside the source.", nameof(storedHunks));
        }

        this.source = source;
        this.hunkSize = hunkSize;
        this.storedHunks = storedHunks.ToArray();
        Length = logicalLength;
    }

    public long Length { get; }

    public async ValueTask ReadExactlyAsync(
        long offset,
        Memory<byte> destination,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        if (offset > Length || destination.Length > Length - offset)
            throw new ArgumentOutOfRangeException(nameof(destination), "The requested range exceeds the CHD logical data.");

        var completed = 0;
        while (completed < destination.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var logicalOffset = checked(offset + completed);
            var hunkIndex = checked((int)(logicalOffset / hunkSize));
            var offsetInHunk = checked((int)(logicalOffset % hunkSize));
            var length = Math.Min(destination.Length - completed, hunkSize - offsetInHunk);
            var target = destination.Slice(completed, length);
            var storedHunk = storedHunks[hunkIndex];
            if (storedHunk == 0)
            {
                target.Span.Clear();
            }
            else
            {
                await source.ReadExactlyAsync(
                    checked((long)storedHunk * hunkSize + offsetInHunk),
                    target,
                    cancellationToken).ConfigureAwait(false);
            }
            completed += length;
        }
    }
}
