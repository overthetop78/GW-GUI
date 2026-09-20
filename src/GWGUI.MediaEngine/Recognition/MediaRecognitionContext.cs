using System.Collections.Concurrent;
using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Recognition;

/// <summary>Shares bounded, page-cached reads of one media source between recognition candidates.</summary>
public sealed class MediaRecognitionContext
{
    private readonly ConcurrentDictionary<long, Lazy<Task<byte[]>>> pages = new();
    private readonly int pageSize;

    public MediaRecognitionContext(MediaSourceDescriptor source, int pageSize = 64 * DataSizeConstants.BytesPerKibibyte)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(source.PrimaryPath);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        Source = source;
        Length = source.KnownLength ?? new FileInfo(source.PrimaryPath).Length;
        if (Length < 0) throw new ArgumentOutOfRangeException(nameof(source), "The known source length cannot be negative.");
        Extension = Path.GetExtension(source.PrimaryPath).ToLowerInvariant();
        this.pageSize = pageSize;
    }

    public MediaSourceDescriptor Source { get; }

    public long Length { get; }

    public string Extension { get; }

    public string? RequestedFormatId => Source.RequestedFormatId;

    public ValueTask<ReadOnlyMemory<byte>> ReadHeaderAsync(int count, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return ReadAsync(0, (int)Math.Min(count, Length), cancellationToken);
    }

    public async ValueTask<ReadOnlyMemory<byte>> ReadAsync(long offset, int count, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (offset > Length || count > Length - offset) throw new ArgumentOutOfRangeException(nameof(count), "The requested range exceeds the source length.");
        if (count == 0) return ReadOnlyMemory<byte>.Empty;

        var destination = new byte[count];
        var firstPage = offset / pageSize;
        var lastPage = (offset + count - 1) / pageSize;
        for (var pageIndex = firstPage; pageIndex <= lastPage; pageIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var page = await GetPageAsync(pageIndex, cancellationToken).ConfigureAwait(false);
            var pageStart = checked(pageIndex * pageSize);
            var sourceOffset = (int)Math.Max(0, offset - pageStart);
            var destinationOffset = (int)Math.Max(0, pageStart - offset);
            var copyCount = Math.Min(page.Length - sourceOffset, count - destinationOffset);
            page.AsMemory(sourceOffset, copyCount).CopyTo(destination.AsMemory(destinationOffset, copyCount));
        }

        return destination;
    }

    public async Task<ReadOnlyMemory<byte>> ReadBytesAsync(CancellationToken cancellationToken = default)
    {
        if (Length > int.MaxValue) throw new NotSupportedException("The complete source cannot be loaded into one memory buffer. Use bounded reads instead.");
        return await ReadAsync(0, (int)Length, cancellationToken).ConfigureAwait(false);
    }

    private Task<byte[]> GetPageAsync(long pageIndex, CancellationToken cancellationToken)
        => pages.GetOrAdd(
            pageIndex,
            index => new Lazy<Task<byte[]>>(
                () => ReadPageAsync(index, cancellationToken),
                LazyThreadSafetyMode.ExecutionAndPublication)).Value;

    private async Task<byte[]> ReadPageAsync(long pageIndex, CancellationToken cancellationToken)
    {
        var offset = checked(pageIndex * pageSize);
        var count = (int)Math.Min(pageSize, Length - offset);
        var buffer = new byte[count];
        await using var stream = new FileStream(Source.PrimaryPath, FileMode.Open, FileAccess.Read, FileShare.Read, pageSize, FileOptions.Asynchronous | FileOptions.RandomAccess);
        stream.Seek(offset, SeekOrigin.Begin);
        var totalRead = 0;
        while (totalRead < count)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(totalRead, count - totalRead), cancellationToken).ConfigureAwait(false);
            if (read == 0) throw new EndOfStreamException($"The media source ended at byte {offset + totalRead} while reading a cached page.");
            totalRead += read;
        }

        return buffer;
    }
}
