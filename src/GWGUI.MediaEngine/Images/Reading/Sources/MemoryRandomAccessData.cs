using IMediaRandomAccessData = global::GWGUI.MediaFileSystems.Interfaces.IMediaRandomAccessData;

namespace GWGUI.MediaEngine.Images.Reading.Sources;

/// <summary>Exposes an immutable in-memory byte sequence through the common bounded random-access contract.</summary>
public sealed class MemoryRandomAccessData : IMediaRandomAccessData
{
    private readonly ReadOnlyMemory<byte> data;

    public MemoryRandomAccessData(ReadOnlyMemory<byte> data) => this.data = data.ToArray();

    public long Length => data.Length;

    public ValueTask ReadExactlyAsync(
        long offset,
        Memory<byte> destination,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        if (offset > data.Length || destination.Length > data.Length - offset)
            throw new ArgumentOutOfRangeException(nameof(destination), "The requested range exceeds the memory data source.");
        data.Span.Slice(checked((int)offset), destination.Length).CopyTo(destination.Span);
        return ValueTask.CompletedTask;
    }
}
