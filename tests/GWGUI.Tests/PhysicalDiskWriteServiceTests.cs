using GWGUI.App.Services.PhysicalDiskWriting;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.Tests;

public sealed class PhysicalDiskWriteServiceTests
{
    [Fact]
    public async Task ServiceWritesScpTracksInPhysicalOrderAndReportsProgress()
    {
        var device = new RecordingWriteDevice();
        var service = new PhysicalDiskWriteService(device);
        var progress = new List<PhysicalTrackWriteProgress>();
        var image = CreateScpImage(
            new ScpTrack(3, 1, 1, [new ScpRevolution(300, 2, [100u, 200u])]),
            new ScpTrack(0, 0, 0, [new ScpRevolution(300, 2, [125u, 175u])]));

        var result = await service.WriteAsync(
            image,
            Options(),
            new ImmediateProgress<PhysicalTrackWriteProgress>(progress.Add));

        Assert.True(result.IsSuccess);
        Assert.Equal([(0, 0), (1, 1)], device.Seeks);
        Assert.Equal([125u, 175u], device.Writes[0]);
        Assert.Equal([100u, 200u], device.Writes[1]);
        Assert.Equal(2, progress.Count);
        Assert.False(device.IsOpen);
    }

    [Fact]
    public async Task ServiceAppliesPrecompensationToEncodedTracks()
    {
        var device = new RecordingWriteDevice();
        var service = new PhysicalDiskWriteService(device);
        var bits = new[] { true, false, true, false, false, true };
        var encoded = new EncodedTrack("test", bits, new FluxRevolution(600, [100u, 200u, 300u]));
        var track = new EncodedDiskTrack(40, 0, 100, encoded);
        var options = Options() with
        {
            Precompensation = [new PhysicalWritePrecompensationStep(40, 25)]
        };

        var result = await service.WriteAsync([track], options);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(new uint[] { 100, 200, 300 }, device.Writes.Single());
    }

    [Fact]
    public async Task ServiceReturnsStructuredWriteProtectedFailureAndClosesDevice()
    {
        var device = new RecordingWriteDevice { WriteFailure = new GreaseweazleProtocolException(
            GreaseweazleCommand.WriteFlux,
            GreaseweazleAcknowledgement.WriteProtected) };
        var service = new PhysicalDiskWriteService(device);

        var result = await service.WriteAsync(CreateScpImage(
            new ScpTrack(0, 0, 0, [new ScpRevolution(300, 2, [100u, 200u])])), Options());

        Assert.False(result.IsSuccess);
        Assert.Equal(PhysicalDiskWriteFailureCategory.WriteProtected, Assert.Single(result.Failures).Category);
        Assert.False(device.IsOpen);
    }

    [Fact]
    public async Task ServiceStopsAfterCancellationAndClosesDevice()
    {
        var device = new RecordingWriteDevice();
        var service = new PhysicalDiskWriteService(device);
        using var cancellation = new CancellationTokenSource();
        var image = CreateScpImage(
            new ScpTrack(0, 0, 0, [new ScpRevolution(300, 2, [100u, 200u])]),
            new ScpTrack(2, 1, 0, [new ScpRevolution(300, 2, [100u, 200u])]));

        var result = await service.WriteAsync(
            image,
            Options(),
            new ImmediateProgress<PhysicalTrackWriteProgress>(_ => cancellation.Cancel()),
            cancellation.Token);

        Assert.True(result.Cancelled);
        Assert.Equal(1, result.WrittenTracks);
        Assert.Single(device.Writes);
        Assert.False(device.IsOpen);
    }

    [Fact]
    public async Task VerificationMustBeAvailableAndCanRejectATrack()
    {
        var image = CreateScpImage(new ScpTrack(0, 0, 0, [new ScpRevolution(300, 2, [100u, 200u])]));
        var unavailable = await new PhysicalDiskWriteService(new RecordingWriteDevice())
            .WriteAsync(image, Options() with { Verify = true });
        Assert.Equal(PhysicalDiskWriteFailureCategory.Validation, Assert.Single(unavailable.Failures).Category);

        var rejected = await new PhysicalDiskWriteService(new RecordingWriteDevice(), new RejectingVerifier())
            .WriteAsync(image, Options() with { Verify = true });
        Assert.Equal(PhysicalDiskWriteFailureCategory.Verification, Assert.Single(rejected.Failures).Category);
    }

    private static PhysicalDiskWriteOptions Options() =>
        new("COM3", GreaseweazleBusType.Shugart, 0);

    private static ScpImage CreateScpImage(params ScpTrack[] tracks) => new(
        new ScpHeader(0x24, 0, 1, 0, 3, ScpFlags.IndexAligned, ScpBitCellEncoding.Default16Bit, ScpHeadSelection.Both, 0, 0),
        tracks,
        true,
        1024);

    private sealed class RejectingVerifier : IPhysicalTrackVerifier
    {
        public ValueTask<bool> VerifyAsync(
            int cylinder,
            int head,
            ReadOnlyMemory<uint> expectedDeviceTicks,
            CancellationToken cancellationToken = default) => ValueTask.FromResult(false);
    }

    private sealed class RecordingWriteDevice : IGreaseweazleWriteDevice
    {
        public GreaseweazleFirmwareInfo? Firmware { get; private set; }
        public bool IsOpen { get; private set; }
        public List<(int Cylinder, int Head)> Seeks { get; } = [];
        public List<uint[]> Writes { get; } = [];
        public Exception? WriteFailure { get; init; }

        public ValueTask<GreaseweazleFirmwareInfo> OpenAsync(
            string portName,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IsOpen = true;
            Firmware = new(1, 6, 22, 40_000_000, 7, 1, 1, 4, 144, 224, 64, true);
            return ValueTask.FromResult(Firmware);
        }

        public ValueTask SetBusTypeAsync(GreaseweazleBusType busType, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask SelectDriveAsync(byte unit, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask SetMotorAsync(bool enabled, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask SeekAsync(short cylinder, byte head, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Seeks.Add((cylinder, head));
            return ValueTask.CompletedTask;
        }

        public ValueTask WriteFluxAsync(
            ReadOnlyMemory<uint> intervals,
            bool cueAtIndex,
            bool terminateAtIndex,
            uint hardSectorTicks = 0,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (WriteFailure is not null) throw WriteFailure;
            Writes.Add(intervals.ToArray());
            return ValueTask.CompletedTask;
        }

        public ValueTask ResetAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask CloseAsync(CancellationToken cancellationToken = default)
        {
            IsOpen = false;
            Firmware = null;
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            IsOpen = false;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ImmediateProgress<T>(Action<T> action) : IProgress<T>
    {
        public void Report(T value) => action(value);
    }
}
