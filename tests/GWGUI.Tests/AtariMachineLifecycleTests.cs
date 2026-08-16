using System.Collections.Concurrent;
using System.IO;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Tests;

public sealed class AtariMachineLifecycleTests
{
    [Fact]
    public async Task RunsThreeHundredFramesOnOneNamedThread()
    {
        var core = new RecordingAtariCore();
        await using var machine = CreateMachine(core);

        await machine.StartAsync();
        await WaitUntil(() => core.FrameCount >= AtariMachineLifecycleTestConstants.RequiredFrameCount);
        await machine.StopAsync();

        Assert.Single(core.ThreadIds);
        Assert.Contains(AtariMachineConstants.ThreadNamePrefix, core.ThreadName, StringComparison.Ordinal);
        Assert.Contains(AtariCoreKind.Stella.ToString(), core.ThreadName, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PauseStopsFramesButProcessesCommandsAndResumeContinues()
    {
        var core = new RecordingAtariCore();
        await using var machine = CreateMachine(core);
        await machine.StartAsync();
        await WaitUntil(() => core.FrameCount > AtariMachineLifecycleTestConstants.MinimumInitialFrames);

        await machine.PauseAsync();
        var pausedFrames = core.FrameCount;
        var statePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        await machine.HardResetAsync();
        await machine.SetOptionAsync(AtariMachineLifecycleTestConstants.OptionKey,
            AtariMachineLifecycleTestConstants.OptionValue);
        await machine.InsertMediaAsync(Media());
        await machine.EjectMediaAsync(EmulationMediaSlot.Cartridge0);
        await machine.SelectDiskAsync(AtariMachineLifecycleTestConstants.FirstDiskIndex);
        await machine.SaveStateAsync(statePath);
        await machine.LoadStateAsync(statePath);
        machine.SetInput(EmulationInputSnapshot.Empty);
        await Task.Delay(AtariMachineLifecycleTestConstants.PauseObservationMilliseconds);

        Assert.Equal(pausedFrames, core.FrameCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedResetCount, core.ResetCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedCommandCount, core.MediaCommandCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedOptionCount, core.OptionCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedStateCommandCount, core.StateCommandCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedInputCount, core.InputCount);
        Assert.Single(core.ThreadIds);
        File.Delete(statePath);
        await machine.ResumeAsync();
        await WaitUntil(() => core.FrameCount > pausedFrames);
    }

    [Fact]
    public async Task StopAndSecondStopAreSafe()
    {
        var core = new RecordingAtariCore();
        await using var machine = CreateMachine(core);
        await machine.StartAsync();

        await machine.StopAsync();
        await machine.StopAsync();

        Assert.Equal(EmulationMachineState.Stopped, machine.State);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedStopCount, core.StopCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedDisposeCount, core.DisposeCount);
    }

    [Fact]
    public async Task InjectedFrameExceptionFaultsAndCleansMachine()
    {
        var core = new RecordingAtariCore { ThrowOnFrame = true };
        await using var machine = CreateMachine(core);
        await machine.StartAsync();

        await WaitUntil(() => machine.State == EmulationMachineState.Faulted);

        Assert.IsType<InvalidOperationException>(machine.Fault);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedStopCount, core.StopCount);
    }

    [Fact]
    public async Task InjectedCommandExceptionFaultsAndCleansMachine()
    {
        var core = new RecordingAtariCore { ThrowOnReset = true };
        await using var machine = CreateMachine(core);
        await machine.StartAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => machine.HardResetAsync().AsTask());
        await WaitUntil(() => machine.State == EmulationMachineState.Faulted);

