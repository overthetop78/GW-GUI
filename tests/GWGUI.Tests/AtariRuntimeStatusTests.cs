using System.IO;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Tests;

public sealed class AtariRuntimeStatusTests
{
    [Fact]
    public async Task Snapshot_ExposesOnlyReliableCoreValues()
    {
        var core = new StatusCore(AtariRuntimeStatusTestConstants.FirstCoreName,
            AtariRuntimeStatusTestConstants.FirstLed, AtariRuntimeStatusTestConstants.FirstHostProcessId);
        await using var machine = CreateMachine(core, AtariMachineModel.St);

        var status = machine.RuntimeStatus;

        Assert.Equal(AtariMachineModel.St, status.Model);
        Assert.Equal(AtariRuntimeRegion.Pal, status.Region);
        Assert.Equal(AtariRuntimeStatusTestConstants.FramesPerSecond, status.FramesPerSecond);
        Assert.Equal(AtariRuntimeStatusTestConstants.SampleRate, status.SampleRate);
        Assert.Equal(AtariRuntimeStatusTestConstants.Width, status.Geometry!.Width);
        Assert.Equal(AtariRuntimeStatusTestConstants.Height, status.Geometry.Height);
        Assert.Equal(AtariRuntimeStatusTestConstants.Pitch, status.Geometry.Pitch);
        Assert.Equal(AtariRuntimeStatusTestConstants.AspectRatio, status.Geometry.AspectRatio);
        Assert.Equal(AtariRuntimeStatusTestConstants.FirstCoreName, status.CoreName);
        Assert.Empty(status.MediaActivity);
        Assert.Equal(AtariRuntimeStatusTestConstants.FirstLed,
            Assert.Single(status.LedStates).Key);
        Assert.Equal(AtariRuntimeStatusTestConstants.BufferedFrames, status.BufferedAudioFrames);
        Assert.Equal(AtariRuntimeStatusTestConstants.Overruns, status.AudioOverrunCount);
        Assert.Equal(AtariRuntimeStatusTestConstants.Underruns, status.AudioUnderrunCount);
        Assert.Null(status.LastError);
        Assert.Equal(AtariHostProcessState.Running, status.HostProcessState);
        Assert.Equal(AtariRuntimeStatusTestConstants.FirstHostProcessId, status.HostProcessId);
    }

    [Fact]
    public async Task Snapshots_AreCopiesAndRemainIsolatedBetweenMachines()
    {
        var firstCore = new StatusCore(AtariRuntimeStatusTestConstants.FirstCoreName,
            AtariRuntimeStatusTestConstants.FirstLed, AtariRuntimeStatusTestConstants.FirstHostProcessId);
        var secondCore = new StatusCore(AtariRuntimeStatusTestConstants.SecondCoreName,
            AtariRuntimeStatusTestConstants.SecondLed, AtariRuntimeStatusTestConstants.SecondHostProcessId);
        await using var first = CreateMachine(firstCore, AtariMachineModel.Atari2600);
        await using var second = CreateMachine(secondCore, AtariMachineModel.Lynx);

        var firstSnapshot = first.RuntimeStatus;
        firstCore.Leds.Clear();
        var secondSnapshot = second.RuntimeStatus;

        Assert.Equal(AtariRuntimeStatusTestConstants.FirstCoreName, firstSnapshot.CoreName);
        Assert.Contains(AtariRuntimeStatusTestConstants.FirstLed, firstSnapshot.LedStates.Keys);
        Assert.DoesNotContain(AtariRuntimeStatusTestConstants.SecondLed, firstSnapshot.LedStates.Keys);
        Assert.Equal(AtariRuntimeStatusTestConstants.SecondCoreName, secondSnapshot.CoreName);
        Assert.Contains(AtariRuntimeStatusTestConstants.SecondLed, secondSnapshot.LedStates.Keys);
    }

    [Fact]
    public async Task Fault_IsPublishedAsLastErrorWithoutInventingHardwareState()
    {
        var core = new StatusCore(AtariRuntimeStatusTestConstants.FirstCoreName,
            AtariRuntimeStatusTestConstants.FirstLed, AtariRuntimeStatusTestConstants.FirstHostProcessId)
        {
            ThrowOnFrame = true
        };
        await using var machine = CreateMachine(core, AtariMachineModel.Jaguar);
        await machine.StartAsync();
        await WaitUntil(() => machine.State == EmulationMachineState.Faulted);

        var status = machine.RuntimeStatus;

        Assert.IsType<InvalidOperationException>(status.LastError);
        Assert.Empty(status.MediaActivity);
    }

    [Theory]
    [InlineData(AtariRuntimeConstants.NativeNtscRegion, AtariRuntimeRegion.Ntsc)]
    [InlineData(AtariRuntimeConstants.NativePalRegion, AtariRuntimeRegion.Pal)]
    public void NativeRegions_AreMappedWithoutGuessing(uint native, AtariRuntimeRegion expected) =>
        Assert.Equal(expected, AtariRuntimeFunctions.Region(native));

