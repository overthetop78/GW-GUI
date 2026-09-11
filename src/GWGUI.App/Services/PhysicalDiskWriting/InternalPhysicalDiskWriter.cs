using GWGUI.App.Contracts.Services.PhysicalDiskWriting;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.Infrastructure.Hardware.Media;
using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.PhysicalWriting;

namespace GWGUI.App.Services.PhysicalDiskWriting;

public sealed class InternalPhysicalDiskWriter(
    Func<IGreaseweazleWriteDevice> deviceFactory,
    FloppyMediaWritePlanningService? planningService = null)
{
    private readonly FloppyMediaWritePlanningService _planningService =
        planningService ?? new FloppyMediaWritePlanningService(DiskImageExplorer.CreateDefault());

    public async Task<PhysicalDiskWriteResult> WriteAsync(
        InternalPhysicalDiskWriteRequest request,
        IProgress<PhysicalTrackWriteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FormatId);

        var plan = await _planningService.CreatePlanAsync(
            request.SourcePath,
            request.FormatId,
            request.Options.ScpRevolution,
            cancellationToken).ConfigureAwait(false);
        var writers = new MediaPhysicalWriterRegistry(
            [new GreaseweazleMediaPhysicalWriter(deviceFactory)]);
        return await new PhysicalDiskWriteService(writers).WriteAsync(
            plan,
            request.Options,
            progress,
            cancellationToken).ConfigureAwait(false);
    }

    public static InternalPhysicalDiskWriter CreateDefault() =>
        new(() => new GreaseweazleProtocolClient(new WindowsGreaseweazleSerialTransport()));
}
