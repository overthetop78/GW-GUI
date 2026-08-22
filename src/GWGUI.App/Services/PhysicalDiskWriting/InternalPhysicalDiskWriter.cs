using GWGUI.App.Contracts.Services.PhysicalDiskWriting;
using System.IO;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Exploration;

namespace GWGUI.App.Services.PhysicalDiskWriting;

public sealed class InternalPhysicalDiskWriter(
    Func<IGreaseweazleWriteDevice> deviceFactory,
    DiskImageExplorer? explorer = null)
{
    private readonly DiskImageExplorer _explorer = explorer ?? DiskImageExplorer.CreateDefault();

    public async Task<PhysicalDiskWriteResult> WriteAsync(
        InternalPhysicalDiskWriteRequest request,
        IProgress<PhysicalTrackWriteProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FormatId);

        await using var device = deviceFactory();
        var service = new PhysicalDiskWriteService(device);
        if (Path.GetExtension(request.SourcePath).Equals(".scp", StringComparison.OrdinalIgnoreCase))
        {
            var image = await new ScpReader().ReadAsync(request.SourcePath, cancellationToken);
            return await service.WriteAsync(image, request.Options, progress, cancellationToken);
        }

        var explored = await _explorer.ExploreAsync(request.SourcePath, request.FormatId, cancellationToken);
        var tracks = new SectorImageTrackEncoder().Encode(explored.Image, cancellationToken);
        return await service.WriteAsync(tracks, request.Options, progress, cancellationToken);
    }

    public static InternalPhysicalDiskWriter CreateDefault() =>
        new(() => new GreaseweazleProtocolClient(new WindowsGreaseweazleSerialTransport()));
}
