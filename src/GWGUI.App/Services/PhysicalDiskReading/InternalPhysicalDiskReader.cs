using GWGUI.Infrastructure.Hardware.Greaseweazle;

namespace GWGUI.App.Services.PhysicalDiskReading;

public sealed class InternalPhysicalDiskReader(Func<IGreaseweazleReadDevice> deviceFactory)
{
    public async Task<PhysicalDiskReadResult> ReadAsync(
        PhysicalDiskReadOptions options,
        string outputPath,
        IProgress<PhysicalDiskReadOperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await using var device = deviceFactory();
        return await PhysicalDiskReadService.CreateDefault(device).ReadAsync(options, outputPath, progress, cancellationToken);
    }

    public static InternalPhysicalDiskReader CreateDefault() => new(
        () => new GreaseweazleProtocolClient(new WindowsGreaseweazleSerialTransport()));
}
