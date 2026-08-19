using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GWGUI.App.Controls;
using GWGUI.App.Input;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariMachineViewTests
{
    [Theory]
    [InlineData(AtariMachineViewTestConstants.FourThreeAspectRatio,
        AtariMachineViewTestConstants.FourThreeWidth, AtariMachineViewTestConstants.FourThreeHeight)]
    [InlineData(AtariMachineViewTestConstants.WideAspectRatio,
        AtariMachineViewTestConstants.WideWidth, AtariMachineViewTestConstants.WideHeight)]
    public void ScreenFitPreservesTheCoreAspectRatio(double aspect, double expectedWidth, double expectedHeight)
    {
        var size = AtariMachineViewFunctions.Fit(AtariMachineViewTestConstants.WideWidth,
            AtariMachineViewTestConstants.WideHeight, (float)aspect);

        Assert.Equal(expectedWidth, size.Width, AtariMachineViewTestConstants.DimensionPrecision);
        Assert.Equal(expectedHeight, size.Height, AtariMachineViewTestConstants.DimensionPrecision);
        Assert.Equal(aspect, size.Width / size.Height, AtariMachineViewTestConstants.AspectRatioPrecision);
    }

    [Fact]
    public void StatusUsesRuntimeValuesWithoutInventingFrequencyOrActivity()
    {
        var activity = new Dictionary<EmulationMediaSlot, bool> { [EmulationMediaSlot.Floppy0] = true };
        var runtime = new AtariRuntimeStatus(AtariMachineModel.Atari800Xl, AtariRuntimeRegion.Ntsc,
            AtariMachineViewTestConstants.NativeFramesPerSecond, AtariMachineViewTestConstants.SampleRate,
            new AtariRuntimeGeometry(AtariMachineViewTestConstants.FrameWidth,
                AtariMachineViewTestConstants.FrameHeight, AtariMachineViewTestConstants.FramePitch,
                (float)AtariMachineViewTestConstants.FourThreeAspectRatio), string.Empty, activity,
            new Dictionary<int, bool>(), default, default, default, null,
            AtariHostProcessState.Running, null);
        var frame = Frame((float)AtariMachineViewTestConstants.FourThreeAspectRatio);

        var status = AtariMachineViewFunctions.Status(runtime, frame,
            AtariMachineViewTestConstants.MeasuredFramesPerSecond, false, true, false);

        Assert.Contains(AtariMachineViewTestConstants.NativeFramesPerSecond.ToString("0.0"), status.Text);
        Assert.True(status.MediaActivity[EmulationMediaSlot.Floppy0]);
        Assert.True(status.AudioActive);
        Assert.True(status.MouseAvailable);
        Assert.False(status.ControllerAvailable);
    }

    [Fact]
    public void ScreenshotIsPngAndDoesNotOverwriteAnExistingCapture()
    {
        RunOnSta(() =>
        {
            var folder = Path.Combine(Path.GetTempPath(), AtariMachineViewTestConstants.CaptureFolderName,
                Guid.NewGuid().ToString(AtariEmulationConstants.IdentifierFormat));
            try
            {
                var pixels = new byte[AtariMachineViewTestConstants.PixelDimension
                    * AtariMachineViewTestConstants.PixelDimension * AtariMachineViewTestConstants.BytesPerPixel];
                var image = BitmapSource.Create(AtariMachineViewTestConstants.PixelDimension,
                    AtariMachineViewTestConstants.PixelDimension, AtariMachineViewTestConstants.PixelDpi,
                    AtariMachineViewTestConstants.PixelDpi, PixelFormats.Bgra32, null, pixels,
                    AtariMachineViewTestConstants.PixelStride);
                var timestamp = DateTime.Now;

                var first = AtariMachineViewFunctions.SaveScreenshot(image, folder, timestamp);
                var second = AtariMachineViewFunctions.SaveScreenshot(image, folder, timestamp);

                Assert.NotEqual(first, second);
                Assert.Equal(".png", Path.GetExtension(first), ignoreCase: true);
                Assert.True(File.Exists(first));
                Assert.True(File.Exists(second));
            }
            finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
        });
    }

    [Theory]
    [InlineData(Key.A, EmulationKey.A)]
    [InlineData(Key.D9, EmulationKey.D9)]
    [InlineData(Key.F10, EmulationKey.F10)]
    [InlineData(Key.Escape, EmulationKey.Escape)]
    public void KeyboardMapperCoversAtariHostKeys(Key source, EmulationKey expected)
    {
        Assert.True(AtariMachineInputFunctions.TryMap(source, out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ConfiguredKeyboardMappingOverridesOnlyItsHostKey()
    {
        var mappings = new Dictionary<string, EmulationKey> { [nameof(EmulationKey.AtariStart)] = EmulationKey.F1 };

        Assert.Equal(EmulationKey.AtariStart,
            AtariMachineInputFunctions.Resolve(EmulationKey.F1, mappings));
        Assert.Equal(EmulationKey.F2,
            AtariMachineInputFunctions.Resolve(EmulationKey.F2, mappings));
    }

    [Fact]
    public void PointerMovementIsPublishedOnlyWhileMouseCaptureIsActive()
    {
        var released = AtariMachineInputFunctions.Snapshot(new HashSet<EmulationKey>(),
            AtariMachineViewTestConstants.PointerDeltaX, AtariMachineViewTestConstants.PointerDeltaY,
            AtariMachineViewTestConstants.PointerWheel, false, []);
        var captured = AtariMachineInputFunctions.Snapshot(new HashSet<EmulationKey>(),
            AtariMachineViewTestConstants.PointerDeltaX, AtariMachineViewTestConstants.PointerDeltaY,
            AtariMachineViewTestConstants.PointerWheel, true, []);

        Assert.Equal(RelativeMouseCaptureConstants.NoMovement, released.Pointer.DeltaX);
        Assert.Equal(RelativeMouseCaptureConstants.NoMovement, released.Pointer.DeltaY);
        Assert.Equal(AtariMachineViewTestConstants.PointerDeltaX, captured.Pointer.DeltaX);
        Assert.Equal(AtariMachineViewTestConstants.PointerDeltaY, captured.Pointer.DeltaY);
        Assert.Equal(AtariMachineViewTestConstants.PointerWheel, captured.Pointer.Wheel);
    }

    [Theory]
    [InlineData(AtariMachineModel.Atari800Xl, AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0, true)]
    [InlineData(AtariMachineModel.Atari800Xl, AtariMediaKind.Cassette, EmulationMediaSlot.Cassette0, true)]
    [InlineData(AtariMachineModel.Atari2600, AtariMediaKind.Cartridge, EmulationMediaSlot.Cartridge0, true)]
    [InlineData(AtariMachineModel.JaguarCd, AtariMediaKind.CompactDisc, EmulationMediaSlot.Cd0, true)]
    [InlineData(AtariMachineModel.St, AtariMediaKind.HardDisk, EmulationMediaSlot.HardDisk0, false)]
    [InlineData(AtariMachineModel.St, AtariMediaKind.Directory, EmulationMediaSlot.HardDisk0, false)]
    public void MediaStripDescriptionFollowsConfiguredDevices(AtariMachineModel model,
        AtariMediaKind kind, EmulationMediaSlot slot, bool removable)
    {
        var configuration = new AtariMachineConfiguration(model, media:
        [
            new AtariMediaConfiguration(AtariMachineViewTestConstants.FirstMediaPath, kind, slot)
        ]);

        var media = AtariMachineViewFunctions.Media(configuration)
            .Single(item => item.Configuration.Kind == kind && item.Configuration.Slot == slot);

        Assert.Equal(kind, media.Configuration.Kind);
        Assert.Equal(slot, media.Configuration.Slot);
        Assert.Equal(removable, media.Removable);
    }

    [Fact]
    public void MediaStripIncludesTheEmptyPrimaryStFloppyDrive()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.St);

        var media = Assert.Single(AtariMachineViewFunctions.Media(configuration));

        Assert.Equal(AtariMediaKind.Floppy, media.Configuration.Kind);
        Assert.Equal(EmulationMediaSlot.Floppy0, media.Configuration.Slot);
        Assert.False(media.Configuration.IsInserted);
        Assert.Equal("A:", media.Label);
        Assert.True(media.Removable);
    }

    [Fact]
    public void ViewStartsStopsAndDisposesItsMachine()
    {
        RunOnSta(() =>
        {
            var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari2600,
                videoRenderer: EmulationVideoRenderer.Wpf);
            var machine = new ViewMachine(configuration);
            var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(
                AtariEmulationConstants.IdentifierFormat));
            var view = new AtariMachineController(machine, value => new ViewMachine(value), configuration,
                null, Path.Combine(folder, AtariMachineViewTestConstants.StateFileName), folder);

            view.StartAsync().GetAwaiter().GetResult();
            Assert.Equal(EmulationMachineState.Running, machine.State);
            view.StopAsync().GetAwaiter().GetResult();
            Assert.Equal(EmulationMachineState.Stopped, machine.State);
            Assert.True(machine.Disposed);
        });
    }

    [Fact]
    public void PowerCycleCreatesANewMachineAndRestoresMountedMedia()
    {
        RunOnSta(() =>
        {
            var media = new AtariMediaConfiguration(AtariMachineViewTestConstants.FirstMediaPath,
                AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0);
            var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari800Xl,
                media: [media], videoRenderer: EmulationVideoRenderer.Wpf);
            var machines = new List<ViewMachine>();
            ViewMachine Create(AtariMachineConfiguration value)
            {
                var machine = new ViewMachine(value);
                machines.Add(machine);
                return machine;
            }
            var original = Create(configuration);
            var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(
                AtariEmulationConstants.IdentifierFormat));
            var view = new AtariMachineController(original, Create, configuration, null,
                Path.Combine(folder, AtariMachineViewTestConstants.StateFileName), folder);

            view.StartAsync().GetAwaiter().GetResult();
            view.TogglePowerAsync().GetAwaiter().GetResult();
            view.TogglePowerAsync().GetAwaiter().GetResult();

            Assert.Equal(AtariMachineViewTestConstants.ExpectedPowerCycleMachineCount, machines.Count);
            Assert.True(original.Disposed);
            Assert.Contains(media, machines[^1].Configuration.Media);
            view.StopAsync().GetAwaiter().GetResult();
        });
    }

    [Fact]
    public void MountedFloppyIsInjectedIntoTheConfigurationUsedAtPowerOn()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.St);
        var floppy = new AtariMediaConfiguration(AtariMachineViewTestConstants.FirstMediaPath,
            AtariMediaKind.Floppy, EmulationMediaSlot.Floppy0, IsInserted: true);

        var runtime = AtariMachineViewFunctions.WithMountedMedia(configuration, [floppy]);

        Assert.Equal(floppy, Assert.Single(runtime.Media));
    }

    [Fact]
    public void FullscreenCanBeEnteredAndExitedWithoutChangingTheMachine()
    {
        RunOnSta(() =>
        {
            var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari2600,
                videoRenderer: EmulationVideoRenderer.Wpf);
            var machine = new ViewMachine(configuration);
            var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(
                AtariEmulationConstants.IdentifierFormat));
            var view = new AtariMachineController(machine, value => new ViewMachine(value), configuration,
                null, Path.Combine(folder, AtariMachineViewTestConstants.StateFileName), folder);

            view.ToggleFullscreenAsync().GetAwaiter().GetResult();
            Assert.True(view.IsFullscreen);
            view.ToggleFullscreenAsync().GetAwaiter().GetResult();
            Assert.False(view.IsFullscreen);
            view.StopAsync().GetAwaiter().GetResult();
        });
    }

    private static VideoFrame Frame(float aspect) => new(
        new byte[AtariMachineViewTestConstants.FrameHeight * AtariMachineViewTestConstants.FramePitch],
        AtariMachineViewTestConstants.FrameWidth, AtariMachineViewTestConstants.FrameHeight,
        AtariMachineViewTestConstants.FramePitch, EmulationPixelFormat.Xrgb8888, aspect, default, default);

    private static void RunOnSta(Action action)
        => WpfTestHost.Run(action);

    private sealed class ViewMachine : IAtariMachine
    {
        internal ViewMachine(AtariMachineConfiguration configuration) => Configuration = configuration;
        public List<AtariMediaConfiguration> InsertedMedia { get; } = [];
        public Guid Id { get; } = Guid.NewGuid();
        public EmulationMachineState State { get; private set; } = EmulationMachineState.Created;
        public AtariMachineConfiguration Configuration { get; }
        public Exception? Fault => null;
        public VideoFrame? LatestVideoFrame => null;
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<AtariCoreOption> AvailableOptions => [];
        public IReadOnlyList<string> Diagnostics => [];
        public IReadOnlyDictionary<int, bool> LedStates => new Dictionary<int, bool>();
        public string CoreName => string.Empty;
        public string CoreVersion => string.Empty;
        public IReadOnlySet<string> SupportedContentExtensions => new HashSet<string>();
        public bool SupportsSaveStates => true;
        public bool IsAudioMuted { get; private set; }
        public float AudioVolume => AtariMachineViewTestConstants.DefaultAudioVolume;
        public AtariRuntimeStatus RuntimeStatus => new(Configuration.Model, null, default, default, null,
            string.Empty, new Dictionary<EmulationMediaSlot, bool>(), new Dictionary<int, bool>(), default,
            default, default, null, AtariHostProcessState.NotStarted, null);
        public bool Disposed { get; private set; }
        public event EventHandler<VideoFrame>? VideoFrameReady { add { } remove { } }
        public event EventHandler<AudioChunk>? AudioChunkReady { add { } remove { } }
        public ValueTask StartAsync(CancellationToken cancellationToken = default)
        { State = EmulationMachineState.Running; return ValueTask.CompletedTask; }
        public ValueTask PauseAsync(CancellationToken cancellationToken = default)
        { State = EmulationMachineState.Paused; return ValueTask.CompletedTask; }
        public ValueTask ResumeAsync(CancellationToken cancellationToken = default)
        { State = EmulationMachineState.Running; return ValueTask.CompletedTask; }
        public ValueTask SoftResetAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask HardResetAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask StopAsync(CancellationToken cancellationToken = default)
        { State = EmulationMachineState.Stopped; return ValueTask.CompletedTask; }
        public void SetInput(EmulationInputSnapshot snapshot) { }
        public void SetControllerPortDevice(int port, AtariPeripheralKind peripheral) { }
        public void SetAudioMuted(bool muted) => IsAudioMuted = muted;
        public void SetAudioVolume(float volume) { }
        public void SetAudioOutputFactory(Func<IAudioOutput?>? factory) { }
        public ValueTask InsertMediaAsync(AtariMediaConfiguration media,
            CancellationToken cancellationToken = default)
        { InsertedMedia.Add(media); return ValueTask.CompletedTask; }
        public ValueTask EjectMediaAsync(EmulationMediaSlot slot,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask SelectDiskAsync(int index, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;
        public ValueTask SaveStateAsync(string path, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;
        public ValueTask LoadStateAsync(string path, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;
        public ValueTask SetOptionAsync(string key, string value,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask DisposeAsync() { Disposed = true; return ValueTask.CompletedTask; }
    }
}
