using System.IO;
using System.Runtime.InteropServices;
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
    public async Task AudioDeviceFailure_DoesNotStopTheMachine()
    {
        var core = new FakeCore { ProduceAudio = true };
        var output = new FailingAudioOutput();
        await using var machine = new AmigaMachine(Guid.NewGuid(), AmigaMachineConfiguration.A500(@"C:\kick.rom"),
            core, Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Lifecycle", Guid.NewGuid().ToString("N")), output);
        await machine.StartAsync();
        await WaitUntil(() => output.Disposed);
        var framesAfterFailure = core.FrameCount;
        await WaitUntil(() => core.FrameCount > framesAfterFailure + 2);
        Assert.Equal(EmulationMachineState.Running, machine.State);
    }

    [Fact]
    public async Task Dispose_RemovesTransientSessionButKeepsPersistentSaves()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Storage", Guid.NewGuid().ToString("N"));
        var session = Path.Combine(root, "Sessions", Guid.NewGuid().ToString("N"));
        var saves = Path.Combine(root, "Configurations", Guid.NewGuid().ToString("N"), "Saves");
        var core = new FakeCore { CreatePersistentFiles = true };
        var machine = new AmigaMachine(Guid.NewGuid(), AmigaMachineConfiguration.A500(@"C:\kick.rom"), core, session,
            saveDirectory: saves);
        try
        {
            await machine.StartAsync();
            await machine.DisposeAsync();
            Assert.False(Directory.Exists(session));
            Assert.True(File.Exists(Path.Combine(saves, "persistent.sav")));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
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

    [Fact]
    public async Task StateV2_RejectsCorruptionAndChangedOptions()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-State", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var rom = Path.Combine(root, "kick.rom");
        var statePath = Path.Combine(root, "state.gwas");
        await File.WriteAllBytesAsync(rom, [1, 2, 3]);
        var machine = new AmigaMachine(Guid.NewGuid(), AmigaMachineConfiguration.A500(rom), new FakeCore(), Path.Combine(root, "session"));
        try
        {
            await machine.StartAsync();
            await machine.SaveStateAsync(statePath);
            var original = await File.ReadAllBytesAsync(statePath);
            var corrupted = original.ToArray();
            corrupted[^1] ^= 0xff;
            await File.WriteAllBytesAsync(statePath, corrupted);
            await Assert.ThrowsAsync<InvalidDataException>(() => machine.LoadStateAsync(statePath).AsTask());

            await File.WriteAllBytesAsync(statePath, original);
            await machine.SetOptionAsync("test", "changed");
            await Assert.ThrowsAsync<InvalidDataException>(() => machine.LoadStateAsync(statePath).AsTask());
        }
        finally
        {
            await machine.DisposeAsync();
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void AudioCallbacks_QueueEveryBatchInOrder()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Audio", Guid.NewGuid().ToString("N"));
        using var host = new AmigaExternalHostCallbacks(Path.Combine(root, "system"), Path.Combine(root, "content"), Path.Combine(root, "save"), null);
        var samples = new short[] { 1, -1, 2, -2 };
        var pointer = Marshal.AllocHGlobal(samples.Length * sizeof(short));
        try
        {
            Marshal.Copy(samples, 0, pointer, samples.Length);
            Assert.Equal((nuint)2, host.AudioBatch(pointer, 2));
            host.AudioSample(3, -3);
            Assert.True(host.TryDequeueAudio(out var first));
            Assert.Equal(samples, first!.InterleavedStereo.ToArray());
            Assert.True(host.TryDequeueAudio(out var second));
            Assert.Equal(new short[] { 3, -3 }, second!.InterleavedStereo.ToArray());
            Assert.False(host.TryDequeueAudio(out _));
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void MouseDeltas_AccumulateUntilPollAndAreConsumedOnce()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Input", Guid.NewGuid().ToString("N"));
        using var host = new AmigaExternalHostCallbacks(Path.Combine(root, "system"), Path.Combine(root, "content"), Path.Combine(root, "save"), null);
        try
        {
            host.Input = EmulationInputSnapshot.Empty with { Pointer = new EmulationPointerState(12, -4, 1, true, false, false) };
            host.Input = EmulationInputSnapshot.Empty with { Pointer = new EmulationPointerState(-3, 2, 0, true, false, false) };
            host.InputPoll();
            Assert.Equal((short)9, host.InputState(0, 2, 0, 0));
            Assert.Equal((short)-2, host.InputState(0, 2, 0, 1));
            Assert.Equal((short)1, host.InputState(0, 2, 0, 4));
            host.InputPoll();
            Assert.Equal((short)0, host.InputState(0, 2, 0, 0));
            Assert.Equal((short)0, host.InputState(0, 2, 0, 1));
            Assert.Equal((short)0, host.InputState(0, 2, 0, 4));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void NativeDiagnostics_AreCapturedAndBounded()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-Diagnostics", Guid.NewGuid().ToString("N"));
        using var host = new AmigaExternalHostCallbacks(Path.Combine(root, "system"), Path.Combine(root, "content"), Path.Combine(root, "save"), null);
        var message = Marshal.StringToCoTaskMemUTF8("native message %s");
        try
        {
            host.Log(1, message);
            Assert.False(host.Environment(9999, 0));
            Assert.Contains(host.Diagnostics, entry => entry.Contains("native message", StringComparison.Ordinal));
            Assert.Contains(host.Diagnostics, entry => entry.Contains("9999", StringComparison.Ordinal));
        }
        finally
        {
            Marshal.FreeCoTaskMem(message);
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task MultipleFloppies_CreateAValidatedMultidrivePlaylist()
    {
        var root = Path.Combine(Path.GetTempPath(), "GWGUI-Amiga-M3U", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var first = Path.Combine(root, "disk 1.adf");
        var second = Path.Combine(root, "disk 2.adf");
        await File.WriteAllBytesAsync(first, [1]);
        await File.WriteAllBytesAsync(second, [2]);
        try
        {
            var configuration = AmigaMachineConfiguration.A500(@"C:\kick.rom") with
            {
                Floppies = [new AmigaFloppyConfiguration(first, "Workbench"), new AmigaFloppyConfiguration(second, "Extras")],
                MountFloppiesInSeparateDrives = true
            };
            var playlist = AmigaExternalCore.PrepareContentPath(configuration, Path.Combine(root, "session"));
            Assert.NotNull(playlist);
            Assert.EndsWith("(MD).m3u", playlist, StringComparison.Ordinal);
            Assert.Equal(new[] { $"{first}|Workbench", $"{second}|Extras" }, await File.ReadAllLinesAsync(playlist!));
        }
        finally { Directory.Delete(root, true); }
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
        public bool ProduceAudio { get; init; }
        public bool CreatePersistentFiles { get; init; }
        public string? OptionValue { get; private set; }
        public bool Stopped { get; private set; }
        public bool Disposed { get; private set; }
        public VideoFrame? LatestVideoFrame => null;
        private readonly Queue<AudioChunk> _audio = new();
        public AudioChunk? LatestAudioChunk { get; private set; }
        public bool TryDequeueAudio(out AudioChunk? chunk)
        {
            if (_audio.Count == 0) { chunk = null; return false; }
            chunk = _audio.Dequeue();
            return true;
        }
        public IReadOnlyList<AmigaCoreOption> Options => [];
        public IReadOnlyList<string> Diagnostics => [];
        public string CoreSha256 => "fake-core";
        public double FramesPerSecond => 200;
        public int SampleRate => 44_100;
        public int DiskCount => 0;
        public int CurrentDiskIndex => -1;
        public void Initialize(AmigaMachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null)
        {
            if (!CreatePersistentFiles) return;
            Directory.CreateDirectory(sessionDirectory);
            File.WriteAllText(Path.Combine(sessionDirectory, "temporary.bin"), "temporary");
            Directory.CreateDirectory(saveDirectory!);
            File.WriteAllText(Path.Combine(saveDirectory!, "persistent.sav"), "persistent");
        }
        public void RunFrame()
        {
            FrameCount++;
            if (FrameCount >= FailAtFrame) throw new InvalidOperationException("Synthetic core failure.");
            if (ProduceAudio)
            {
                LatestAudioChunk = new AudioChunk(new short[] { 1, -1 }, SampleRate, 1, FrameCount, TimeSpan.Zero);
                _audio.Enqueue(LatestAudioChunk);
            }
        }
        public void HardReset() { }
        public void Stop() => Stopped = true;
        public void SetInput(EmulationInputSnapshot snapshot) { }
        public void InsertFloppy(string path) { }
        public void EjectFloppy() { }
        public void SelectDisk(int index) { }
        public byte[] SaveState() => [9, 8, 7];
        public void LoadState(ReadOnlySpan<byte> state) { }
        public void SetOption(string key, string value) => OptionValue = value;
        public void Dispose() => Disposed = true;
    }

    private sealed class FailingAudioOutput : IAudioOutput
    {
        public bool Disposed { get; private set; }
        public void Start(int sampleRate) { }
        public void Write(ReadOnlySpan<short> interleavedStereo) => throw new InvalidOperationException("Synthetic audio failure.");
        public void Flush() { }
        public void Stop() { }
        public void Dispose() => Disposed = true;
    }
}
