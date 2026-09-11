namespace GWGUI.MediaEngine.Interfaces;

/// <summary>Provides bounded asynchronous reads from one logical media data source.</summary>
public interface IMediaRandomAccessData
{
    long Length { get; }

    ValueTask ReadExactlyAsync(
        long offset,
        Memory<byte> destination,
        CancellationToken cancellationToken = default);
}
