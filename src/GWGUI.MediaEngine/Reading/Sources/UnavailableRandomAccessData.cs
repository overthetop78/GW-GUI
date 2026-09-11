using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Reading.Sources;

/// <summary>Retains a known logical length when legacy metadata does not provide readable bytes.</summary>
public sealed class UnavailableRandomAccessData : IMediaRandomAccessData
{
    public UnavailableRandomAccessData(long length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        Length = length;
    }

    public long Length { get; }

    public ValueTask ReadExactlyAsync(
        long offset,
        Memory<byte> destination,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        cancellationToken.ThrowIfCancellationRequested();
        throw new NotSupportedException("This legacy media descriptor contains metadata only and has no readable data source.");
    }
}
