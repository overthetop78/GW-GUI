namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public interface IGreaseweazleSerialTransport : IAsyncDisposable
{
    bool IsOpen { get; }

    ValueTask OpenAsync(string portName, int baudRate, CancellationToken cancellationToken = default);

    ValueTask SetBaudRateAsync(int baudRate, CancellationToken cancellationToken = default);

    ValueTask DiscardBuffersAsync(CancellationToken cancellationToken = default);

    ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);

    ValueTask ReadExactlyAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);

    ValueTask CloseAsync(CancellationToken cancellationToken = default);
}
