using System.Reflection;
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
    [InlineData("Common", "Stop", false)]
    [InlineData("Common", "Stop", true)]
    [InlineData("Common", "Dispose", false)]
    [InlineData("Common", "Dispose", true)]
    [InlineData("Pcsx2", "Stop", false)]
    [InlineData("Pcsx2", "Stop", true)]
    [InlineData("Pcsx2", "Dispose", false)]
    [InlineData("Pcsx2", "Dispose", true)]
    [InlineData("Ppsspp", "Stop", false)]
    [InlineData("Ppsspp", "Stop", true)]
    [InlineData("Ppsspp", "Dispose", false)]
    [InlineData("Ppsspp", "Dispose", true)]
    public void ReentrantTeardownReleasesNativeContentAndCoreOnce(string adapter, string operation, bool failUnload)
    {
        IDisposable core = adapter switch
        {
            "Common" => new GWGUI.Emulation.Sony.Emulators.Common.Interop.Services.ExternalCore(GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.CoreConstants.Definition, string.Empty),
            "Pcsx2" => new GWGUI.Emulation.Sony.Emulators.Pcsx2.Services.ExternalCore(string.Empty),
            "Ppsspp" => new GWGUI.Emulation.Sony.Emulators.Ppsspp.Services.ExternalCore(string.Empty),
            _ => throw new ArgumentOutOfRangeException(nameof(adapter))
        };
        var type = core.GetType();
        var method = type.GetMethod(operation)!;
        var calls = new List<string>();
        var reentered = false;
        var injected = new InvalidOperationException("Injected unload failure");
        Action unload = () =>
        {
            calls.Add("unload");
            if (reentered) return;
            reentered = true;
            method.Invoke(core, null);
            if (failUnload) throw injected;
        };
        Action deinitialize = () => calls.Add("deinitialize");
        try
        {
            foreach (var (name, action) in new[] { ("_unloadGame", unload), ("_deinitialize", deinitialize) })
            {
                var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!;
                field.SetValue(core, Delegate.CreateDelegate(field.FieldType, action.Target, action.Method));
            }
            type.GetField("_gameLoaded", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(core, true);
            type.GetField("_initialized", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(core, true);
            if (failUnload)
            {
                var error = Assert.Throws<TargetInvocationException>(() => method.Invoke(core, null));
                Assert.Same(injected, error.InnerException);
            }
            else method.Invoke(core, null);
            method.Invoke(core, null);
        }
        finally { core.Dispose(); }
        Assert.Equal(new[] { "unload", "deinitialize" }, calls);
    }

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
