namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public interface IGreaseweazleWriteDevice : IAsyncDisposable
{
    GreaseweazleFirmwareInfo? Firmware { get; }

    ValueTask<GreaseweazleFirmwareInfo> OpenAsync(
        string portName,
        CancellationToken cancellationToken = default);

    ValueTask SetBusTypeAsync(
        GreaseweazleBusType busType,
        CancellationToken cancellationToken = default);

    ValueTask SelectDriveAsync(byte unit, CancellationToken cancellationToken = default);

    ValueTask SetMotorAsync(bool enabled, CancellationToken cancellationToken = default);

    ValueTask SeekAsync(short cylinder, byte head, CancellationToken cancellationToken = default);

    ValueTask WriteFluxAsync(
        ReadOnlyMemory<uint> intervals,
        bool cueAtIndex,
        bool terminateAtIndex,
        uint hardSectorTicks = 0,
        CancellationToken cancellationToken = default);

    ValueTask ResetAsync(CancellationToken cancellationToken = default);

    ValueTask CloseAsync(CancellationToken cancellationToken = default);
}
