using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;
using System.IO;

namespace GWGUI.Emulation.Acorn.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{
    public async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        Task started;
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (State != EmulationMachineState.Created)
                throw new InvalidOperationException(ErrorMessages.MachineInvalidState);
            State = EmulationMachineState.Starting;
            var threadStarted = false;
            try
            {
                GWGUI.Emulation.Functions.ExternalCoreLifecycleFunctions.StartThread(
                    Run, MachineFunctions.ThreadName(Id, Configuration.Core), cancellationToken,
                    ref _stopSource, out _started, out _runLoop, ref _thread);
                threadStarted = true;
                started = _started.Task;
            }
            finally
            {
                if (!threadStarted) State = EmulationMachineState.Faulted;
            }
        }
        await GWGUI.Emulation.Functions.ExternalCoreLifecycleFunctions.WaitForStartOrStopAsync(
            started, cancellationToken, () => StopAsync()).ConfigureAwait(false);
    }

    public ValueTask PauseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (State != EmulationMachineState.Running) return ValueTask.CompletedTask;
            _pauseRequested = true;
            State = EmulationMachineState.Paused;
            _audio.Pause();
        }
        return QueueCommand(() => MachineFunctions.ReleaseInput(_core), cancellationToken);
    }

    public ValueTask ResumeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (State != EmulationMachineState.Paused) return ValueTask.CompletedTask;
            _pauseRequested = false;
            State = EmulationMachineState.Running;
            _audio.Resume();
            Monitor.PulseAll(_gate);
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask SoftResetAsync(CancellationToken cancellationToken = default) => QueueCommand(() => ResetCore(), cancellationToken);

    public ValueTask HardResetAsync(CancellationToken cancellationToken = default) => QueueCommand(() => ResetCore(), cancellationToken);

    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        Task? loop = null;
        try
        {
            lock (_gate)
            {
                if (State == EmulationMachineState.Created) return;
                loop = _runLoop;
                if (State is not EmulationMachineState.Faulted and not EmulationMachineState.Stopped)
                {
                    State = EmulationMachineState.Stopping;
                    try { _stopSource?.Cancel(); }
                    finally
                    {
                        _pauseRequested = false;
                        Monitor.PulseAll(_gate);
                    }
                }
            }
            if (loop is not null) await loop.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await GWGUI.Emulation.Functions.ExternalCoreLifecycleFunctions.WaitForThreadAsync(loop, _thread).ConfigureAwait(false);
        }
    }

    public void SetInput(EmulationInputSnapshot snapshot) => _input.Update(InputSnapshotFunctions.Apply(snapshot, Configuration.Input));

    public void SetControllerPortDevice(int port, PeripheralCategory peripheral) => QueueCommand(() => _core.SetControllerPortDevice(port, peripheral), CancellationToken.None).AsTask().GetAwaiter().GetResult();

    ValueTask<bool> IEmulationInput.SwitchControllerPointerAsync(CancellationToken cancellationToken) => ValueTask.FromResult(false);

    public void SetAudioMuted(bool muted)
    {
        _audio.SetMuted(muted);
    }

    public void SetAudioVolume(float volume) => _audio.SetVolume(volume);

    public void SetAudioOutputFactory(Func<IAudioOutput?>? factory) => _audio.ReplaceFactory(factory);

    public ValueTask InsertMediaAsync(MediaConfiguration media, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            var inserted = media with { IsInserted = true };
            _core.InsertMedia(inserted);
            MediaRuntimeFunctions.Register(_mountedMedia, inserted);
        }, cancellationToken);

    public ValueTask EjectMediaAsync(EmulationMediaSlot slot, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            _core.EjectMedia(slot);
            _mountedMedia.RemoveAll(item => item.Slot == slot);
        }, cancellationToken);

    public ValueTask SelectDiskAsync(int index, CancellationToken cancellationToken = default) => QueueCommand(() => _core.SelectDisk(index), cancellationToken);

    public ValueTask SaveStateAsync(string path, CancellationToken cancellationToken = default) => QueueCommand(() =>
    {
        if (!_core.SupportsSaveStates)
            throw new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateUnavailable);
        var configuration = StateConfiguration();
        var state = _core.SaveState();
        var header = new EmulationSavedStateHeader<MachineModel, Emulator>(
            EmulationSavedStateConstants.CurrentFormatVersion, _core.Emulator, _core.CoreName, _core.CoreVersion,
            _core.CoreSha256, configuration.Model, EmulationSdk.StateFiles.HashDocument(configuration),
            StateContentHash(configuration), EmulationSdk.StateFiles.HashBytes(state));
        EmulationSdk.StateFiles.WriteState(path, header, state);
    }, cancellationToken);

    public ValueTask LoadStateAsync(string path, CancellationToken cancellationToken = default) => QueueCommand(() =>
    {
        if (!_core.SupportsSaveStates)
            throw new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateUnavailable);
        var saved = EmulationSdk.StateFiles.ReadState<MachineModel, Emulator>(path,
            static () => new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateInvalid));
        var configuration = StateConfiguration();
        if (!EmulationSdk.StateFiles.IsCompatible(saved.Header, configuration.Model, _core.Emulator, _core,
                () => EmulationSdk.StateFiles.HashDocument(configuration), () => StateContentHash(configuration)))
            throw new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateInvalid);
        _core.LoadState(saved.State);
    }, cancellationToken);

    private MachineConfiguration StateConfiguration() => Configuration with
    {
        Id = Guid.Empty,
        Media = _mountedMedia.ToArray(),
        Options = _core.Options.ToDictionary(option => option.Key, option => option.CurrentValue, StringComparer.Ordinal)
    };

    private static string StateContentHash(MachineConfiguration configuration) => EmulationSdk.StateFiles.HashDocument(
        configuration.Firmwares.OrderBy(firmware => firmware.Category)
            .Select(firmware => new EmulationStateContentEntry("firmware", firmware.Category.ToString(), HashStatePath(firmware.Path)))
            .Concat(configuration.Media.Where(media => media.IsInserted).OrderBy(media => media.Slot)
                .ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase)
                .Select(media => new EmulationStateContentEntry("media", media.Category + ":" + media.Slot,
                    HashStatePath(media.Path)))).ToArray());

    private static string HashStatePath(string path) => EmulationSdk.StateFiles.HashPath(path, "*", '/', 64 * 1024,
        () => new FileNotFoundException(ErrorMessages.StateInvalid, path));

    public ValueTask SetOptionAsync(string key, string value, CancellationToken cancellationToken = default) => QueueCommand(() => _core.SetOption(key, value), cancellationToken);


}
