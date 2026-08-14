using System.IO;
using GWGUI.Emulation;
using GWGUI.Emulation.Amiga;
using GWGUI.Emulation.Amiga.Cores;

namespace GWGUI.Tests;

public sealed class AmigaMachineLifecycleTests
{
    [Fact]
    public async Task CommandWhilePaused_DoesNotAdvanceTheCore()
    {
        var core = new FakeCore();
        await using var machine = CreateMachine(core);
        await machine.StartAsync();
        await WaitUntil(() => core.FrameCount >= 3);
        await machine.PauseAsync();
        await Task.Delay(50);
        var pausedAt = core.FrameCount;

        await machine.SetOptionAsync("test", "value");
        await Task.Delay(80);

        Assert.Equal(pausedAt, core.FrameCount);
        Assert.Equal("value", core.OptionValue);
        await machine.ResumeAsync();
        await WaitUntil(() => core.FrameCount > pausedAt);
    }

    [Fact]
    public async Task RunFault_IsReportedAndMachineCanStillBeDisposed()
    {
        var core = new FakeCore { FailAtFrame = 2 };
        var machine = CreateMachine(core);
        await machine.StartAsync();
        await WaitUntil(() => machine.State == EmulationMachineState.Faulted);

        Assert.IsType<InvalidOperationException>(machine.Fault);
        await machine.StopAsync();
        await machine.DisposeAsync();
        Assert.True(core.Stopped);
        Assert.True(core.Disposed);
    }

    [Fact]
    public async Task EjectingDisk_ProducesAStateWithoutMediaHash()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Lifecycle", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var rom = Path.Combine(root, "kick.rom");
        var disk = Path.Combine(root, "disk.adf");
        var state = Path.Combine(root, "state.gwas");
        await File.WriteAllBytesAsync(rom, [1, 2, 3]);
        await File.WriteAllBytesAsync(disk, [4, 5, 6]);
        var core = new FakeCore();
        await using var machine = new AmigaMachine(Guid.NewGuid(),
            AmigaMachineConfiguration.A500(rom, disk), core, Path.Combine(root, "session"));
        try
        {
            await machine.StartAsync();
            await machine.EjectFloppyAsync();
            await machine.SaveStateAsync(state);
            Assert.Null(AmigaStateStore.Read(state).Header.MediaSha256);
        }
        finally
        {
            await machine.StopAsync();
            Directory.Delete(root, true);
        }
    }

    private static AmigaMachine CreateMachine(FakeCore core) => new(Guid.NewGuid(),
        AmigaMachineConfiguration.A500(@"C:\kick.rom"), core,
        Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Lifecycle", Guid.NewGuid().ToString("N")));

    private static async Task WaitUntil(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!condition()) await Task.Delay(10, timeout.Token);
    }

    private sealed class FakeCore : IAmigaCore
    {
        public int FrameCount { get; private set; }
        public int FailAtFrame { get; init; } = int.MaxValue;
        public string? OptionValue { get; private set; }
        public bool Stopped { get; private set; }
        public bool Disposed { get; private set; }
        public VideoFrame? LatestVideoFrame => null;
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<AmigaCoreOption> Options => [];
        public string CoreSha256 => "fake-core";
        public double FramesPerSecond => 200;
        public int SampleRate => 44_100;
        public void Initialize(AmigaMachineConfiguration configuration, string sessionDirectory) { }
        public void RunFrame()
        {
            FrameCount++;
            if (FrameCount >= FailAtFrame) throw new InvalidOperationException("Synthetic core failure.");
        }
        public void HardReset() { }
        public void Stop() => Stopped = true;
        public void SetInput(EmulationInputSnapshot snapshot) { }
        public void InsertFloppy(string path) { }
        public void EjectFloppy() { }
        public byte[] SaveState() => [9, 8, 7];
        public void LoadState(ReadOnlySpan<byte> state) { }
        public void SetOption(string key, string value) => OptionValue = value;
        public void Dispose() => Disposed = true;
    }
}
