using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime,
    IEmulationCassetteTransport
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
            TaskCompletionSource? completed = null;
            var threadStarted = false;
            try
            {
                _stopSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                completed = completion;
                _thread = new Thread(() =>
                {
                    try { Run(_stopSource.Token); }
                    finally { completion.TrySetResult(); }
                })
                {
                    IsBackground = true,
                    Name = MachineFunctions.ThreadName(Id, Configuration.Core)
                };
                _runLoop = completion.Task;
                _thread.Start();
                threadStarted = true;
                started = _started.Task;
            }
            finally
            {
                if (!threadStarted)
                {
                    _thread = null;
                    completed?.TrySetResult();
                    try { _stopSource?.Dispose(); }
                    finally
                    {
                        _stopSource = null;
                        State = EmulationMachineState.Faulted;
                    }
                }
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

    public ValueTask SoftResetAsync(CancellationToken cancellationToken = default) => QueueCommand(() => ResetCore(MachineValues.Value0), cancellationToken);

    public ValueTask HardResetAsync(CancellationToken cancellationToken = default) => QueueCommand(() => ResetCore(MachineValues.Value1), cancellationToken);

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
            await ExternalCoreLifecycleFunctions.WaitForThreadAsync(loop, _thread).ConfigureAwait(false);
        }
    }

    public void SetInput(EmulationInputSnapshot snapshot) => _cassetteInput.SetPhysicalInput(InputSnapshotFunctions.Apply(snapshot, Configuration.Input, Configuration.Model));

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

    public ValueTask SaveStateAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            if (!_core.SupportsSaveStates)
                throw SavedStateFunctions.Invalid(ErrorCode.StateInvalid, ErrorMessages.StateUnavailable);
            var state = _core.SaveState();
            var header = SavedStateFunctions.CreateHeader(CurrentConfiguration(), _core, state);
            StateFileFunctions.Write(path, header, state);
        }, cancellationToken);

    public ValueTask LoadStateAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            if (!_core.SupportsSaveStates)
                throw SavedStateFunctions.Invalid(ErrorCode.StateInvalid, ErrorMessages.StateUnavailable);
            var saved = StateFileFunctions.Read(path);
            SavedStateFunctions.Validate(saved.Header, CurrentConfiguration(), _core);
            _core.LoadState(saved.State);
        }, cancellationToken);

    public ValueTask SetOptionAsync(string key, string value, CancellationToken cancellationToken = default) => QueueCommand(() => _core.SetOption(key, value), cancellationToken);

    ValueTask IEmulationCassetteTransport.ExecuteAsync(EmulationCassetteCommand command, CancellationToken cancellationToken) => QueueCommand(() =>
        {
            if (command != EmulationCassetteCommand.Play)
                throw new NotSupportedException(command.ToString());
            _cassetteInput.Play();
        }, cancellationToken);
}
