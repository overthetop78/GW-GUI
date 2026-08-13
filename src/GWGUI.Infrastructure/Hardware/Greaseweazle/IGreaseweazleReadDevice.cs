namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public interface IGreaseweazleReadDevice : IGreaseweazleDevice
{
    ValueTask<GreaseweazleFluxCapture> ReadFluxAsync(
        int revolutions,
        uint tickLimit = 0,
        int retries = 5,
        CancellationToken cancellationToken = default);
}
