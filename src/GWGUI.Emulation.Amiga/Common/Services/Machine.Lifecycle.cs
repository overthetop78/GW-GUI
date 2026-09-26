using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Common.Constants;
using System.Collections.Concurrent;

namespace GWGUI.Emulation.Amiga.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{
public async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        Task started;
        lock (_gate)
        {
            ThrowIfDisposed();
            if (State is not EmulationMachineState.Created and not EmulationMachineState.Stopped)
                throw MachineExceptions.MachineNotRunning();
            State = EmulationMachineState.Starting;
            _stop = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                try { Run(_stop.Token); }
                finally { completion.TrySetResult(); }
            })
            {
                IsBackground = true,
                Name = $"GWGUI Amiga {Id:N}"
            };
            _runLoop = completion.Task;
            thread.Start();
            started = _started.Task;
        }
        await started.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public ValueTask PauseAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (State != EmulationMachineState.Running) return ValueTask.CompletedTask;
            _pauseRequested = true;
            State = EmulationMachineState.Paused;
            FlushAudio();
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask ResumeAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (State != EmulationMachineState.Paused) return ValueTask.CompletedTask;
            _pauseRequested = false;
            State = EmulationMachineState.Running;
            Monitor.PulseAll(_gate);
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask HardResetAsync(CancellationToken cancellationToken = default) =>
        QueueCommand(() => { FlushAudio(); _core.HardReset(); }, cancellationToken);

    public ValueTask SoftResetAsync(CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            FlushAudio();
            var resetKeys = new HashSet<EmulationKey>
                { EmulationKey.LeftControl, EmulationKey.LeftAmiga, EmulationKey.RightAmiga };
            _core.SetInput(EmulationInputSnapshot.Empty with { Keys = resetKeys });
            _core.RunFrame();
            _core.SetInput(EmulationInputSnapshot.Empty);
        }, cancellationToken);

    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        Task? loop;
        lock (_gate)
        {
            if (State is EmulationMachineState.Stopped or EmulationMachineState.Created) return;
            loop = _runLoop;
            if (State != EmulationMachineState.Faulted)
            {
                State = EmulationMachineState.Stopping;
                _stop?.Cancel();
                _pauseRequested = false;
                Monitor.PulseAll(_gate);
            }
        }
        if (loop is not null) await loop.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public void SetInput(EmulationInputSnapshot snapshot)
    {
        _lastPhysicalInput = snapshot;
        _core.SetInput(InputSnapshotFunctions.Apply(snapshot, Configuration.Input,
            _controllerPointerSwitchPressed));
    }

    private async ValueTask<bool> SwitchControllerPointerAsync(CancellationToken cancellationToken)
    {
        _controllerPointerSwitchPressed = true;
        SetInput(_lastPhysicalInput);
        await Task.Delay(100, cancellationToken).ConfigureAwait(false);
        _controllerPointerSwitchPressed = false;
        _controllerPointerMode = !_controllerPointerMode;
        SetInput(_lastPhysicalInput);
        return _controllerPointerMode;
    }

    public void SetAudioMuted(bool muted)
    {
        _audioMuted = muted;
        if (muted) FlushAudio();
    }

    public async ValueTask InsertMediaAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(path);
        if (_currentDiskPath is not null)
        {
            await EjectMediaAsync(cancellationToken).ConfigureAwait(false);
            await Task.Delay(MediaConstants.DiskChangeDelayMilliseconds, cancellationToken)
                .ConfigureAwait(false);
        }

        await QueueCommand(() =>
        {
            _core.InsertMedia(fullPath);
            var index = Math.Max(0, _core.CurrentDiskIndex);
            if (index < _mediaPaths.Count) _mediaPaths[index] = fullPath;
            else _mediaPaths.Add(fullPath);
            _currentDiskPath = fullPath;
        }, cancellationToken, EmulationMessageCategory.Media,
            EmulationMessageCode.MediaOperationFailed).ConfigureAwait(false);
    }

    public ValueTask EjectMediaAsync(CancellationToken cancellationToken = default) =>
        QueueCommand(() => { _core.EjectMedia(); _currentDiskPath = null; }, cancellationToken,
            EmulationMessageCategory.Media, EmulationMessageCode.MediaOperationFailed);

    public ValueTask InsertFloppyAsync(string path, CancellationToken cancellationToken = default) =>
        InsertMediaAsync(path, cancellationToken);

    public ValueTask EjectFloppyAsync(CancellationToken cancellationToken = default) =>
        EjectMediaAsync(cancellationToken);

    public ValueTask SelectDiskAsync(int index, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            _core.SelectDisk(index);
            if (index < _mediaPaths.Count) _currentDiskPath = _mediaPaths[index];
        }, cancellationToken, EmulationMessageCategory.Media,
            EmulationMessageCode.MediaOperationFailed);

    public ValueTask SaveStateAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            var state = _core.SaveState();
            var header = new SavedStateHeader(3, Configuration.Model, _core.CoreSha256,
                StateStore.HashFile(Configuration.KickstartPath),
                _currentDiskPath is null ? null : StateStore.HashPath(_currentDiskPath),
                new Dictionary<string, string>(_currentOptions, StringComparer.Ordinal),
                HashOptionalFile(Configuration.ExtendedRomPath), HashOptionalFile(Configuration.RomKeyPath),
                StateStore.HashBytes(state), _mediaPaths.Select(StateStore.HashPath).ToArray());
            StateStore.Write(path, header, state);
        }, cancellationToken, EmulationMessageCategory.SavedState,
            EmulationMessageCode.SavedStateOperationFailed);

    public ValueTask LoadStateAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            var saved = StateStore.Read(path);
            if (saved.Header.FormatVersion is < 1 or > 3 || saved.Header.Model != Configuration.Model
                || saved.Header.CoreSha256 != _core.CoreSha256
                || saved.Header.KickstartSha256 != StateStore.HashFile(Configuration.KickstartPath))
                throw MachineExceptions.SavedStateIncompatible();
            if (saved.Header.FormatVersion >= 2
                && (saved.Header.ExtendedRomSha256 != HashOptionalFile(Configuration.ExtendedRomPath)
                    || saved.Header.RomKeySha256 != HashOptionalFile(Configuration.RomKeyPath)
                    || saved.Header.MediaSha256 != HashOptionalPath(_currentDiskPath)
                    || !OptionsEqual(saved.Header.Options, _currentOptions)))
                throw MachineExceptions.SavedStateIncompatible();
            if (saved.Header.FormatVersion >= 3
                && !(saved.Header.MediaSha256s ?? []).SequenceEqual(_mediaPaths.Select(StateStore.HashPath), StringComparer.OrdinalIgnoreCase))
                throw MachineExceptions.SavedStateIncompatible();
            _core.LoadState(saved.State);
        }, cancellationToken, EmulationMessageCategory.SavedState,
            EmulationMessageCode.SavedStateOperationFailed);

    public ValueTask SetOptionAsync(string key, string value, CancellationToken cancellationToken = default) =>
        QueueCommand(() => { _core.SetOption(key, value); _currentOptions[key] = value; }, cancellationToken,
            EmulationMessageCategory.Machine, EmulationMessageCode.OptionInvalid);
}
