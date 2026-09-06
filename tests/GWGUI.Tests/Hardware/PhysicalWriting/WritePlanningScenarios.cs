using GWGUI.App.Contracts.Services.PhysicalDiskWriting;
using GWGUI.App.Enums.Services.PhysicalDiskWriting;
using GWGUI.App.Services.PhysicalDiskWriting;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.Tests.Hardware.PhysicalWriting;
internal static class WritePlanningScenarios
{
    internal static PhysicalDiskWriteOptions Options => new("virtual", (GreaseweazleBusType)1, 1, CueAtIndex: false, HardSectorTicks: 200);
    internal static ScpImage Image => new(new ScpHeader(0x24,0,1,0,3,ScpFlags.None,ScpBitCellEncoding.Default16Bit,ScpHeadSelection.Both,0,0),
        [new ScpTrack(3,1,1,[new ScpRevolution(1000,2,new uint[]{80,160})]), new ScpTrack(0,0,0,[new ScpRevolution(1000,2,new uint[]{40,120})])],true,0);
    public static async Task Order()
    {
        var device = new Device();
        var result = await new PhysicalDiskWriteService(device).WriteAsync(Image, Options);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.WrittenTracks);
        Assert.Equal(new[] { "open:virtual", "bus:1", "select:1", "motor:True", "seek:0:0", "write", "seek:1:1", "write", "close" }, device.Calls);
        Assert.Equal(new uint[] { 24, 72 }, device.Writes[0]);
        Assert.Equal(new uint[] { 48, 96 }, device.Writes[1]);
    }
    public static async Task Invalid(bool verify)
    {
        var device = new Device();
        var options = verify ? Options with { Verify = true } : Options with { PortName = " " };
        var result = await new PhysicalDiskWriteService(device).WriteAsync(Image, options);
        Assert.False(result.IsSuccess);
        Assert.Equal(0, result.WrittenTracks);
        Assert.Equal(PhysicalDiskWriteFailureCategory.Validation, Assert.Single(result.Failures).Category);
        Assert.Empty(device.Calls);
    }
    internal sealed class Device : IGreaseweazleWriteDevice
    {
        public List<string> Calls { get; } = [];
        public List<uint[]> Writes { get; } = [];
        public Action<int, CancellationToken>? OnWrite { get; init; }
        public GreaseweazleFirmwareInfo? Firmware { get; } = new(1,0,30,24000000,0,0,0,0,0,0,0,true);
        public ValueTask<GreaseweazleFirmwareInfo> OpenAsync(string portName, CancellationToken cancellationToken = default) { Calls.Add($"open:{portName}"); return ValueTask.FromResult(Firmware!); }
        public ValueTask SetBusTypeAsync(GreaseweazleBusType busType, CancellationToken cancellationToken = default) { Calls.Add($"bus:{(int)busType}"); return ValueTask.CompletedTask; }
        public ValueTask SelectDriveAsync(byte unit, CancellationToken cancellationToken = default) { Calls.Add($"select:{unit}"); return ValueTask.CompletedTask; }
        public ValueTask SetMotorAsync(bool enabled, CancellationToken cancellationToken = default) { Calls.Add($"motor:{enabled}"); return ValueTask.CompletedTask; }
        public ValueTask SeekAsync(short cylinder, byte head, CancellationToken cancellationToken = default) { Calls.Add($"seek:{cylinder}:{head}"); return ValueTask.CompletedTask; }
        public ValueTask ResetAsync(CancellationToken cancellationToken = default) => throw new InvalidOperationException();
        public ValueTask CloseAsync(CancellationToken cancellationToken = default) { Assert.False(cancellationToken.IsCancellationRequested); Calls.Add("close"); return ValueTask.CompletedTask; }
        public ValueTask DisposeAsync() => throw new InvalidOperationException();
        public ValueTask WriteFluxAsync(ReadOnlyMemory<uint> intervals, bool cueAtIndex, bool terminateAtIndex, uint hardSectorTicks = 0, CancellationToken cancellationToken = default)
        {
            Assert.False(cueAtIndex); Assert.True(terminateAtIndex); Assert.Equal(200u, hardSectorTicks);
            Calls.Add("write"); Writes.Add(intervals.ToArray()); OnWrite?.Invoke(Writes.Count, cancellationToken);
            return ValueTask.CompletedTask;
        }
    }
}