        Assert.IsType<InvalidOperationException>(machine.Fault);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedStopCount, core.StopCount);
    }

    private static AtariMachine CreateMachine(RecordingAtariCore core) => new(Guid.NewGuid(),
        new AtariMachineConfiguration(AtariMachineModel.Atari2600), core,
        Path.Combine(Path.GetTempPath(), $"gwgui-atari-machine-{Guid.NewGuid():N}"));

    private static AtariMediaConfiguration Media() => new(
        Path.Combine(Path.GetTempPath(), AtariMachineLifecycleTestConstants.MediaFileName),
        AtariMediaKind.Cartridge, EmulationMediaSlot.Cartridge0);

    private static async Task WaitUntil(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(AtariMachineLifecycleTestConstants.TimeoutMilliseconds);
        while (!condition()) await Task.Delay(AtariMachineLifecycleTestConstants.PollMilliseconds, timeout.Token);
    }

    private sealed class RecordingAtariCore : IAtariCore
    {
        private readonly ConcurrentDictionary<int, byte> _threads = new();
        public int FrameCount;
        public int ResetCount;
        public int StopCount;
        public int DisposeCount;
        public int MediaCommandCount;
        public int OptionCount;
        public int StateCommandCount;
        public int InputCount;
        public bool ThrowOnFrame { get; init; }
        public bool ThrowOnReset { get; init; }
        public IReadOnlyCollection<int> ThreadIds => _threads.Keys.ToArray();
        public string ThreadName { get; private set; } = string.Empty;
        public AtariCoreKind Kind => AtariCoreKind.Stella;
        public VideoFrame? LatestVideoFrame => null;
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<AtariCoreOption> Options => [];
        public IReadOnlyList<string> Diagnostics => [];
        public IReadOnlyDictionary<int, bool> LedStates => new Dictionary<int, bool>();
        public string CoreName => nameof(RecordingAtariCore);
        public string CoreVersion => AtariMachineLifecycleTestConstants.CoreVersion;
        public string CoreSha256 => string.Empty;
        public IReadOnlySet<string> SupportedContentExtensions => new HashSet<string>();
        public double FramesPerSecond => AtariMachineLifecycleTestConstants.TestFramesPerSecond;
        public int SampleRate => AtariMachineLifecycleTestConstants.TestSampleRate;
        public bool TryDequeueAudio(out AudioChunk? chunk) { chunk = null; return false; }
        public void Initialize(AtariMachineConfiguration configuration, string sessionDirectory,
            string? saveDirectory = null) => CaptureThread();
        public void RunFrame()
        {
            CaptureThread();
            if (ThrowOnFrame) throw new InvalidOperationException(AtariMachineLifecycleTestConstants.FaultMessage);
            Interlocked.Increment(ref FrameCount);
        }
        public void HardReset()
        {
            CaptureThread();
            if (ThrowOnReset) throw new InvalidOperationException(AtariMachineLifecycleTestConstants.FaultMessage);
            Interlocked.Increment(ref ResetCount);
        }
        public void Stop() { CaptureThread(); Interlocked.Increment(ref StopCount); }
        public void SetInput(EmulationInputSnapshot snapshot) { CaptureThread(); Interlocked.Increment(ref InputCount); }
        public void InsertMedia(AtariMediaConfiguration media) { CaptureThread(); Interlocked.Increment(ref MediaCommandCount); }
        public void EjectMedia(EmulationMediaSlot slot) { CaptureThread(); Interlocked.Increment(ref MediaCommandCount); }
        public void SelectDisk(int index) { CaptureThread(); Interlocked.Increment(ref MediaCommandCount); }
        public void SaveMediaChanges(EmulationMediaSlot slot) => CaptureThread();
        public AtariDiskStatus GetDiskStatus()
        {
            CaptureThread();
            return new AtariDiskStatus(AtariMachineLifecycleTestConstants.EmptyCount,
                AtariMachineLifecycleTestConstants.EmptyCount, true, []);
        }
        public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) { CaptureThread(); return false; }
        public byte[] SaveState() { CaptureThread(); Interlocked.Increment(ref StateCommandCount); return []; }
        public void LoadState(ReadOnlySpan<byte> state) { CaptureThread(); Interlocked.Increment(ref StateCommandCount); }
        public void SetOption(string key, string value) { CaptureThread(); Interlocked.Increment(ref OptionCount); }
        public void Dispose() { CaptureThread(); Interlocked.Increment(ref DisposeCount); }
        private void CaptureThread()
        {
            _threads.TryAdd(Environment.CurrentManagedThreadId, default);
            ThreadName = Thread.CurrentThread.Name ?? string.Empty;
        }
    }
}

internal static class AtariMachineLifecycleTestConstants
{
    internal const int RequiredFrameCount = 300;
    internal const int MinimumInitialFrames = 3;
    internal const int ExpectedResetCount = 1;
    internal const int ExpectedStopCount = 1;
    internal const int ExpectedDisposeCount = 1;
    internal const int ExpectedCommandCount = 3;
    internal const int ExpectedOptionCount = 1;
    internal const int ExpectedStateCommandCount = 2;
    internal const int ExpectedInputCount = 1;
    internal const int FirstDiskIndex = 0;
    internal const int PauseObservationMilliseconds = 100;
    internal const int TimeoutMilliseconds = 10000;
    internal const int PollMilliseconds = 5;
    internal const int TestSampleRate = 44100;
    internal const int EmptyCount = 0;
    internal const double TestFramesPerSecond = 1000;
    internal const string OptionKey = "test_option";
    internal const string OptionValue = "enabled";
    internal const string MediaFileName = "test.a26";
    internal const string CoreVersion = "test";
    internal const string FaultMessage = "Injected Atari frame failure.";
}