    [Fact]
    public void UnknownNativeRegion_IsNotInvented() =>
        Assert.Null(AtariRuntimeFunctions.Region(AtariRuntimeStatusTestConstants.UnknownNativeRegion));

    private static AtariMachine CreateMachine(IAtariCore core, AtariMachineModel model) => new(Guid.NewGuid(),
        new AtariMachineConfiguration(model), core,
        Path.Combine(Path.GetTempPath(), $"gwgui-atari-status-{Guid.NewGuid():N}"));

    private static async Task WaitUntil(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(AtariRuntimeStatusTestConstants.TimeoutMilliseconds);
        while (!condition()) await Task.Delay(AtariRuntimeStatusTestConstants.PollMilliseconds, timeout.Token);
    }

    private sealed class StatusCore : IAtariCore
    {
        internal StatusCore(string name, int led, int processId)
        {
            CoreName = name;
            Leds[led] = true;
            HostProcessId = processId;
        }

        internal Dictionary<int, bool> Leds { get; } = [];
        internal bool ThrowOnFrame { get; init; }
        public AtariCoreKind Kind => AtariCoreKind.Hatari;
        public VideoFrame? LatestVideoFrame => new(new byte[AtariRuntimeStatusTestConstants.FrameLength],
            AtariRuntimeStatusTestConstants.Width, AtariRuntimeStatusTestConstants.Height,
            AtariRuntimeStatusTestConstants.Pitch, EmulationPixelFormat.Xrgb8888,
            AtariRuntimeStatusTestConstants.AspectRatio, AtariRuntimeStatusTestConstants.FirstSequence, TimeSpan.Zero);
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<AtariCoreOption> Options => [];
        public IReadOnlyList<string> Diagnostics => [];
        public IReadOnlyDictionary<int, bool> LedStates => Leds;
        public string CoreName { get; }
        public string CoreVersion => AtariRuntimeStatusTestConstants.CoreVersion;
        public string CoreSha256 => string.Empty;
        public IReadOnlySet<string> SupportedContentExtensions => new HashSet<string>();
        public bool SupportsSaveStates => true;
        public double FramesPerSecond => AtariRuntimeStatusTestConstants.FramesPerSecond;
        public int SampleRate => AtariRuntimeStatusTestConstants.SampleRate;
        public AtariRuntimeRegion? Region => AtariRuntimeRegion.Pal;
        public int BufferedAudioFrames => AtariRuntimeStatusTestConstants.BufferedFrames;
        public long AudioOverrunCount => AtariRuntimeStatusTestConstants.Overruns;
        public long AudioUnderrunCount => AtariRuntimeStatusTestConstants.Underruns;
        public AtariHostProcessState HostProcessState => AtariHostProcessState.Running;
        public int? HostProcessId { get; }
        public bool TryDequeueAudio(out AudioChunk? chunk) { chunk = null; return false; }
        public void Initialize(AtariMachineConfiguration configuration, string sessionDirectory,
            string? saveDirectory = null) { }
        public void RunFrame()
        {
            if (ThrowOnFrame) throw new InvalidOperationException(AtariRuntimeStatusTestConstants.FaultMessage);
        }
        public void HardReset() { }
        public void Stop() { }
        public void SetInput(EmulationInputSnapshot snapshot) { }
        public void SetControllerPortDevice(int port, AtariPeripheralKind peripheral) { }
        public void InsertMedia(AtariMediaConfiguration media) { }
        public void EjectMedia(EmulationMediaSlot slot) { }
        public void SelectDisk(int index) { }
        public void SaveMediaChanges(EmulationMediaSlot slot) { }
        public AtariDiskStatus GetDiskStatus() => new(AtariRuntimeStatusTestConstants.EmptyCount,
            AtariRuntimeStatusTestConstants.EmptyCount, true, []);
        public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => false;
        public byte[] SaveState() => [];
        public void LoadState(ReadOnlySpan<byte> state) { }
        public void SetOption(string key, string value) { }
        public void Dispose() { }
    }
}

internal static class AtariRuntimeStatusTestConstants
{
    internal const int Width = 2;
    internal const int Height = 1;
    internal const int BytesPerPixel = 4;
    internal const int Pitch = Width * BytesPerPixel;
    internal const int FrameLength = Pitch * Height;
    internal const float AspectRatio = 2f;
    internal const long FirstSequence = 1;
    internal const double FramesPerSecond = 50d;
    internal const int SampleRate = 44100;
    internal const int BufferedFrames = 128;
    internal const long Overruns = 3;
    internal const long Underruns = 4;
    internal const int FirstLed = 1;
    internal const int SecondLed = 2;
    internal const int FirstHostProcessId = 1001;
    internal const int SecondHostProcessId = 1002;
    internal const uint UnknownNativeRegion = 99;
    internal const int EmptyCount = 0;
    internal const int TimeoutMilliseconds = 5000;
    internal const int PollMilliseconds = 5;
    internal const string FirstCoreName = "first Atari core";
    internal const string SecondCoreName = "second Atari core";
    internal const string CoreVersion = "test";
    internal const string FaultMessage = "Injected Atari runtime fault.";
}
