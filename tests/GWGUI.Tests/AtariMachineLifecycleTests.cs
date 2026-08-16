using System.Collections.Concurrent;
using System.IO;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Tests;

public sealed class AtariMachineLifecycleTests
{
    [Fact]
    public async Task TwoMachinesKeepConfigurationsSessionsAndCommandsIsolated()
    {
        var firstCore = new RecordingAtariCore();
        var secondCore = new RecordingAtariCore();
        var firstSession = SessionDirectory();
        var secondSession = SessionDirectory();
        var firstConfiguration = Configuration(AtariMachineModel.Atari800Xl,
            AtariMachineLifecycleTestConstants.FirstMappingValue);
        var secondConfiguration = Configuration(AtariMachineModel.Atari800Xl,
            AtariMachineLifecycleTestConstants.SecondMappingValue);
        await using var first = CreateMachine(firstCore, firstSession, firstConfiguration);
        await using var second = CreateMachine(secondCore, secondSession, secondConfiguration);

        await Task.WhenAll(first.StartAsync().AsTask(), second.StartAsync().AsTask());
        await first.SetOptionAsync(AtariMachineLifecycleTestConstants.OptionKey,
            AtariMachineLifecycleTestConstants.OptionValue);
        await first.InsertMediaAsync(Media());
        first.SetInput(EmulationInputSnapshot.Empty);

        Assert.NotEqual(firstCore.SessionDirectory, secondCore.SessionDirectory);
        var initializedFirst = Assert.IsType<AtariMachineConfiguration>(firstCore.Configuration);
        var initializedSecond = Assert.IsType<AtariMachineConfiguration>(secondCore.Configuration);
        var firstMappings = Assert.IsAssignableFrom<IReadOnlyDictionary<string, EmulationKey>>(
            initializedFirst.Input.KeyboardMappings);
        var secondMappings = Assert.IsAssignableFrom<IReadOnlyDictionary<string, EmulationKey>>(
            initializedSecond.Input.KeyboardMappings);
        Assert.Equal(AtariMachineLifecycleTestConstants.FirstMappingValue,
            firstMappings[AtariMachineLifecycleTestConstants.MappingKey]);
        Assert.Equal(AtariMachineLifecycleTestConstants.SecondMappingValue,
            secondMappings[AtariMachineLifecycleTestConstants.MappingKey]);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedOptionCount, firstCore.OptionCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedSingleMediaCommandCount, firstCore.MediaCommandCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedInputCountWithoutPause, firstCore.InputCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.EmptyCount, secondCore.OptionCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.EmptyCount, secondCore.MediaCommandCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.EmptyCount, secondCore.InputCount);
    }

    [Fact]
    public async Task DifferentFamiliesAndCoresRunAtTheSameTime()
    {
        var firstCore = new RecordingAtariCore { CoreKind = AtariCoreKind.Stella };
        var secondCore = new RecordingAtariCore { CoreKind = AtariCoreKind.BeetleLynx };
        await using var first = CreateMachine(firstCore, configuration:
            new AtariMachineConfiguration(AtariMachineModel.Atari2600));
        await using var second = CreateMachine(secondCore, configuration:
            new AtariMachineConfiguration(AtariMachineModel.Lynx));

        await Task.WhenAll(first.StartAsync().AsTask(), second.StartAsync().AsTask());
        await WaitUntil(() => firstCore.FrameCount > AtariMachineLifecycleTestConstants.MinimumInitialFrames
                              && secondCore.FrameCount > AtariMachineLifecycleTestConstants.MinimumInitialFrames);

        Assert.Equal(AtariCoreKind.Stella, firstCore.Kind);
        Assert.Equal(AtariCoreKind.BeetleLynx, secondCore.Kind);
        Assert.Equal(EmulationMachineState.Running, first.State);
        Assert.Equal(EmulationMachineState.Running, second.State);
    }

    [Fact]
    public async Task FaultedMachineDoesNotStopOtherMachineOutputs()
    {
        var faultedCore = new RecordingAtariCore { ThrowOnFrame = true };
        var healthyCore = new RecordingAtariCore { EmitOutputs = true };
        await using var faulted = CreateMachine(faultedCore);
        await using var healthy = CreateMachine(healthyCore);
        var videoCount = AtariMachineLifecycleTestConstants.EmptyCount;
        var audioCount = AtariMachineLifecycleTestConstants.EmptyCount;
        healthy.VideoFrameReady += (_, _) => Interlocked.Increment(ref videoCount);
        healthy.AudioChunkReady += (_, _) => Interlocked.Increment(ref audioCount);

        await Task.WhenAll(faulted.StartAsync().AsTask(), healthy.StartAsync().AsTask());
        await WaitUntil(() => faulted.State == EmulationMachineState.Faulted
                              && videoCount > AtariMachineLifecycleTestConstants.MinimumOutputCount
                              && audioCount > AtariMachineLifecycleTestConstants.MinimumOutputCount);
        var previousVideoCount = videoCount;
        var previousAudioCount = audioCount;
        await WaitUntil(() => videoCount > previousVideoCount && audioCount > previousAudioCount);

        Assert.Equal(EmulationMachineState.Running, healthy.State);
    }

    [Fact]
    public async Task MachineCollectionStopsEveryMachineAtApplicationShutdown()
    {
        var firstCore = new RecordingAtariCore();
        var secondCore = new RecordingAtariCore();
        var first = CreateMachine(firstCore);
        var second = CreateMachine(secondCore);
        await using var machines = new AtariMachineCollection();
        machines.Register(first);
        machines.Register(second);
        await Task.WhenAll(first.StartAsync().AsTask(), second.StartAsync().AsTask());

        await machines.StopAllAsync();

        Assert.Empty(machines.Machines);
        Assert.Equal(EmulationMachineState.Stopped, first.State);
        Assert.Equal(EmulationMachineState.Stopped, second.State);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedDisposeCount, firstCore.DisposeCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedDisposeCount, secondCore.DisposeCount);
    }

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
    public async Task ExposesEveryNonFaultedLifecycleState()
    {
        var core = new RecordingAtariCore { BlockInitialization = true, BlockStop = true };
        await using var machine = CreateMachine(core);
        Assert.Equal(EmulationMachineState.Created, machine.State);

        var start = machine.StartAsync().AsTask();
        Assert.True(core.InitializationEntered.Wait(AtariMachineLifecycleTestConstants.TimeoutMilliseconds));
        Assert.Equal(EmulationMachineState.Starting, machine.State);
        core.ContinueInitialization.Set();
        await start;
        Assert.Equal(EmulationMachineState.Running, machine.State);

        await machine.PauseAsync();
        Assert.Equal(EmulationMachineState.Paused, machine.State);
        await machine.ResumeAsync();
        Assert.Equal(EmulationMachineState.Running, machine.State);

        var stop = machine.StopAsync().AsTask();
        Assert.True(core.StopEntered.Wait(AtariMachineLifecycleTestConstants.TimeoutMilliseconds));
        Assert.Equal(EmulationMachineState.Stopping, machine.State);
        core.ContinueStop.Set();
        await stop;
        Assert.Equal(EmulationMachineState.Stopped, machine.State);
    }

    [Fact]
    public async Task CommandsAreRejectedOutsideRunningAndPausedStates()
    {
        var core = new RecordingAtariCore();
        await using var machine = CreateMachine(core);

        await Assert.ThrowsAsync<InvalidOperationException>(() => machine.HardResetAsync().AsTask());
        await machine.StartAsync();
        await machine.StopAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => machine.SetOptionAsync(
            AtariMachineLifecycleTestConstants.OptionKey,
            AtariMachineLifecycleTestConstants.OptionValue).AsTask());
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
        machine.SetControllerPortDevice(AtariMachineLifecycleTestConstants.FirstControllerPort,
            AtariPeripheralKind.Automatic);
        await Task.Delay(AtariMachineLifecycleTestConstants.PauseObservationMilliseconds);

        Assert.Equal(pausedFrames, core.FrameCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedResetCount, core.ResetCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedCommandCount, core.MediaCommandCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedOptionCount, core.OptionCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedStateCommandCount, core.StateCommandCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedPausedInputCount, core.InputCount);
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedControllerConfigurationCount,
            core.ControllerConfigurationCount);
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
    public async Task StopRemovesOnlyItsTemporarySessionDirectory()
    {
        var session = Path.Combine(Path.GetTempPath(), AtariMachineLifecycleTestConstants.SessionDirectoryPrefix
            + Guid.NewGuid().ToString(AtariMachineLifecycleTestConstants.IdentifierFormat));
        Directory.CreateDirectory(session);
        var sibling = session + AtariMachineLifecycleTestConstants.SiblingDirectorySuffix;
        Directory.CreateDirectory(sibling);
        try
        {
            await using var machine = CreateMachine(new RecordingAtariCore(), session);
            await machine.StartAsync();
            await machine.StopAsync();

            Assert.False(Directory.Exists(session));
            Assert.True(Directory.Exists(sibling));
        }
        finally
        {
            if (Directory.Exists(session)) Directory.Delete(session, recursive: true);
            if (Directory.Exists(sibling)) Directory.Delete(sibling, recursive: true);
        }
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
        Assert.Equal(AtariMachineLifecycleTestConstants.ExpectedReleasedInputCount, core.InputCount);
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

    private static AtariMachine CreateMachine(RecordingAtariCore core, string? sessionDirectory = null,
        AtariMachineConfiguration? configuration = null) => new(Guid.NewGuid(),
        configuration ?? new AtariMachineConfiguration(AtariMachineModel.Atari2600), core,
        sessionDirectory ?? SessionDirectory());

    private static string SessionDirectory() => Path.Combine(Path.GetTempPath(),
        AtariMachineLifecycleTestConstants.SessionDirectoryPrefix
        + Guid.NewGuid().ToString(AtariMachineLifecycleTestConstants.IdentifierFormat));

    private static AtariMachineConfiguration Configuration(AtariMachineModel model, EmulationKey mapping) =>
        new(model, input: new AtariInputConfiguration(KeyboardMappings:
            new Dictionary<string, EmulationKey> { [AtariMachineLifecycleTestConstants.MappingKey] = mapping }));

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
        private readonly ConcurrentQueue<AudioChunk> _audio = new();
        public int FrameCount;
        public int ResetCount;
        public int StopCount;
        public int DisposeCount;
        public int MediaCommandCount;
        public int OptionCount;
        public int StateCommandCount;
        public int InputCount;
        public int ControllerConfigurationCount;
        public bool ThrowOnFrame { get; init; }
        public bool ThrowOnReset { get; init; }
        public bool BlockInitialization { get; init; }
        public bool BlockStop { get; init; }
        public bool EmitOutputs { get; init; }
        public AtariCoreKind CoreKind { get; init; } = AtariCoreKind.Stella;
        public AtariMachineConfiguration? Configuration { get; private set; }
        public string? SessionDirectory { get; private set; }
        public ManualResetEventSlim InitializationEntered { get; } = new();
        public ManualResetEventSlim ContinueInitialization { get; } = new();
        public ManualResetEventSlim StopEntered { get; } = new();
        public ManualResetEventSlim ContinueStop { get; } = new();
        public IReadOnlyCollection<int> ThreadIds => _threads.Keys.ToArray();
        public string ThreadName { get; private set; } = string.Empty;
        public AtariCoreKind Kind => CoreKind;
        public VideoFrame? LatestVideoFrame { get; private set; }
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<AtariCoreOption> Options => [];
        public IReadOnlyList<string> Diagnostics => [];
        public IReadOnlyDictionary<int, bool> LedStates => new Dictionary<int, bool>();
        public string CoreName => nameof(RecordingAtariCore);
        public string CoreVersion => AtariMachineLifecycleTestConstants.CoreVersion;
        public string CoreSha256 => AtariMachineLifecycleTestConstants.CoreSha256;
        public IReadOnlySet<string> SupportedContentExtensions => new HashSet<string>();
        public bool SupportsSaveStates => true;
        public double FramesPerSecond => AtariMachineLifecycleTestConstants.TestFramesPerSecond;
        public int SampleRate => AtariMachineLifecycleTestConstants.TestSampleRate;
        public AtariRuntimeRegion? Region => AtariRuntimeRegion.Ntsc;
        public int BufferedAudioFrames => AtariMachineLifecycleTestConstants.EmptyCount;
        public long AudioOverrunCount => AtariMachineLifecycleTestConstants.EmptyCount;
        public long AudioUnderrunCount => AtariMachineLifecycleTestConstants.EmptyCount;
        public AtariHostProcessState HostProcessState => AtariHostProcessState.InProcess;
        public int? HostProcessId => null;
        public bool TryDequeueAudio(out AudioChunk? chunk) => _audio.TryDequeue(out chunk);
        public void Initialize(AtariMachineConfiguration configuration, string sessionDirectory,
            string? saveDirectory = null)
        {
            CaptureThread();
            Configuration = configuration;
            SessionDirectory = sessionDirectory;
            InitializationEntered.Set();
            if (BlockInitialization) ContinueInitialization.Wait();
        }
        public void RunFrame()
        {
            CaptureThread();
            if (ThrowOnFrame) throw new InvalidOperationException(AtariMachineLifecycleTestConstants.FaultMessage);
            var sequence = Interlocked.Increment(ref FrameCount);
            if (!EmitOutputs) return;
            LatestVideoFrame = new VideoFrame(AtariMachineLifecycleTestConstants.VideoPixels,
                AtariMachineLifecycleTestConstants.VideoWidth, AtariMachineLifecycleTestConstants.VideoHeight,
                AtariMachineLifecycleTestConstants.VideoPitch, EmulationPixelFormat.Xrgb8888,
                AtariMachineLifecycleTestConstants.VideoAspectRatio, sequence, TimeSpan.Zero);
            _audio.Enqueue(new AudioChunk(AtariMachineLifecycleTestConstants.AudioSamples,
                AtariMachineLifecycleTestConstants.TestSampleRate,
                AtariMachineLifecycleTestConstants.AudioFrameCount, sequence, TimeSpan.Zero));
        }
        public void HardReset()
        {
            CaptureThread();
            if (ThrowOnReset) throw new InvalidOperationException(AtariMachineLifecycleTestConstants.FaultMessage);
            Interlocked.Increment(ref ResetCount);
        }
        public void Stop()
        {
            CaptureThread();
            StopEntered.Set();
            if (BlockStop) ContinueStop.Wait();
            Interlocked.Increment(ref StopCount);
        }
        public void SetInput(EmulationInputSnapshot snapshot) { CaptureThread(); Interlocked.Increment(ref InputCount); }
        public void SetControllerPortDevice(int port, AtariPeripheralKind peripheral)
        {
            CaptureThread();
            Interlocked.Increment(ref ControllerConfigurationCount);
        }
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
        public byte[] SaveState()
        {
            CaptureThread();
            Interlocked.Increment(ref StateCommandCount);
            return [AtariMachineLifecycleTestConstants.StateByte];
        }
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
    internal const int ExpectedSingleMediaCommandCount = 1;
    internal const int ExpectedOptionCount = 1;
    internal const int ExpectedStateCommandCount = 2;
    internal const int ExpectedPausedInputCount = 2;
    internal const int ExpectedReleasedInputCount = 1;
    internal const int ExpectedInputCountWithoutPause = 1;
    internal const int MinimumOutputCount = 1;
    internal const byte StateByte = 42;
    internal const int ExpectedControllerConfigurationCount = 1;
    internal const int FirstControllerPort = 0;
    internal const int FirstDiskIndex = 0;
    internal const int PauseObservationMilliseconds = 100;
    internal const int TimeoutMilliseconds = 10000;
    internal const int PollMilliseconds = 5;
    internal const int TestSampleRate = 44100;
    internal const int EmptyCount = 0;
    internal const double TestFramesPerSecond = 1000;
    internal const int VideoWidth = 1;
    internal const int VideoHeight = 1;
    internal const int VideoPitch = 4;
    internal const float VideoAspectRatio = 1;
    internal const int AudioFrameCount = 1;
    internal const string OptionKey = "test_option";
    internal const string OptionValue = "enabled";
    internal const string MediaFileName = "test.a26";
    internal const string CoreVersion = "test";
    internal const string CoreSha256 = "test-core-sha256";
    internal const string FaultMessage = "Injected Atari frame failure.";
    internal const string SessionDirectoryPrefix = "gwgui-atari-machine-";
    internal const string IdentifierFormat = "N";
    internal const string SiblingDirectorySuffix = "-keep";
    internal const string MappingKey = "FIRE";
    internal const EmulationKey FirstMappingValue = EmulationKey.Space;
    internal const EmulationKey SecondMappingValue = EmulationKey.Return;
    internal static readonly byte[] VideoPixels = [0, 0, 0, 0];
    internal static readonly short[] AudioSamples = [0, 0];
}
