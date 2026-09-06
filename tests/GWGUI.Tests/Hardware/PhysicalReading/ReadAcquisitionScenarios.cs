using GWGUI.App.Contracts.Services.PhysicalDiskReading;
using GWGUI.App.Services.PhysicalDiskReading;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.Tests.Hardware.PhysicalReading;

internal static class ReadAcquisitionScenarios
{
    public static async Task IndexModes(bool hard)
    {
        var device=new Device();
        device.Read=(_,_)=>hard?new(new uint[]{50,100,100,100,40,40},new uint[]{50,100,100,100,40,40},1000,[]):new(new uint[]{1,50,50,50},[],1000,[]);
        var options=Options with{Tracks=[new(0,1)],HardSectors=hard,FakeIndexPeriod=hard?null:TimeSpan.FromMilliseconds(100),FluxOverflowRetries=2};
        var result=await new PhysicalDiskFluxAcquisitionService(device).AcquireAsync(options);
        var revolution=Assert.Single(Assert.Single(result.Image.Tracks).Revolutions);
        Assert.Equal(hard?15200000u:4000000u,revolution.IndexTimeTicks);
        Assert.Equal((ulong)revolution.IndexTimeTicks,revolution.FluxIntervals.Aggregate(0ul,(sum,value)=>sum+value));
        Assert.Contains(hard?"read:0:2000:2":"read:0:102:2",device.Calls); Assert.Equal("close",device.Calls[^1]);
    }
    internal static PhysicalDiskReadOptions Options => new("virtual-port", (GreaseweazleBusType)1, 1,
        [new(2, 1, 4, 0), new(0, 0)], (ScpDiskType)0, Revolutions: 1, SeekRetries: 1);

    public static async Task Acquire()
    {
        var device = new Device();
        var progress = new Reports();
        var result = await new PhysicalDiskFluxAcquisitionService(device).AcquireAsync(Options, progress);
        Assert.Equal(new[] { "open:virtual-port", "bus:1", "select:1", "motor:True", "seek:4:0", "read:1:0:5", "seek:0:0", "read:1:0:5", "close" }, device.Calls);
        Assert.Equal(new[] { 0, 5 }, result.Image.Tracks.Select(track => (int)track.TrackNumber));
        Assert.Equal(2, result.RawCaptures.Count);
        Assert.All(result.Image.Tracks, track => {
            var revolution = Assert.Single(track.Revolutions);
            Assert.Equal(4000000u, revolution.IndexTimeTicks);
            Assert.Equal(new uint[] { 2000000, 2000000 }, revolution.FluxIntervals);
        });
        Assert.Equal(new[] { 0, 1, 1, 2 }, progress.Values.Select(value => value.CompletedTracks));
        Assert.All(progress.Values, value => Assert.Equal(2, value.TotalTracks));
        Assert.Equal(2, progress.Values.Count(value => value.CapturedTrack is not null));
    }

    internal sealed class Reports : IProgress<PhysicalDiskReadProgress>
    {
        public List<PhysicalDiskReadProgress> Values { get; } = [];
        public void Report(PhysicalDiskReadProgress value) => Values.Add(value);
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
        public ValueTask DisposeAsync() => throw new InvalidOperationException("Device lifetime belongs to the caller");
        public ValueTask<GreaseweazleFluxCapture> ReadFluxAsync(int revolutions, uint tickLimit = 0, int retries = 5, CancellationToken cancellationToken = default)
        {
            Calls.Add($"read:{revolutions}:{tickLimit}:{retries}");
            return ValueTask.FromResult(Read?.Invoke(++reads, cancellationToken) ?? Capture());
        }
        internal static GreaseweazleFluxCapture Capture() => new(new uint[] { 50, 50, 50 }, new uint[] { 50, 100 }, 1000, []);
    }
}
