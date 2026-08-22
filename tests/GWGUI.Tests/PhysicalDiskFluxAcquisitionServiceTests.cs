using GWGUI.App.Contracts.Services.PhysicalDiskReading;
using GWGUI.App.Services.PhysicalDiskReading;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Exploration.Scp;
using System.IO;

namespace GWGUI.Tests;

public sealed class PhysicalDiskFluxAcquisitionServiceTests
{
    [Fact]
    public async Task AcquisitionKeepsRequestedRevolutionsSeparateInMemory()
    {
        var device = new RecordingReadDevice(CreateIndexedCapture());
        var service = new PhysicalDiskFluxAcquisitionService(device);
        var options = new PhysicalDiskReadOptions(
            "COM8",
            GreaseweazleBusType.Shugart,
            0,
            [new PhysicalDiskTrackAddress(0, 0)],
            ScpDiskType.Amiga,
            Revolutions: 3);

        var result = await service.AcquireAsync(options);

        var track = Assert.Single(result.Image.Tracks);
        Assert.Equal(3, track.Revolutions.Count);
        Assert.All(track.Revolutions, revolution => Assert.Equal(8_000_000u, revolution.IndexTimeTicks));
        Assert.All(track.Revolutions, revolution => Assert.Equal(ScpRevolutionOrigin.Captured, revolution.Origin));
        Assert.Same(device.Capture, result.RawCaptures[options.Tracks[0]]);
        Assert.Equal([(0, 0)], device.Seeks);
        Assert.True(device.Closed);
    }

    [Fact]
    public async Task FakeIndexUsesTimedCaptureWithoutChangingRawIndexes()
    {
        var capture = new GreaseweazleFluxCapture(
            Enumerable.Repeat(1_000_000u, 80).ToArray(),
            [],
            40_000_000,
            [0]);
        var device = new RecordingReadDevice(capture);
        var options = new PhysicalDiskReadOptions(
            "COM9",
            GreaseweazleBusType.IbmPc,
            1,
            [new PhysicalDiskTrackAddress(2, 1)],
            ScpDiskType.IbmPc720,
            Revolutions: 2,
            FakeIndexPeriod: TimeSpan.FromMilliseconds(200));

        var result = await new PhysicalDiskFluxAcquisitionService(device).AcquireAsync(options);

        Assert.Equal(0, device.RequestedRevolutions);
        Assert.Equal(16_040_000u, device.RequestedTickLimit);
        Assert.Equal(2, Assert.Single(result.Image.Tracks).Revolutions.Count);
        Assert.Empty(capture.IndexIntervals);
    }

    [Fact]
    public async Task SeekFailuresAreRetriedFromTrackZero()
    {
        var device = new RecordingReadDevice(CreateIndexedCapture()) { SeekFailures = 1 };
        var options = new PhysicalDiskReadOptions(
            "COM10",
            GreaseweazleBusType.IbmPc,
            0,
            [new PhysicalDiskTrackAddress(4, 0)],
            ScpDiskType.IbmPc720,
            Revolutions: 3,
            SeekRetries: 1);

        await new PhysicalDiskFluxAcquisitionService(device).AcquireAsync(options);

        Assert.Equal([(4, 0), (0, 0), (4, 0)], device.Seeks);
    }

    [Fact]
    public void HardSectorIndexesAreGroupedIntoCompleteRevolutions()
    {
        var indexIntervals = new uint[]
        {
            5,
            40, 40, 40, 5, 5,
            40, 40, 40, 5, 5
        };
        var capture = new GreaseweazleFluxCapture([10], indexIntervals, 40_000_000, [0]);

        var layout = GreaseweazleFluxIndexNormalizer.FromHardSectorIndexes(capture, 2);

        Assert.Equal(new uint[] { 130, 130 }, layout.RevolutionTicks);
        Assert.Equal([4, 4], layout.HardSectorCounts);
    }

    private static GreaseweazleFluxCapture CreateIndexedCapture()
    {
        var flux = Enumerable.Repeat(1_000_000u, 25).ToArray();
        return new GreaseweazleFluxCapture(
            flux,
            [1_000_000, 8_000_000, 8_000_000, 8_000_000],
            40_000_000,
            [0]);
    }

    private sealed class RecordingReadDevice(GreaseweazleFluxCapture capture) : IGreaseweazleReadDevice
    {
        private int _seekFailures;

        public GreaseweazleFluxCapture Capture { get; } = capture;
        public int SeekFailures { get => _seekFailures; init => _seekFailures = value; }
        public List<(int Cylinder, int Head)> Seeks { get; } = [];
        public int RequestedRevolutions { get; private set; }
        public uint RequestedTickLimit { get; private set; }
        public bool Closed { get; private set; }
        public GreaseweazleFirmwareInfo? Firmware { get; private set; }

        public ValueTask<GreaseweazleFirmwareInfo> OpenAsync(string portName, CancellationToken cancellationToken = default)
        {
            Firmware = new(1, 6, 22, Capture.SampleFrequency, 7, 1, 1, 4, 144, 224, 64, true);
            return ValueTask.FromResult(Firmware);
        }

        public ValueTask SetBusTypeAsync(GreaseweazleBusType busType, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask SelectDriveAsync(byte unit, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask SetMotorAsync(bool enabled, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask SeekAsync(short cylinder, byte head, CancellationToken cancellationToken = default)
        {
            Seeks.Add((cylinder, head));
            if (_seekFailures-- > 0) throw new IOException("Transient seek failure.");
            return ValueTask.CompletedTask;
        }

        public ValueTask<GreaseweazleFluxCapture> ReadFluxAsync(int revolutions, uint tickLimit = 0, int retries = 5, CancellationToken cancellationToken = default)
        {
            RequestedRevolutions = revolutions;
            RequestedTickLimit = tickLimit;
            return ValueTask.FromResult(Capture);
        }

        public ValueTask ResetAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask CloseAsync(CancellationToken cancellationToken = default)
        {
            Closed = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
