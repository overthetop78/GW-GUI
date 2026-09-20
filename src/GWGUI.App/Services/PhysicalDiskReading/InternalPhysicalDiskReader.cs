using System.Globalization;
using GWGUI.App.Constants.Services.PhysicalDiskReading;
using GWGUI.App.Contracts.Services.PhysicalDiskReading;
using GWGUI.App.Enums.Services.PhysicalDiskReading;
using GWGUI.App.Functions.Services.PhysicalDiskReading;
using MediaPhysicalMetadataKeys = global::GWGUI.MediaEngine.Constants.MediaPhysicalMetadataKeys;
using MediaAcquisitionProgress = global::GWGUI.MediaEngine.Contracts.MediaAcquisitionProgress;
using GWGUI.MediaEngine.Enums;
using GWGUI.Infrastructure.Constants;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.Infrastructure.Hardware.Media;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Reading;
using GWGUI.MediaEngine.Exploration.Contracts;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;

namespace GWGUI.App.Services.PhysicalDiskReading;

public sealed class InternalPhysicalDiskReader(Func<IGreaseweazleReadDevice> deviceFactory)
{
    public async Task<PhysicalDiskReadResult> ReadAsync(
        PhysicalDiskReadOptions options,
        string outputPath,
        IProgress<PhysicalDiskReadOperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var fluxService = new FloppyFluxAcquisitionService();
        var registry = new MediaAcquisitionProviderRegistry(
            [new GreaseweazleMediaAcquisitionProvider(deviceFactory)]);
        var acquisitionProgress = new Progress<MediaAcquisitionProgress>(value =>
            ReportProgress(value, options.Tracks, fluxService, progress));
        var acquisition = await registry.AcquireAsync(
            MediaKind.Floppy,
            options.PortName,
            CreateOptions(options),
            acquisitionProgress,
            cancellationToken).ConfigureAwait(false);
        return await PhysicalDiskReadService.CreateDefault()
            .ReadAsync(acquisition, outputPath, progress, cancellationToken)
            .ConfigureAwait(false);
    }

    public static InternalPhysicalDiskReader CreateDefault() => new(
        () => new GreaseweazleProtocolClient(new WindowsGreaseweazleSerialTransport()));

    private static IReadOnlyDictionary<string, string> CreateOptions(PhysicalDiskReadOptions options)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GreaseweazleMediaOptionKeys.BusType] = options.BusType.ToString(),
            [GreaseweazleMediaOptionKeys.DriveUnit] = options.DriveUnit.ToString(CultureInfo.InvariantCulture),
            [GreaseweazleMediaOptionKeys.Tracks] = string.Join(';', options.Tracks.Select(track =>
                $"{track.Cylinder}:{track.Head}:{track.DriveCylinder}:{track.DriveHead}")),
            [GreaseweazleMediaOptionKeys.DiskType] = options.DiskType.ToString(),
            [GreaseweazleMediaOptionKeys.Revolutions] = options.Revolutions.ToString(CultureInfo.InvariantCulture),
            [GreaseweazleMediaOptionKeys.FluxOverflowRetries] = options.FluxOverflowRetries.ToString(CultureInfo.InvariantCulture),
            [GreaseweazleMediaOptionKeys.SeekRetries] = options.SeekRetries.ToString(CultureInfo.InvariantCulture),
            [GreaseweazleMediaOptionKeys.HardSectors] = options.HardSectors.ToString(CultureInfo.InvariantCulture)
        };
        AddMilliseconds(values, GreaseweazleMediaOptionKeys.FakeIndexMilliseconds, options.FakeIndexPeriod);
        AddMilliseconds(values, GreaseweazleMediaOptionKeys.MotorSpinUpMilliseconds, options.MotorSpinUpDelay);
        AddMilliseconds(values, GreaseweazleMediaOptionKeys.TrackSettleMilliseconds, options.TrackSettleDelay);
        return values;
    }

    private static void AddMilliseconds(
        IDictionary<string, string> values,
        string key,
        TimeSpan? duration)
    {
        if (duration is { } present)
            values[key] = present.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
    }

    private static void ReportProgress(
        MediaAcquisitionProgress value,
        IReadOnlyList<PhysicalDiskTrackAddress> tracks,
        FloppyFluxAcquisitionService fluxService,
        IProgress<PhysicalDiskReadOperationProgress>? progress)
    {
        var cylinder = ReadPosition(value.Metadata, MediaPhysicalMetadataKeys.Cylinder);
        var head = ReadPosition(value.Metadata, MediaPhysicalMetadataKeys.Head);
        IPiste? acquiredTrack = null;
        if (value.AcquiredUnit is not null)
        {
            var track = fluxService.CreateTrack(value.AcquiredUnit);
            acquiredTrack = ScpTrackContractMapper.FromScpTrack(
                track,
                ScpFormatConstants.ResolutionStepNanoseconds
                * (ScpFormatConstants.InternalCaptureResolution + ScpFormatConstants.ResolutionIndexOffset));
        }
        progress?.Report(new PhysicalDiskReadOperationProgress(
            PhysicalDiskReadStage.Acquiring,
            value.CompletedUnits,
            value.TotalUnits,
            cylinder,
            head,
            value.Attempt,
            tracks,
            acquiredTrack));
    }

    private static int ReadPosition(IReadOnlyDictionary<string, string> metadata, string key) =>
        metadata.TryGetValue(key, out var value)
        && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
}
