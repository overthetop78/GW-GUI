using GWGUI.App.Constants.Services.PhysicalDiskReading;
using GWGUI.App.Contracts.Services.PhysicalDiskReading;
using GWGUI.App.Functions.Services.PhysicalDiskReading;
using GWGUI.Infrastructure.Hardware.Greaseweazle;

namespace GWGUI.App.Services.PhysicalDiskReading;

public sealed class PhysicalDiskFluxAcquisitionService(IGreaseweazleReadDevice device)
{
    public async Task<PhysicalDiskFluxAcquisition> AcquireAsync(
        PhysicalDiskReadOptions options,
        IProgress<PhysicalDiskReadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Validate(options);
        var captures = new Dictionary<PhysicalDiskTrackAddress, GreaseweazleFluxCapture>();
        var normalized = new Dictionary<PhysicalDiskTrackAddress, (GreaseweazleFluxCapture, GreaseweazleRotationLayout)>();
        try
        {
            var firmware = await device.OpenAsync(options.PortName, cancellationToken);
            await device.SetBusTypeAsync(options.BusType, cancellationToken);
            await device.SelectDriveAsync(options.DriveUnit, cancellationToken);
            await device.SetMotorAsync(true, cancellationToken);
            if (options.MotorSpinUpDelay is { } spinUp) await Task.Delay(spinUp, cancellationToken);

            var completed = 0;
            foreach (var track in options.Tracks)
            {
                var pair = await AcquireTrackAsync(track, options, firmware.SampleFrequency, progress, completed, cancellationToken);
                captures.Add(track, pair.Capture);
                normalized.Add(track, (pair.Capture, pair.Layout));
                completed++;
                var capturedTrack = GreaseweazleScpImageBuilder.BuildTrack(
                    track,
                    pair.Capture,
                    pair.Layout,
                    options.Revolutions);
                progress?.Report(new(
                    completed,
                    options.Tracks.Count,
                    track.Cylinder,
                    track.Head,
                    pair.Attempt,
                    capturedTrack));
            }

            var image = GreaseweazleScpImageBuilder.Build(normalized, options);
            return new PhysicalDiskFluxAcquisition(image, captures);
        }
        finally
        {
            await device.CloseAsync(CancellationToken.None);
        }
    }

    private async Task<(GreaseweazleFluxCapture Capture, GreaseweazleRotationLayout Layout, int Attempt)> AcquireTrackAsync(
        PhysicalDiskTrackAddress track,
        PhysicalDiskReadOptions options,
        uint sampleFrequency,
        IProgress<PhysicalDiskReadProgress>? progress,
        int completed,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await device.SeekAsync(checked((short)track.DriveCylinder), checked((byte)track.DriveHead), cancellationToken);
                if (options.TrackSettleDelay is { } settle) await Task.Delay(settle, cancellationToken);
                progress?.Report(new(completed, options.Tracks.Count, track.Cylinder, track.Head, attempt));
                var capture = await ReadTrackAsync(options, sampleFrequency, cancellationToken);
                return (capture, CreateLayout(capture, options), attempt);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch when (attempt <= options.SeekRetries)
            {
                await device.SeekAsync(0, 0, cancellationToken);
            }
        }
    }

    private async ValueTask<GreaseweazleFluxCapture> ReadTrackAsync(
        PhysicalDiskReadOptions options,
        uint sampleFrequency,
        CancellationToken cancellationToken)
    {
        if (options.FakeIndexPeriod is { } fakeIndex)
        {
            var revolutionTicks = DurationToTicks(fakeIndex, sampleFrequency);
            var leadTicks = checked((uint)Math.Max(1, Math.Round(sampleFrequency * GreaseweazleProtocol.FakeIndexLeadSeconds)));
            var captureTicks = checked(revolutionTicks * (uint)options.Revolutions + leadTicks * 2);
            return await device.ReadFluxAsync(0, captureTicks, options.FluxOverflowRetries, cancellationToken);
        }
        if (options.HardSectors)
        {
            var captureTicks = DurationToTicks(TimeSpan.FromSeconds(PhysicalDiskReadDefaults.HardSectorCaptureSeconds), sampleFrequency);
            return await device.ReadFluxAsync(0, captureTicks, options.FluxOverflowRetries, cancellationToken);
        }
        return await device.ReadFluxAsync(options.Revolutions, retries: options.FluxOverflowRetries, cancellationToken: cancellationToken);
    }

    private static GreaseweazleRotationLayout CreateLayout(
        GreaseweazleFluxCapture capture,
        PhysicalDiskReadOptions options)
    {
        if (options.FakeIndexPeriod is { } fakeIndex)
            return GreaseweazleFluxIndexNormalizer.FromFakeIndex(capture, options.Revolutions, DurationToTicks(fakeIndex, capture.SampleFrequency));
        return options.HardSectors
            ? GreaseweazleFluxIndexNormalizer.FromHardSectorIndexes(capture, options.Revolutions)
            : GreaseweazleFluxIndexNormalizer.FromPhysicalIndexes(capture, options.Revolutions);
    }

    private static uint DurationToTicks(TimeSpan duration, uint sampleFrequency)
    {
        if (duration <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(duration));
        return checked((uint)Math.Round(duration.TotalSeconds * sampleFrequency));
    }

    private static void Validate(PhysicalDiskReadOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.PortName);
        ArgumentNullException.ThrowIfNull(options.Tracks);
        if (options.Tracks.Count == 0) throw new ArgumentException("At least one track must be selected.", nameof(options));
        if (options.Revolutions is <= 0 or > byte.MaxValue) throw new ArgumentOutOfRangeException(nameof(options));
        if (options.FluxOverflowRetries < 0 || options.SeekRetries < 0) throw new ArgumentOutOfRangeException(nameof(options));
        if (options.FakeIndexPeriod is not null && options.HardSectors)
            throw new ArgumentException("Fake index and hard-sector acquisition are mutually exclusive.", nameof(options));
        if (options.Tracks.Any(track => track.Cylinder is < 0 or > 83 || track.Head is < 0 or > 1 || track.DriveCylinder is < 0 or > short.MaxValue || track.DriveHead is < 0 or > byte.MaxValue))
            throw new ArgumentOutOfRangeException(nameof(options), "A selected track is outside the supported range.");
        if (options.Tracks.GroupBy(track => (track.Cylinder, track.Head)).Any(group => group.Count() > 1))
            throw new ArgumentException("A logical track is selected more than once.", nameof(options));
    }
}
