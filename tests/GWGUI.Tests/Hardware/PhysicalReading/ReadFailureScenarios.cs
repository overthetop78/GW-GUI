using MediaPhysicalMetadataKeys = global::GWGUI.MediaEngine.Constants.MediaPhysicalMetadataKeys;
using GWGUI.MediaEngine.Acquisition;

namespace GWGUI.Tests.Hardware.PhysicalReading;

internal static class ReadFailureScenarios
{
    public static async Task PartialInvalidCapture()
    {
        var device = new ReadAcquisitionScenarios.Device
        {
            Read = (attempt, _) => attempt == 1
                ? ReadAcquisitionScenarios.Device.Capture()
                : new([1, 2], [], 1000, [])
        };
        var reports = new ReadAcquisitionScenarios.Reports();
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            ReadAcquisitionScenarios.AcquireAsync(device, ReadAcquisitionScenarios.Options, reports));
        Assert.Equal(3, device.Calls.Count(call => call.StartsWith("read:")));
        Assert.Equal("close", device.Calls[^1]);
        var completed = Assert.Single(reports.Values, report => report.AcquiredUnit is not null);
        Assert.Equal(1, completed.CompletedUnits);
        Assert.Equal("2", completed.AcquiredUnit!.Metadata[MediaPhysicalMetadataKeys.Cylinder]);
        Assert.Equal("1", completed.AcquiredUnit.Metadata[MediaPhysicalMetadataKeys.Head]);
        var track = new FloppyFluxAcquisitionService().CreateTrack(completed.AcquiredUnit);
        Assert.Equal(5, track.TrackNumber);
        Assert.Equal(new uint[] { 2000000, 2000000 }, Assert.Single(track.Revolutions).FluxIntervals);
    }

    public static async Task Retry()
    {
        var device = new ReadAcquisitionScenarios.Device
        {
            Read = (attempt, _) => attempt == 1
                ? throw new IOException("synthetic overflow")
                : ReadAcquisitionScenarios.Device.Capture()
        };
        var progress = new ReadAcquisitionScenarios.Reports();
        var acquisition = await ReadAcquisitionScenarios.AcquireAsync(
            device,
            ReadAcquisitionScenarios.Options with { Tracks = [new(2, 1, 4, 0)] },
            progress);
        Assert.Single(acquisition.DataUnits);
        Assert.Equal(new[] { "seek:4:0", "seek:0:0", "seek:4:0" }, device.Calls.Where(call => call.StartsWith("seek:")));
        Assert.Equal(new[] { 1, 2, 2 }, progress.Values.Select(value => value.Attempt));
        Assert.Equal("close", device.Calls.Last());
    }

    public static async Task Failure(bool cancel)
    {
        using var source = new CancellationTokenSource();
        var error = new IOException("synthetic device disconnected");
        var device = new ReadAcquisitionScenarios.Device
        {
            Read = (_, token) =>
            {
                if (cancel)
                {
                    source.Cancel();
                    token.ThrowIfCancellationRequested();
                }
                throw error;
            }
        };
        var task = ReadAcquisitionScenarios.AcquireAsync(
            device,
            ReadAcquisitionScenarios.Options,
            cancellationToken: source.Token);
        if (cancel) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        else Assert.Same(error, await Assert.ThrowsAsync<IOException>(() => task));
        Assert.Equal(cancel ? 1 : 2, device.Calls.Count(call => call.StartsWith("read:")));
        Assert.Equal("close", device.Calls.Last());
        if (cancel) Assert.DoesNotContain("seek:0:0", device.Calls);
    }
}
