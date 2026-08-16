using System.IO;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

public sealed class AtariShortcutTests
{
    [Fact]
    public void CommonActionsAreAvailableButStatesFollowNativeCapability()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari800Xl);
        var unavailable = AtariShortcutFunctions.Rules(configuration, statesAvailable: false,
            quickStateExists: true);

        Assert.True(AtariShortcutFunctions.IsAvailable(unavailable, EmulationShortcutActions.Power));
        Assert.True(AtariShortcutFunctions.IsAvailable(unavailable, EmulationShortcutActions.PauseResume));
        Assert.True(AtariShortcutFunctions.IsAvailable(unavailable, EmulationShortcutActions.HardReset));
        Assert.True(AtariShortcutFunctions.IsAvailable(unavailable, EmulationShortcutActions.ToggleFullscreen));
        Assert.True(AtariShortcutFunctions.IsAvailable(unavailable, EmulationShortcutActions.ReleaseMouse));
        Assert.False(AtariShortcutFunctions.IsAvailable(unavailable, EmulationShortcutActions.QuickSave));
        Assert.False(AtariShortcutFunctions.IsAvailable(unavailable, EmulationShortcutActions.QuickLoad));

        var available = AtariShortcutFunctions.Rules(configuration, statesAvailable: true,
            quickStateExists: true);
        Assert.True(AtariShortcutFunctions.IsAvailable(available, EmulationShortcutActions.QuickSave));
        Assert.True(AtariShortcutFunctions.IsAvailable(available, EmulationShortcutActions.QuickLoad));
    }

    [Fact]
    public void MediaActionsFollowModelAndMountedMedia()
    {
        var empty = new AtariMachineConfiguration(AtariMachineModel.Atari800Xl);
        var emptyRules = AtariShortcutFunctions.Rules(empty, statesAvailable: false, quickStateExists: false);
        Assert.True(AtariShortcutFunctions.IsAvailable(emptyRules, EmulationShortcutActions.InsertMedia));
        Assert.False(AtariShortcutFunctions.IsAvailable(emptyRules, EmulationShortcutActions.EjectMedia));
        Assert.False(AtariShortcutFunctions.IsAvailable(emptyRules, EmulationShortcutActions.NextMedia));

        var populated = new AtariMachineConfiguration(AtariMachineModel.Atari800Xl, media:
        [
            new AtariMediaConfiguration(AtariShortcutTestConstants.FirstDiskPath, AtariMediaKind.Floppy,
                EmulationMediaSlot.Floppy0),
            new AtariMediaConfiguration(AtariShortcutTestConstants.SecondDiskPath, AtariMediaKind.Floppy,
                EmulationMediaSlot.Floppy1)
        ]);
        var populatedRules = AtariShortcutFunctions.Rules(populated, statesAvailable: false,
            quickStateExists: false);
        Assert.True(AtariShortcutFunctions.IsAvailable(populatedRules, EmulationShortcutActions.EjectMedia));
        Assert.True(AtariShortcutFunctions.IsAvailable(populatedRules, EmulationShortcutActions.NextMedia));

        var cartridge = new AtariMachineConfiguration(AtariMachineModel.Atari2600, media:
        [
            new AtariMediaConfiguration(AtariShortcutTestConstants.CartridgePath, AtariMediaKind.Cartridge,
                EmulationMediaSlot.Cartridge0)
        ]);
        var cartridgeRules = AtariShortcutFunctions.Rules(cartridge, statesAvailable: false,
            quickStateExists: false);
        Assert.True(AtariShortcutFunctions.IsAvailable(cartridgeRules, EmulationShortcutActions.InsertMedia));
        Assert.False(AtariShortcutFunctions.IsAvailable(cartridgeRules, EmulationShortcutActions.EjectMedia));
    }

    [Fact]
    public async Task ExecutorUsesMachineAndHostActionsOnlyWhenAvailable()
    {
        var statePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.state");
        var machine = new ShortcutMachine(new AtariMachineConfiguration(AtariMachineModel.Atari800Xl, media:
        [
            new AtariMediaConfiguration(AtariShortcutTestConstants.FirstDiskPath, AtariMediaKind.Floppy,
                EmulationMediaSlot.Floppy0)
        ]));
        var hostActions = new List<string>();
        var context = Context(statePath, hostActions);
        try
        {
            Assert.True(await AtariShortcutExecutionFunctions.ExecuteAsync(
                EmulationShortcutActions.PauseResume, machine, context));
            Assert.Equal(EmulationMachineState.Paused, machine.State);
            Assert.True(await AtariShortcutExecutionFunctions.ExecuteAsync(
                EmulationShortcutActions.PauseResume, machine, context));
            Assert.Equal(EmulationMachineState.Running, machine.State);
            Assert.True(await AtariShortcutExecutionFunctions.ExecuteAsync(
                EmulationShortcutActions.QuickSave, machine, context));
            Assert.True(await AtariShortcutExecutionFunctions.ExecuteAsync(
                EmulationShortcutActions.QuickLoad, machine, context));
            Assert.True(await AtariShortcutExecutionFunctions.ExecuteAsync(
                EmulationShortcutActions.ToggleFullscreen, machine, context));
            Assert.True(await AtariShortcutExecutionFunctions.ExecuteAsync(
                EmulationShortcutActions.InsertMedia, machine, context));
            Assert.Contains(EmulationShortcutActions.QuickSave, machine.ExecutedActions);
            Assert.Contains(EmulationShortcutActions.QuickLoad, machine.ExecutedActions);
            Assert.Contains(EmulationShortcutActions.ToggleFullscreen, hostActions);
            Assert.Contains(EmulationShortcutActions.InsertMedia, hostActions);
        }
        finally
        {
            if (File.Exists(statePath)) File.Delete(statePath);
        }
    }

    [Fact]
    public async Task ExecutorRejectsUnavailableStateAndMediaActions()
    {
        var machine = new ShortcutMachine(new AtariMachineConfiguration(AtariMachineModel.Atari2600))
        {
            SupportsSaveStates = false
        };
        var context = Context(AtariShortcutTestConstants.MissingStatePath, []);

        Assert.False(await AtariShortcutExecutionFunctions.ExecuteAsync(
            EmulationShortcutActions.QuickSave, machine, context));
        Assert.False(await AtariShortcutExecutionFunctions.ExecuteAsync(
            EmulationShortcutActions.QuickLoad, machine, context));
        Assert.False(await AtariShortcutExecutionFunctions.ExecuteAsync(
            EmulationShortcutActions.EjectMedia, machine, context));
        Assert.False(await AtariShortcutExecutionFunctions.ExecuteAsync(
            AtariShortcutTestConstants.UnknownAction, machine, context));
        Assert.Empty(machine.ExecutedActions);
    }

    private static AtariShortcutExecutionContext Context(string statePath, ICollection<string> actions)
    {
        Func<CancellationToken, ValueTask> Action(string action) => _ =>
        {
            actions.Add(action);
            return ValueTask.CompletedTask;
        };
        return new AtariShortcutExecutionContext(
            statePath,
            Action(EmulationShortcutActions.Power),
            Action(EmulationShortcutActions.ToggleFullscreen),
            Action(EmulationShortcutActions.ReleaseMouse),
            Action(EmulationShortcutActions.Screenshot),
            Action(EmulationShortcutActions.FastForward),
            Action(EmulationShortcutActions.InsertMedia),
            Action(EmulationShortcutActions.EjectMedia),
            Action(EmulationShortcutActions.NextMedia));
    }

    private sealed class ShortcutMachine(AtariMachineConfiguration configuration) : IAtariMachine
    {
        public List<string> ExecutedActions { get; } = [];
        public Guid Id { get; } = Guid.NewGuid();
        public EmulationMachineState State { get; private set; } = EmulationMachineState.Running;
        public AtariMachineConfiguration Configuration { get; } = configuration;
        public Exception? Fault => null;
        public VideoFrame? LatestVideoFrame => null;
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<AtariCoreOption> AvailableOptions => [];
        public IReadOnlyList<string> Diagnostics => [];
        public IReadOnlyDictionary<int, bool> LedStates => new Dictionary<int, bool>();
        public string CoreName => string.Empty;
        public string CoreVersion => string.Empty;
        public IReadOnlySet<string> SupportedContentExtensions => new HashSet<string>();
        public bool SupportsSaveStates { get; init; } = true;
        public bool IsAudioMuted { get; private set; }
        public float AudioVolume => AtariShortcutTestConstants.DefaultAudioVolume;
        public AtariRuntimeStatus RuntimeStatus => throw new NotSupportedException();
        public event EventHandler<VideoFrame>? VideoFrameReady { add { } remove { } }
        public event EventHandler<AudioChunk>? AudioChunkReady { add { } remove { } }
        public ValueTask StartAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask PauseAsync(CancellationToken cancellationToken = default)
        {
            State = EmulationMachineState.Paused;
            return ValueTask.CompletedTask;
        }
        public ValueTask ResumeAsync(CancellationToken cancellationToken = default)
        {
            State = EmulationMachineState.Running;
            return ValueTask.CompletedTask;
        }
        public ValueTask SoftResetAsync(CancellationToken cancellationToken = default)
        {
            ExecutedActions.Add(EmulationShortcutActions.SoftReset);
            return ValueTask.CompletedTask;
        }
        public ValueTask HardResetAsync(CancellationToken cancellationToken = default)
        {
            ExecutedActions.Add(EmulationShortcutActions.HardReset);
            return ValueTask.CompletedTask;
        }
        public ValueTask StopAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public void SetInput(EmulationInputSnapshot snapshot) { }
        public void SetControllerPortDevice(int port, AtariPeripheralKind peripheral) { }
        public void SetAudioMuted(bool muted) => IsAudioMuted = muted;
        public void SetAudioVolume(float volume) { }
        public void SetAudioOutputFactory(Func<IAudioOutput?>? factory) { }
        public ValueTask InsertMediaAsync(AtariMediaConfiguration media,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask EjectMediaAsync(EmulationMediaSlot slot,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask SelectDiskAsync(int index, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;
        public ValueTask SaveStateAsync(string path, CancellationToken cancellationToken = default)
        {
            File.WriteAllText(path, AtariShortcutTestConstants.StateContents);
            ExecutedActions.Add(EmulationShortcutActions.QuickSave);
            return ValueTask.CompletedTask;
        }
        public ValueTask LoadStateAsync(string path, CancellationToken cancellationToken = default)
        {
            ExecutedActions.Add(EmulationShortcutActions.QuickLoad);
            return ValueTask.CompletedTask;
        }
        public ValueTask SetOptionAsync(string key, string value,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}

internal static class AtariShortcutTestConstants
{
    internal const string FirstDiskPath = "first.atr";
    internal const string SecondDiskPath = "second.atr";
    internal const string CartridgePath = "game.a26";
    internal const string MissingStatePath = "missing-atari-shortcut-state.bin";
    internal const string UnknownAction = "unknown-action";
    internal const string StateContents = "state";
    internal const float DefaultAudioVolume = 1.0f;
}
