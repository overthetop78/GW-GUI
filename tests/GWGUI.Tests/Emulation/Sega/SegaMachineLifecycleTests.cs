using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sega.Common.Contracts;
using GWGUI.Emulation.Sega.Common.Interfaces;
using GWGUI.Emulation.Sega.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sega.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sega.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sega.Common.Services;
using GenesisPlusGxExternalCore = GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Services.ExternalCore;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Constants;
using FlycastHostConstants = GWGUI.Emulation.Sega.Emulators.Flycast.Constants.ExternalHostCallbacksConstants;

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

    [Fact]
    public void GenesisPlusGxLoadsConfiguredMasterSystemMediaWhenSmokePathsAreProvided()
    {
        var corePath = Environment.GetEnvironmentVariable("GWGUI_SEGA_GENESIS_CORE");
        var mediaPath = Environment.GetEnvironmentVariable("GWGUI_SEGA_SMS_MEDIA");
        if (string.IsNullOrWhiteSpace(corePath) || string.IsNullOrWhiteSpace(mediaPath)
            || !File.Exists(corePath) || !File.Exists(mediaPath)) return;

        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-genesis-smoke-tests",
            Guid.NewGuid().ToString("N"));
        var session = Path.Combine(root, "session");
        Directory.CreateDirectory(session);
        var configuration = new MachineConfiguration(ModelConstants.MasterSystem,
            "genesisplusgx")
        {
            Media =
            [
                new MediaConfiguration(mediaPath, MediaCategory.Cartridge,
                    EmulationMediaSlot.Cartridge0, IsInserted: true)
            ]
        };
        var core = new GenesisPlusGxExternalCore(corePath);
        try
        {
            core.Initialize(configuration, session);
            core.RunFrame();
            Assert.NotNull(core.LatestVideoFrame);
            Assert.True(core.LatestVideoFrame!.Width > 0);
            Assert.True(core.LatestVideoFrame.Height > 0);
        }
        finally
        {
            core.Dispose();
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GenesisPlusGxLoadsConfiguredPicoMediaWhenSmokePathsAreProvided()
    {
        var corePath = Environment.GetEnvironmentVariable("GWGUI_SEGA_GENESIS_CORE");
        var mediaPath = Environment.GetEnvironmentVariable("GWGUI_SEGA_PICO_MEDIA");
        if (string.IsNullOrWhiteSpace(corePath) || string.IsNullOrWhiteSpace(mediaPath)
            || !File.Exists(corePath) || !File.Exists(mediaPath)) return;

        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-genesis-pico-smoke-tests",
            Guid.NewGuid().ToString("N"));
        var session = Path.Combine(root, "session");
        Directory.CreateDirectory(session);
        var configuration = new MachineConfiguration(ModelConstants.Pico,
            "genesisplusgx")
        {
            Media =
            [
                new MediaConfiguration(mediaPath, MediaCategory.Cartridge,
                    EmulationMediaSlot.Cartridge0, IsInserted: true)
            ]
        };
        var core = new GenesisPlusGxExternalCore(corePath);
        try
        {
            core.Initialize(configuration, session);
            core.RunFrame();
            Assert.NotNull(core.LatestVideoFrame);
            Assert.True(core.LatestVideoFrame!.Width > 0);
            Assert.True(core.LatestVideoFrame.Height > 0);
        }
        finally
        {
            core.Dispose();
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void FlycastPublishesLegacyCoreOptions()
    {
        var root = Path.Combine(Path.GetTempPath(), "gwgui-sega-options-tests",
            Guid.NewGuid().ToString("N"));
        var system = Path.Combine(root, "system");
        var content = Path.Combine(root, "content");
        var save = Path.Combine(root, "save");
        Directory.CreateDirectory(root);
        using var callbacks = new GWGUI.Emulation.Sega.Emulators.Flycast.Services.ExternalHostCallbacks(
            system, content, save, null);
        var pointers = new List<nint>();
        var definitionSize = (FlycastHostConstants.LegacyCoreOptionPointerFieldsBeforeValues
            + FlycastHostConstants.MaximumCoreOptionValues * FlycastHostConstants.CoreOptionValueFieldCount
            + FlycastHostConstants.CoreOptionTerminatorFieldCount) * IntPtr.Size;
        var definitions = Marshal.AllocHGlobal(definitionSize * 2);
        try
        {
            Marshal.Copy(new byte[definitionSize * 2], 0, definitions, definitionSize * 2);
            nint Native(string value)
            {
                var pointer = Marshal.StringToCoTaskMemUTF8(value);
                pointers.Add(pointer);
                return pointer;
            }

            Marshal.WriteIntPtr(definitions, 0, Native("flycast_test_option"));
            Marshal.WriteIntPtr(definitions, IntPtr.Size, Native("Test option"));
            Marshal.WriteIntPtr(definitions, IntPtr.Size * 2, Native("Test option description"));
            var valuesOffset = FlycastHostConstants.LegacyCoreOptionPointerFieldsBeforeValues * IntPtr.Size;
            Marshal.WriteIntPtr(definitions, valuesOffset, Native("true"));
            Marshal.WriteIntPtr(definitions, valuesOffset + IntPtr.Size, Native("Enabled"));
            Marshal.WriteIntPtr(definitions, valuesOffset + IntPtr.Size * FlycastHostConstants.CoreOptionValueFieldCount,
                Native("false"));
            Marshal.WriteIntPtr(definitions, valuesOffset + IntPtr.Size * (FlycastHostConstants.CoreOptionValueFieldCount + 1),
                Native("Disabled"));
            var defaultOffset = valuesOffset
                + FlycastHostConstants.MaximumCoreOptionValues * FlycastHostConstants.CoreOptionValueFieldCount * IntPtr.Size;
            Marshal.WriteIntPtr(definitions, defaultOffset, Native("true"));

            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetCoreOptions, definitions));
            var option = Assert.Single(callbacks.OptionCatalog);
            Assert.Equal("flycast_test_option", option.Key);
            Assert.Equal("true", option.DefaultValue);
            Assert.Equal(2, option.Values.Count);
        }
        finally
        {
            Marshal.FreeHGlobal(definitions);
            foreach (var pointer in pointers) Marshal.FreeCoTaskMem(pointer);
            callbacks.Dispose();
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
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
