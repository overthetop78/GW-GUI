namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public interface IGreaseweazleWriteDevice : IGreaseweazleDevice
{
    ValueTask WriteFluxAsync(
        ReadOnlyMemory<uint> intervals,
        bool cueAtIndex,
        bool terminateAtIndex,
        uint hardSectorTicks = 0,
        CancellationToken cancellationToken = default);

}
