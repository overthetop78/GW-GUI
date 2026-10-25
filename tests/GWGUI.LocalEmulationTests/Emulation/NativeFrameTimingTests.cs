using GWGUI.Emulation;
using GWGUI.Emulation.Services;

namespace GWGUI.Tests.Emulation;

public sealed class NativeFrameTimingTests
{
    [Fact]
    public void FutureDeadlineSleepsForTheRemainingTimeWithoutResettingTheDeadline()
    {
        var clock = new Clock(110);
        var waits = new List<TimeSpan>();
        Assert.Equal(120, EmulationSdk.MachineExecution.WaitForNextFrame(100, 50, clock, waits.Add));
        Assert.Equal(TimeSpan.FromMilliseconds(10), Assert.Single(waits));
        Assert.Equal(1, clock.Reads);
    }

    [Theory]
    [InlineData(120)]
    [InlineData(500)]
    public void ReachedOrMissedDeadlineResetsFromASecondTimestampWithoutSleeping(long now)
    {
        var clock = new Clock(now, now + 7);
        Assert.Equal(now + 7, EmulationSdk.MachineExecution.WaitForNextFrame(100, 50, clock,
            _ => throw new InvalidOperationException("Unexpected wait")));
        Assert.Equal(2, clock.Reads);
    }

    [Theory]
    [InlineData(-5, 1000)]
    [InlineData(0, 1000)]
    [InlineData(1, 1000)]
    [InlineData(50, 20)]
    [InlineData(1000, 1)]
    [InlineData(2000, 1)]
    public void FrequencyBoundsKeepTheExistingOneToOneThousandRange(double frequency, long interval)
    {
        var clock = new Clock(0);
        var waits = new List<TimeSpan>();
        Assert.Equal(interval, EmulationSdk.MachineExecution.WaitForNextFrame(0, frequency, clock, waits.Add));
        Assert.Equal(TimeSpan.FromMilliseconds(interval), Assert.Single(waits));
    }

    [Fact]
    public void WaitFailureEscapesWithoutReadingAnotherTimestamp()
    {
        var clock = new Clock(110);
        var error = new InvalidOperationException("wait failed");
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
            EmulationSdk.MachineExecution.WaitForNextFrame(100, 50, clock, _ => throw error)));
        Assert.Equal(1, clock.Reads);
    }

    private sealed class Clock(params long[] timestamps) : TimeProvider
    {
        public int Reads { get; private set; }
        public override long TimestampFrequency => 1000;
        public override long GetTimestamp() => timestamps[Reads++];
    }
}
