using System.Globalization;
using GWGUI.App.Contracts.Services.PhysicalDiskReading;
using MediaAcquisitionProgress = global::GWGUI.MediaEngine.Contracts.MediaAcquisitionProgress;
using MediaAcquisitionResult = global::GWGUI.MediaEngine.Contracts.MediaAcquisitionResult;
using GWGUI.MediaEngine.Enums;
using GWGUI.Infrastructure.Constants;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Reading;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;

namespace GWGUI.Tests.Hardware.PhysicalReading;

internal static class ReadAcquisitionScenarios
{
    public static async Task IndexModes(bool hard)
    {
        var device = new Device();
        device.Read = (_, _) => hard
            ? new(new uint[] { 50, 100, 100, 100, 40, 40 }, new uint[] { 50, 100, 100, 100, 40, 40 }, 1000, [])
            : new(new uint[] { 1, 50, 50, 50 }, [], 1000, []);
        var options = Options with
        {
            Tracks = [new(0, 1)],
            HardSectors = hard,
            FakeIndexPeriod = hard ? null : TimeSpan.FromMilliseconds(100),
            FluxOverflowRetries = 2
        };
        var acquisition = await AcquireAsync(device, options);
        var image = new FloppyFluxAcquisitionService().CreateScpImage(acquisition);
        var revolution = Assert.Single(Assert.Single(image.Tracks).Revolutions);
        Assert.Equal(hard ? 15200000u : 4000000u, revolution.IndexTimeTicks);
        Assert.Equal((ulong)revolution.IndexTimeTicks, revolution.FluxIntervals.Aggregate(0ul, (sum, value) => sum + value));
        Assert.Contains(hard ? "read:0:2000:2" : "read:0:102:2", device.Calls);
        Assert.Equal("close", device.Calls[^1]);
    }

    internal static PhysicalDiskReadOptions Options => new(
        "virtual-port",
        (GreaseweazleBusType)1,
        1,
        [new(2, 1, 4, 0), new(0, 0)],
        (ScpDiskType)0,
        Revolutions: 1,
        SeekRetries: 1);

    public static async Task Acquire()
    {
        var device = new Device();
        var progress = new Reports();
        var acquisition = await AcquireAsync(device, Options, progress);
        var image = new FloppyFluxAcquisitionService().CreateScpImage(acquisition);
        Assert.Equal(new[] { "open:virtual-port", "bus:1", "select:1", "motor:True", "seek:4:0", "read:1:0:5", "seek:0:0", "read:1:0:5", "close" }, device.Calls);
        Assert.Equal(new[] { 0, 5 }, image.Tracks.Select(track => (int)track.TrackNumber));
        Assert.Equal(2, acquisition.DataUnits.Count);
        Assert.All(image.Tracks, track =>
        {
            var revolution = Assert.Single(track.Revolutions);
            Assert.Equal(4000000u, revolution.IndexTimeTicks);
            Assert.Equal(new uint[] { 2000000, 2000000 }, revolution.FluxIntervals);
        });
        Assert.Equal(new[] { 0, 1, 1, 2 }, progress.Values.Select(value => value.CompletedUnits));
        Assert.All(progress.Values, value => Assert.Equal(2, value.TotalUnits));
        Assert.Equal(2, progress.Values.Count(value => value.AcquiredUnit is not null));
    }

    internal static Task<MediaAcquisitionResult> AcquireAsync(
        Device device,
        PhysicalDiskReadOptions options,
        IProgress<MediaAcquisitionProgress>? progress = null,
        CancellationToken cancellationToken = default) =>
        new GreaseweazleMediaAcquisitionProvider(() => device).AcquireAsync(
            MediaKind.Floppy,
            options.PortName,
            CreateProviderOptions(options),
            progress,
            cancellationToken);

    internal static IReadOnlyDictionary<string, string> CreateProviderOptions(PhysicalDiskReadOptions options)
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

    private static void AddMilliseconds(IDictionary<string, string> values, string key, TimeSpan? duration)
    {
        if (duration is { } present)
            values[key] = present.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
    }

    internal sealed class Reports : IProgress<MediaAcquisitionProgress>
    {
        public List<MediaAcquisitionProgress> Values { get; } = [];

        public void Report(MediaAcquisitionProgress value) => Values.Add(value);
    }

    internal sealed class Device : IGreaseweazleReadDevice
    {
        public List<string> Calls { get; } = [];
        public Func<int, CancellationToken, GreaseweazleFluxCapture>? Read { get; set; }
        private int reads;
        public GreaseweazleFirmwareInfo? Firmware { get; } = new(1, 0, 30, 1000, 0, 0, 0, 0, 0, 0, 0, true);
        public ValueTask<GreaseweazleFirmwareInfo> OpenAsync(string portName, CancellationToken cancellationToken = default) { Calls.Add($"open:{portName}"); return ValueTask.FromResult(Firmware!); }
        public ValueTask SetBusTypeAsync(GreaseweazleBusType busType, CancellationToken cancellationToken = default) { Calls.Add($"bus:{(int)busType}"); return ValueTask.CompletedTask; }
        public ValueTask SelectDriveAsync(byte unit, CancellationToken cancellationToken = default) { Calls.Add($"select:{unit}"); return ValueTask.CompletedTask; }
        public ValueTask SetMotorAsync(bool enabled, CancellationToken cancellationToken = default) { Calls.Add($"motor:{enabled}"); return ValueTask.CompletedTask; }
        public ValueTask SeekAsync(short cylinder, byte head, CancellationToken cancellationToken = default) { Calls.Add($"seek:{cylinder}:{head}"); return ValueTask.CompletedTask; }
        public ValueTask ResetAsync(CancellationToken cancellationToken = default) => throw new InvalidOperationException("Unexpected reset");
        public ValueTask CloseAsync(CancellationToken cancellationToken = default) { Assert.False(cancellationToken.IsCancellationRequested); Calls.Add("close"); return ValueTask.CompletedTask; }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        public ValueTask<GreaseweazleFluxCapture> ReadFluxAsync(int revolutions, uint tickLimit = 0, int retries = 5, CancellationToken cancellationToken = default)
        {
            Calls.Add($"read:{revolutions}:{tickLimit}:{retries}");
            return ValueTask.FromResult(Read?.Invoke(++reads, cancellationToken) ?? Capture());
        }

        internal static GreaseweazleFluxCapture Capture() => new(new uint[] { 50, 50, 50 }, new uint[] { 50, 100 }, 1000, []);
    }
}
