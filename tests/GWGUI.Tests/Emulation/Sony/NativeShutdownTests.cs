using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sony.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Interfaces;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sony.Common.Services;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class NativeShutdownTests
{
    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task RequestedShutdownStopsNormallyAndReleasesTheCoreOnce(bool duringInitialize, int expectedFrames)
    {
        var core = new ShutdownCore(duringInitialize);
        var deleted = 0;
        var machine = new Machine(Guid.NewGuid(), new MachineConfiguration("PlayStation3", "rpcs3", AudioEnabled: false), core, [], "unused-session", deleteSession: _ => deleted++);
        try
        {
            await machine.Lifecycle.StartAsync();
            for (var i = 0; i < 100 && machine.State == EmulationMachineState.Running; i++) await Task.Delay(10);
            Assert.Equal(EmulationMachineState.Stopped, machine.State);
            Assert.Equal(expectedFrames, core.Frames);
            Assert.Equal(1, core.Stops);
        }
        finally { await machine.DisposeAsync(); }
        Assert.Equal(1, core.Disposals);
        Assert.Equal(1, deleted);
    }

    private sealed class ShutdownCore(bool duringInitialize) : IEmulatorCore
    {
        public bool ShutdownRequested { get; private set; }
        public int Frames { get; private set; }
        public int Stops { get; private set; }
        public int Disposals { get; private set; }
        public VideoFrame? LatestVideoFrame => null;
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<CoreOption> Options => [];
        public IReadOnlyList<string> Diagnostics => [];
        public IReadOnlyDictionary<int, bool> LedStates => new Dictionary<int, bool>();
        public string CoreName => "Test core";
        public string CoreVersion => "1";
        public IReadOnlySet<string> SupportedContentExtensions => new HashSet<string>();
        public string CoreSha256 => string.Empty;
        public double FramesPerSecond => 60;
        public int SampleRate => 44100;
        public int DiskCount => 0;
        public int CurrentDiskIndex => -1;
        public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null) => ShutdownRequested = duringInitialize;
        public void RunFrame() { Frames++; ShutdownRequested = true; }
        public void Stop() => Stops++;
        public void Dispose() => Disposals++;
        public bool TryDequeueAudio(out AudioChunk? chunk) { chunk = null; return false; }
        public void HardReset() => throw new NotSupportedException();
        public void SoftReset() => throw new NotSupportedException();
        public void SetInput(EmulationInputSnapshot snapshot) { }
        public void SetControllerPortDevice(int port, ControllerType type) { }
        public void InsertMedia(string path) => throw new NotSupportedException();
        public void EjectMedia() => throw new NotSupportedException();
        public void SelectDisk(int index) => throw new NotSupportedException();
        public byte[] SaveState() => throw new NotSupportedException();
        public void LoadState(ReadOnlySpan<byte> state) => throw new NotSupportedException();
        public void SetOption(string key, string value) => throw new NotSupportedException();
    }
}
