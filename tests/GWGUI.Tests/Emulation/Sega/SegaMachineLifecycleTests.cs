using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sega.Common.Contracts;
using GWGUI.Emulation.Sega.Common.Interfaces;
using GWGUI.Emulation.Sega.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sega.Common.Services;

namespace GWGUI.Tests.Emulation.Sega;

public sealed class SegaMachineLifecycleTests
{
    [Fact]
    public async Task StopReleasesInputBeforeDisposingCore()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-machine-tests",
            Guid.NewGuid().ToString("N"));
        var session = Path.Combine(root, "session");
        Directory.CreateDirectory(session);
        var core = new Core();
        var machine = CreateMachine(core, session);
        try
        {
            await machine.StartAsync();
            machine.SetInput(EmulationInputSnapshot.Empty with
            {
                Controllers = [new EmulationControllerState(1, 0, 0, 0, 0, 0, 0)
                {
                    DeviceId = "gameinput:pad-0"
                }]
            });
            Assert.True(core.FrameEntered.Wait(TimeSpan.FromSeconds(5)));
            core.FrameRelease.Set();
            await machine.StopAsync();
            AssertEmptyInput(core.LastInput);
        }
        finally
        {
            await machine.DisposeAsync();
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }

        Assert.True(core.Stopped);
        Assert.True(core.Disposed);
        Assert.False(Directory.Exists(session));
    }

    [Fact]
    public async Task FaultReleasesInputBeforeDisposingCore()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-machine-tests",
            Guid.NewGuid().ToString("N"));
        var session = Path.Combine(root, "session");
        Directory.CreateDirectory(session);
        var core = new Core { ThrowOnFrame = true };
        var machine = CreateMachine(core, session);
        try
        {
            await machine.StartAsync();
            machine.SetInput(EmulationInputSnapshot.Empty with
            {
                Controllers = [new EmulationControllerState(1, 0, 0, 0, 0, 0, 0)
                {
                    DeviceId = "gameinput:pad-0"
                }]
            });
            Assert.True(core.FrameEntered.Wait(TimeSpan.FromSeconds(5)));
            core.FrameRelease.Set();
            await core.Exited.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(EmulationMachineState.Faulted, machine.State);
            AssertEmptyInput(core.LastInput);
        }
        finally
        {
            await machine.DisposeAsync();
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }

        Assert.True(core.Disposed);
        Assert.False(Directory.Exists(session));
    }

    private static Machine CreateMachine(Core core, string session) => new(
        Guid.NewGuid(),
        new MachineConfiguration(ModelConstants.MegaDrive, "genesis-plus-gx"),
        core, [], session);

    private static void AssertEmptyInput(EmulationInputSnapshot input)
    {
        Assert.Empty(input.Keys);
        Assert.All(input.Controllers, controller =>
        {
            Assert.Equal(0u, controller.Buttons);
            Assert.Empty(controller.DeviceId);
        });
        Assert.Equal(EmulationInputSnapshot.Empty.Pointer, input.Pointer);
    }

    private sealed class Core : IEmulatorCore
    {
        public readonly ManualResetEventSlim FrameEntered = new();
        public readonly ManualResetEventSlim FrameRelease = new();
        public readonly TaskCompletionSource Exited = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool ThrowOnFrame;
        public bool Stopped;
        public bool Disposed;
        public EmulationInputSnapshot LastInput { get; private set; } = EmulationInputSnapshot.Empty;
        public VideoFrame? LatestVideoFrame => null;
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<CoreOption> Options => [];
        public IReadOnlyList<string> Diagnostics => [];
        public IReadOnlyDictionary<int, bool> LedStates => new Dictionary<int, bool>();
        public string CoreName => "test";
        public string CoreVersion => "1";
        public string CoreSha256 => "test";
        public IReadOnlySet<string> SupportedContentExtensions => new HashSet<string>();
        public double FramesPerSecond => 60;
        public int SampleRate => 44_100;
        public int DiskCount => 0;
        public int CurrentDiskIndex => 0;

        public void Initialize(MachineConfiguration configuration, string sessionDirectory,
            string? saveDirectory = null) { }

        public void RunFrame()
        {
            FrameEntered.Set();
            FrameRelease.Wait(TimeSpan.FromSeconds(5));
            if (ThrowOnFrame) throw new InvalidOperationException("synthetic frame failure");
        }

        public void SetInput(EmulationInputSnapshot snapshot) => LastInput = snapshot;
        public bool TryDequeueAudio(out AudioChunk? chunk) { chunk = null; return false; }
        public void Stop() { Stopped = true; Exited.TrySetResult(); }
        public void Dispose() { Disposed = true; FrameRelease.Set(); }
        public void HardReset() { }
        public void SoftReset() { }
        public void InsertMedia(string path) { }
        public void EjectMedia() { }
        public void SelectDisk(int index) { }
        public byte[] SaveState() => [];
        public void LoadState(ReadOnlySpan<byte> state) { }
        public void SetOption(string key, string value) { }
    }
}
